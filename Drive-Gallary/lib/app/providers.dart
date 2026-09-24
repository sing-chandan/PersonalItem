import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/constants/app_constants.dart';
import '../data/db/database.dart';
import '../data/drive/drive_repository_impl.dart';
import '../data/drive/google_auth_service.dart';
import '../data/media/image_picker_media_source.dart';
import '../data/media/media_bytes_reader_impl.dart';
import '../data/repositories/album_member_repository_impl.dart';
import '../data/repositories/album_repository_impl.dart';
import '../data/repositories/media_repository_impl.dart';
import '../data/repositories/settings_repository.dart';
import '../data/repositories/sync_job_repository_impl.dart';
import '../domain/models/album.dart';
import '../domain/models/album_member.dart';
import '../domain/models/app_user.dart';
import '../domain/models/media_item.dart';
import '../domain/repositories/album_member_repository.dart';
import '../domain/repositories/album_repository.dart';
import '../domain/repositories/drive_repository.dart';
import '../domain/repositories/media_repository.dart';
import '../domain/repositories/sync_job_repository.dart';
import '../domain/services/album_service.dart';
import '../domain/services/auth_service.dart';
import '../domain/services/bulk_media_service.dart';
import '../domain/services/bulk_rename_service.dart';
import '../domain/services/drive_root_service.dart';
import '../domain/services/linked_album_service.dart';
import '../domain/services/media_bytes_reader.dart';
import '../domain/services/media_service.dart';
import '../domain/services/media_source.dart';
import '../domain/services/remote_sync_service.dart';
import '../domain/services/sharing_service.dart';
import '../domain/services/sync_controller.dart';
import '../domain/services/upload_engine.dart';

/// The application database. Overridden in tests with an in-memory instance.
final databaseProvider = Provider<AppDatabase>((ref) {
  final db = AppDatabase();
  ref.onDispose(db.close);
  return db;
});

final albumRepositoryProvider = Provider<AlbumRepository>((ref) {
  return AlbumRepositoryImpl(ref.watch(databaseProvider));
});

final mediaRepositoryProvider = Provider<MediaRepository>((ref) {
  return MediaRepositoryImpl(ref.watch(databaseProvider));
});

final syncJobRepositoryProvider = Provider<SyncJobRepository>((ref) {
  return SyncJobRepositoryImpl(ref.watch(databaseProvider));
});

final settingsRepositoryProvider = Provider<SettingsRepository>((ref) {
  return SettingsRepository(ref.watch(databaseProvider));
});

/// Google authentication/authorization service (spec §18).
final authServiceProvider = Provider<AuthService>((ref) {
  return GoogleAuthService();
});

/// Streams the current signed-in user (null when signed out).
final authStateProvider = StreamProvider<AppUser?>((ref) {
  final auth = ref.watch(authServiceProvider);
  // Kick off session restore; emissions arrive via the auth event stream.
  auth.restoreSession();
  return auth.authState();
});

final driveRepositoryProvider = Provider<DriveRepository>((ref) {
  return DriveRepositoryImpl(ref.watch(authServiceProvider));
});

/// Watches a single app setting value by key.
final settingWatchProvider = StreamProvider.family<String?, String>((ref, key) {
  return ref.watch(settingsRepositoryProvider).watch(key);
});

final driveRootServiceProvider = Provider<DriveRootService>((ref) {
  return DriveRootService(
    ref.watch(driveRepositoryProvider),
    ref.watch(settingsRepositoryProvider),
  );
});

/// The owner id for locally-created content. Uses the signed-in Google account
/// id when available, otherwise a stable local device owner.
final currentOwnerIdProvider = Provider<String>((ref) {
  final user = ref.watch(authStateProvider).value;
  return user?.id ?? 'local-owner';
});

final mediaSourceProvider = Provider<MediaSource>((ref) {
  return ImagePickerMediaSource();
});

final albumServiceProvider = Provider<AlbumService>((ref) {
  return AlbumService(ref.watch(albumRepositoryProvider));
});

final mediaServiceProvider = Provider<MediaService>((ref) {
  return MediaService(ref.watch(mediaRepositoryProvider));
});

