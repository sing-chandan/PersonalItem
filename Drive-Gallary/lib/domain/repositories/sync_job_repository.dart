import '../models/sync_job.dart';
import '../models/enums.dart';

/// Repository contract for the persistent sync job queue.
abstract class SyncJobRepository {
  Future<SyncJob> enqueue(SyncJob job);

  Future<List<SyncJob>> getRunnableJobs({int limit = 20});

  Stream<List<SyncJob>> watchActiveJobs();

  Future<SyncJob?> getById(String id);

  Future<void> updateStatus(
    String id,
    SyncJobStatus status, {
    int? attemptCount,
    String? lastError,
    DateTime? nextAttemptAt,
  });

  Future<void> clearCompleted();

  Future<void> pauseAll();

  Future<void> resumeAll();

  Future<int> countActive();
}
