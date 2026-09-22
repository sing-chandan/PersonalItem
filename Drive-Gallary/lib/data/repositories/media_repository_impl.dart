import 'package:drift/drift.dart';

import '../../domain/models/enums.dart';
import '../../domain/models/media_item.dart';
import '../../domain/repositories/media_repository.dart';
import '../db/database.dart';
import '../db/mappers.dart';

class MediaRepositoryImpl implements MediaRepository {
  MediaRepositoryImpl(this._db);

  final AppDatabase _db;

  static const _pendingStatuses = [
    'queued',
    'uploading',
    'failed',
    'waitingForNetwork',
    'localOnly',
  ];

  @override
  Future<List<MediaItem>> getMediaInAlbum(
    String albumId, {
    int limit = 120,
    int offset = 0,
  }) async {
    final query = _db.select(_db.mediaItems)
      ..where((t) => t.albumId.equals(albumId))
      ..orderBy([
        (t) => OrderingTerm(expression: t.capturedAt, mode: OrderingMode.desc),
        (t) => OrderingTerm(expression: t.createdAt, mode: OrderingMode.desc),
      ])
      ..limit(limit, offset: offset);
    final rows = await query.get();
    return rows.map(mediaFromRow).toList();
  }

  @override
  Stream<List<MediaItem>> watchMediaInAlbum(String albumId) {
    final query = _db.select(_db.mediaItems)
      ..where((t) => t.albumId.equals(albumId))
      ..orderBy([
        (t) => OrderingTerm(expression: t.capturedAt, mode: OrderingMode.desc),
        (t) => OrderingTerm(expression: t.createdAt, mode: OrderingMode.desc),
      ]);
    return query.watch().map((rows) => rows.map(mediaFromRow).toList());
  }

  @override
  Future<MediaItem?> getMediaById(String id) async {
    final row = await (_db.select(
      _db.mediaItems,
    )..where((t) => t.id.equals(id))).getSingleOrNull();
    return row == null ? null : mediaFromRow(row);
  }

  @override
  Future<MediaItem?> findByDriveFileId(String driveFileId) async {
    final row = await (_db.select(
      _db.mediaItems,
    )..where((t) => t.driveFileId.equals(driveFileId))).getSingleOrNull();
    return row == null ? null : mediaFromRow(row);
  }

  @override
  Future<MediaItem?> findByContentHash(
    String albumId,
    String contentHash,
  ) async {
    final row =
        await (_db.select(_db.mediaItems)..where(
              (t) =>
                  t.albumId.equals(albumId) & t.contentHash.equals(contentHash),
            ))
            .getSingleOrNull();
    return row == null ? null : mediaFromRow(row);
  }

  @override
  Future<MediaItem?> findByLocalStoreId(
    String albumId,
    String localStoreId,
  ) async {
    final row =
        await (_db.select(_db.mediaItems)..where(
              (t) =>
                  t.albumId.equals(albumId) &
                  t.localMediaStoreId.equals(localStoreId),
            ))
            .getSingleOrNull();
    return row == null ? null : mediaFromRow(row);
  }

  @override
  Future<MediaItem> upsertMedia(MediaItem item) async {
    await _db.into(_db.mediaItems).insertOnConflictUpdate(item.toCompanion());
    return item;
  }

  @override
  Future<void> updateSyncStatus(String id, SyncStatus status) async {
    await (_db.update(_db.mediaItems)..where((t) => t.id.equals(id))).write(
      MediaItemsCompanion(
        syncStatus: Value(status.name),
        updatedAt: Value(DateTime.now()),
      ),
    );
  }

  @override
  Future<void> setDriveFileId(String id, String driveFileId) async {
    await (_db.update(_db.mediaItems)..where((t) => t.id.equals(id))).write(
      MediaItemsCompanion(
        driveFileId: Value(driveFileId),
        updatedAt: Value(DateTime.now()),
      ),
    );
  }

  @override
  Future<int> countInAlbum(String albumId) async {
    final count = _db.mediaItems.id.count();
    final query = _db.selectOnly(_db.mediaItems)
      ..addColumns([count])
      ..where(_db.mediaItems.albumId.equals(albumId));
    final row = await query.getSingle();
    return row.read(count) ?? 0;
  }

  @override
  Future<int> countPendingInAlbum(String albumId) async {
    final count = _db.mediaItems.id.count();
    final query = _db.selectOnly(_db.mediaItems)
      ..addColumns([count])
      ..where(
        _db.mediaItems.albumId.equals(albumId) &
            _db.mediaItems.syncStatus.isIn(_pendingStatuses),
      );
    final row = await query.getSingle();
    return row.read(count) ?? 0;
  }

  @override
  Future<void> deleteMediaLocal(String id) async {
    await (_db.delete(_db.mediaItems)..where((t) => t.id.equals(id))).go();
  }
}
