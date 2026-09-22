/// Canonical error codes used throughout the application. These map to the
/// error taxonomy defined in the master specification (section 38).
enum AppErrorCode {
  authRequired,
  permissionDenied,
  networkUnavailable,
  driveRateLimit,
  driveNotFound,
  fileNotFound,
  uploadFailed,
  downloadFailed,
  conflict,
  duplicate,
  storageFull,
  mediaPermissionDenied,
  unknown,
}

/// A safe, user-presentable error object. Raw exception details are kept in
/// [debugDetail] for logging only and must never be shown directly to users.
class AppError implements Exception {
  const AppError({
    required this.code,
    required this.message,
    this.debugDetail,
    this.cause,
  });

  final AppErrorCode code;

  /// Human-readable, safe message to display to end users.
  final String message;

  /// Extra technical context for logs. Never surface this to normal users.
  final String? debugDetail;

  /// The original error/exception, retained for logging.
  final Object? cause;

  factory AppError.authRequired([String? detail]) => AppError(
    code: AppErrorCode.authRequired,
    message: 'Please sign in with your Google account to continue.',
    debugDetail: detail,
  );

  factory AppError.permissionDenied([String? detail]) => AppError(
    code: AppErrorCode.permissionDenied,
    message: 'You do not have permission to perform this action.',
    debugDetail: detail,
  );

  factory AppError.networkUnavailable([String? detail]) => AppError(
    code: AppErrorCode.networkUnavailable,
    message: 'No internet connection. This will resume automatically.',
    debugDetail: detail,
  );

  factory AppError.driveRateLimit([String? detail]) => AppError(
    code: AppErrorCode.driveRateLimit,
    message: 'Google Drive is busy. Retrying shortly.',
    debugDetail: detail,
  );

  factory AppError.driveNotFound([String? detail]) => AppError(
    code: AppErrorCode.driveNotFound,
    message: 'The linked Google Drive folder could not be found.',
    debugDetail: detail,
  );

  factory AppError.fileNotFound([String? detail]) => AppError(
    code: AppErrorCode.fileNotFound,
    message: 'The file could not be found.',
    debugDetail: detail,
  );

  factory AppError.uploadFailed([String? detail]) => AppError(
    code: AppErrorCode.uploadFailed,
    message: 'Upload failed. It will be retried automatically.',
    debugDetail: detail,
  );

  factory AppError.downloadFailed([String? detail]) => AppError(
    code: AppErrorCode.downloadFailed,
    message: 'Download failed. Please try again.',
    debugDetail: detail,
  );

  factory AppError.conflict([String? detail]) => AppError(
    code: AppErrorCode.conflict,
    message: 'A sync conflict needs your attention.',
    debugDetail: detail,
  );

  factory AppError.duplicate([String? detail]) => AppError(
    code: AppErrorCode.duplicate,
    message: 'This item already exists and was skipped.',
    debugDetail: detail,
  );

  factory AppError.storageFull([String? detail]) => AppError(
    code: AppErrorCode.storageFull,
    message: 'Not enough storage space available.',
    debugDetail: detail,
  );

  factory AppError.mediaPermissionDenied([String? detail]) => AppError(
    code: AppErrorCode.mediaPermissionDenied,
    message: 'Photo & media access is required to browse your photos.',
    debugDetail: detail,
  );

  factory AppError.unknown([String? detail, Object? cause]) => AppError(
    code: AppErrorCode.unknown,
    message: 'Something went wrong. Please try again.',
    debugDetail: detail,
    cause: cause,
  );

  @override
  String toString() =>
      'AppError(${code.name}): $message'
      '${debugDetail != null ? ' | $debugDetail' : ''}';
}
