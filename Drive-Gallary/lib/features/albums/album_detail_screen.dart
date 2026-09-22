import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../app/providers.dart';
import '../../app/router.dart';
import '../../domain/models/album.dart';
import '../../domain/models/enums.dart';
import '../../domain/models/media_item.dart';
import '../../widgets/media_thumbnail.dart';
import '../media/media_detail_screen.dart';
import 'widgets/album_dialogs.dart';
import 'widgets/album_tile.dart';

/// Album detail: shows sub-albums and the media grid, and lets the user add
/// photos (picker / camera) and manage the album. Works fully offline.
class AlbumDetailScreen extends ConsumerWidget {
  const AlbumDetailScreen({super.key, required this.albumId});

  final String albumId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final albumAsync = ref.watch(albumByIdProvider(albumId));

    return albumAsync.when(
      loading: () =>
          const Scaffold(body: Center(child: CircularProgressIndicator())),
      error: (e, _) => Scaffold(
        appBar: AppBar(),
        body: Center(child: Text('Could not load album.\n$e')),
      ),
      data: (album) {
        if (album == null) {
          return Scaffold(
            appBar: AppBar(),
            body: const Center(child: Text('Album not found.')),
          );
        }
        return _AlbumDetailBody(album: album);
      },
    );
  }
}

class _AlbumDetailBody extends ConsumerWidget {
  const _AlbumDetailBody({required this.album});

  final Album album;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final childrenAsync = ref.watch(childAlbumsProvider(album.id));
    final mediaAsync = ref.watch(albumMediaProvider(album.id));

    return Scaffold(
      appBar: AppBar(
        title: Text(album.name),
        actions: [
          IconButton(
            tooltip: 'New sub-album',
            icon: const Icon(Icons.create_new_folder_outlined),
            onPressed: () => _createSubAlbum(context, ref),
          ),
        ],
      ),
      body: CustomScrollView(
        slivers: [
          childrenAsync.maybeWhen(
            data: (children) => _subAlbumsSliver(context, ref, children),
            orElse: () => const SliverToBoxAdapter(child: SizedBox.shrink()),
          ),
          mediaAsync.when(
            loading: () => const SliverToBoxAdapter(
              child: Padding(
                padding: EdgeInsets.all(32),
                child: Center(child: CircularProgressIndicator()),
              ),
            ),
            error: (e, _) => SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Text('Could not load media.\n$e'),
              ),
            ),
            data: (media) => _mediaSliver(context, media),
          ),
        ],
      ),
      floatingActionButton: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.end,
        children: [
          FloatingActionButton.small(
            heroTag: 'camera',
            tooltip: 'Camera',
            onPressed: () => _capture(context, ref),
            child: const Icon(Icons.photo_camera),
          ),
          const SizedBox(height: 12),
          FloatingActionButton.extended(
            heroTag: 'add',
            onPressed: () => _addPhotos(context, ref),
            icon: const Icon(Icons.add_photo_alternate),
            label: const Text('Add Photos'),
          ),
        ],
      ),
    );
  }

  Widget _subAlbumsSliver(
    BuildContext context,
    WidgetRef ref,
    List<Album> children,
  ) {
    if (children.isEmpty) {
      return const SliverToBoxAdapter(child: SizedBox.shrink());
    }
    return SliverList.builder(
      itemCount: children.length,
      itemBuilder: (context, i) {
        final child = children[i];
        return AlbumTile(
          album: child,
          onTap: () => context.push(Routes.albumPath(child.id)),
          onRename: () => _renameAlbum(context, ref, child),
          onDelete: () => _deleteAlbum(context, ref, child),
        );
      },
    );
  }

  Widget _mediaSliver(BuildContext context, List<MediaItem> media) {
    if (media.isEmpty) {
      return const SliverToBoxAdapter(
        child: Padding(
          padding: EdgeInsets.all(40),
          child: Center(
            child: Text('No photos yet. Tap "Add Photos" to get started.'),
          ),
        ),
      );
    }
    return SliverPadding(
      padding: const EdgeInsets.all(8),
      sliver: SliverGrid(
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 3,
          crossAxisSpacing: 6,
          mainAxisSpacing: 6,
        ),
        delegate: SliverChildBuilderDelegate((context, i) {
          final item = media[i];
          return GestureDetector(
            onTap: () => Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => MediaDetailScreen(item: item)),
            ),
            child: ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: Stack(
                fit: StackFit.expand,
                children: [
                  MediaThumbnail(item: item),
                  if (item.syncStatus != SyncStatus.synced)
                    const Positioned(
                      right: 4,
                      top: 4,
                      child: Icon(
                        Icons.cloud_off,
                        size: 16,
                        color: Colors.white,
                      ),
                    ),
                ],
              ),
            ),
          );
        }, childCount: media.length),
      ),
    );
  }

  Future<void> _createSubAlbum(BuildContext context, WidgetRef ref) async {
    final name = await promptAlbumName(context, title: 'New sub-album');
    if (name == null || name.isEmpty) return;
    await ref
        .read(albumServiceProvider)
        .createAlbum(
          name: name,
          ownerUserId: ref.read(currentOwnerIdProvider),
          parentAlbumId: album.id,
        );
  }

  Future<void> _renameAlbum(
    BuildContext context,
    WidgetRef ref,
    Album target,
  ) async {
    final name = await promptAlbumName(
      context,
      title: 'Rename album',
      initial: target.name,
      actionLabel: 'Rename',
    );
    if (name == null || name.isEmpty) return;
    await ref.read(albumServiceProvider).rename(target, name);
  }

  Future<void> _deleteAlbum(
    BuildContext context,
    WidgetRef ref,
    Album target,
  ) async {
    final ok = await confirmAction(
      context,
      title: 'Delete album?',
      message:
          'This removes "${target.name}" from the app. Files already in Google '
          'Drive are NOT deleted.',
      confirmLabel: 'Delete',
      destructive: true,
    );
    if (!ok) return;
    await ref.read(albumServiceProvider).delete(target.id);
  }

  Future<void> _addPhotos(BuildContext context, WidgetRef ref) async {
    final picked = await ref.read(mediaSourceProvider).pickImages();
    if (picked.isEmpty) return;
    final added = await ref
        .read(mediaServiceProvider)
        .addToAlbum(album.id, picked);
    if (context.mounted) {
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text('Added ${added.length} item(s).')));
    }
  }

  Future<void> _capture(BuildContext context, WidgetRef ref) async {
    final shot = await ref.read(mediaSourceProvider).capturePhoto();
    if (shot == null) return;
    await ref.read(mediaServiceProvider).addToAlbum(album.id, [shot]);
    if (context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Photo captured and added.')),
      );
    }
  }
}
