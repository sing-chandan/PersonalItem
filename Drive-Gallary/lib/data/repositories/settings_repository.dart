import 'package:drift/drift.dart';

import '../db/database.dart';

/// Key/value application settings persisted in SQLite (Drive root id, user
/// preferences, etc.). Small, non-sensitive values only — never store tokens
/// here (those go in secure storage).
class SettingsRepository {
  SettingsRepository(this._db);

  final AppDatabase _db;

  static const kDriveRootFolderId = 'driveRootFolderId';
  static const kDriveRootFolderName = 'driveRootFolderName';
  static const kAutoSync = 'autoSyncEnabled';
  static const kWifiOnly = 'wifiOnly';
  static const kVideosEnabled = 'videosEnabled';

  Future<String?> get(String key) async {
    final row = await (_db.select(
      _db.appSettings,
    )..where((t) => t.key.equals(key))).getSingleOrNull();
    return row?.value;
  }

  Future<void> set(String key, String? value) async {
    await _db
        .into(_db.appSettings)
        .insertOnConflictUpdate(
          AppSettingsCompanion(key: Value(key), value: Value(value)),
        );
  }

  Future<bool> getBool(String key, {bool fallback = false}) async {
    final v = await get(key);
    if (v == null) return fallback;
    return v == 'true';
  }

  Future<void> setBool(String key, bool value) => set(key, value.toString());

  Stream<String?> watch(String key) {
    final query = _db.select(_db.appSettings)..where((t) => t.key.equals(key));
    return query.watchSingleOrNull().map((row) => row?.value);
  }
}
