import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../features/albums/album_detail_screen.dart';
import '../features/home/home_screen.dart';
import '../features/new_photos/new_photos_screen.dart';
import '../features/settings/settings_screen.dart';
import '../features/sharing/members_screen.dart';
import '../features/sync/upload_queue_screen.dart';

/// Route path constants and builders (avoid magic strings).
class Routes {
  Routes._();
  static const home = '/';
  static const album = '/album/:id';
  static const settings = '/settings';
  static const uploadQueue = '/queue';
  static const newPhotos = '/new-photos';
  static const albumMembers = '/album/:id/members';

  static String albumPath(String id) => '/album/$id';
  static String albumMembersPath(String id) => '/album/$id/members';
}

final routerProvider = Provider<GoRouter>((ref) {
  return GoRouter(
    initialLocation: Routes.home,
    routes: [
      GoRoute(
        path: Routes.home,
        builder: (context, state) => const HomeScreen(),
      ),
      GoRoute(
        path: Routes.album,
        builder: (context, state) =>
            AlbumDetailScreen(albumId: state.pathParameters['id']!),
      ),
      GoRoute(
        path: Routes.albumMembers,
        builder: (context, state) =>
            MembersScreen(albumId: state.pathParameters['id']!),
      ),
      GoRoute(
        path: Routes.settings,
        builder: (context, state) => const SettingsScreen(),
      ),
      GoRoute(
        path: Routes.uploadQueue,
        builder: (context, state) => const UploadQueueScreen(),
      ),
      GoRoute(
        path: Routes.newPhotos,
        builder: (context, state) => const NewPhotosScreen(),
      ),
    ],
  );
});
