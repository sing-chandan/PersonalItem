/// Application-wide constants. Avoid scattering magic strings/numbers.
class AppConstants {
  AppConstants._();

  static const String appName = 'Drive-Linked Gallery';

  /// Default Drive root folder name suggested during first-time setup.
  static const String defaultDriveRootName = 'Gallery Cloud';

  /// Google Drive MIME type for folders.
  static const String driveFolderMimeType =
      'application/vnd.google-apps.folder';

  /// Least-privilege Drive scope: only files created/opened by this app.
  static const String driveFileScope =
      'https://www.googleapis.com/auth/drive.file';

  /// Upload retry policy.
  static const int maxUploadAttempts = 6;
  static const Duration baseRetryBackoff = Duration(seconds: 4);
  static const Duration maxRetryBackoff = Duration(minutes: 30);

  /// Media pagination page size.
  static const int mediaPageSize = 120;

  /// Default bulk-rename categories (section 25).
  static const List<String> defaultCategories = <String>[
    'Before',
    'WIP',
    'Meeting',
    'After',
    'Distribution',
    'Other',
  ];
}
