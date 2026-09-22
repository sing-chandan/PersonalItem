import 'package:flutter/material.dart';

import '../../../domain/models/album.dart';
import '../../../domain/models/enums.dart';

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
        leading: CircleAvatar(
          child: Icon(
            album.type == AlbumType.folder ? Icons.folder : Icons.photo_album,
          ),
        ),
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
