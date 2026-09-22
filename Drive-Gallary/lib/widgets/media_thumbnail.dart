import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

import '../domain/models/media_item.dart';

/// Displays a media thumbnail from a local URI, adapting to the platform:
/// on web the URI is a blob/object URL (Image.network), on native it is a file
/// path (Image.file). Full-resolution decoding is avoided via cacheWidth.
class MediaThumbnail extends StatelessWidget {
  const MediaThumbnail({super.key, required this.item, this.size = 120});

  final MediaItem item;
  final double size;

  @override
  Widget build(BuildContext context) {
    final placeholder = Container(
      color: Theme.of(context).colorScheme.surfaceContainerHighest,
      alignment: Alignment.center,
      child: Icon(item.isVideo ? Icons.videocam : Icons.image_outlined),
    );

    if (item.isVideo) return placeholder;

    final cacheW = (size * 2).round();
    Widget errorBuilder(BuildContext c, Object e, StackTrace? s) => placeholder;

    if (kIsWeb) {
      return Image.network(
        item.localUri,
        fit: BoxFit.cover,
        cacheWidth: cacheW,
        errorBuilder: errorBuilder,
      );
    }
    return Image.file(
      File(item.localUri),
      fit: BoxFit.cover,
      cacheWidth: cacheW,
      errorBuilder: errorBuilder,
    );
  }
}
