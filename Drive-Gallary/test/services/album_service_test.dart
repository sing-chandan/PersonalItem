import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/repositories/album_repository_impl.dart';
import 'package:drive_linked_gallery/domain/models/enums.dart';
import 'package:drive_linked_gallery/domain/services/album_service.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  late AppDatabase db;
  late AlbumService service;

  setUp(() {
    db = AppDatabase.forTesting(NativeDatabase.memory());
    service = AlbumService(AlbumRepositoryImpl(db));
  });

  tearDown(() => db.close());

  test('creates a root album', () async {
    final album = await service.createAlbum(
      name: 'Project A',
      ownerUserId: 'u',
    );
    expect(album.name, 'Project A');
    expect(album.parentAlbumId, isNull);
    expect(album.type, AlbumType.album);
  });

  test('creates nested album and inherits drive parent', () async {
    final parent = await service.createAlbum(
      name: 'Project A',
      ownerUserId: 'u',
    );
    final child = await service.createAlbum(
      name: 'Jaipur',
      ownerUserId: 'u',
      parentAlbumId: parent.id,
    );
    expect(child.parentAlbumId, parent.id);
  });

  test('rejects empty name', () async {
    expect(
      () => service.createAlbum(name: '   ', ownerUserId: 'u'),
      throwsArgumentError,
    );
  });

  test('rename updates the name', () async {
    final album = await service.createAlbum(name: 'Old', ownerUserId: 'u');
    final renamed = await service.rename(album, 'New');
    expect(renamed.name, 'New');
    final fetched = await service.getById(album.id);
    expect(fetched!.name, 'New');
  });

  test('delete soft-deletes the album', () async {
    final album = await service.createAlbum(name: 'Temp', ownerUserId: 'u');
    await service.delete(album.id);
    final children = await service.watchChildren(null).first;
    expect(children, isEmpty);
  });
}
