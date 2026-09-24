# Google Drive Integration

Spec sections 17–20, 31.

## Principles

- UI never calls the Drive API directly. All access goes through the abstract
  `DriveRepository` (implemented in Phase 2 under `lib/data/drive/`).
- Albums store the **Drive folder id** (`album.driveFolderId`) as the stable
  identifier. Folder names are never used as primary keys — renames keep the id.
- A single Drive root folder id (`driveRootFolderId`) is persisted in
  `app_settings`; the app never searches for "Gallery Cloud" by name at runtime.

## Auth

- Google Sign-In with least-privilege scope `drive.file`
  (`https://www.googleapis.com/auth/drive.file`), which limits access to files
  and folders the app creates.
- Tokens stored via `flutter_secure_storage`. Tokens/credentials are never
  logged.
- Supports login, logout, token refresh, re-auth, and account switching.

## Repository contract (target)

```dart
abstract class DriveRepository {
  Future<DriveFolder> createFolder(...);
  Future<DriveFolder?> getFolderById(...);
  Future<void> renameFolder(...);
  Future<void> deleteFolder(...);
  Future<DriveFile> uploadFile(...);
  Future<DriveFile?> getFileById(...);
  Future<void> renameFile(...);
  Future<List<DriveFile>> listChildren(...);
  Future<void> downloadFile(...);
  Future<void> createPermission(...);
  Future<void> deletePermission(...);
}
```

## Folder creation

```
name     = album.name
mimeType = application/vnd.google-apps.folder
parents  = [album.parent.driveFolderId]  // or driveRootFolderId
```

The returned id is persisted to `album.driveFolderId` and the album is marked
`isDriveLinked = true`.

## Client secrets

OAuth client configuration lives in platform config
(`android/app/google-services.json` / configured client id) and must NOT be
committed. See README setup.

## Setup: providing OAuth credentials (required for real sign-in)

Sign-in/upload need a Google Cloud project with the **Drive API enabled** and
OAuth client id(s). The app reads client ids at build time via `--dart-define`
(never committed):

1. Google Cloud Console → create project → enable **Google Drive API**.
2. Configure the OAuth consent screen (add the `drive.file` scope; add test
   users while in testing).
3. Create OAuth client IDs:
   - **Android**: type "Android", with your package name
     (`com.drivegallery.drive_linked_gallery`) and the signing SHA-1.
   - **Web** (for Chrome dev): type "Web application".
4. Run with the ids:

```
flutter run -d chrome \
  --dart-define=GOOGLE_WEB_CLIENT_ID=xxxx.apps.googleusercontent.com
```

On Android the client is resolved from the Android OAuth client + package/SHA-1
(no id needed in code); `serverClientId` can be supplied via
`GOOGLE_SERVER_CLIENT_ID` if server auth codes are needed later.

Web note: `google_sign_in` on web does not support programmatic
`authenticate()`; it needs a rendered Google button. Interactive sign-in is
therefore validated on an Android device. Without credentials the app still
boots and works offline; the Settings screen shows "Not connected".

## Implementation status

Phase 2 complete (code): `AuthService`/`GoogleAuthService` (google_sign_in 7.x,
separate authenticate + authorize), `DriveRepository`/`DriveRepositoryImpl`
(googleapis Drive v3), `DriveRootService` (root create/reuse + persist),
`SettingsRepository`, and a Settings screen (connect/disconnect, Drive root
setup). Real Drive connection is gated on the credentials above.
