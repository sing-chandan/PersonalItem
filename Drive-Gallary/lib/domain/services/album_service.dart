import '../../core/utils/id_generator.dart';
import '../models/album.dart';
import '../models/enums.dart';
import '../repositories/album_repository.dart';

/// Application service that orchestrates album use-cases. Keeps business rules
/// out of the UI and out of the repository (which is pure persistence).
class AlbumService {
  AlbumService(this._repo, [this._ids = const IdGenerator()]);

  final AlbumRepository _repo;
  final IdGenerator _ids;

  Stream<List<Album>> watchChildren(String? parentId) =>
      _repo.watchChildAlbums(parentId);

  Future<Album?> getById(String id) => _repo.getAlbumById(id);

  /// Creates a local album. Drive linking is added in later phases; a newly
  /// created album inherits its Drive parent from [parent] when available.
  Future<Album> createAlbum({
    required String name,
    required String ownerUserId,
    String? parentAlbumId,
    AlbumType type = AlbumType.album,
    bool autoSyncEnabled = true,
  }) async {
    final trimmed = name.trim();
    if (trimmed.isEmpty) {
      throw ArgumentError('Album name cannot be empty');
    }
    Album? parent;
    if (parentAlbumId != null) {
      parent = await _repo.getAlbumById(parentAlbumId);
    }
    final now = DateTime.now();
    final album = Album(
      id: _ids.newId(),
      parentAlbumId: parentAlbumId,
      name: trimmed,
      type: type,
      ownerUserId: ownerUserId,
      driveParentFolderId: parent?.driveFolderId,
      isDriveLinked: false,
      autoSyncEnabled: autoSyncEnabled,
      createdAt: now,
      updatedAt: now,
    );
    return _repo.createAlbum(album);
  }

  Future<Album> rename(Album album, String newName) {
    final trimmed = newName.trim();
    if (trimmed.isEmpty) {
      throw ArgumentError('Album name cannot be empty');
    }
    return _repo.updateAlbum(
      album.copyWith(name: trimmed, updatedAt: DateTime.now()),
    );
  }

  /// Soft-deletes locally. Per spec safety rule (§34), never deletes Drive data.
  Future<void> delete(String albumId) => _repo.softDeleteAlbum(albumId);
}
