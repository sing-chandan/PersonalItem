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

## Implementation status

Phase 0: scope + constants defined (`AppConstants.driveFileScope`,
`driveFolderMimeType`). Auth and repository land in Phase 2.
