# Development Status

## Current Phase
Phase 1 — Local Media (in progress; runs cleanly in Chrome).

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
- flutter test: PASS (12/12)
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
