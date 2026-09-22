import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/db/database.dart';
import '../data/media/image_picker_media_source.dart';
import '../data/repositories/album_repository_impl.dart';
import '../data/repositories/media_repository_impl.dart';
import '../data/repositories/sync_job_repository_impl.dart';
import '../domain/models/album.dart';
import '../domain/models/media_item.dart';
import '../domain/repositories/album_repository.dart';
import '../domain/repositories/media_repository.dart';
import '../domain/repositories/sync_job_repository.dart';
import '../domain/services/album_service.dart';
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

/// The owner id for locally-created content. Until Google Sign-In (Phase 2)
/// this is a stable local device owner; it is replaced by the Google account
/// id once the user connects.
final currentOwnerIdProvider = Provider<String>((ref) => 'local-owner');

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
