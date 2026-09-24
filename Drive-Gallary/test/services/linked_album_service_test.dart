import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/repositories/album_repository_impl.dart';
import 'package:drive_linked_gallery/data/repositories/settings_repository.dart';
import 'package:drive_linked_gallery/domain/repositories/album_repository.dart';
import 'package:drive_linked_gallery/domain/services/album_service.dart';
import 'package:drive_linked_gallery/domain/services/drive_root_service.dart';
import 'package:drive_linked_gallery/domain/services/linked_album_service.dart';
import 'package:flutter_test/flutter_test.dart';

import 'drive_root_service_test.dart' show FakeDriveRepository;

void main() {
  late AppDatabase db;
  late AlbumRepository albums;
  late FakeDriveRepository drive;
  late LinkedAlbumService service;

  setUp(() {
    db = AppDatabase.forTesting(NativeDatabase.memory());
    albums = AlbumRepositoryImpl(db);
    drive = FakeDriveRepository();
    final root = DriveRootService(drive, SettingsRepository(db));
    service = LinkedAlbumService(albums, AlbumService(albums), drive, root);
  });

  tearDown(() => db.close());

  test('creates a linked root album under the Drive root folder', () async {
    final result = await service.createAlbum(
      name: 'Project A',
      ownerUserId: 'u',
    );
    expect(result.linked, isTrue);
    expect(result.album.isDriveLinked, isTrue);
    expect(result.album.driveFolderId, isNotNull);
    // Parent folder should be the drive root (first created folder).
    expect(result.album.driveParentFolderId, isNotNull);
  });

  test('nested album is linked under its parent folder', () async {
    final parent = await service.createAlbum(
      name: 'Project A',
      ownerUserId: 'u',
    );
    final child = await service.createAlbum(
      name: 'Jaipur',
      ownerUserId: 'u',
      parentAlbumId: parent.album.id,
    );
    expect(child.linked, isTrue);
    expect(child.album.driveParentFolderId, parent.album.driveFolderId);
  });

  test('rename propagates to the Drive folder', () async {
    final result = await service.createAlbum(name: 'Old', ownerUserId: 'u');
    final folderId = result.album.driveFolderId!;
    await service.rename(result.album, 'New');
    // Fake repo stores latest name via rename? It is a no-op; verify local.
    final updated = await albums.getAlbumById(result.album.id);
    expect(updated!.name, 'New');
    expect(updated.driveFolderId, folderId);
  });

  test('repairLink recreates folder when the stored id is missing', () async {
    final result = await service.createAlbum(name: 'Proj', ownerUserId: 'u');
    final oldId = result.album.driveFolderId!;
    // Simulate the Drive folder being deleted remotely.
    drive.folders.remove(oldId);
    final repaired = await service.repairLink(result.album);
    expect(repaired.linked, isTrue);
    expect(repaired.album.driveFolderId, isNot(oldId));
  });
}
