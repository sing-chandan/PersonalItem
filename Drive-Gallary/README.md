# Drive-Linked Gallery Companion

An Android-first Flutter app that preserves the user's existing photo workflow
(WhatsApp → download → gallery) while eliminating repetitive Google Drive work.

Once an album is linked to a Google Drive folder, adding newly downloaded photos
becomes: **New Photos → select → choose linked album → Add & Upload**, with
automatic background sync. No manual Drive navigation required.

All documentation lives in the **[`docs/`](docs/README.md)** folder.
See **[docs/Drive_Linked_Gallery_MASTER_SPEC.md](docs/Drive_Linked_Gallery_MASTER_SPEC.md)**
for the authoritative specification.

## Tech stack

Flutter/Dart · Riverpod · go_router · Drift/SQLite · Google Sign-In ·
Google Drive API v3 · Android MediaStore · WorkManager (Android) · Kotlin
native integration where needed.

## Documentation (in `docs/`)

Start with **[docs/README.md](docs/README.md)** — the index. Contents:

| File | Contents |
|------|----------|
| `docs/Drive_Linked_Gallery_MASTER_SPEC.md` | Authoritative product/technical spec |
| `docs/DEVELOPMENT_STATUS.md` | Current phase, progress, what remains |
| `docs/ARCHITECTURE.md` | Layered architecture and module layout |
| `docs/DATABASE.md` | Drift schema, indexes, migrations |
| `docs/SYNC_ENGINE.md` | Offline-first upload/sync design |
| `docs/DRIVE_INTEGRATION.md` | Google auth + Drive repository |
| `docs/TESTING.md` | How to test; acceptance suite mapping |
| `docs/TECHNICAL_LIMITATIONS.md` | Public-API/environment constraints |

## Getting started

Prerequisites: Flutter SDK (stable). For Android builds also install the Android
SDK and accept licenses.

```
flutter pub get
dart run build_runner build      # generate Drift code
flutter run -d chrome            # run in Chrome (current dev target)
```

### Verification

```
dart format .
flutter analyze
flutter test
flutter build web                # interim gate
flutter build apk --debug        # once Android SDK installed
```

## Android build (device stage)

The CI/dev environment here has no Android SDK, so Android builds are done on a
machine that has it:

```
# One-time
flutter doctor --android-licenses

# Debug build
flutter build apk --debug

# Release builds (configure signing in android/ first)
flutter build apk --release
flutter build appbundle --release
```

Provide the Google OAuth client id at build/run time:

```
flutter run --dart-define=GOOGLE_WEB_CLIENT_ID=xxxx.apps.googleusercontent.com
```

See `DRIVE_INTEGRATION.md` for the full Google Cloud / OAuth setup and
`TECHNICAL_LIMITATIONS.md` for platform notes (WorkManager, MediaStore, web).

## Configuration / secrets

OAuth client configuration (`google-services.json` / client id) is required for
Google Drive and must NOT be committed to source control.
