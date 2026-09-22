# Database

The app uses **Drift over SQLite** (spec sections 6, 12).

- Definition: `lib/data/db/tables.dart`
- Database + migrations + indexes: `lib/data/db/database.dart`
- Generated code: `lib/data/db/database.g.dart` (via `build_runner`)
- Row ↔ domain mappers: `lib/data/db/mappers.dart`

Regenerate generated code after changing tables:

```
dart run build_runner build
```

## Tables

| Table         | Purpose                                    |
|---------------|--------------------------------------------|
| `users`       | Signed-in Google user (section 8.1)        |
| `albums`      | Hierarchical albums/folders (section 8.2)  |
| `media_items` | Media + sync status (section 9)            |
| `album_members` | Collaboration roles (section 10)         |
| `sync_jobs`   | Persistent upload/sync queue (section 11)  |
| `app_settings`| Key/value store (Drive root id, prefs)     |

Drift generates data classes suffixed with `Row` (e.g. `AlbumRow`) via
`@DataClassName` so they do not collide with the immutable domain models
(`Album`, `MediaItem`, ...).

## Indexes (spec section 12)

Created in `AppDatabase._createIndexes()`:

- `albums(parent_album_id)`
- `albums(drive_folder_id)` + unique partial index on non-null values
- `media_items(album_id)`
- `media_items(drive_file_id)` + unique partial index on non-null values
- `media_items(local_media_store_id)`
- `media_items(sync_status)`
- `sync_jobs(status)`
- `sync_jobs(album_id)`

Foreign-key enforcement is enabled via `PRAGMA foreign_keys = ON` in
`beforeOpen`.

## Migrations

`schemaVersion = 1`. Future schema changes must bump the version and add an
`onUpgrade` step plus a migration test.
