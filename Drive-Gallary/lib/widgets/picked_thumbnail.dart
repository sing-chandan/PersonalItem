import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

import '../domain/models/picked_media.dart';

/// Thumbnail for a just-picked (not yet persisted) media item.
class PickedThumbnail extends StatelessWidget {
  const PickedThumbnail({super.key, required this.media, this.size = 120});

  final PickedMedia media;
  final double size;

  @override
  Widget build(BuildContext context) {
    final placeholder = Container(
      color: Theme.of(context).colorScheme.surfaceContainerHighest,
      alignment: Alignment.center,
      child: const Icon(Icons.image_outlined),
    );
    final cacheW = (size * 2).round();
    Widget onError(BuildContext c, Object e, StackTrace? s) => placeholder;

    if (media.mimeType.startsWith('video/')) {
      return Container(
        color: Colors.black12,
        alignment: Alignment.center,
        child: const Icon(Icons.videocam),
      );
    }
    if (kIsWeb) {
      return Image.network(
        media.localUri,
        fit: BoxFit.cover,
        cacheWidth: cacheW,
        errorBuilder: onError,
      );
    }
    return Image.file(
      File(media.localUri),
      fit: BoxFit.cover,
      cacheWidth: cacheW,
      errorBuilder: onError,
    );
  }
}
