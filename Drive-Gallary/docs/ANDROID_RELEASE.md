# Android Release Build Guide

How to produce a production-ready, signed APK/AAB for the Drive-Linked Gallery
Companion, and how Google OAuth client IDs work for release.

- Package (application id): `com.drivegallery.drive_linked_gallery`
- Do everything on a machine that has the **Android SDK** installed
  (`flutter doctor` should show Android toolchain OK).

---

## 1. Do you need a different client ID? (Important)

There are two different kinds of OAuth client IDs, and they are used
differently:

| Client ID type | Where it's used | Passed in code? |
|----------------|-----------------|-----------------|
| **Web** client ID | `flutter run --dart-define=GOOGLE_WEB_CLIENT_ID=...` (web/Chrome) and as an optional `serverClientId` | Yes (only if needed) |
| **Android** client ID | Android sign-in on device | **No** — resolved automatically from your **package name + signing SHA‑1** |

Key point for the release APK:

- On Android, Google Sign‑In does **not** read a client id from the run command.
  It matches your app by **package name + the SHA‑1 fingerprint of the keystore
  that signed the build.**
- Your on-device test worked because the **debug** keystore's SHA‑1 was
  registered (or you passed a web client id). A **release** APK is signed with a
  **different** keystore, so its SHA‑1 is different.
- Therefore you must **register the release keystore's SHA‑1** as an Android
  OAuth client in the **same** Google Cloud project. You do **not** create a new
  Google Cloud project — just add another Android OAuth client (or add the SHA‑1).

So: keep your existing project + web client ID, and **add an Android OAuth
client for the release SHA‑1** (steps below). You can register both the debug
and release SHA‑1 (two Android OAuth clients) so both builds work.

---

## 2. Create a release keystore (one time)

```bash
keytool -genkey -v -keystore upload-keystore.jks -storetype JKS \
  -keyalg RSA -keysize 2048 -validity 10000 -alias upload
```

Keep `upload-keystore.jks` safe and private (losing it means you can't update
the app on Play). Move it somewhere outside the repo, e.g. your home folder.

## 3. Get the release SHA-1 / SHA-256

```bash
keytool -list -v -keystore upload-keystore.jks -alias upload
```

Copy the **SHA1** (and SHA256) lines from the output.

## 4. Register the SHA-1 in Google Cloud Console

1. Google Cloud Console → your project → **APIs & Services → Credentials**.
2. **Create Credentials → OAuth client ID → Android**.
3. Package name: `com.drivegallery.drive_linked_gallery`
4. SHA-1 certificate fingerprint: paste the release SHA‑1 from step 3.
5. Save. (Optionally repeat with the debug SHA‑1 so debug builds also work.)
6. Make sure the **Google Drive API** is enabled and the OAuth consent screen is
   configured (add test users while the app is in "Testing").

> Tip: if you publish via Google Play with Play App Signing, also add the SHA‑1
> that Google Play shows under *App integrity* to a matching Android OAuth
> client, since Play re-signs your app.

## 5. Wire the keystore into the build

Create `android/key.properties` (already git-ignored — never commit it):

```properties
storePassword=YOUR_STORE_PASSWORD
keyPassword=YOUR_KEY_PASSWORD
keyAlias=upload
storeFile=C:/Users/you/upload-keystore.jks
```

`android/app/build.gradle.kts` is already set up to read this file and sign the
release build with it. If the file is absent it falls back to debug signing (so
`flutter run --release` still works during development).

---

## 6. Build commands

```bash
# from the project root: C:\PersonalItem\Drive-Gallary
flutter pub get
dart run build_runner build          # regenerate Drift code if needed

# --- APK (for direct install / sideloading) ---
flutter build apk --release

# Smaller, per-architecture APKs (recommended for direct distribution):
flutter build apk --release --split-per-abi

# --- App Bundle (for Google Play) ---
flutter build appbundle --release
```

Outputs:

- `build/app/outputs/flutter-apk/app-release.apk`
  (or `app-arm64-v8a-release.apk` etc. with `--split-per-abi`)
- `build/app/outputs/bundle/release/app-release.aab`

### Passing OAuth values at build time (optional)

- You normally do **not** need any `--dart-define` for the Android release,
  because Android sign-in resolves via package name + SHA‑1.
- Only if you want an ID token / server auth code, pass your **web** client id
  as the server client id:

```bash
flutter build apk --release \
  --dart-define=GOOGLE_SERVER_CLIENT_ID=xxxx.apps.googleusercontent.com
```

(Do **not** pass a web client id as `GOOGLE_WEB_CLIENT_ID` for the Android build;
that flag is for the web/Chrome target.)

---

## 7. Install / test the APK on a device

```bash
# List connected devices
flutter devices
adb devices

# Install the release APK
flutter install --release
# or directly:
adb install -r build/app/outputs/flutter-apk/app-release.apk
```

Then sign in with a Google account that is allowed on the OAuth consent screen,
link an album to Drive, and run the New Photos → Add & Upload flow.

---

## 8. Version bumps

Update `version:` in `pubspec.yaml` (`versionName+versionCode`, e.g. `1.0.1+2`)
before each Play upload; Gradle reads it automatically.

---

## Troubleshooting

- **Sign-in error `ApiException: 10` (DEVELOPER_ERROR):** the signing SHA‑1 is
  not registered for this package in Google Cloud. Re-check step 3–4 and confirm
  you registered the SHA‑1 of the keystore that actually signed the build.
- **`sign_in_failed` / no Drive access:** confirm the Drive API is enabled and
  the account is a listed test user (while consent screen is in Testing).
- **Release build fails on missing key.properties:** create the file (step 5) or
  it will fall back to debug signing (not suitable for Play).
