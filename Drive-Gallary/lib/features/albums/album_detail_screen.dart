import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../app/providers.dart';
import '../../app/router.dart';
import '../../core/errors/app_error.dart';
import '../../domain/models/album.dart';
import '../../domain/models/enums.dart';
import '../../domain/models/media_item.dart';
import '../../domain/services/remote_sync_service.dart';
import '../../widgets/media_thumbnail.dart';
import '../../widgets/sync_status_chip.dart';
import '../media/media_detail_screen.dart';
import 'widgets/album_dialogs.dart';
import 'widgets/album_tile.dart';
import 'widgets/bulk_rename_dialog.dart';

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

class _AlbumDetailBody extends ConsumerStatefulWidget {
  const _AlbumDetailBody({required this.album});

  final Album album;

  @override
  ConsumerState<_AlbumDetailBody> createState() => _AlbumDetailBodyState();
}

class _AlbumDetailBodyState extends ConsumerState<_AlbumDetailBody> {
  /// Selected media item ids (selection mode is active when non-empty).
  final Set<String> _selected = {};

  bool get _selectionMode => _selected.isNotEmpty;
  Album get album => widget.album;

  void _toggleSelect(String id) {
    setState(() {
      if (!_selected.remove(id)) _selected.add(id);
    });
  }

