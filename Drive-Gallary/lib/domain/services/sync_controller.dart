import 'dart:async';

import 'package:connectivity_plus/connectivity_plus.dart';

import '../../core/logging/app_logger.dart';
import '../../core/utils/id_generator.dart';
import '../models/enums.dart';
import '../models/media_item.dart';
import '../models/sync_job.dart';
import '../repositories/media_repository.dart';
import '../repositories/sync_job_repository.dart';
import 'upload_engine.dart';

// ignore_for_file: prefer_initializing_formals

/// Drives the persistent upload queue: enqueues jobs, processes runnable jobs,
/// and reacts to connectivity changes. This is the in-app processor; on Android
/// the same [UploadEngine] is additionally scheduled via WorkManager for true
/// background execution (see SYNC_ENGINE.md).
class SyncController {
  SyncController({
    required SyncJobRepository jobs,
    required MediaRepository media,
    required UploadEngine engine,
    Connectivity? connectivity,
    IdGenerator ids = const IdGenerator(),
  }) : _jobs = jobs,
       _media = media,
       _engine = engine,
       _connectivity = connectivity ?? Connectivity(),
       _ids = ids;

  final SyncJobRepository _jobs;
  final MediaRepository _media;
  final UploadEngine _engine;
  final Connectivity _connectivity;
  final IdGenerator _ids;

  final _log = const AppLogger('SyncController');
  bool _running = false;
  StreamSubscription<List<ConnectivityResult>>? _connSub;

  /// Call once at app start: recovers persisted jobs and starts processing.
  void start() {
    _connSub ??= _connectivity.onConnectivityChanged.listen((results) {
      if (_hasNetwork(results)) {
        _log.i('Connectivity restored; processing queue');
        processQueue();
      }
    });
    processQueue();
  }

  void dispose() {
    _connSub?.cancel();
  }

  /// Enqueues an upload for a media item and kicks off processing.
  Future<void> enqueueUpload(MediaItem item) async {
    final now = DateTime.now();
    await _jobs.enqueue(
      SyncJob(
        id: _ids.newId(),
        type: SyncJobType.upload,
        albumId: item.albumId,
        mediaItemId: item.id,
        priority: 0,
        status: SyncJobStatus.queued,
        attemptCount: 0,
        createdAt: now,
        updatedAt: now,
      ),
    );
    await _media.updateSyncStatus(item.id, SyncStatus.queued);
    processQueue();
  }

  Future<void> enqueueUploads(Iterable<MediaItem> items) async {
    for (final item in items) {
      await enqueueUpload(item);
    }
  }

  /// Processes all currently-runnable jobs. Re-entrancy protected.
  Future<void> processQueue() async {
    if (_running) return;
    _running = true;
    try {
      if (!await _online()) {
        _log.i('Offline; deferring queue processing');
        return;
      }
      while (true) {
        final batch = await _jobs.getRunnableJobs(limit: 5);
        if (batch.isEmpty) break;
        for (final job in batch) {
          if (!await _online()) return;
          await _engine.processJob(job);
        }
      }
    } finally {
      _running = false;
    }
  }

  Future<void> retryFailed() async {
    await _jobs.resumeAll();
    await processQueue();
  }

  Future<void> pause() => _jobs.pauseAll();

  Future<void> resume() async {
    await _jobs.resumeAll();
    await processQueue();
  }

  Future<void> clearCompleted() => _jobs.clearCompleted();

  Future<bool> _online() async =>
      _hasNetwork(await _connectivity.checkConnectivity());

  bool _hasNetwork(List<ConnectivityResult> results) =>
      results.isNotEmpty && !results.every((r) => r == ConnectivityResult.none);
}
