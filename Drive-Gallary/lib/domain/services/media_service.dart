import '../../core/utils/id_generator.dart';
import '../models/enums.dart';
import '../models/media_item.dart';
import '../models/picked_media.dart';
import '../repositories/media_repository.dart';

/// Orchestrates adding/removing media within albums. Adding always works
/// offline (spec §23); network sync is handled separately by the sync engine.
class MediaService {
  MediaService(this._repo, {this._ids = const IdGenerator()});

  final MediaRepository _repo;
  final IdGenerator _ids;

  Stream<List<MediaItem>> watchAlbumMedia(String albumId) =>
      _repo.watchMediaInAlbum(albumId);

  Future<int> countInAlbum(String albumId) => _repo.countInAlbum(albumId);

  Future<int> countPending(String albumId) =>
      _repo.countPendingInAlbum(albumId);

  /// Adds picked media to an album, skipping items already present (duplicate
  /// prevention by local MediaStore id when available). Returns the items that
  /// were newly added.
  Future<List<MediaItem>> addToAlbum(
    String albumId,
    List<PickedMedia> picked,
  ) async {
    final added = <MediaItem>[];
    for (final p in picked) {
      if (p.localMediaStoreId != null) {
        final existing = await _repo.findByLocalStoreId(
          albumId,
          p.localMediaStoreId!,
        );
        if (existing != null) continue;
      }
      final now = DateTime.now();
      final item = MediaItem(
        id: _ids.newId(),
        localMediaStoreId: p.localMediaStoreId,
        albumId: albumId,
        localUri: p.localUri,
        fileName: p.fileName,
        mimeType: p.mimeType,
        sizeBytes: p.sizeBytes,
        width: p.width,
        height: p.height,
        capturedAt: p.capturedAt,
        modifiedAt: p.modifiedAt,
        syncStatus: SyncStatus.localOnly,
        createdAt: now,
        updatedAt: now,
      );
      added.add(await _repo.upsertMedia(item));
    }
    return added;
  }

  /// Removes media locally only. Per spec §34, never deletes the Drive file.
  Future<void> removeLocal(String mediaId) => _repo.deleteMediaLocal(mediaId);
}