final albumMemberRepositoryProvider = Provider<AlbumMemberRepository>((ref) {
  return AlbumMemberRepositoryImpl(ref.watch(databaseProvider));
});

/// Album sharing/collaboration (spec §29–31).
final sharingServiceProvider = Provider<SharingService>((ref) {
  return SharingService(
    ref.watch(albumMemberRepositoryProvider),
    ref.watch(driveRepositoryProvider),
  );
});

/// Watches an album's members.
final albumMembersProvider = StreamProvider.family<List<AlbumMember>, String>((
  ref,
  albumId,
) {
  return ref.watch(sharingServiceProvider).watchMembers(albumId);
});

/// Detects and reconciles remote (Drive) changes (spec §32, §33).
final remoteSyncServiceProvider = Provider<RemoteSyncService>((ref) {
  return RemoteSyncService(
    ref.watch(driveRepositoryProvider),
    ref.watch(mediaRepositoryProvider),
  );
});

/// Orchestrates album ↔ Drive folder linking (spec §3, §16, §46).
final linkedAlbumServiceProvider = Provider<LinkedAlbumService>((ref) {
  return LinkedAlbumService(
    ref.watch(albumRepositoryProvider),
    ref.watch(albumServiceProvider),
    ref.watch(driveRepositoryProvider),
    ref.watch(driveRootServiceProvider),
  );
});

final mediaBytesReaderProvider = Provider<MediaBytesReader>((ref) {
  return const MediaBytesReaderImpl();
});

final bulkRenameServiceProvider = Provider<BulkRenameService>((ref) {
  return const BulkRenameService();
});

final bulkMediaServiceProvider = Provider<BulkMediaService>((ref) {
  return BulkMediaService(
    ref.watch(mediaRepositoryProvider),
    ref.watch(driveRepositoryProvider),
    ref.watch(bulkRenameServiceProvider),
  );
});

/// Available bulk-rename categories (spec §25). Defaults for now; user-managed
/// categories can be layered on later.
final categoriesProvider = Provider<List<String>>((ref) {
  return AppConstants.defaultCategories;
});

/// All non-deleted albums (for destination pickers).
final allAlbumsProvider = FutureProvider((ref) {
  return ref.watch(albumRepositoryProvider).getAllAlbums();
});

final uploadEngineProvider = Provider<UploadEngine>((ref) {
  return UploadEngine(
    jobs: ref.watch(syncJobRepositoryProvider),
    media: ref.watch(mediaRepositoryProvider),
    albums: ref.watch(albumRepositoryProvider),
    drive: ref.watch(driveRepositoryProvider),
    reader: ref.watch(mediaBytesReaderProvider),
    linker: ref.watch(linkedAlbumServiceProvider),
  );
});

/// The upload queue processor. Kept alive for the app's lifetime.
final syncControllerProvider = Provider<SyncController>((ref) {
  final controller = SyncController(
    jobs: ref.watch(syncJobRepositoryProvider),
    media: ref.watch(mediaRepositoryProvider),
    engine: ref.watch(uploadEngineProvider),
  );
  ref.onDispose(controller.dispose);
  return controller;
});

/// Watches active/failed sync jobs for the Upload Queue screen.
final activeJobsProvider = StreamProvider((ref) {
  return ref.watch(syncJobRepositoryProvider).watchActiveJobs();
});

/// Watches the child albums of [parentId] (null = root).
final childAlbumsProvider = StreamProvider.family<List<Album>, String?>((
  ref,
  parentId,
) {
  return ref.watch(albumServiceProvider).watchChildren(parentId);
});

/// Watches the media items in an album.
final albumMediaProvider = StreamProvider.family<List<MediaItem>, String>((
  ref,
  albumId,
) {
  return ref.watch(mediaServiceProvider).watchAlbumMedia(albumId);
});

/// Resolves a single album by id.
final albumByIdProvider = FutureProvider.family<Album?, String>((
  ref,
  id,
) async {
  return ref.watch(albumServiceProvider).getById(id);
});

/// Resolves a single media item by id.
final mediaByIdProvider = FutureProvider.family<MediaItem?, String>((
  ref,
  id,
) async {
  return ref.watch(mediaRepositoryProvider).getMediaById(id);
});
