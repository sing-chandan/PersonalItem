import '../../core/constants/app_constants.dart';
import '../../core/errors/app_error.dart';
import '../../core/logging/app_logger.dart';
import '../../core/utils/backoff.dart';
import '../models/enums.dart';
import '../models/sync_job.dart';
import '../repositories/album_repository.dart';
import '../repositories/drive_repository.dart';
import '../repositories/media_repository.dart';
import '../repositories/sync_job_repository.dart';
import 'linked_album_service.dart';
import 'media_bytes_reader.dart';

/// Executes a single [SyncJob]. Pure orchestration so it can be driven from an
/// in-app processor now and an Android WorkManager task later.
class UploadEngine {
  UploadEngine({
    required SyncJobRepository jobs,
    required MediaRepository media,
    required AlbumRepository albums,
    required DriveRepository drive,
    required MediaBytesReader reader,
    required LinkedAlbumService linker,
  }) : _jobs = jobs,
       _media = media,
       _albums = albums,
       _drive = drive,
       _reader = reader,
       _linker = linker;
  // Using explicit fields (not initializing formals) keeps named params typed.
  // ignore_for_file: prefer_initializing_formals

  final SyncJobRepository _jobs;
  final MediaRepository _media;
  final AlbumRepository _albums;
  final DriveRepository _drive;
  final MediaBytesReader _reader;
  final LinkedAlbumService _linker;

  final _log = const AppLogger('UploadEngine');

  /// Processes one job. Returns true on success. Idempotent and safe to retry.
  Future<bool> processJob(SyncJob job) async {
    if (job.type != SyncJobType.upload) {
      await _jobs.updateStatus(job.id, SyncJobStatus.succeeded);
      return true;
    }
    final mediaId = job.mediaItemId;
    if (mediaId == null) {
      await _jobs.updateStatus(
        job.id,
        SyncJobStatus.cancelled,
        lastError: 'no media',
      );
      return false;
    }

    final item = await _media.getMediaById(mediaId);
    if (item == null) {
      // Media was removed locally; nothing to upload.
      await _jobs.updateStatus(
        job.id,
        SyncJobStatus.cancelled,
        lastError: 'media removed',
      );
      return true;
    }

    // Duplicate protection (spec §24): already uploaded.
    if (item.driveFileId != null && item.driveFileId!.isNotEmpty) {
      await _media.updateSyncStatus(item.id, SyncStatus.synced);
      await _jobs.updateStatus(job.id, SyncJobStatus.succeeded);
      return true;
    }

    await _jobs.updateStatus(job.id, SyncJobStatus.running);

    // Resolve the destination Drive folder, linking the album if necessary.
    var album = await _albums.getAlbumById(item.albumId);
    if (album == null) {
      await _jobs.updateStatus(
        job.id,
        SyncJobStatus.cancelled,
        lastError: 'album missing',
      );
      return false;
    }
    if (album.driveFolderId == null) {
      final result = await _linker.linkExisting(album);
      album = result.album;
    }
    if (album.driveFolderId == null) {
      return _fail(
        job,
        item.id,
        AppError.authRequired('Album not linked to Drive.'),
        network: false,
      );
    }

    try {
      await _media.updateSyncStatus(item.id, SyncStatus.uploading);
      final bytes = await _reader.read(item.localUri);
      final uploaded = await _drive.uploadFile(
        parentId: album.driveFolderId!,
        name: item.fileName,
        mimeType: item.mimeType,
        bytes: bytes,
      );
      await _media.setDriveFileId(item.id, uploaded.id);
      await _media.updateSyncStatus(item.id, SyncStatus.synced);
      await _jobs.updateStatus(
        job.id,
        SyncJobStatus.succeeded,
        lastError: null,
      );
      _log.i('Uploaded ${item.fileName} -> ${uploaded.id}');
      return true;
    } on AppError catch (e) {
      final isNet = e.code == AppErrorCode.networkUnavailable;
      return _fail(job, item.id, e, network: isNet);
    } catch (e) {
      return _fail(job, item.id, AppError.uploadFailed('$e'), network: false);
    }
  }

  Future<bool> _fail(
    SyncJob job,
    String mediaId,
    AppError error, {
    required bool network,
  }) async {
    final attempt = job.attemptCount + 1;
    if (attempt >= AppConstants.maxUploadAttempts) {
      await _media.updateSyncStatus(mediaId, SyncStatus.failed);
      await _jobs.updateStatus(
        job.id,
        SyncJobStatus.failed,
        attemptCount: attempt,
        lastError: error.message,
      );
      _log.w('Upload permanently failed for job ${job.id}: ${error.message}');
      return false;
    }
    await _media.updateSyncStatus(
      mediaId,
      network ? SyncStatus.waitingForNetwork : SyncStatus.failed,
    );
    // Re-queue with backoff so the processor picks it up later.
    await _jobs.updateStatus(
      job.id,
      SyncJobStatus.queued,
      attemptCount: attempt,
      lastError: error.message,
      nextAttemptAt: DateTime.now().add(computeBackoff(attempt)),
    );
    return false;
  }
}
