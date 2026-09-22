import 'package:drift/drift.dart';

import '../../domain/models/album.dart';
import '../../domain/models/album_member.dart';
import '../../domain/models/enums.dart';
import '../../domain/models/media_item.dart';
import '../../domain/models/sync_job.dart';
import 'database.dart';

/// Conversions between Drift data rows and immutable domain models.
extension AlbumRowMapper on Album {
  AlbumsCompanion toCompanion() => AlbumsCompanion(
    id: Value(id),
    parentAlbumId: Value(parentAlbumId),
    name: Value(name),
    type: Value(type.name),
    ownerUserId: Value(ownerUserId),
    driveFolderId: Value(driveFolderId),
    driveParentFolderId: Value(driveParentFolderId),
    isDriveLinked: Value(isDriveLinked),
    autoSyncEnabled: Value(autoSyncEnabled),
    createdAt: Value(createdAt),
    updatedAt: Value(updatedAt),
    deletedAt: Value(deletedAt),
  );
}

Album albumFromRow(AlbumRow r) => Album(
  id: r.id,
  parentAlbumId: r.parentAlbumId,
  name: r.name,
  type: enumFromName(AlbumType.values, r.type, AlbumType.album),
  ownerUserId: r.ownerUserId,
  driveFolderId: r.driveFolderId,
  driveParentFolderId: r.driveParentFolderId,
  isDriveLinked: r.isDriveLinked,
  autoSyncEnabled: r.autoSyncEnabled,
  createdAt: r.createdAt,
  updatedAt: r.updatedAt,
  deletedAt: r.deletedAt,
);

extension MediaRowMapper on MediaItem {
  MediaItemsCompanion toCompanion() => MediaItemsCompanion(
    id: Value(id),
    localMediaStoreId: Value(localMediaStoreId),
    albumId: Value(albumId),
    localUri: Value(localUri),
    fileName: Value(fileName),
    mimeType: Value(mimeType),
    sizeBytes: Value(sizeBytes),
    width: Value(width),
    height: Value(height),
    capturedAt: Value(capturedAt),
    modifiedAt: Value(modifiedAt),
    contentHash: Value(contentHash),
    category: Value(category),
    sequenceNumber: Value(sequenceNumber),
    driveFileId: Value(driveFileId),
    syncStatus: Value(syncStatus.name),
    createdAt: Value(createdAt),
    updatedAt: Value(updatedAt),
  );
}

MediaItem mediaFromRow(MediaItemRow r) => MediaItem(
  id: r.id,
  localMediaStoreId: r.localMediaStoreId,
  albumId: r.albumId,
  localUri: r.localUri,
  fileName: r.fileName,
  mimeType: r.mimeType,
  sizeBytes: r.sizeBytes,
  width: r.width,
  height: r.height,
  capturedAt: r.capturedAt,
  modifiedAt: r.modifiedAt,
  contentHash: r.contentHash,
  category: r.category,
  sequenceNumber: r.sequenceNumber,
  driveFileId: r.driveFileId,
  syncStatus: enumFromName(
    SyncStatus.values,
    r.syncStatus,
    SyncStatus.localOnly,
  ),
  createdAt: r.createdAt,
  updatedAt: r.updatedAt,
);

extension SyncJobRowMapper on SyncJob {
  SyncJobsCompanion toCompanion() => SyncJobsCompanion(
    id: Value(id),
    type: Value(type.name),
    albumId: Value(albumId),
    mediaItemId: Value(mediaItemId),
    priority: Value(priority),
    status: Value(status.name),
    attemptCount: Value(attemptCount),
    lastError: Value(lastError),
    nextAttemptAt: Value(nextAttemptAt),
    createdAt: Value(createdAt),
    updatedAt: Value(updatedAt),
  );
}

SyncJob syncJobFromRow(SyncJobRow r) => SyncJob(
  id: r.id,
  type: enumFromName(SyncJobType.values, r.type, SyncJobType.upload),
  albumId: r.albumId,
  mediaItemId: r.mediaItemId,
  priority: r.priority,
  status: enumFromName(SyncJobStatus.values, r.status, SyncJobStatus.queued),
  attemptCount: r.attemptCount,
  lastError: r.lastError,
  nextAttemptAt: r.nextAttemptAt,
  createdAt: r.createdAt,
  updatedAt: r.updatedAt,
);

AlbumMember albumMemberFromRow(AlbumMemberRow r) => AlbumMember(
  id: r.id,
  albumId: r.albumId,
  userId: r.userId,
  email: r.email,
  role: enumFromName(MemberRole.values, r.role, MemberRole.viewer),
  createdAt: r.createdAt,
  updatedAt: r.updatedAt,
);
