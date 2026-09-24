# Development Status

_Last updated: 2026-09-24. All documentation is consolidated in this `docs/`
folder (see [docs/README.md](README.md))._

## Final Status — Summary

**All 8 phases are implemented.** The full application logic is complete and
verified on the web (Chrome) target. Remaining work is strictly the
Android-device stage, which cannot run in this environment.

| Phase | Scope | Status |
|-------|-------|--------|
| 0 | Foundation (arch, DB, docs, tests) | ✅ Complete |
| 1 | Local Media (albums, media, camera) | ✅ Complete |
| 2 | Google Drive (auth, Drive repo) | ✅ Complete (code) |
| 3 | Linked Albums (album ↔ Drive folder) | ✅ Complete |
| 4 | Upload Engine (queue, retry, offline) | ✅ Complete |
| 5 | Father's Workflow (New Photos, bulk) | ✅ Complete |
| 6 | Remote Sync (scan, conflicts) | ✅ Complete |
| 7 | Collaboration (share, roles) | ✅ Complete |
| 8 | Production Hardening (perf, safety) | ✅ Complete (web) |

**Verification:** `dart format` clean · `flutter analyze` no issues ·
`flutter test` **38/38 passing** · `flutter build web` succeeds · boots in Chrome.

### Remaining before production release (Android device stage)
These require an Android SDK and/or Google OAuth credentials and are validated
on a real device:
1. Provide Google OAuth client id(s) + enable Drive API (see DRIVE_INTEGRATION.md).
2. Android WorkManager background-upload wiring (Kotlin) — engine already
   decoupled; in-app processing + persisted queue already work.
3. On-device MediaStore recent-photos enumeration + runtime media permissions
   (web uses the file picker).
4. Interactive Google Sign-In verification on device (web needs a rendered
   button; Android flow is implemented).
5. Release signing, `flutter build apk --release` / `appbundle`, and on-device
   acceptance runs (kill-app, reboot, poor network, token expiry).

## Phase 8 progress
- Album-level cloud status now derived from media (Synced / Uploading /
  Waiting / Failed / Not linked) per spec §35, with photo count.
