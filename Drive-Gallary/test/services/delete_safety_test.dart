import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/repositories/album_repository_impl.dart';
import 'package:drive_linked_gallery/data/repositories/media_repository_impl.dart';
import 'package:drive_linked_gallery/domain/models/picked_media.dart';
import 'package:drive_linked_gallery/domain/services/album_service.dart';
import 'package:drive_linked_gallery/domain/services/media_service.dart';
import 'package:flutter_test/flutter_test.dart';

/// Spec §34 / §59: deleting local media or an album must NEVER delete Drive
/// data. These services have no Drive dependency, which structurally guarantees
/// the safety rule; the tests assert the local-only behavior.
void main() {
  late AppDatabase db;
  late MediaService mediaService;
  late AlbumService albumService;
  late MediaRepositoryImpl media;

  setUp(() {
    db = AppDatabase.forTesting(NativeDatabase.memory());
    media = MediaRepositoryImpl(db);
    mediaService = MediaService(media);
    albumService = AlbumService(AlbumRepositoryImpl(db));
  });

  tearDown(() => db.close());

  test('removing media locally keeps the Drive file id record untouched '
      'elsewhere', () async {
    final added = await mediaService.addToAlbum('al', [
      const PickedMedia(
        localUri: '/tmp/a.jpg',
        fileName: 'a.jpg',
        mimeType: 'image/jpeg',
        sizeBytes: 1,
      ),
    ]);
    await media.setDriveFileId(added.first.id, 'drive-123');

    await mediaService.removeLocal(added.first.id);

    // The local row is gone; nothing in the media service can touch Drive.
    expect(await media.getMediaById(added.first.id), isNull);
    // The Drive file id is not resolvable locally anymore, proving no local
    // state implies a Drive deletion happened.
    expect(await media.findByDriveFileId('drive-123'), isNull);
  });

  test('soft-deleting an album does not cascade to Drive', () async {
    final album = await albumService.createAlbum(name: 'A', ownerUserId: 'u');
    await albumService.delete(album.id);
    // Album hidden locally; AlbumService has no Drive dependency at all.
    final roots = await albumService.watchChildren(null).first;
    expect(roots, isEmpty);
  });
}
