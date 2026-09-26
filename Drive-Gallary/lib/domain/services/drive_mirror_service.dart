import '../../core/constants/app_constants.dart';
import '../../core/errors/app_error.dart';
import '../../core/logging/app_logger.dart';
import '../../core/utils/id_generator.dart';
import '../models/album.dart';
import '../models/drive_models.dart';
import '../models/enums.dart';
import '../models/media_item.dart';
import '../repositories/album_repository.dart';
import '../repositories/drive_repository.dart';
import '../repositories/media_repository.dart';
import 'drive_root_service.dart';

// ignore_for_file: prefer_initializing_formals

/// Outcome of a Drive-tree mirror operation.
class DriveMirrorResult {
  const DriveMirrorResult({
    required this.albumsCreated,
    required this.filesImported,
  });
  final int albumsCreated;
  final int filesImported;
}

/// Mirrors the existing Google Drive folder tree (under the configured Drive
/// root) into the app as linked albums, and imports the files inside each
/// folder as `remoteOnly` media (download-on-demand). Idempotent: re-running
/// only adds what's missing (spec §32).
class DriveMirrorService {
  DriveMirrorService(
    this._drive,
    this._albums,
    this._media,
    this._root, {
    IdGenerator ids = const IdGenerator(),
  }) : _ids = ids;

  final DriveRepository _drive;
  final AlbumRepository _albums;
  final MediaRepository _media;
  final DriveRootService _root;
  final IdGenerator _ids;

  final _log = const AppLogger('DriveMirrorService');

  Future<DriveMirrorResult> importTree({required String ownerUserId}) async {
    final rootId = await _root.getRootFolderId();
    if (rootId == null || rootId.isEmpty) {
      throw AppError.driveNotFound('Set up the Drive root folder first.');
    }

    // Map existing linked albums by their Drive folder id to avoid duplicates.
    final existing = await _albums.getAllAlbums();
    final byFolderId = <String, Album>{
      for (final a in existing)
        if (a.driveFolderId != null) a.driveFolderId!: a,
    };

    var albumsCreated = 0;
    var filesImported = 0;

    // Depth-first traversal. Each entry: (driveFolderId, parentAppAlbumId).
    final stack = <(String, String?)>[(rootId, null)];
    while (stack.isNotEmpty) {
      final (folderId, parentAlbumId) = stack.removeLast();
      final children = await _drive.listChildren(folderId);

      for (final f in children) {
        if (f.mimeType == AppConstants.driveFolderMimeType) {
          var album = byFolderId[f.id];
          if (album == null) {
            album = await _createAlbum(
              f.id,
              f.name,
              folderId,
              parentAlbumId,
              ownerUserId,
            );
            byFolderId[f.id] = album;
            albumsCreated++;
          }
          stack.add((f.id, album.id));
        }
      }

      // Import files that live directly inside a folder that maps to an album.
      final albumForFolder = byFolderId[folderId];
      if (albumForFolder != null) {
        final files = children
            .where((f) => f.mimeType != AppConstants.driveFolderMimeType)
            .toList();
        filesImported += await _importFiles(albumForFolder.id, files);
      }
    }

    _log.i('Drive mirror: +$albumsCreated albums, +$filesImported files');
    return DriveMirrorResult(
      albumsCreated: albumsCreated,
      filesImported: filesImported,
    );
  }

  Future<Album> _createAlbum(
    String driveFolderId,
    String name,
    String driveParentFolderId,
    String? parentAlbumId,
    String ownerUserId,
  ) async {
    final now = DateTime.now();
    final album = Album(
      id: _ids.newId(),
      parentAlbumId: parentAlbumId,
      name: name,
      type: AlbumType.album,
      ownerUserId: ownerUserId,
      driveFolderId: driveFolderId,
      driveParentFolderId: driveParentFolderId,
      isDriveLinked: true,
      autoSyncEnabled: true,
      createdAt: now,
      updatedAt: now,
    );
    return _albums.createAlbum(album);
  }

  Future<int> _importFiles(String albumId, List<DriveFile> files) async {
    var count = 0;
    for (final f in files) {
      final existing = await _media.findByDriveFileId(f.id);
      if (existing != null) continue;
      final now = DateTime.now();
      await _media.upsertMedia(
        MediaItem(
          id: _ids.newId(),
          albumId: albumId,
          localUri: '',
          fileName: f.name.isEmpty ? 'file' : f.name,
          mimeType: f.mimeType ?? 'application/octet-stream',
          sizeBytes: f.size ?? 0,
          modifiedAt: f.modifiedTime,
          driveFileId: f.id,
          syncStatus: SyncStatus.remoteOnly,
          createdAt: now,
          updatedAt: now,
        ),
      );
      count++;
    }
    return count;
  }
}
