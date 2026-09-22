import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/db/database.dart';
import '../data/drive/drive_repository_impl.dart';
import '../data/drive/google_auth_service.dart';
import '../data/media/image_picker_media_source.dart';
import '../data/repositories/album_repository_impl.dart';
import '../data/repositories/media_repository_impl.dart';
import '../data/repositories/settings_repository.dart';
import '../data/repositories/sync_job_repository_impl.dart';
import '../domain/models/album.dart';
import '../domain/models/app_user.dart';
import '../domain/models/media_item.dart';
import '../domain/repositories/album_repository.dart';
import '../domain/repositories/drive_repository.dart';
import '../domain/repositories/media_repository.dart';
import '../domain/repositories/sync_job_repository.dart';
import '../domain/services/album_service.dart';
import '../domain/services/auth_service.dart';
import '../domain/services/drive_root_service.dart';
import '../domain/services/media_service.dart';
import '../domain/services/media_source.dart';

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
