import '../models/media_item.dart';
import '../models/enums.dart';

/// Repository contract for media item persistence.
abstract class MediaRepository {
  Future<List<MediaItem>> getMediaInAlbum(
    String albumId, {
    int limit = 120,
    int offset = 0,
  });

  Stream<List<MediaItem>> watchMediaInAlbum(String albumId);

  Future<MediaItem?> getMediaById(String id);

  Future<MediaItem?> findByDriveFileId(String driveFileId);

  Future<MediaItem?> findByContentHash(String albumId, String contentHash);

  Future<MediaItem?> findByLocalStoreId(String albumId, String localStoreId);

  Future<MediaItem> upsertMedia(MediaItem item);

  Future<void> updateSyncStatus(String id, SyncStatus status);

  Future<void> setDriveFileId(String id, String driveFileId);

  Future<int> countInAlbum(String albumId);

  Future<int> countPendingInAlbum(String albumId);

  Future<void> deleteMediaLocal(String id);
}
