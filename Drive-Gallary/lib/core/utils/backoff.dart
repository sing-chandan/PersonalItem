import '../constants/app_constants.dart';

/// Computes a bounded exponential backoff delay for retrying failed jobs.
Duration computeBackoff(
  int attemptCount, {
  Duration base = AppConstants.baseRetryBackoff,
  Duration max = AppConstants.maxRetryBackoff,
}) {
  if (attemptCount <= 0) return base;
  final factor = 1 << (attemptCount - 1).clamp(0, 20);
  final millis = base.inMilliseconds * factor;
  final capped = millis.clamp(base.inMilliseconds, max.inMilliseconds);
  return Duration(milliseconds: capped);
}