  @override
  Widget build(BuildContext context) {
    final childrenAsync = ref.watch(childAlbumsProvider(album.id));
    final mediaAsync = ref.watch(albumMediaProvider(album.id));

    return Scaffold(
      appBar: _selectionMode
          ? AppBar(
              leading: IconButton(
                icon: const Icon(Icons.close),
                onPressed: () => setState(_selected.clear),
              ),
              title: Text('${_selected.length} selected'),
              actions: [
                IconButton(
                  tooltip: 'Rename selected',
                  icon: const Icon(Icons.drive_file_rename_outline),
                  onPressed: () => _renameSelected(context),
                ),
              ],
            )
          : AppBar(
              title: Text(album.name),
              actions: [
                IconButton(
                  tooltip: 'New sub-album',
                  icon: const Icon(Icons.create_new_folder_outlined),
                  onPressed: () => _createSubAlbum(context, ref),
                ),
                PopupMenuButton<String>(
                  onSelected: (v) {
                    if (v == 'repair') _repairLink(context, ref);
                    if (v == 'bulk_rename') _bulkRename(context, ref);
                    if (v == 'sync_drive') _syncFromDrive(context, ref);
                    if (v == 'members') {
                      context.push(Routes.albumMembersPath(album.id));
                    }
                  },
                  itemBuilder: (context) => [
                    const PopupMenuItem(
                      value: 'members',
                      child: Text('Share / Members'),
                    ),
                    const PopupMenuItem(
                      value: 'sync_drive',
                      child: Text('Sync from Drive'),
                    ),
                    const PopupMenuItem(
                      value: 'bulk_rename',
                      child: Text('Bulk rename photos'),
                    ),
                    const PopupMenuItem(
                      value: 'repair',
                      child: Text('Repair Drive link'),
                    ),
                  ],
                ),
              ],
            ),
      body: CustomScrollView(
        slivers: [
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(12, 8, 12, 0),
              child: Row(
                children: [
                  AlbumCloudStatusChip(
                    status: _aggregateStatus(
                      album,
                      mediaAsync.value ?? const [],
                    ),
                  ),
                  const Spacer(),
                  Text(
                    '${(mediaAsync.value ?? const []).length} photos',
                    style: Theme.of(context).textTheme.bodySmall,
                  ),
                ],
              ),
            ),
          ),
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
          childAspectRatio: 0.78,
        ),
        delegate: SliverChildBuilderDelegate((context, i) {
          final item = media[i];
          final selected = _selected.contains(item.id);
          return GestureDetector(
            onTap: () {
              if (_selectionMode) {
                _toggleSelect(item.id);
              } else {
                Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => MediaDetailScreen(item: item),
                  ),
                );
              }
            },
            onLongPress: () => _toggleSelect(item.id),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Expanded(
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
                        if (_selectionMode)
                          Positioned(
                            left: 4,
                            top: 4,
                            child: Icon(
                              selected
                                  ? Icons.check_circle
                                  : Icons.radio_button_unchecked,
                              color: selected
                                  ? Theme.of(context).colorScheme.primary
                                  : Colors.white,
                            ),
                          ),
                        if (selected)
                          Container(
                            color: Theme.of(
                              context,
                            ).colorScheme.primary.withValues(alpha: 0.25),
                          ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  item.fileName,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.bodySmall,
                ),
              ],
            ),
          );
        }, childCount: media.length),
      ),
    );
  }

  /// Renames the currently selected images by category (spec §26). Updates
  /// Drive names for already-synced items via BulkMediaService.
  Future<void> _renameSelected(BuildContext context) async {
    final all = await ref
        .read(mediaRepositoryProvider)
        .getMediaInAlbum(album.id, limit: 100000);
    final selectedItems = all.where((m) => _selected.contains(m.id)).toList()
      ..sort((a, b) {
        final at = a.capturedAt ?? a.createdAt;
        final bt = b.capturedAt ?? b.createdAt;
        return at.compareTo(bt);
      });
    if (selectedItems.isEmpty || !context.mounted) return;
    final options = await promptBulkRename(
      context,
      categories: ref.read(categoriesProvider),
      photoCount: selectedItems.length,
    );
    if (options == null || !context.mounted) return;
    final messenger = ScaffoldMessenger.of(context);
    final updated = await ref
        .read(bulkMediaServiceProvider)
        .rename(
          items: selectedItems,
          category: options.category,
          startNumber: options.startNumber,
        );
    setState(_selected.clear);
    messenger.showSnackBar(
      SnackBar(content: Text('Renamed ${updated.length} photo(s).')),
    );
  }

  /// Derives the album-level cloud status from its media (spec §35).
  AlbumCloudStatus _aggregateStatus(Album album, List<MediaItem> media) {
    if (!album.isDriveLinked) return AlbumCloudStatus.notLinked;
    var hasFailed = false;
    var hasUploading = false;
    var hasWaiting = false;
    for (final m in media) {
      switch (m.syncStatus) {
        case SyncStatus.failed:
          hasFailed = true;
        case SyncStatus.uploading:
        case SyncStatus.queued:
          hasUploading = true;
        case SyncStatus.waitingForNetwork:
        case SyncStatus.localOnly:
          hasWaiting = true;
        default:
          break;
      }
    }
    if (hasFailed) return AlbumCloudStatus.failed;
    if (hasUploading) return AlbumCloudStatus.uploading;
    if (hasWaiting) return AlbumCloudStatus.waitingForNetwork;
    return AlbumCloudStatus.synced;
  }

  Future<void> _createSubAlbum(BuildContext context, WidgetRef ref) async {
    final name = await promptAlbumName(context, title: 'New sub-album');
    if (name == null || name.isEmpty || !context.mounted) return;
    final messenger = ScaffoldMessenger.of(context);
    final result = await ref
        .read(linkedAlbumServiceProvider)
        .createAlbum(
          name: name,
          ownerUserId: ref.read(currentOwnerIdProvider),
          parentAlbumId: album.id,
        );
    messenger.showSnackBar(SnackBar(content: Text(linkResultMessage(result))));
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
    await ref.read(linkedAlbumServiceProvider).rename(target, name);
  }

  Future<void> _bulkRename(BuildContext context, WidgetRef ref) async {
    final media = await ref
        .read(mediaRepositoryProvider)
        .getMediaInAlbum(album.id, limit: 100000);
    if (!context.mounted) return;
    if (media.isEmpty) {
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(const SnackBar(content: Text('No photos to rename.')));
      return;
    }
    // Order by capture time (fallback createdAt), oldest first, for numbering.
    media.sort((a, b) {
      final at = a.capturedAt ?? a.createdAt;
      final bt = b.capturedAt ?? b.createdAt;
      return at.compareTo(bt);
    });
    final options = await promptBulkRename(
      context,
      categories: ref.read(categoriesProvider),
      photoCount: media.length,
    );
    if (options == null || !context.mounted) return;
    final messenger = ScaffoldMessenger.of(context);
    final updated = await ref
        .read(bulkMediaServiceProvider)
        .rename(
          items: media,
          category: options.category,
          startNumber: options.startNumber,
        );
    messenger.showSnackBar(
      SnackBar(content: Text('Renamed ${updated.length} photo(s).')),
    );
  }

  Future<void> _syncFromDrive(BuildContext context, WidgetRef ref) async {
    final messenger = ScaffoldMessenger.of(context);
    final remote = ref.read(remoteSyncServiceProvider);
    try {
      final result = await remote.scan(album);
      final imported = await remote.importRemoteOnly(album, result.remoteOnly);
      if (context.mounted && result.conflicts.isNotEmpty) {
        await _resolveConflicts(context, ref, result.conflicts);
      }
      messenger.showSnackBar(
        SnackBar(
          content: Text(
            'Drive scan: $imported new remote file(s), '
            '${result.conflicts.length} conflict(s).',
          ),
        ),
      );
    } on AppError catch (e) {
      messenger.showSnackBar(SnackBar(content: Text(e.message)));
    } catch (e) {
      messenger.showSnackBar(
        const SnackBar(content: Text('Could not sync from Drive.')),
      );
    }
  }

  Future<void> _resolveConflicts(
    BuildContext context,
    WidgetRef ref,
    List<SyncConflict> conflicts,
  ) async {
    final remote = ref.read(remoteSyncServiceProvider);
    for (final c in conflicts) {
      if (!context.mounted) return;
      final choice = await showDialog<String>(
        context: context,
        builder: (context) => AlertDialog(
          title: const Text('Sync Conflict'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text('Local:'),
              Text(
                c.local.fileName,
                style: const TextStyle(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 8),
              const Text('Cloud:'),
              Text(
                c.remote.name,
                style: const TextStyle(fontWeight: FontWeight.bold),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context, 'local'),
              child: const Text('Keep Local'),
            ),
            TextButton(
              onPressed: () => Navigator.pop(context, 'cloud'),
              child: const Text('Keep Cloud'),
            ),
          ],
        ),
      );
      if (choice == 'local') {
        await remote.keepLocal(c);
      } else if (choice == 'cloud') {
        await remote.keepCloud(c);
      }
    }
  }

  Future<void> _repairLink(BuildContext context, WidgetRef ref) async {
    final messenger = ScaffoldMessenger.of(context);
    final result = await ref.read(linkedAlbumServiceProvider).repairLink(album);
    ref.invalidate(albumByIdProvider(album.id));
    messenger.showSnackBar(
      SnackBar(
        content: Text(
          result.linked == true
              ? 'Drive link OK for "${result.album.name}".'
              : 'Could not link to Drive. Connect Google, then try again.',
        ),
      ),
    );
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
    await ref.read(syncControllerProvider).enqueueUploads(added);
    if (context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Added ${added.length} item(s). Uploading...')),
      );
    }
  }

  Future<void> _capture(BuildContext context, WidgetRef ref) async {
    final shot = await ref.read(mediaSourceProvider).capturePhoto();
    if (shot == null) return;
    final added = await ref.read(mediaServiceProvider).addToAlbum(album.id, [
      shot,
    ]);
    await ref.read(syncControllerProvider).enqueueUploads(added);
    if (context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Photo captured and added. Uploading...')),
      );
    }
  }
}
