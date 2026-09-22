import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/repositories/album_repository_impl.dart';
import 'package:drive_linked_gallery/domain/models/album.dart';
import 'package:drive_linked_gallery/domain/models/enums.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  late AppDatabase db;
  late AlbumRepositoryImpl albums;

  setUp(() {
    db = AppDatabase.forTesting(NativeDatabase.memory());
    albums = AlbumRepositoryImpl(db);
  });

  tearDown(() async {
    await db.close();
  });

  Album makeAlbum(String id, {String? parent}) {
    final now = DateTime.now();
    return Album(
      id: id,
      parentAlbumId: parent,
      name: 'Album $id',
      type: AlbumType.album,
      ownerUserId: 'u1',
      isDriveLinked: false,
      autoSyncEnabled: true,
      createdAt: now,
      updatedAt: now,
    );
  }

  test('create and read root albums', () async {
    await albums.createAlbum(makeAlbum('a'));
    await albums.createAlbum(makeAlbum('b', parent: 'a'));

    final roots = await albums.getChildAlbums(null);
    expect(roots.length, 1);
    expect(roots.first.id, 'a');

    final children = await albums.getChildAlbums('a');
    expect(children.length, 1);
    expect(children.first.id, 'b');
  });

  test('soft delete hides album', () async {
    await albums.createAlbum(makeAlbum('a'));
    await albums.softDeleteAlbum('a');
    final roots = await albums.getChildAlbums(null);
    expect(roots, isEmpty);
  });
}
