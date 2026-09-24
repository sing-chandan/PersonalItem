import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../app/providers.dart';
import '../../app/router.dart';
import '../../core/constants/app_constants.dart';
import '../albums/widgets/album_dialogs.dart';
import '../albums/widgets/album_tile.dart';

/// Home screen: lists the user's top-level albums and the main entry points
/// (spec §42). New Photos and Cloud Status are wired in later phases.
class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final rootAlbums = ref.watch(childAlbumsProvider(null));

    final user = ref.watch(authStateProvider).value;

    return Scaffold(
      appBar: AppBar(
        title: const Text(AppConstants.appName),
        actions: [
          IconButton(
            tooltip: user == null ? 'Not connected' : user.email,
            icon: Icon(user == null ? Icons.cloud_off : Icons.cloud_done),
            onPressed: () => context.push(Routes.settings),
          ),
          IconButton(
            tooltip: 'Upload Queue',
            icon: const Icon(Icons.cloud_sync),
            onPressed: () => context.push(Routes.uploadQueue),
          ),
          IconButton(
            tooltip: 'Settings',
            icon: const Icon(Icons.settings),
            onPressed: () => context.push(Routes.settings),
          ),
        ],
      ),
      body: rootAlbums.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(child: Text('Could not load albums.\n$e')),
        data: (albums) {
          if (albums.isEmpty) {
            return _EmptyState(onCreate: () => _createAlbum(context, ref));
          }
          return ListView(
            padding: const EdgeInsets.symmetric(vertical: 8),
            children: [
              Padding(
                padding: const EdgeInsets.fromLTRB(12, 8, 12, 4),
                child: Card(
                  color: Theme.of(context).colorScheme.primaryContainer,
                  child: ListTile(
                    leading: const Icon(Icons.add_a_photo, size: 32),
                    title: const Text('New Photos'),
                    subtitle: const Text(
                      'Pick newly downloaded photos and add them to an album.',
                    ),
                    trailing: const Icon(Icons.chevron_right),
                    onTap: () => context.push(Routes.newPhotos),
                  ),
                ),
              ),
              const Padding(
                padding: EdgeInsets.fromLTRB(16, 8, 16, 4),
                child: Text(
                  'My Albums',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
              ),
              for (final album in albums)
                AlbumTile(
                  album: album,
                  onTap: () => context.push(Routes.albumPath(album.id)),
                  onRename: () => _renameAlbum(context, ref, album.id),
                  onDelete: () => _deleteAlbum(context, ref, album.id),
                ),
            ],
          );
        },
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _createAlbum(context, ref),
        icon: const Icon(Icons.add),
        label: const Text('New Album'),
      ),
    );
  }

  Future<void> _createAlbum(BuildContext context, WidgetRef ref) async {
    final name = await promptAlbumName(context, title: 'Create album');
    if (name == null || name.isEmpty || !context.mounted) return;
    final messenger = ScaffoldMessenger.of(context);
    final result = await ref
        .read(linkedAlbumServiceProvider)
        .createAlbum(name: name, ownerUserId: ref.read(currentOwnerIdProvider));
    messenger.showSnackBar(SnackBar(content: Text(linkResultMessage(result))));
  }

  Future<void> _renameAlbum(
    BuildContext context,
    WidgetRef ref,
    String id,
  ) async {
    final album = await ref.read(albumServiceProvider).getById(id);
    if (album == null || !context.mounted) return;
    final name = await promptAlbumName(
      context,
      title: 'Rename album',
      initial: album.name,
      actionLabel: 'Rename',
    );
    if (name == null || name.isEmpty) return;
    await ref.read(linkedAlbumServiceProvider).rename(album, name);
  }

  Future<void> _deleteAlbum(
    BuildContext context,
    WidgetRef ref,
    String id,
  ) async {
    final ok = await confirmAction(
      context,
      title: 'Delete album?',
      message:
          'This removes the album from the app. Files already in Google Drive '
          'are NOT deleted.',
      confirmLabel: 'Delete',
      destructive: true,
    );
    if (!ok) return;
    await ref.read(albumServiceProvider).delete(id);
  }
}

class _EmptyState extends StatelessWidget {
  const _EmptyState({required this.onCreate});
  final VoidCallback onCreate;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.photo_library_outlined, size: 64),
            const SizedBox(height: 16),
            const Text(
              'No albums yet',
              style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            const Text(
              'Create your first album, then add photos to it.',
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 24),
            FilledButton.icon(
              onPressed: onCreate,
              icon: const Icon(Icons.add),
              label: const Text('Create album'),
            ),
          ],
        ),
      ),
    );
  }
}
