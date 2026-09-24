import '../../core/logging/app_logger.dart';
import '../models/media_item.dart';
import '../repositories/drive_repository.dart';
import '../repositories/media_repository.dart';
import 'bulk_rename_service.dart';

/// Bulk operations on already-added media: category assignment and bulk rename
/// (spec §25, §26). Renames update the local record and, when the item is
/// already uploaded, the Drive file name too.
class BulkMediaService {
  BulkMediaService(this._media, this._drive, this._rename);

  final MediaRepository _media;
  final DriveRepository _drive;
  final BulkRenameService _rename;

  final _log = const AppLogger('BulkMediaService');

  Future<void> setCategory(List<MediaItem> items, String? category) async {
    for (final item in items) {
      await _media.upsertMedia(
        item.copyWith(category: category, updatedAt: DateTime.now()),
      );
    }
  }

  /// Renames [items] as `Category_001.ext` etc. Ordering is the given order
  /// (callers sort by capture time or selection order first). Drive files are
  /// renamed for already-synced items; failures are logged and do not lose the
  /// local mapping.
  Future<List<MediaItem>> rename({
    required List<MediaItem> items,
    required String category,
    int startNumber = 1,
  }) async {
    final names = _rename.generateNames(
      originalNames: items.map((i) => i.fileName).toList(),
      category: category,
      startNumber: startNumber,
    );
    final updated = <MediaItem>[];
    for (var i = 0; i < items.length; i++) {
      final item = items[i];
      final newName = names[i];
      final next = item.copyWith(
        fileName: newName,
        category: category,
        sequenceNumber: startNumber + i,
        updatedAt: DateTime.now(),
      );
      await _media.upsertMedia(next);
      if (item.driveFileId != null && item.driveFileId!.isNotEmpty) {
        try {
          await _drive.renameFile(item.driveFileId!, newName);
        } catch (e) {
          _log.w('Drive rename failed for ${item.driveFileId}', e);
        }
      }
      updated.add(next);
    }
    return updated;
  }
}
