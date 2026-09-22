# Testing

## Commands

```
dart format .
flutter analyze
flutter test
flutter build web        # interim buildability gate (see TECHNICAL_LIMITATIONS.md)
flutter build apk --debug  # once Android SDK is installed
```

Run the app in Chrome for manual testing:

```
flutter run -d chrome
```

## Automated tests

- `test/db/database_test.dart` — album repository CRUD + soft-delete against an
  in-memory Drift database.
- `test/widget_test.dart` — app boots to the home screen.

Repository tests use `AppDatabase.forTesting(NativeDatabase.memory())` and
override `databaseProvider` where a full provider graph is needed.

## Acceptance test suite (spec section 52)

The 27 acceptance scenarios (create album, nested album, connect Google, linked
album, add photos, offline add, resume, kill app during upload, duplicate
prevention, camera, bulk rename, conflict, delete safety, large library, shared
album roles, etc.) are implemented incrementally as the corresponding phases
land, with automated tests plus manual instructions.

| Phase | Scenarios covered |
|-------|-------------------|
| 0     | Foundation (DB CRUD, app boot) |
| 1     | 1, 2, 13 (albums, nested, camera) |
| 2–3   | 3, 4, 5, 16, 17 (Drive connect/link) |
| 4     | 6–12 (upload, offline, resume, duplicate) |
| 5     | 14, 15 (bulk category/rename) |
| 6     | 24, 25 (remote detection, conflict) |
| 7     | 20–23 (shared album roles) |
| 8     | 18, 19, 26, 27 (token, permission, delete safety, perf) |
