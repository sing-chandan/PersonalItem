import 'package:drift/drift.dart';

import '../../domain/models/album.dart';
import '../../domain/repositories/album_repository.dart';
import '../db/database.dart';
import '../db/mappers.dart';

class AlbumRepositoryImpl implements AlbumRepository {
  AlbumRepositoryImpl(this._db);

  final AppDatabase _db;

  @override
  Future<List<Album>> getAllAlbums() async {
    final rows = await (_db.select(
      _db.albums,
    )..where((t) => t.deletedAt.isNull())).get();
    return rows.map(albumFromRow).toList();
  }

  @override
  Future<List<Album>> getChildAlbums(String? parentId) async {
    final query = _db.select(_db.albums)..where((t) => t.deletedAt.isNull());
    if (parentId == null) {
      query.where((t) => t.parentAlbumId.isNull());
    } else {
      query.where((t) => t.parentAlbumId.equals(parentId));
    }
    query.orderBy([(t) => OrderingTerm(expression: t.name)]);
    final rows = await query.get();
    return rows.map(albumFromRow).toList();
  }

  @override
  Future<Album?> getAlbumById(String id) async {
    final row = await (_db.select(
      _db.albums,
    )..where((t) => t.id.equals(id))).getSingleOrNull();
    return row == null ? null : albumFromRow(row);
  }

  @override
  Stream<List<Album>> watchChildAlbums(String? parentId) {
    final query = _db.select(_db.albums)..where((t) => t.deletedAt.isNull());
    if (parentId == null) {
      query.where((t) => t.parentAlbumId.isNull());
    } else {
      query.where((t) => t.parentAlbumId.equals(parentId));
    }
    query.orderBy([(t) => OrderingTerm(expression: t.name)]);
    return query.watch().map((rows) => rows.map(albumFromRow).toList());
  }

  @override
  Future<Album> createAlbum(Album album) async {
    await _db.into(_db.albums).insert(album.toCompanion());
    return album;
  }

  @override
  Future<Album> updateAlbum(Album album) async {
    await (_db.update(
      _db.albums,
    )..where((t) => t.id.equals(album.id))).write(album.toCompanion());
    return album;
  }

  @override
  Future<void> softDeleteAlbum(String id) async {
    await (_db.update(_db.albums)..where((t) => t.id.equals(id))).write(
      AlbumsCompanion(
        deletedAt: Value(DateTime.now()),
        updatedAt: Value(DateTime.now()),
      ),
    );
  }
}
