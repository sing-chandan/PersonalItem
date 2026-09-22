import 'enums.dart';

/// Immutable domain representation of a persisted sync job (section 11).
class SyncJob {
  const SyncJob({
    required this.id,
    required this.type,
    required this.albumId,
    this.mediaItemId,
    required this.priority,
    required this.status,
    required this.attemptCount,
    this.lastError,
    this.nextAttemptAt,
    required this.createdAt,
    required this.updatedAt,
  });

  final String id;
  final SyncJobType type;
  final String albumId;
  final String? mediaItemId;
  final int priority;
  final SyncJobStatus status;
  final int attemptCount;
  final String? lastError;
  final DateTime? nextAttemptAt;
  final DateTime createdAt;
  final DateTime updatedAt;
}
