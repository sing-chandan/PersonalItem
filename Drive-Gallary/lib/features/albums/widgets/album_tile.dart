import 'package:flutter/material.dart';

import '../../../domain/models/album.dart';
import 'album_cover.dart';

/// A list tile representing an album/folder in a browsing list.
class AlbumTile extends StatelessWidget {
  const AlbumTile({
    super.key,
    required this.album,
    required this.onTap,
    this.onRename,
    this.onDelete,
    this.subtitle,
  });

  final Album album;
  final VoidCallback onTap;
  final VoidCallback? onRename;
  final VoidCallback? onDelete;
  final String? subtitle;

  @override
  Widget build(BuildContext context) {
    return Card(
      clipBehavior: Clip.antiAlias,
      child: ListTile(
        leading: AlbumCover(album: album, size: 48),
        title: Text(album.name),
        subtitle: subtitle == null ? null : Text(subtitle!),
        trailing: (onRename == null && onDelete == null)
            ? const Icon(Icons.chevron_right)
            : PopupMenuButton<String>(
                onSelected: (v) {
                  if (v == 'rename') onRename?.call();
                  if (v == 'delete') onDelete?.call();
                },
                itemBuilder: (context) => [
                  if (onRename != null)
                    const PopupMenuItem(value: 'rename', child: Text('Rename')),
                  if (onDelete != null)
                    const PopupMenuItem(value: 'delete', child: Text('Delete')),
                ],
              ),
        onTap: onTap,
      ),
    );
  }
}

/// A gallery-style album card: large cover image with the name and optional
/// subtitle below, plus an overflow menu.
class AlbumGridCard extends StatelessWidget {
  const AlbumGridCard({
    super.key,
    required this.album,
    required this.onTap,
    this.onRename,
    this.onDelete,
    this.subtitle,
  });

  final Album album;
  final VoidCallback onTap;
  final VoidCallback? onRename;
  final VoidCallback? onDelete;
  final String? subtitle;

  @override
  Widget build(BuildContext context) {
    return Card(
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Expanded(child: AlbumCover(album: album, size: 200)),
            Padding(
              padding: const EdgeInsets.fromLTRB(10, 8, 4, 8),
              child: Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          album.name,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(fontWeight: FontWeight.w600),
                        ),
                        if (subtitle != null)
                          Text(
                            subtitle!,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: Theme.of(context).textTheme.bodySmall,
                          ),
                      ],
                    ),
                  ),
                  if (onRename != null || onDelete != null)
                    SizedBox(
                      width: 32,
                      child: PopupMenuButton<String>(
                        padding: EdgeInsets.zero,
                        onSelected: (v) {
                          if (v == 'rename') onRename?.call();
                          if (v == 'delete') onDelete?.call();
                        },
                        itemBuilder: (context) => [
                          if (onRename != null)
                            const PopupMenuItem(
                              value: 'rename',
                              child: Text('Rename'),
                            ),
                          if (onDelete != null)
                            const PopupMenuItem(
                              value: 'delete',
                              child: Text('Delete'),
                            ),
                        ],
                      ),
                    ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
