# Drive-Linked Gallery Companion
## Master Codex / AI Coding-Agent Build Specification
### Version 1.0 — Android-first Flutter application

---

## 0. HOW TO USE THIS DOCUMENT

This file is the **single source of truth** for building the application.

Give this entire `MASTER_SPEC.md` file to Codex or another capable coding agent together with the Flutter repository.

The coding agent must:

1. Read this entire document before changing code.
2. Treat the requirements in this document as authoritative.
3. Implement the application phase-by-phase in the order specified.
4. Make reasonable engineering decisions without repeatedly asking the user.
5. Stop and ask the user only when a decision is genuinely impossible to resolve from this specification.
6. Never replace the architecture merely because another approach is easier.
7. Keep the project buildable after every phase.
8. Update `DEVELOPMENT_STATUS.md` after every completed phase.
9. Add tests for every important feature.
10. Never leave core MVP features as TODOs, fake implementations, or placeholder buttons.

The user may later provide new requirements. When new requirements conflict with this document, explicitly identify the conflict, preserve existing working functionality, and update the specification/status documentation before implementing the change.

---

# 1. PRODUCT SUMMARY

Build an Android-first Flutter application called **Drive-Linked Gallery Companion**.

The app is NOT initially intended to replace Redmi Gallery or Google Photos.

Its purpose is to preserve the user's existing photo workflow while eliminating repetitive Google Drive operations.

### Existing workflow

```text
WhatsApp
    ↓
Receive images
    ↓
Download images to phone
    ↓
Open Redmi Gallery
    ↓
Create album
    ↓
Open album
    ↓
Select downloaded images
    ↓
Later:
Open album
    ↓
Select all
    ↓
Copy album name
    ↓
Share to Google Drive
    ↓
Find correct Drive location
    ↓
Create folder
    ↓
Rename folder if required
    ↓
Upload images
    ↓
Rename images manually
```

### Target workflow

```text
WhatsApp
    ↓
Download images normally
    ↓
Open Drive-Linked Gallery
    ↓
New Photos
    ↓
Select photos
    ↓
Choose existing linked album
    ↓
Add & Upload
    ↓
Background sync
```

Once an album is linked to a Google Drive folder, the user should normally never need to open Google Drive for uploading.

---

# 2. PRIMARY PRODUCT PRINCIPLE

## Reduce effort, do not change the user's habits unnecessarily.

The user's father currently receives and downloads images through WhatsApp and uses the Redmi Gallery.

Do not force him to learn a complicated new gallery.

The app should make these actions extremely simple:

- Create an album once.
- Link it to a Drive folder once.
- Add photos whenever they arrive.
- Let synchronization happen automatically.

The user should see clear states such as:

```text
Synced
Uploading
Waiting for Internet
Failed
```

---

# 3. CORE PRODUCT CONCEPT

## Linked Album

Every app album can be linked to a Google Drive folder.

Example:

```text
App:

Project A
└── Jaipur
    └── Before

Google Drive:

Gallery Cloud
└── Project A
    └── Jaipur
        └── Before
```

The local album stores the **Google Drive folder ID**.

Never rely on folder name as the permanent identifier.

Example:

```text
album.driveFolderId = "1AbCdEf..."
```

If the folder is renamed, the ID remains stable.

---

# 4. PRODUCT BOUNDARIES

## MVP must include

- Android
- Flutter/Dart
- Local media browsing
- Albums
- Nested albums/folders
- Google authentication
- Google Drive integration
- Linked Drive folders
- Recent/new photos
- Multi-select media
- Add media to album
- Camera capture
- Background upload
- Persistent upload queue
- Retry/resume
- Offline-first behavior
- Upload status
- Bulk categorization
- Optional bulk rename
- Duplicate prevention
- Drive-linked album management
- Basic collaboration architecture
- Owner/editor/contributor/viewer roles
- Tests
- Production buildability

## Do NOT initially build

- Full Google Photos clone
- Face recognition
- AI image recognition
- AI editing
- Advanced photo editor
- Private Xiaomi Gallery database integration
- Custom cloud storage backend
- Automatic WhatsApp API integration
- Social media integration

---

# 5. IMPORTANT ANDROID RULE

