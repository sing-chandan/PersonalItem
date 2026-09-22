import 'package:drift/drift.dart';

import '../../domain/models/enums.dart';
import '../../domain/models/sync_job.dart';
import '../../domain/repositories/sync_job_repository.dart';
import '../db/database.dart';
import '../db/mappers.dart';

class SyncJobRepositoryImpl implements SyncJobRepository {
  SyncJobRepositoryImpl(this._db);

  final AppDatabase _db;

  @override
  Future<SyncJob> enqueue(SyncJob job) async {
    await _db.into(_db.syncJobs).insert(job.toCompanion());
    return job;
  }

  @override
  Future<List<SyncJob>> getRunnableJobs({int limit = 20}) async {
    final now = DateTime.now();
    final query = _db.select(_db.syncJobs)
      ..where(
        (t) =>
            t.status.equals(SyncJobStatus.queued.name) &
            (t.nextAttemptAt.isNull() |
                t.nextAttemptAt.isSmallerOrEqualValue(now)),
      )
      ..orderBy([
        (t) => OrderingTerm(expression: t.priority, mode: OrderingMode.desc),
        (t) => OrderingTerm(expression: t.createdAt),
      ])
      ..limit(limit);
    final rows = await query.get();
    return rows.map(syncJobFromRow).toList();
  }

  @override
  Stream<List<SyncJob>> watchActiveJobs() {
    final query = _db.select(_db.syncJobs)
      ..where(
        (t) => t.status.isIn([
          SyncJobStatus.queued.name,
          SyncJobStatus.running.name,
          SyncJobStatus.paused.name,
          SyncJobStatus.failed.name,
        ]),
      )
      ..orderBy([(t) => OrderingTerm(expression: t.createdAt)]);
    return query.watch().map((rows) => rows.map(syncJobFromRow).toList());
  }

  @override
  Future<SyncJob?> getById(String id) async {
    final row = await (_db.select(
      _db.syncJobs,
    )..where((t) => t.id.equals(id))).getSingleOrNull();
    return row == null ? null : syncJobFromRow(row);
  }

  @override
  Future<void> updateStatus(
    String id,
    SyncJobStatus status, {
    int? attemptCount,
    String? lastError,
    DateTime? nextAttemptAt,
  }) async {
    await (_db.update(_db.syncJobs)..where((t) => t.id.equals(id))).write(
      SyncJobsCompanion(
        status: Value(status.name),
        attemptCount: attemptCount == null
            ? const Value.absent()
            : Value(attemptCount),
        lastError: Value(lastError),
        nextAttemptAt: Value(nextAttemptAt),
        updatedAt: Value(DateTime.now()),
      ),
    );
  }

  @override
  Future<void> clearCompleted() async {
    await (_db.delete(
      _db.syncJobs,
    )..where((t) => t.status.equals(SyncJobStatus.succeeded.name))).go();
  }

  @override
  Future<void> pauseAll() async {
    await (_db.update(
      _db.syncJobs,
    )..where((t) => t.status.equals(SyncJobStatus.queued.name))).write(
      SyncJobsCompanion(
        status: Value(SyncJobStatus.paused.name),
        updatedAt: Value(DateTime.now()),
      ),
    );
  }

  @override
  Future<void> resumeAll() async {
    await (_db.update(_db.syncJobs)..where(
          (t) => t.status.isIn([
            SyncJobStatus.paused.name,
            SyncJobStatus.failed.name,
          ]),
        ))
        .write(
          SyncJobsCompanion(
            status: Value(SyncJobStatus.queued.name),
            nextAttemptAt: const Value(null),
            updatedAt: Value(DateTime.now()),
          ),
        );
  }

  @override
  Future<int> countActive() async {
    final count = _db.syncJobs.id.count();
    final query = _db.selectOnly(_db.syncJobs)
      ..addColumns([count])
      ..where(
        _db.syncJobs.status.isIn([
          SyncJobStatus.queued.name,
          SyncJobStatus.running.name,
        ]),
      );
    final row = await query.getSingle();
    return row.read(count) ?? 0;
  }
}
