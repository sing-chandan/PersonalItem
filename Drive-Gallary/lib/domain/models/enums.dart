/// Album container type (section 8.2). Both are organizational containers.
enum AlbumType { album, folder }

/// Per-media synchronization status (section 9).
enum SyncStatus {
  localOnly,
  queued,
  uploading,
  synced,
  failed,
  waitingForNetwork,
  remoteOnly,
  conflict,
  deletedLocal,
  deletedRemote,
}

/// Collaboration roles (section 10).
enum MemberRole { owner, editor, contributor, viewer }

/// Sync job type (section 11).
enum SyncJobType { upload, download, metadata, rename, delete }

/// Sync job lifecycle status (section 11).
enum SyncJobStatus { queued, running, paused, succeeded, failed, cancelled }

/// Album-level aggregate cloud status (section 35).
enum AlbumCloudStatus {
  synced,
  uploading,
  waitingForNetwork,
  failed,
  notLinked,
}

T enumFromName<T extends Enum>(List<T> values, String? name, T fallback) {
  if (name == null) return fallback;
  for (final v in values) {
    if (v.name == name) return v;
  }
  return fallback;
}