Do NOT depend on Xiaomi/Redmi Gallery's private database, undocumented APIs, or internal implementation.

The app must work through supported Android APIs.

Use:

- MediaStore
- Android document/file APIs where necessary
- Android intents
- Android permissions
- WorkManager/background execution
- Camera APIs

The app should work even if Xiaomi changes the internal implementation of its Gallery.

---

# 6. TECHNOLOGY STACK

## Required

```text
Flutter
Dart
Android
Kotlin
Google Drive API v3
SQLite
Drift
Riverpod
go_router
```

## Recommended supporting libraries

Use current stable, well-maintained packages where appropriate.

Potential packages:

```text
flutter_riverpod
go_router
drift
drift_flutter
path_provider
permission_handler
image_picker
camera
connectivity_plus
flutter_secure_storage
google_sign_in
```

For Google Drive, use a maintained Google API/client approach and isolate it behind a repository/service interface.

Do not tightly couple the entire application to a third-party Drive package.

---

# 7. ARCHITECTURE

Use a layered architecture.

```text
Presentation
    ↓
Application Services
    ↓
Domain
    ↓
Repositories
    ↓
Infrastructure
```

Suggested structure:

```text
lib/
├── app/
│   ├── app.dart
│   ├── router.dart
│   └── theme.dart
│
├── core/
│   ├── constants/
│   ├── errors/
│   ├── result/
│   ├── utils/
│   └── logging/
│
├── data/
│   ├── db/
│   │   ├── tables/
│   │   ├── daos/
│   │   ├── migrations/
│   │   └── database.dart
│   │
│   ├── drive/
│   │   ├── drive_client.dart
│   │   ├── drive_models.dart
│   │   └── drive_repository_impl.dart
│   │
│   ├── media/
│   │   ├── media_store_service.dart
│   │   └── media_repository_impl.dart
│   │
│   └── repositories/
│
├── domain/
│   ├── models/
│   ├── repositories/
│   └── services/
│
├── features/
│   ├── onboarding/
│   ├── home/
│   ├── albums/
│   ├── media/
│   ├── camera/
│   ├── sync/
│   ├── drive/
│   ├── sharing/
│   ├── rename/
│   └── settings/
│
└── widgets/
```

Android:

```text
android/app/src/main/kotlin/<package>/
├── MainActivity.kt
├── media/
├── background/
├── permissions/
└── integrations/
```

---

# 8. DOMAIN MODEL

## 8.1 User

```text
User
- id
- googleAccountId
- email
- displayName
- photoUrl
- createdAt
- updatedAt
```

## 8.2 Album

```text
Album
- id
- parentAlbumId nullable
- name
- type
- ownerUserId
- driveFolderId nullable
- driveParentFolderId nullable
- isDriveLinked
- autoSyncEnabled
- createdAt
- updatedAt
- deletedAt nullable
```

`type`:

```text
album
folder
```

Treat both as organizational containers internally, while UI may call them albums/folders depending on context.

---

# 9. MEDIA MODEL

```text
MediaItem
- id
- localMediaStoreId nullable
- albumId
- localUri
- fileName
- mimeType
- sizeBytes
- width nullable
- height nullable
- capturedAt nullable
- modifiedAt nullable
- contentHash nullable
- category nullable
- sequenceNumber nullable
- driveFileId nullable
- syncStatus
- createdAt
- updatedAt
```

Sync status:

```text
localOnly
queued
uploading
synced
failed
waitingForNetwork
remoteOnly
conflict
deletedLocal
deletedRemote
```

---

# 10. ALBUM MEMBER MODEL

```text
AlbumMember
- id
- albumId
- userId
- email
- role
- createdAt
- updatedAt
```

Roles:

```text
owner
editor
contributor
viewer
```

---

# 11. SYNC JOB MODEL

```text
SyncJob
- id
- type
- albumId
- mediaItemId nullable
- priority
- status
- attemptCount
- lastError nullable
- nextAttemptAt nullable
- createdAt
- updatedAt
```

Types:

```text
upload
download
metadata
rename
delete
```

Statuses:

```text
queued
running
paused
succeeded
failed
cancelled
```

---

# 12. DATABASE REQUIREMENTS

