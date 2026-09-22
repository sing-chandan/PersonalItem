# Sync Engine

Describes the offline-first upload/sync design (spec sections 21–24, 35, 36).

## Model

Each pending action is a persisted `SyncJob` row (`sync_jobs` table). Jobs
survive app restarts and device reboots because they live in SQLite, not in
memory.

- Types: `upload`, `download`, `metadata`, `rename`, `delete`
- Statuses: `queued`, `running`, `paused`, `succeeded`, `failed`, `cancelled`

Media items carry their own `syncStatus` (`localOnly`, `queued`, `uploading`,
`synced`, `failed`, `waitingForNetwork`, `remoteOnly`, `conflict`, ...).

## Upload flow (target)

```
User selects media
  → local album membership saved (works offline)
  → SyncJob(upload) enqueued, media.syncStatus = queued
  → UI updates immediately
  → worker picks runnable jobs
  → read file → upload to album.driveFolderId → receive driveFileId
  → persist driveFileId, media.syncStatus = synced, job = succeeded
```

On failure: `media.syncStatus = failed`, `job.attemptCount += 1`,
`job.lastError` set, `job.nextAttemptAt` scheduled via bounded exponential
backoff (`core/utils/backoff.dart`, base 4s, cap 30m, max 6 attempts).

## Duplicate prevention (section 24)

Primary key is `MediaItem.driveFileId`. Before uploading, the engine checks for
an existing Drive file id; if absent it falls back to local MediaStore id +
content hash + size. Uploads are idempotent. Unique partial DB indexes on
`drive_file_id` enforce this at the storage layer.

## Offline behavior

Adding media never requires network. Jobs sit `queued`/`waitingForNetwork`
until connectivity returns (`connectivity_plus`), then transition
`waiting → uploading → synced`.

## Status reporting

- Album-level aggregate: `AlbumCloudStatus` (Synced / Uploading /
  WaitingForNetwork / Failed / NotLinked).
- Queue screen shows per-item progress with Pause / Resume / Retry failed /
  Clear completed. Clearing completed history never deletes Drive files.

## Implementation status

Phase 0 defines the persistence layer (`SyncJobs` table + repository). The
worker, WorkManager integration, and progress reporting are implemented in
Phase 4.
