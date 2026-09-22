# Drive-Linked Gallery Companion

An Android-first Flutter app that preserves the user's existing photo workflow
(WhatsApp → download → gallery) while eliminating repetitive Google Drive work.

Once an album is linked to a Google Drive folder, adding newly downloaded photos
becomes: **New Photos → select → choose linked album → Add & Upload**, with
automatic background sync. No manual Drive navigation required.

See `Drive_Linked_Gallery_MASTER_SPEC.md` for the authoritative specification.

## Tech stack

Flutter/Dart · Riverpod · go_router · Drift/SQLite · Google Sign-In ·
Google Drive API v3 · Android MediaStore · WorkManager (Android) · Kotlin
native integration where needed.

## Documentation

| File | Contents |
|------|----------|
| `ARCHITECTURE.md` | Layered architecture and module layout |
| `DATABASE.md` | Drift schema, indexes, migrations |
| `SYNC_ENGINE.md` | Offline-first upload/sync design |
| `DRIVE_INTEGRATION.md` | Google auth + Drive repository |
| `TESTING.md` | How to test; acceptance suite mapping |
| `TECHNICAL_LIMITATIONS.md` | Public-API/environment constraints |
| `DEVELOPMENT_STATUS.md` | Current phase and progress |

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

## Configuration / secrets

OAuth client configuration (`google-services.json` / client id) is required for
Google Drive and must NOT be committed to source control.