Use Drift/SQLite.

Requirements:

- Foreign keys where appropriate.
- Indexed album parent IDs.
- Indexed Drive folder IDs.
- Indexed Drive file IDs.
- Indexed sync status.
- Indexed local MediaStore IDs.
- Unique constraints preventing duplicate Drive mappings.
- Explicit migration versions.
- Migration tests.

Important indexes:

```text
Album.parentAlbumId
Album.driveFolderId
MediaItem.albumId
MediaItem.driveFileId
MediaItem.localMediaStoreId
MediaItem.syncStatus
SyncJob.status
SyncJob.albumId
```

---

# 13. LOCAL MEDIA ACCESS

Use Android MediaStore.

The app must:

- Request only necessary permissions.
- Support modern Android media permissions.
- Enumerate images/videos.
- Read metadata.
- Paginate results.
- Show thumbnails efficiently.
- Detect recently added media.
- Detect media changes where practical.
- Avoid loading full-resolution images unnecessarily.

Do not request broad storage access unless a documented Android limitation proves it necessary.

---

# 14. RECENT / NEW PHOTOS

This is a critical screen.

Purpose: solve the father's WhatsApp workflow.

Example:

```text
New Photos

Today

[IMG] [IMG] [IMG] [IMG]
[IMG] [IMG]

6 selected

Add to:
[ Project A / Jaipur ▼ ]

[ Add & Upload ]
```

Default ordering:

1. Newest first.
2. Group by date.
3. Show local source/folder where available.

The user should not have to browse the entire phone library to find newly downloaded WhatsApp images.

---

# 15. ALBUM SCREEN

Example:

```text
Project A / Jaipur

Cloud: 🟢 Synced

245 Photos
7 Pending

[ + Add Photos ]
[ 📷 Camera ]

Before       34
WIP          17
Meeting       8
After        22
Distribution 13
```

The design should remain simple.

---

# 16. ALBUM CREATION

Flow:

```text
Create Album

Name:
[ Jaipur Visit ]

Parent:
[ Project A ]

Drive:
Automatically use parent's Drive folder

Auto Sync:
ON

[ Create ]
```

If there is no parent:

```text
Drive Parent:
[ Gallery Cloud / Project A ]
```

When created:

1. Create local album.
2. Create Drive folder.
3. Persist Drive folder ID.
4. Mark album as linked.
5. Display synced/linked state.

Avoid partial state.

If Drive folder creation succeeds but local database write fails, recover safely.

---

# 17. DRIVE ROOT

First setup:

```text
Google Drive

Select root:

[ Gallery Cloud ]

or

[ Create new root folder ]
```

Persist:

```text
driveRootFolderId
```

Do not search for "Gallery Cloud" by name during every operation.

---

# 18. GOOGLE AUTHENTICATION

Use Google Sign-In and appropriate Drive scopes.

Requirements:

- Login
- Logout
- Token refresh
- Re-authentication
- Account switching
- Secure token storage
- Clear auth state on disconnect
- Never log credentials/tokens

Use least-privilege scopes.

Do not place client secrets in source control.

---

# 19. GOOGLE DRIVE SERVICE

Create an abstraction:

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

UI must never call Google Drive APIs directly.

---

# 20. DRIVE FOLDER CREATION

When creating a child folder:

```text
name = album.name
mimeType = application/vnd.google-apps.folder
parent = album.parent.driveFolderId
```

Persist returned ID.

Never use folder name as the primary key.

---

# 21. UPLOAD ENGINE

Upload flow:

```text
User selects media
       ↓
Local album membership saved
       ↓
SyncJob created
       ↓
UI immediately updates
       ↓
Background worker starts
       ↓
Read file
       ↓
Upload to album.driveFolderId
       ↓
Receive Drive file ID
       ↓
Persist driveFileId
       ↓
syncStatus = synced
```

If upload fails:

```text
syncStatus = failed
lastError = ...
attemptCount += 1
```

Retry with bounded exponential backoff.

---

# 22. BACKGROUND UPLOAD

Use Android WorkManager or a reliable equivalent.

Requirements:

