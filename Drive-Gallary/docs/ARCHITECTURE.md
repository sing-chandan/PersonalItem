# Architecture

Drive-Linked Gallery Companion follows a layered, modular architecture as
mandated by the master specification (section 7).

```
Presentation (features/*, widgets/)        Flutter widgets + Riverpod
        ↓
Application Services (domain/services)     Use-case orchestration
        ↓
Domain (domain/models, domain/repositories) Pure Dart, no Flutter/Drive deps
        ↓
Repositories (data/repositories)           Drift-backed implementations
        ↓
Infrastructure (data/db, data/drive, data/media)  Drift, Drive API, MediaStore
```

## Directory layout

```
lib/
├── app/            App wiring: MaterialApp, router, theme, Riverpod providers
├── core/           Cross-cutting: Result, AppError, logging, constants, utils
├── data/
│   ├── db/         Drift tables, generated database, mappers
│   └── repositories/  Repository implementations
├── domain/
│   ├── models/     Immutable domain models + enums
│   └── repositories/  Abstract repository contracts
└── features/       UI feature modules (home, albums, media, sync, ...)
```

## Key principles

- **UI never calls Drive or the database directly.** Widgets depend on
  repositories/services via Riverpod providers only.
- **Drive is isolated behind `DriveRepository`** so the app is not coupled to a
  single Drive client package (spec section 6, 19).
- **Domain models are immutable** and free of Drift/Flutter imports. Drift row
  classes are renamed via `@DataClassName('...Row')` and converted through
  `data/db/mappers.dart`.
- **Errors are explicit** via `Result<T>` and the `AppError` taxonomy
  (spec section 38) rather than leaking raw exceptions to the UI.
- **Offline-first:** local writes always succeed without network; sync happens
  through the persisted `SyncJobs` queue (spec sections 21–23).

## State management

Riverpod is the single DI + state solution. `databaseProvider` holds the app
database; repository providers wrap it. Tests override `databaseProvider` with
an in-memory database.

## Navigation

`go_router` drives navigation. Route constants live in `app/router.dart`.
