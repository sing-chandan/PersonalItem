import '../../core/logging/app_logger.dart';
import '../models/album.dart';
import '../models/enums.dart';
import '../repositories/album_repository.dart';
import '../repositories/drive_repository.dart';
import 'album_service.dart';
import 'drive_root_service.dart';

/// Result of a link attempt, so the UI can inform the user when an album was
/// created locally but could not (yet) be linked to Drive.
class AlbumLinkResult {
  const AlbumLinkResult(this.album, {this.linked, this.reason});
  final Album album;
  final bool? linked;
  final String? reason;
}

/// Orchestrates the album ↔ Drive folder mapping (spec §3, §16, §20, §46).
///
/// Album creation always succeeds locally first (offline-first). Drive linking
/// is then attempted as a best effort; failures leave the album unlinked so it
/// can be repaired later, never blocking the user's local work.
class LinkedAlbumService {
  LinkedAlbumService(this._albums, this._albumService, this._drive, this._root);

  final AlbumRepository _albums;
  final AlbumService _albumService;
  final DriveRepository _drive;
  final DriveRootService _root;

  final _log = const AppLogger('LinkedAlbumService');

  /// Creates a local album and attempts to create/link its Drive folder.
  Future<AlbumLinkResult> createAlbum({
    required String name,
    required String ownerUserId,
    String? parentAlbumId,
    AlbumType type = AlbumType.album,
    bool autoSyncEnabled = true,
  }) async {
    final album = await _albumService.createAlbum(
      name: name,
      ownerUserId: ownerUserId,
      parentAlbumId: parentAlbumId,
      type: type,
      autoSyncEnabled: autoSyncEnabled,
    );
    return _tryLink(album);
  }

  /// Renames an album locally, propagating the rename to Drive when linked.
  Future<Album> rename(Album album, String newName) async {
    final updated = await _albumService.rename(album, newName);
    if (updated.isDriveLinked && updated.driveFolderId != null) {
      try {
        await _drive.renameFolder(updated.driveFolderId!, updated.name);
      } catch (e) {
        _log.w('Drive folder rename failed (will need repair)', e);
      }
    }
    return updated;
  }

  /// Links (or re-links) an existing local album to Drive. Used for repair and
  /// for albums created while offline/unauthenticated.
  Future<AlbumLinkResult> linkExisting(Album album) => _tryLink(album);

  /// Repairs a broken mapping: if the stored folder id no longer resolves,
  /// clears it and creates a fresh folder.
  Future<AlbumLinkResult> repairLink(Album album) async {
    if (album.driveFolderId != null) {
      try {
        final folder = await _drive.getFolderById(album.driveFolderId!);
        if (folder != null) {
          // Mapping is valid; ensure the flag is consistent.
          if (!album.isDriveLinked) {
            final fixed = await _albums.updateAlbum(
              album.copyWith(isDriveLinked: true, updatedAt: DateTime.now()),
            );
            return AlbumLinkResult(fixed, linked: true);
          }
          return AlbumLinkResult(album, linked: true);
        }
      } catch (e) {
        _log.w('repairLink lookup failed', e);
        return AlbumLinkResult(album, linked: false, reason: 'lookup_failed');
      }
      // Folder is gone: clear the stale id before recreating.
      album = await _albums.updateAlbum(
        album.copyWith(
          driveFolderId: null,
          isDriveLinked: false,
          updatedAt: DateTime.now(),
        ),
      );
    }
    return _tryLink(album);
  }

  Future<AlbumLinkResult> _tryLink(Album album) async {
    if (album.isDriveLinked && album.driveFolderId != null) {
      return AlbumLinkResult(album, linked: true);
    }
    try {
      final parentFolderId = await _resolveParentFolderId(album);
      final folder = await _drive.createFolder(
        album.name,
        parentId: parentFolderId,
      );
      final linked = await _albums.updateAlbum(
        album.copyWith(
          driveFolderId: folder.id,
          driveParentFolderId: parentFolderId,
          isDriveLinked: true,
          updatedAt: DateTime.now(),
        ),
      );
      return AlbumLinkResult(linked, linked: true);
    } catch (e) {
      _log.w('Album created locally but Drive link deferred', e);
      return AlbumLinkResult(album, linked: false, reason: 'drive_unavailable');
    }
  }

  /// Determines the Drive parent folder id for [album]: the parent album's
  /// linked folder (linking the parent first if needed), or the Drive root.
  Future<String?> _resolveParentFolderId(Album album) async {
    if (album.parentAlbumId != null) {
      final parent = await _albums.getAlbumById(album.parentAlbumId!);
      if (parent != null) {
        if (parent.driveFolderId != null) return parent.driveFolderId;
        // Parent not linked yet: link it recursively so hierarchy matches.
        final linkedParent = await _tryLink(parent);
        if (linkedParent.album.driveFolderId != null) {
          return linkedParent.album.driveFolderId;
        }
      }
    }
    // No parent (or parent link failed): use the Drive root.
    final root = await _root.ensureRoot();
    return root.id;
  }
}
