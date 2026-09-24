import 'dart:typed_data';

import '../../core/errors/app_error.dart';
import '../../core/utils/id_generator.dart';
import '../models/album.dart';
import '../models/drive_models.dart';
import '../models/enums.dart';
import '../models/media_item.dart';
import '../repositories/drive_repository.dart';
import '../repositories/media_repository.dart';

// ignore_for_file: prefer_initializing_formals

/// A detected local/remote naming conflict (spec §33).
class SyncConflict {
  const SyncConflict(this.local, this.remote);
  final MediaItem local;
  final DriveFile remote;
}

/// Result of scanning a linked album's Drive folder (spec §32).
class RemoteScanResult {
  const RemoteScanResult({
    required this.remoteFiles,
    required this.remoteOnly,
    required this.conflicts,
  });

  /// All non-folder files found in the Drive folder.
  final List<DriveFile> remoteFiles;

  /// Files present in Drive but not yet represented locally.
  final List<DriveFile> remoteOnly;

  /// Files matched to local media (by driveFileId) whose names diverge.
  final List<SyncConflict> conflicts;
}

/// Detects and reconciles remote (Drive) changes for linked albums. The initial
/// implementation uses download-on-demand and never auto-downloads bytes
/// (spec §32) and never silently overwrites conflicts (spec §33).
class RemoteSyncService {
  RemoteSyncService(
    this._drive,
    this._media, {
    IdGenerator ids = const IdGenerator(),
  }) : _ids = ids;

  final DriveRepository _drive;
  final MediaRepository _media;
  final IdGenerator _ids;

  Future<RemoteScanResult> scan(Album album) async {
    final folderId = album.driveFolderId;
    if (folderId == null) {
      throw AppError.driveNotFound('Album is not linked to a Drive folder.');
    }
    final children = await _drive.listChildren(folderId);
    final files = children
        .where((f) => f.mimeType != 'application/vnd.google-apps.folder')
        .toList();

    final remoteOnly = <DriveFile>[];
    final conflicts = <SyncConflict>[];
    for (final file in files) {
      final local = await _media.findByDriveFileId(file.id);
      if (local == null) {
        remoteOnly.add(file);
      } else if (local.fileName != file.name) {
        conflicts.add(SyncConflict(local, file));
      }
    }
    return RemoteScanResult(
      remoteFiles: files,
      remoteOnly: remoteOnly,
      conflicts: conflicts,
    );
  }

  /// Imports remote-only files as `remoteOnly` media entries (no bytes yet).
  Future<int> importRemoteOnly(Album album, List<DriveFile> files) async {
    var count = 0;
    for (final file in files) {
      final existing = await _media.findByDriveFileId(file.id);
      if (existing != null) continue;
      final now = DateTime.now();
      await _media.upsertMedia(
        MediaItem(
          id: _ids.newId(),
          albumId: album.id,
          localUri: '',
          fileName: file.name,
          mimeType: file.mimeType ?? 'application/octet-stream',
          sizeBytes: file.size ?? 0,
          modifiedAt: file.modifiedTime,
          driveFileId: file.id,
          syncStatus: SyncStatus.remoteOnly,
          createdAt: now,
          updatedAt: now,
        ),
      );
      count++;
    }
    return count;
  }

  /// Download-on-demand: fetches a remote file's bytes into memory.
  Future<Uint8List> download(String driveFileId) =>
      _drive.downloadFile(driveFileId);

  // --- Conflict resolution (spec §33) ---

  /// Keep the local name: rename the Drive file to match, then mark synced.
  Future<void> keepLocal(SyncConflict c) async {
    await _drive.renameFile(c.remote.id, c.local.fileName);
    await _media.updateSyncStatus(c.local.id, SyncStatus.synced);
  }

  /// Keep the cloud name: update the local record to match, then mark synced.
  Future<void> keepCloud(SyncConflict c) async {
    await _media.upsertMedia(
      c.local.copyWith(
        fileName: c.remote.name,
        syncStatus: SyncStatus.synced,
        updatedAt: DateTime.now(),
      ),
    );
  }
}