- Performance smoke test: 10,000 media rows, paginated + indexed queries under
  the time bound (spec §40, §52 #27).
- Delete-safety tests: local media/album deletion never affects Drive
  (spec §34, §59) — services have no Drive dependency by design.
- Full suite green (38 tests), analyze clean, boots in Chrome.
- Remaining for device stage: Android WorkManager background wiring, on-device
  MediaStore recent enumeration + permissions, and release APK/AAB builds (need
  Android SDK + OAuth credentials).

## Phase 7 progress
- `AlbumMemberRepository(+Impl)` for membership metadata; `DrivePermission`
  model; `DriveRepository` gains `listPermissions` and `createPermission` now
  returns the permission id.
- `SharingService`: share album (validate email → create Drive permission →
  persist member), list/watch members, change role (updates Drive + local),
  remove member (revoke Drive permission by email → remove member). Ownership
  transfer blocked; sharing requires a linked album (spec §29–31, §39).
- Members & Sharing screen (share dialog with role, per-member change-role /
  remove) reachable from Album Detail.
- Tests: +4 (share/invalid-email/remove/change-role) = 35 passing.

## Phase 6 progress
- `RemoteSyncService`: scans a linked album's Drive folder, detects remote-only
  files and name conflicts (matched by driveFileId), imports remote-only files
  as `remoteOnly` media entries (download-on-demand, no auto byte download),
  and resolves conflicts via Keep Local / Keep Cloud (spec §32, §33). Never
  auto-downloads all files; never silently overwrites.
- Album Detail: "Sync from Drive" action with a conflict-resolution dialog.
- Tests: +2 (remote-only import, conflict keepCloud) = 31 passing.

## Phase 5 progress
- New Photos screen (spec §14, §48): pick photos → multi-select grid → choose
  destination album (flattened tree, shows not-linked) → optional category
  chips → optional bulk rename (category + start number, live preview) →
  "Add & Upload" which adds and enqueues background uploads.
- Home: prominent "New Photos" card entry.
- `BulkRenameService` (pure): collision-free `Category_001.ext` naming,
  extension-preserving, custom start, sanitized category.
- `BulkMediaService`: category assignment + bulk rename on existing media,
  propagating renames to Drive for already-synced items (spec §26).
- Album Detail: "Bulk rename photos" action (orders by capture time).
- Tests: +5 (BulkRenameService) +2 (BulkMediaService) = 29 passing.

## Phase 4 progress
- `UploadEngine.processJob`: duplicate short-circuit (driveFileId), auto-links
  album, reads bytes, uploads to Drive, persists driveFileId + synced; on
  failure requeues with bounded exponential backoff (max attempts) and marks
  media failed/waitingForNetwork.
- `SyncController`: enqueue + connectivity-aware queue processing (defers
  offline, resumes on reconnect), retry/pause/resume/clear. Started at app
  launch to recover the persisted queue after restart (spec §4, §22).
- `MediaBytesReader(+Impl)`: native file path / web blob URL.
- Add-photos & camera flows now enqueue uploads automatically.
- Upload Queue screen (spec §36) + Home entry.
- Android WorkManager OS-background wiring documented for the device stage
  (engine already decoupled).
- Tests: +3 UploadEngine tests (success / duplicate / retry) = 22 passing.

## Phase 3 progress
- `LinkedAlbumService`: creates album locally first (offline-first), then
  best-effort creates/links the matching Drive folder using the parent album's
  folder id (linking the parent recursively if needed) or the Drive root.
- Stores the stable `driveFolderId` + `driveParentFolderId`; marks album linked.
  Failures leave the album unlinked for later repair (never blocks local work).
- Rename propagates to the Drive folder when linked.
- `repairLink`: validates the stored folder id, recreates the folder if it was
  deleted remotely, and fixes the linked flag.
- UI: Home + Album Detail album creation/rename use the linked service and show
  link feedback; Album Detail shows a linked/not-linked chip and a
  "Repair Drive link" action.
- Tests: +4 LinkedAlbumService tests (fake Drive) = 19 passing.

## Earlier phases

## Phase 2 progress
- `DriveRepository` interface + `DriveRepositoryImpl` (googleapis Drive v3):
  folder create/get/find/rename/delete, file upload/get/rename/list/download,
  permission create/delete.
- `AuthService` + `GoogleAuthService` (google_sign_in 7.x): separate
  authenticate + authorize (drive.file scope), auth-state stream, session
  restore, sign-out/disconnect, authorized HTTP client for googleapis.
- `SettingsRepository` (key/value) + `DriveRootService` (create/reuse + persist
  root folder id; never searches by name during normal use — spec §17).
- Providers: auth, authState stream, drive repo, drive root, settings, setting
  watch. Owner id now uses the signed-in Google id when available.
- Settings screen: connect/sign-out/disconnect, Drive root setup, graceful web
  messaging. Home app bar shows cloud/settings entry.
- Credential setup documented in DRIVE_INTEGRATION.md (`--dart-define`).
- App boots cleanly in Chrome; missing-credentials handled gracefully.
- Tests: +3 DriveRootService tests (fake Drive repo) = 15 passing.

## Phase 1 progress
- Album service (create / nested / rename / soft-delete) + providers.
- Media source abstraction (`MediaSource`) with image_picker implementation
  (pick + camera); recent-media enumeration deferred to Android/MediaStore.
- Media service: add-to-album (offline, localOnly), duplicate skip by local
  store id, pending counts, local-only remove (spec §34 safety).
- UI: Home (root albums, empty state, create), Album Detail (sub-albums + media
  grid, add photos, camera, rename/delete), Media Detail (preview + metadata +
  local remove). Adaptive thumbnail + cloud-status chip widgets.
- Drift web support wired: `web/sqlite3.wasm` + `web/drift_worker.js` +
  `DriftWebOptions` (fixes web runtime crash). App verified running in Chrome.
- Tests: album service (5), media service (4), db (2), widget boot (1) = 12
  passing. Fixed widget-test drift dispose-timer flakiness.

Pending in Phase 1 (device stage): Android MediaStore recent-photos
enumeration + runtime media permissions (cannot run/test on web).

## Completed
- Flutter project scaffolded (`drive_linked_gallery`, org `com.drivegallery`,
  platforms: android, web).
- Dependencies added: flutter_riverpod, go_router, drift + drift_flutter +
  sqlite3_flutter_libs, path_provider, path, permission_handler, image_picker,
  connectivity_plus, flutter_secure_storage, google_sign_in, googleapis,
  googleapis_auth, extension_google_sign_in_as_googleapis_auth, http, crypto,
  intl, uuid, collection; dev: drift_dev, build_runner.
- Layered architecture directories created (app / core / data / domain /
  features).
- Core: `Result<T>`, `AppError` taxonomy (spec §38), `AppLogger`,
  `AppConstants`, backoff util.
- Domain models (immutable): Album, MediaItem, AppUser, SyncJob, AlbumMember +
  shared enums.
- Drift database with 6 tables, required indexes + unique partial indexes
  (spec §12), FK pragma, `@DataClassName('...Row')` to avoid domain collisions;
  generated code via build_runner.
- Repository contracts (domain) + Drift-backed implementations for Album,
  Media, SyncJob.
- Riverpod providers (database + repositories).
- App wiring: theme, go_router, MaterialApp.router, home placeholder screen.
- Documentation: README, ARCHITECTURE, DATABASE, SYNC_ENGINE, DRIVE_INTEGRATION,
  TESTING, TECHNICAL_LIMITATIONS, this status file.
- Tests: album repository (in-memory DB), app boot widget test.

## In Progress
- Phase 1 — Local Media (MediaStore, permissions, photo/video grids, album
  create/nested/rename/delete, media detail, camera, thumbnails).

## Tests
- dart format: PASS
- flutter analyze: PASS (no issues)
- flutter test: PASS (38/38)
- Web run (`flutter run -d chrome`): PASS (loads home, DB works)
- Android build (`flutter build apk --debug`): BLOCKED — Android SDK not
  installed in this environment (see TECHNICAL_LIMITATIONS.md). Interim gate is
  the web build, per agreed Chrome-first testing.

## Known Issues
- Android APK/AAB build and on-device testing pending Android SDK setup.

## Technical Decisions
- Drive scope limited to `drive.file` (least privilege).
- Drift chosen with generated data classes suffixed `Row` to keep domain models
  clean and framework-free.
- Offline-first: all pending work persisted as `SyncJob` rows in SQLite so it
  survives restarts/reboots.
- Chrome/web used as the interim run + build target; Android infrastructure sits
  behind repository/service interfaces so it can be added without UI changes.

## Next Phase
- Phase 1 — Local Media.
