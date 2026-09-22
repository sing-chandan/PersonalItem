import '../models/album.dart';

/// Repository contract for album persistence and hierarchy queries.
abstract class AlbumRepository {
  Future<List<Album>> getAllAlbums();

  /// Direct children of [parentId]; pass null for root-level albums.
  Future<List<Album>> getChildAlbums(String? parentId);

  Future<Album?> getAlbumById(String id);

  Stream<List<Album>> watchChildAlbums(String? parentId);

  Future<Album> createAlbum(Album album);

  Future<Album> updateAlbum(Album album);

  /// Soft-delete (sets deletedAt). Never touches Drive.
  Future<void> softDeleteAlbum(String id);
}
