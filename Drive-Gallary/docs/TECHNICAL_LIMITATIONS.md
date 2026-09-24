# Technical Limitations

Per spec section 56, limitations of public APIs and the current build
environment are documented here rather than silently omitted.

## Build environment

- **Android SDK not installed in the current dev environment.**
  `flutter build apk --debug` cannot run here yet. As agreed with the project
  owner, development and verification currently target **Chrome (Flutter web)**;
  the Android APK/AAB build and on-device testing will be done later once the
  Android SDK is installed. Web build (`flutter build web`) is used as the
  interim buildability gate.
  - Action to enable Android builds: install Android Studio / command-line
    tools, accept SDK licenses (`flutter doctor --android-licenses`), set
    `ANDROID_HOME`.

## Platform API limitations (planned features)

- **Xiaomi/Redmi Gallery private database:** intentionally NOT used
  (spec section 5). Local media is read exclusively through Android MediaStore.
- **MediaStore on web:** the web platform has no MediaStore. On web the media
  layer falls back to `image_picker`/file selection for testing the workflow;
  full MediaStore enumeration is Android-only.
- **WorkManager background upload:** Android-only. On web, uploads run while the
  app/tab is open. True OS-scheduled background execution is an Android feature
  implemented via a Kotlin integration in a later phase.
- **`flutter_secure_storage` on web** uses browser storage which is less secure
  than Android Keystore; production credentials are only fully protected on
  Android.

## Drift on web

Drift's web backend requires `web/sqlite3.wasm` and `web/drift_worker.js`
(downloaded from the matching drift GitHub release, currently `drift-2.35.0`)
and a `DriftWebOptions` config in `AppDatabase`. These files must be kept in
sync with the `drift` version in `pubspec.lock` when it is upgraded. The browser
falls back to `sharedIndexedDb` storage when SharedArrayBuffers / dedicated
workers in shared workers are unavailable (expected on plain `flutter run`).

These limitations do not block the core product architecture, which is designed
so that Android-specific infrastructure sits behind repository/service
interfaces.