- Continue after app UI closes.
- Resume after app restart.
- Retry after temporary network failure.
- Respect connectivity.
- Avoid uncontrolled battery usage.
- Avoid keeping huge files in RAM.
- Show notification where Android requires it.
- Persist job state.

---

# 23. OFFLINE-FIRST RULE

Adding media to a local album must work without internet.

Example:

```text
Internet OFF

Add 10 photos
     ↓
Album shows 10 photos immediately
     ↓
Cloud: Waiting for Internet
```

When internet returns:

```text
Waiting
   ↓
Uploading
   ↓
Synced
```

Never lose the user's local work because the network is unavailable.

---

# 24. DUPLICATE PREVENTION

Avoid duplicate uploads.

Primary protection:

```text
MediaItem.driveFileId
```

If no Drive ID exists, use:

- local MediaStore ID where stable
- content hash
- file size
- modified time
- filename as supporting metadata

Do not use filename alone.

The upload process must be idempotent.

---

# 25. BULK CATEGORY SYSTEM

Default categories:

```text
Before
WIP
Meeting
After
Distribution
Other
```

Allow user-configurable categories later.

Selecting multiple photos:

```text
12 selected

[Before]
[WIP]
[Meeting]
[After]
[Distribution]
[Other]
```

Category is optional.

Do not force categorization.

---

# 26. BULK RENAME

Example:

```text
Rename Selected

Category:
Before

Starting number:
001

Preview:

Before_001.jpg
Before_002.jpg
Before_003.jpg
```

Rules:

- Preserve extension.
- Prevent collisions.
- Use capture time by default for ordering.
- Allow selection order as an alternative.
- Require confirmation.
- If already synced, update Drive file names.
- Handle failures without losing mappings.

---

# 27. RECOMMENDED ORGANIZATION

Use folders/categories rather than relying exclusively on filenames.

Example:

```text
Project A
├── Jaipur
│   ├── Before
│   ├── WIP
│   ├── Meeting
│   ├── After
│   └── Distribution
```

Filename can remain:

```text
IMG_20260919_103421.jpg
```

or optionally become:

```text
Before_001.jpg
```

The user should never be forced to rename every image.

---

# 28. CAMERA

From Album Detail:

```text
[ Camera ]
```

Flow:

```text
Camera
 ↓
Capture
 ↓
Save local media
 ↓
Attach to current album
 ↓
Create SyncJob
 ↓
Background upload
```

Support repeated capture without returning to the home screen.

---

# 29. SHARING / COLLABORATION

Implement after core sync is stable.

Roles:

### Owner

- Full control
- Create/delete album
- Manage members
- Change permissions
- Rename
- Move
- Add/delete media

### Editor

- Add
- Create subfolders
- Rename
- Move
- Delete according to album policy

### Contributor

- Add photos/videos
- Limited organization

### Viewer

- View
- Download

---

# 30. SHARED ALBUM MODEL

Example:

```text
Project A
├── Site 1
│   ├── Before
│   ├── WIP
│   ├── Meeting
│   └── After
```

Multiple users may work in the same tree.

Each user sees only albums to which they have access.

Do not expose unrelated private albums.

---

# 31. DRIVE PERMISSIONS

Use Google Drive permissions where appropriate.

However, maintain application-level membership metadata.

Do not assume Drive permissions alone are enough for future application logic.

Shared album flow:

```text
Owner
 ↓
Select Album
 ↓
Share
 ↓
Enter Google account/email
 ↓
Select role
 ↓
Create/Update Drive permission
 ↓
Persist AlbumMember
```

---

# 32. REMOTE SYNC

After local-to-Drive synchronization is stable:

Support:

```text
Drive
 ↓
Linked album
 ↓
Detect new remote files
 ↓
Display remote files
```

The initial implementation may use download-on-demand.

Do not automatically download every cloud file unless explicitly configured.

---

# 33. CONFLICT RULES

Never silently overwrite conflicting changes.

Potential conflicts:

- Local rename vs remote rename.
- Local delete vs remote modification.
- Two users rename the same file.
- Same logical media uploaded by different users.

Show:

```text
Sync Conflict

Local:
Before_001.jpg

Cloud:
Before_Final_001.jpg

[Keep Local]
[Keep Cloud]
[Create Copy]
```

