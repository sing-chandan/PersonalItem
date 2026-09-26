import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/providers.dart';
import '../../../domain/models/album.dart';
import '../../../domain/models/enums.dart';
import '../../../widgets/media_thumbnail.dart';

/// Shows an album's cover: its most recent photo if available, otherwise a
/// folder/album icon (gallery-style).
class AlbumCover extends ConsumerWidget {
  const AlbumCover({super.key, required this.album, this.size = 56});

  final Album album;
  final double size;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final coverAsync = ref.watch(albumCoverProvider(album.id));
    final fallback = _fallback(context);

    return ClipRRect(
      borderRadius: BorderRadius.circular(8),
      child: SizedBox(
        width: size,
        height: size,
        child: coverAsync.maybeWhen(
          data: (item) =>
              item == null ? fallback : MediaThumbnail(item: item, size: size),
          orElse: () => fallback,
        ),
      ),
    );
  }

  Widget _fallback(BuildContext context) {
    return Container(
      color: Theme.of(context).colorScheme.surfaceContainerHighest,
      alignment: Alignment.center,
      child: Icon(
        album.type == AlbumType.folder ? Icons.folder : Icons.photo_album,
        color: Theme.of(context).colorScheme.primary,
      ),
    );
  }
}
