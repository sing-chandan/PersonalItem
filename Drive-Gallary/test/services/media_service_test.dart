import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/repositories/media_repository_impl.dart';
import 'package:drive_linked_gallery/domain/models/enums.dart';
import 'package:drive_linked_gallery/domain/models/picked_media.dart';
import 'package:drive_linked_gallery/domain/services/media_service.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  late AppDatabase db;
  late MediaService service;

  setUp(() {
    db = AppDatabase.forTesting(NativeDatabase.memory());
    service = MediaService(MediaRepositoryImpl(db));
  });

  tearDown(() => db.close());

  PickedMedia picked(String name, {String? storeId}) => PickedMedia(
    localUri: '/tmp/$name',
    fileName: name,
    mimeType: 'image/jpeg',
    sizeBytes: 1000,
    localMediaStoreId: storeId,
  );

  test('adds media to an album as localOnly', () async {
    final added = await service.addToAlbum('album1', [picked('a.jpg')]);
    expect(added.length, 1);
    expect(added.first.syncStatus, SyncStatus.localOnly);
    expect(await service.countInAlbum('album1'), 1);
  });

  test('skips duplicates by local store id', () async {
    await service.addToAlbum('album1', [picked('a.jpg', storeId: 's1')]);
    final second = await service.addToAlbum('album1', [
      picked('a.jpg', storeId: 's1'),
    ]);
    expect(second, isEmpty);
    expect(await service.countInAlbum('album1'), 1);
  });

  test('pending count reflects unsynced items', () async {
    await service.addToAlbum('album1', [picked('a.jpg'), picked('b.jpg')]);
    expect(await service.countPending('album1'), 2);
  });

  test('removeLocal deletes only locally', () async {
    final added = await service.addToAlbum('album1', [picked('a.jpg')]);
    await service.removeLocal(added.first.id);
    expect(await service.countInAlbum('album1'), 0);
  });
}