---

# 34. DELETE RULES

Critical safety rule:

Deleting local media must NOT automatically delete the Drive file in MVP.

Similarly, deleting a Drive file must not silently delete local media.

MVP behavior:

```text
Delete local
    ≠
Delete Drive
```

If later implementing two-way delete, make it an explicit opt-in setting with strong confirmation.

---

# 35. SYNC STATUS

Album-level status:

```text
🟢 Synced
🟡 Uploading
🟠 Waiting for Internet
🔴 Failed
⚪ Not linked
```

Photo-level status:

```text
Queued
Uploading 62%
Synced
Failed
Remote
Conflict
```

---

# 36. UPLOAD QUEUE SCREEN

Example:

```text
Upload Queue

Project A / Jaipur

Before_001.jpg
██████████ 100%
Synced

Before_002.jpg
██████░░░░ 62%
Uploading

Meeting_001.jpg
Waiting
```

Actions:

```text
Pause
Resume
Retry failed
Clear completed
```

Do not delete completed cloud files when clearing queue history.

---

# 37. SETTINGS

Settings should include:

### Account

- Google account
- Disconnect

### Drive

- Root folder
- Reconnect
- Repair links

### Upload

- Auto sync
- Wi-Fi only
- Videos enabled/disabled
- Background upload

### Categories

- Manage categories

### Rename

- Default prefix format
- Numbering format

### Notifications

- Upload completion
- Upload failure

### Storage

- Thumbnail cache
- Clear cache

---

# 38. ERROR HANDLING

Define errors:

```text
AUTH_REQUIRED
PERMISSION_DENIED
NETWORK_UNAVAILABLE
DRIVE_RATE_LIMIT
DRIVE_NOT_FOUND
FILE_NOT_FOUND
UPLOAD_FAILED
DOWNLOAD_FAILED
CONFLICT
DUPLICATE
STORAGE_FULL
MEDIA_PERMISSION_DENIED
UNKNOWN
```

Every error must:

- Have a readable message.
- Be logged safely.
- Have retry/recovery where appropriate.
- Never expose raw exception details to normal users.

---

# 39. SECURITY

Rules:

- Never log OAuth tokens.
- Never commit secrets.
- Store credentials securely.
- Use least privilege.
- Validate permissions.
- Validate all file paths.
- Do not expose private media to other users.
- Do not trust client-only permission checks for cloud operations.
- Validate sharing actions before changing Drive permissions.

---

# 40. PERFORMANCE

Target:

- 10,000+ media records.
- Large albums.
- Hundreds of pending uploads.
- Large video files.

Rules:

- Paginated database queries.
- Lazy image grids.
- Thumbnail caching.
- No full-resolution bulk loading.
- No loading all files into memory.
- Background hashing.
- Background uploads.
- Efficient database indexes.

---

# 41. REQUIRED SCREENS

Implement:

1. Splash
2. Onboarding
3. Google Account
4. Drive Root Setup
5. Home
6. Albums
7. Album Tree
8. Album Detail
9. New Photos
10. Media Picker
11. Camera
12. Upload Queue
13. Sync Details
14. Shared Albums
15. Members/Permissions
16. Bulk Rename
17. Settings
18. Drive Connection
19. Repair Drive Link
20. Error/Retry states

---

# 42. HOME SCREEN

Keep it simple.

Example:

```text
Good Morning

New Photos
6 photos

[ Add to Album ]

My Albums

Project A
Project B
Family

Cloud Status
🟢 All synced

[ Upload Queue ]
[ Shared Albums ]
```

---

# 43. PHASED DEVELOPMENT PLAN

## PHASE 0 — FOUNDATION

### Tasks

- Create Flutter project.
- Configure Android.
- Configure package ID.
- Configure linting.
- Configure Riverpod.
- Configure go_router.
- Configure Drift.
- Create architecture.
- Create logging.
- Create error handling.
- Create test structure.
- Create DEVELOPMENT_STATUS.md.
- Create ARCHITECTURE.md.
- Create TECHNICAL_LIMITATIONS.md.

### Acceptance

```text
flutter analyze
flutter test
flutter build apk --debug
```

all pass.

---

