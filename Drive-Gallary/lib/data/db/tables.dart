import 'package:drift/drift.dart';

/// Users table (section 8.1).
@DataClassName('UserRow')
class Users extends Table {
  TextColumn get id => text()();
  TextColumn get googleAccountId => text()();
  TextColumn get email => text()();
  TextColumn get displayName => text()();
  TextColumn get photoUrl => text().nullable()();
  DateTimeColumn get createdAt => dateTime()();
  DateTimeColumn get updatedAt => dateTime()();

  @override
  Set<Column> get primaryKey => {id};
}

/// Albums table (section 8.2). Both albums and folders.
@DataClassName('AlbumRow')
class Albums extends Table {
  TextColumn get id => text()();
  TextColumn get parentAlbumId => text().nullable()();
  TextColumn get name => text()();
  TextColumn get type => text()();
  TextColumn get ownerUserId => text()();
  TextColumn get driveFolderId => text().nullable()();
  TextColumn get driveParentFolderId => text().nullable()();
  BoolColumn get isDriveLinked =>
      boolean().withDefault(const Constant(false))();
  BoolColumn get autoSyncEnabled =>
      boolean().withDefault(const Constant(true))();
  DateTimeColumn get createdAt => dateTime()();
  DateTimeColumn get updatedAt => dateTime()();
  DateTimeColumn get deletedAt => dateTime().nullable()();

  @override
  Set<Column> get primaryKey => {id};
}

/// Media items table (section 9).
@DataClassName('MediaItemRow')
class MediaItems extends Table {
  TextColumn get id => text()();
  TextColumn get localMediaStoreId => text().nullable()();
  TextColumn get albumId => text()();
  TextColumn get localUri => text()();
  TextColumn get fileName => text()();
  TextColumn get mimeType => text()();
  IntColumn get sizeBytes => integer().withDefault(const Constant(0))();
  IntColumn get width => integer().nullable()();
  IntColumn get height => integer().nullable()();
  DateTimeColumn get capturedAt => dateTime().nullable()();
  DateTimeColumn get modifiedAt => dateTime().nullable()();
  TextColumn get contentHash => text().nullable()();
  TextColumn get category => text().nullable()();
  IntColumn get sequenceNumber => integer().nullable()();
  TextColumn get driveFileId => text().nullable()();
  TextColumn get syncStatus => text()();
  DateTimeColumn get createdAt => dateTime()();
  DateTimeColumn get updatedAt => dateTime()();

  @override
  Set<Column> get primaryKey => {id};
}

/// Album members table (section 10).
@DataClassName('AlbumMemberRow')
class AlbumMembers extends Table {
  TextColumn get id => text()();
  TextColumn get albumId => text()();
  TextColumn get userId => text().nullable()();
  TextColumn get email => text()();
  TextColumn get role => text()();
  DateTimeColumn get createdAt => dateTime()();
  DateTimeColumn get updatedAt => dateTime()();

  @override
  Set<Column> get primaryKey => {id};
}

/// Sync jobs table (section 11).
@DataClassName('SyncJobRow')
class SyncJobs extends Table {
  TextColumn get id => text()();
  TextColumn get type => text()();
  TextColumn get albumId => text()();
  TextColumn get mediaItemId => text().nullable()();
  IntColumn get priority => integer().withDefault(const Constant(0))();
  TextColumn get status => text()();
  IntColumn get attemptCount => integer().withDefault(const Constant(0))();
  TextColumn get lastError => text().nullable()();
  DateTimeColumn get nextAttemptAt => dateTime().nullable()();
  DateTimeColumn get createdAt => dateTime()();
  DateTimeColumn get updatedAt => dateTime()();

  @override
  Set<Column> get primaryKey => {id};
}

/// Simple key/value settings store (Drive root id, preferences, etc.).
@DataClassName('AppSettingRow')
class AppSettings extends Table {
  TextColumn get key => text()();
  TextColumn get value => text().nullable()();

  @override
  Set<Column> get primaryKey => {key};
}
