# Documentation Index

All project documentation lives in this `docs/` folder. Start here to
understand the Drive-Linked Gallery Companion.

## Read in this order

1. **[Drive_Linked_Gallery_MASTER_SPEC.md](Drive_Linked_Gallery_MASTER_SPEC.md)**
   — the authoritative product & technical specification (what we're building
   and why).
2. **[DEVELOPMENT_STATUS.md](DEVELOPMENT_STATUS.md)** — current status, what is
   done per phase, test results, and what remains (device stage).
3. **[ARCHITECTURE.md](ARCHITECTURE.md)** — layered architecture, folder layout,
   and key design principles.
4. **[DATABASE.md](DATABASE.md)** — Drift/SQLite schema, indexes, migrations.
5. **[DRIVE_INTEGRATION.md](DRIVE_INTEGRATION.md)** — Google auth + Drive
   repository, and how to provide OAuth credentials.
   - **[ANDROID_RELEASE.md](ANDROID_RELEASE.md)** — build a signed production
     APK/AAB and register the release OAuth client (SHA‑1).
6. **[SYNC_ENGINE.md](SYNC_ENGINE.md)** — offline-first upload/sync design and
   the Android background story.
7. **[TESTING.md](TESTING.md)** — how to test, and the acceptance-suite mapping.
8. **[TECHNICAL_LIMITATIONS.md](TECHNICAL_LIMITATIONS.md)** — platform/API and
   environment constraints (web vs Android).

The project [README.md](../README.md) (repo root) is the quick-start entry
point and links back here.

## What is this project?

An Android-first Flutter app that preserves the user's existing photo habit
(WhatsApp → download → gallery) while removing repetitive Google Drive work.
Once an album is linked to a Drive folder, adding new photos becomes:

> New Photos → select → choose linked album → **Add & Upload** → automatic
> background sync.

No manual Drive navigation required.

## Where the code lives

```
lib/
├── app/       App wiring (router, theme, Riverpod providers)
├── core/      Result, errors, logging, constants, utils
├── data/      Drift DB, Drive client, media, repository implementations
├── domain/    Models, repository contracts, services (business logic)
├── features/  UI: home, albums, media, new_photos, sync, sharing, settings
└── widgets/   Shared widgets
test/          Unit/widget/performance tests (mirrors lib/)
```