# 44. PHASE 1 — LOCAL MEDIA

Implement:

- MediaStore.
- Permissions.
- Photo grid.
- Video grid.
- Recent media.
- Media detail.
- Album creation.
- Nested albums.
- Add/remove media.
- Rename album.
- Delete album.
- Camera.
- Thumbnail caching.

### Acceptance

User can use the application completely offline as a basic album manager.

---

# 45. PHASE 2 — GOOGLE DRIVE

Implement:

- Google Sign-In.
- Drive authorization.
- Drive repository.
- Root folder.
- Folder creation.
- Folder listing.
- Folder metadata.
- Secure credentials.
- Re-authentication.

### Acceptance

User can connect Drive and create a test folder from the app.

---

# 46. PHASE 3 — LINKED ALBUMS

Implement:

- Local album ↔ Drive folder mapping.
- Create linked album.
- Nested linked album.
- Rename linked album.
- Repair mapping.
- Drive ID persistence.

### Acceptance

```text
App:
Project A / Jaipur

Drive:
Gallery Cloud / Project A / Jaipur
```

are reliably linked.

---

# 47. PHASE 4 — UPLOAD ENGINE

Implement:

- SyncJob persistence.
- Upload worker.
- Progress.
- Retry.
- Offline queue.
- Background upload.
- Duplicate protection.
- Large file handling.
- App restart recovery.

### Acceptance

100+ images can be queued and reliably uploaded while the app UI is closed.

---

# 48. PHASE 5 — FATHER'S WORKFLOW

This phase is the most important product phase.

Implement:

- New Photos.
- Recent WhatsApp/downloaded media discovery.
- Multi-select.
- Destination album.
- Add & Upload.
- One-tap album workflow.
- Bulk categories.
- Bulk rename.
- Clear sync indicators.

### Target workflow

```text
WhatsApp
 ↓
Download
 ↓
Open app
 ↓
New Photos
 ↓
Select
 ↓
Album
 ↓
Add & Upload
```

No Drive app required.

---

# 49. PHASE 6 — REMOTE SYNC

Implement:

- Remote folder scan.
- Remote file detection.
- Remote metadata.
- Remote additions.
- Download-on-demand.
- Conflict detection.
- Remote rename handling.

---

# 50. PHASE 7 — COLLABORATION

Implement:

- Share album.
- Invite member.
- Roles.
- Permissions.
- Shared albums.
- Shared album tree.
- Member removal.
- Activity/audit metadata.

---

# 51. PHASE 8 — PRODUCTION HARDENING

Test:

- 10,000 media.
- 1,000+ uploads.
- Large videos.
- Poor internet.
- Internet loss.
- App force close.
- Device reboot.
- Battery restrictions.
- Google token expiry.
- Drive rate limiting.
- Drive folder deletion.
- Drive rename.
- Permission denial.
- Storage full.
- Camera permission denial.
- Media permission denial.
- Shared album permission changes.

Build:

```text
Debug APK
Release APK
Release AAB
```

---

# 52. ACCEPTANCE TEST SUITE

The coding agent must implement automated tests plus manual test instructions for:

1. Create local album.
2. Create nested album.
3. Connect Google account.
4. Create Drive root.
5. Create linked album.
6. Add one photo.
7. Add 100 photos.
8. Add photo offline.
9. Resume after network returns.
10. Kill app during upload.
11. Restart phone during pending upload.
12. Duplicate upload prevention.
13. Camera capture.
14. Bulk category.
15. Bulk rename.
16. Drive rename.
17. Drive folder missing.
18. Token expiration.
19. Permission denied.
20. Shared album viewer.
21. Shared album contributor.
22. Shared album editor.
23. Owner member management.
24. Remote file detection.
25. Conflict handling.
26. Delete safety.
27. Large media library performance.

---

# 53. CODING STANDARDS

Use:

- Dart null safety.
- Strong typing.
- Small testable classes.
- Dependency injection.
- Repository pattern.
- Service layer.
- Immutable domain models where practical.
- Meaningful names.
- No magic strings.
- No duplicated Drive logic.
- No UI business logic.
- No direct database calls from widgets.
- No direct Drive calls from widgets.

Run:

