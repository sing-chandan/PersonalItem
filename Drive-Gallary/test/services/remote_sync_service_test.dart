import 'dart:typed_data';

import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/repositories/album_repository_impl.dart';
import 'package:drive_linked_gallery/data/repositories/media_repository_impl.dart';
import 'package:drive_linked_gallery/data/repositories/settings_repository.dart';
import 'package:drive_linked_gallery/domain/models/enums.dart';
import 'package:drive_linked_gallery/domain/models/picked_media.dart';
import 'package:drive_linked_gallery/domain/services/album_service.dart';
import 'package:drive_linked_gallery/domain/services/drive_root_service.dart';
import 'package:drive_linked_gallery/domain/services/linked_album_service.dart';
import 'package:drive_linked_gallery/domain/services/media_service.dart';
import 'package:drive_linked_gallery/domain/services/remote_sync_service.dart';
import 'package:flutter_test/flutter_test.dart';

import 'drive_root_service_test.dart' show FakeDriveRepository;

void main() {
  late AppDatabase db;
  late FakeDriveRepository drive;
  late MediaRepositoryImpl media;
  late LinkedAlbumService linker;
  late MediaService mediaService;
  late RemoteSyncService remote;

  setUp(() {
    db = AppDatabase.forTesting(NativeDatabase.memory());
    final albums = AlbumRepositoryImpl(db);
    drive = FakeDriveRepository();
    media = MediaRepositoryImpl(db);
    final root = DriveRootService(drive, SettingsRepository(db));
    linker = LinkedAlbumService(albums, AlbumService(albums), drive, root);
    mediaService = MediaService(media);
    remote = RemoteSyncService(drive, media);
  });

  tearDown(() => db.close());

  test('detects remote-only files and imports them', () async {
    final album = (await linker.createAlbum(name: 'A', ownerUserId: 'u')).album;
    // A file exists in the Drive folder with no local counterpart.
    await drive.uploadFile(
      parentId: album.driveFolderId!,
      name: 'remote.jpg',
      mimeType: 'image/jpeg',
      bytes: Uint8List(4),
    );
    final result = await remote.scan(album);
    expect(result.remoteOnly.length, 1);
    expect(result.conflicts, isEmpty);

    final imported = await remote.importRemoteOnly(album, result.remoteOnly);
    expect(imported, 1);
    final inAlbum = await media.getMediaInAlbum(album.id);
    expect(inAlbum.single.syncStatus, SyncStatus.remoteOnly);
  });

  test('detects a name conflict and keepCloud resolves it', () async {
    final album = (await linker.createAlbum(name: 'A', ownerUserId: 'u')).album;
    final added = await mediaService.addToAlbum(album.id, [
      const PickedMedia(
        localUri: '/tmp/a.jpg',
        fileName: 'local.jpg',
        mimeType: 'image/jpeg',
        sizeBytes: 1,
      ),
    ]);
    // Simulate the item having been uploaded with a matching drive file.
    final file = await drive.uploadFile(
      parentId: album.driveFolderId!,
      name: 'cloud.jpg',
      mimeType: 'image/jpeg',
      bytes: Uint8List(4),
    );
    await media.setDriveFileId(added.first.id, file.id);

    final result = await remote.scan(album);
    expect(result.conflicts.length, 1);

    await remote.keepCloud(result.conflicts.first);
    final updated = await media.getMediaById(added.first.id);
    expect(updated!.fileName, 'cloud.jpg');
    expect(updated.syncStatus, SyncStatus.synced);
  });
}
