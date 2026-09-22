import 'package:drift/drift.dart';
import 'package:drift_flutter/drift_flutter.dart';

import 'tables.dart';

part 'database.g.dart';

@DriftDatabase(
  tables: [Users, Albums, MediaItems, AlbumMembers, SyncJobs, AppSettings],
)
class AppDatabase extends _$AppDatabase {
  AppDatabase([QueryExecutor? executor])
    : super(
        executor ??
            driftDatabase(
              name: 'drive_linked_gallery',
              web: DriftWebOptions(
                sqlite3Wasm: Uri.parse('sqlite3.wasm'),
                driftWorker: Uri.parse('drift_worker.js'),
              ),
            ),
      );

  /// In-memory database for tests.
  AppDatabase.forTesting(super.executor);

  @override
  int get schemaVersion => 1;

  @override
  MigrationStrategy get migration => MigrationStrategy(
    onCreate: (m) async {
      await m.createAll();
      await _createIndexes();
    },
    beforeOpen: (details) async {
      await customStatement('PRAGMA foreign_keys = ON');
    },
  );

  /// Indexes required by section 12 of the specification.
  Future<void> _createIndexes() async {
    await customStatement(
      'CREATE INDEX IF NOT EXISTS idx_albums_parent ON albums(parent_album_id)',
    );
    await customStatement(
      'CREATE INDEX IF NOT EXISTS idx_albums_drive_folder ON albums(drive_folder_id)',
    );
    await customStatement(
      'CREATE UNIQUE INDEX IF NOT EXISTS uq_albums_drive_folder ON albums(drive_folder_id) WHERE drive_folder_id IS NOT NULL',
    );
    await customStatement(
      'CREATE INDEX IF NOT EXISTS idx_media_album ON media_items(album_id)',
    );
    await customStatement(
      'CREATE INDEX IF NOT EXISTS idx_media_drive_file ON media_items(drive_file_id)',
    );
    await customStatement(
      'CREATE INDEX IF NOT EXISTS idx_media_local_store ON media_items(local_media_store_id)',
    );
    await customStatement(
      'CREATE INDEX IF NOT EXISTS idx_media_sync_status ON media_items(sync_status)',
    );
    await customStatement(
      'CREATE UNIQUE INDEX IF NOT EXISTS uq_media_drive_file ON media_items(drive_file_id) WHERE drive_file_id IS NOT NULL',
    );
    await customStatement(
      'CREATE INDEX IF NOT EXISTS idx_syncjobs_status ON sync_jobs(status)',
    );
    await customStatement(
      'CREATE INDEX IF NOT EXISTS idx_syncjobs_album ON sync_jobs(album_id)',
    );
  }
}