```bash
dart format .
flutter analyze
flutter test
flutter build apk --debug
```

before considering a phase complete.

---

# 54. DOCUMENTATION GENERATED BY THE AGENT

Maintain:

```text
README.md
MASTER_SPEC.md
ARCHITECTURE.md
DEVELOPMENT_STATUS.md
TECHNICAL_LIMITATIONS.md
SYNC_ENGINE.md
DRIVE_INTEGRATION.md
DATABASE.md
TESTING.md
```

Update these documents whenever architecture or behavior changes.

---

# 55. DEVELOPMENT_STATUS FORMAT

Maintain:

```markdown
# Development Status

## Current Phase
Phase X

## Completed
- ...

## In Progress
- ...

## Tests
- flutter analyze: PASS/FAIL
- flutter test: PASS/FAIL
- Android build: PASS/FAIL

## Known Issues
- ...

## Technical Decisions
- ...

## Next Phase
- ...
```

---

# 56. TECHNICAL LIMITATIONS

If a requested behavior cannot be implemented through public Android APIs:

1. Explain the limitation.
2. Identify the public API alternative.
3. Do not use private Xiaomi APIs.
4. Do not silently omit the feature.
5. Document it in `TECHNICAL_LIMITATIONS.md`.
6. Continue implementing the closest supported behavior.

---

# 57. CODING AGENT BEHAVIOR

The agent must NOT repeatedly ask:

- Which database?
- Which architecture?
- Should we use Flutter?
- How should sync work?
- How should albums map to Drive?
- What should the folder hierarchy be?

These decisions are already defined here.

The agent SHOULD independently decide:

- Exact widget implementation.
- Exact repository method signatures.
- Internal helper classes.
- Test fixture structure.
- Appropriate current stable package versions.
- UI spacing/typography within the simple design direction.
- Internal error classes.
- Exact SQL implementation.

---

# 58. PRODUCT DECISIONS ALREADY MADE

These are fixed unless the user explicitly changes them:

```text
Platform: Android
Framework: Flutter
Native language: Kotlin where needed
Cloud: Google Drive
Local DB: Drift/SQLite
Architecture: layered/modular
Sync: offline-first
Album model: hierarchical
Cloud relationship: album ↔ Drive folder ID
Background sync: required
Manual Drive navigation: eliminated
Per-image manual rename: optional, not required
Xiaomi private APIs: prohibited
Full gallery replacement: not MVP
```

---

# 59. IMPORTANT SAFETY RULE

Never automatically delete user data during synchronization.

MVP:

```text
Delete local
    ≠
Delete Drive

Delete Drive
    ≠
Delete local
```

Any future two-way deletion must be explicitly enabled and confirmed.

---

# 60. FUTURE EXTENSIONS

Architecture should allow:

- AI photo classification.
- OCR.
- Smart search.
- Duplicate detection.
- Similar image detection.
- People.
- Places.
- Advanced photo editor.
- Custom backend.
- Web application.
- iOS.
- Advanced collaboration.
- Audit history.
- Organization/team accounts.

Do not implement these unless requested.

---

# 61. MASTER EXECUTION PROMPT

Copy this prompt into Codex/AI coding agent together with this document.

---

## MASTER CODEX PROMPT

You are the principal engineer responsible for implementing the **Drive-Linked Gallery Companion**.

Read `MASTER_SPEC.md` completely before writing code.

Treat it as the authoritative product and technical specification.

Your mission is to create a **fully functional, production-quality Android Flutter application**, not a mockup or prototype with fake buttons.

### Required architecture

- Flutter/Dart
- Kotlin Android integration where needed
- Riverpod
- go_router
- Drift/SQLite
- Google Sign-In
- Google Drive API v3
- Android MediaStore
- Android WorkManager/background execution

### Core product behavior

The existing user workflow is:

WhatsApp → download images → Redmi Gallery → create album → add images → later manually copy album name → open Google Drive → navigate → create folder → upload → rename images.

Replace the repetitive part with:

WhatsApp → download → Drive-Linked Gallery → New Photos → select → linked album → Add & Upload → automatic background sync.

### Critical requirements

