import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../features/albums/album_detail_screen.dart';
import '../features/home/home_screen.dart';

/// Route path constants and builders (avoid magic strings).
class Routes {
  Routes._();
  static const home = '/';
  static const album = '/album/:id';

  static String albumPath(String id) => '/album/$id';
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
    ],
  );
});
