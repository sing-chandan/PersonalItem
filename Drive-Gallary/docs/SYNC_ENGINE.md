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

Phase 4 implemented:
- `UploadEngine.processJob` — pure per-job execution: duplicate short-circuit
  via `driveFileId`, links the album if needed, reads bytes
  (`MediaBytesReader`), uploads via `DriveRepository`, persists `driveFileId` +
  `synced`, and on failure requeues with bounded exponential backoff
  (`core/utils/backoff.dart`) up to `maxUploadAttempts`.
- `SyncController` — in-app processor: `enqueueUpload(s)`, `processQueue`
  (re-entrancy guarded), connectivity-aware (defers when offline, resumes on
  `onConnectivityChanged`), and queue actions (retry/pause/resume/clear).
  Started at app launch (`app.dart`) so the persisted queue is recovered after
  restart.
- `MediaBytesReader` / `MediaBytesReaderImpl` — file path (native) or blob URL
  (web).
- Upload Queue screen (`features/sync`) with per-item status + actions.

### Android background execution (device stage)

The in-app `SyncController` processes uploads while the app is running and
resumes on next launch. True OS-scheduled background upload after the UI is
closed uses **WorkManager** on Android: a periodic/one-off worker will invoke
the same `UploadEngine` over the persisted `SyncJobs` queue. This Kotlin/
WorkManager wiring is added at the device stage (cannot run on web); the engine
is already decoupled so it plugs in without logic changes.