1. Albums are hierarchical.
2. Albums may be linked to Drive folders.
3. Drive folder IDs are stored and used as stable identifiers.
4. New media can be added locally without internet.
5. Uploads are persisted as jobs.
6. Uploads continue in background.
7. App restart does not lose upload jobs.
8. Network failure does not lose local changes.
9. Duplicate uploads are prevented.
10. Drive operations are hidden from normal daily workflow.
11. Bulk category assignment is optional.
12. Bulk rename is optional.
13. Camera capture can target an album.
14. Existing Redmi Gallery continues to work normally.
15. Do not depend on Redmi Gallery private APIs.
16. Do not automatically delete cloud files when local files disappear.
17. Do not require manual Google Drive folder creation after an album is linked.
18. Shared albums use explicit roles.
19. All important operations are testable.
20. The final application must build successfully.

### Execution order

Implement exactly in this order:

```text
Phase 0 Foundation
Phase 1 Local Media
Phase 2 Google Drive
Phase 3 Linked Albums
Phase 4 Upload Engine
Phase 5 Father's Workflow
Phase 6 Remote Sync
Phase 7 Collaboration
Phase 8 Production Hardening
```

### At the beginning

Inspect the repository.

If it is empty, initialize the Flutter project.

Create:

```text
MASTER_SPEC.md
ARCHITECTURE.md
DEVELOPMENT_STATUS.md
TECHNICAL_LIMITATIONS.md
SYNC_ENGINE.md
DRIVE_INTEGRATION.md
DATABASE.md
TESTING.md
```

Then create the architecture and initial database.

### After every phase

Run:

```bash
dart format .
flutter analyze
flutter test
flutter build apk --debug
```

Fix all errors.

Update `DEVELOPMENT_STATUS.md`.

Do not continue while core failures remain.

### Coding rules

- Do not put business logic in widgets.
- Do not call Drive API directly from UI.
- Do not call database directly from UI.
- Do not store tokens in plaintext.
- Do not hard-code credentials.
- Do not use private Xiaomi APIs.
- Do not use filename as unique identity.
- Do not load entire photo libraries into RAM.
- Do not create duplicate Drive folders.
- Do not mark an upload successful before the Drive response is persisted.
- Make background operations idempotent.
- Use transactions where appropriate.
- Use migrations for schema changes.
- Preserve existing functionality when modifying code.

### Decision rule

If the specification already answers a question, do not ask the user.

If a technical implementation detail is unspecified, choose the safest maintainable implementation consistent with the architecture.

If a requirement cannot be achieved through public Android/Google APIs, document the limitation and implement the closest supported alternative.

Only ask the user for clarification when a product-level decision cannot reasonably be inferred.

### Completion requirement

Do not declare the project complete until:

- Core features are implemented.
- Tests pass.
- `flutter analyze` passes.
- Android debug build succeeds.
- Production build configuration is documented.
- No core functionality remains a placeholder.
- Documentation reflects the actual implementation.
- Known limitations are documented.

Start with **Phase 0** now.

---

# 62. FINAL MVP DEFINITION

The first usable production milestone is reached when a user can:

```text
1. Sign in with Google.

2. Select/create a Drive root.

3. Create:
   Project A
      └── Jaipur

4. The app automatically creates:
   Google Drive
      └── Project A
          └── Jaipur

5. Download photos from WhatsApp normally.

6. Open the app.

7. Open New Photos.

8. Select the downloaded images.

9. Choose:
   Project A / Jaipur

10. Tap:
    Add & Upload

11. Photos immediately appear in the local album.

12. Upload runs in the background.

13. User can close the app.

14. Later:
    Album = 🟢 Synced

15. Open Google Drive manually only if they want to browse it.

16. Later receive more WhatsApp images.

17. Repeat:
    New Photos → Album → Add & Upload

No repeated Drive folder creation.
No repeated Drive navigation.
No repeated select-all from the old gallery.
No manual upload process.
No mandatory per-image renaming.
```

---

# 63. PRODUCT SUCCESS CRITERIA

The application is successful if the user's father can use it without learning a complicated new system.

The desired mental model is:

> **"I create my album once. After that, I just keep putting photos into it. The Drive folder takes care of itself."**

That is the core product.

END OF MASTER SPECIFICATION.
