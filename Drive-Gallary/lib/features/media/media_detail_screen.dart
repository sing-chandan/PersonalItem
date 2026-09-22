import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../app/providers.dart';
import '../../domain/models/enums.dart';
import '../../domain/models/media_item.dart';
import '../albums/widgets/album_dialogs.dart';

/// Full-screen media viewer with metadata and local delete (spec §34: local
/// delete never touches Drive).
class MediaDetailScreen extends ConsumerWidget {
  const MediaDetailScreen({super.key, required this.item, this.onDeleted});

  final MediaItem item;
  final VoidCallback? onDeleted;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Scaffold(
      appBar: AppBar(
        title: Text(item.fileName, overflow: TextOverflow.ellipsis),
        actions: [
          IconButton(
            tooltip: 'Remove from album',
            icon: const Icon(Icons.delete_outline),
            onPressed: () => _delete(context, ref),
          ),
        ],
      ),
      body: Column(
        children: [
          Expanded(
            child: ColoredBox(
              color: Colors.black,
              child: Center(child: _preview()),
            ),
          ),
          _metadata(context),
        ],
      ),
    );
  }

  Widget _preview() {
    if (item.isVideo) {
      return const Icon(Icons.videocam, size: 96, color: Colors.white54);
    }
    if (kIsWeb) {
      return Image.network(item.localUri, fit: BoxFit.contain);
    }
    return Image.file(File(item.localUri), fit: BoxFit.contain);
  }

  Widget _metadata(BuildContext context) {
    final df = DateFormat.yMMMd().add_jm();
    final sizeKb = (item.sizeBytes / 1024).toStringAsFixed(0);
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _row('Status', _statusLabel(item.syncStatus)),
          _row('Type', item.mimeType),
          _row('Size', '$sizeKb KB'),
          if (item.capturedAt != null)
            _row('Captured', df.format(item.capturedAt!)),
          if (item.category != null) _row('Category', item.category!),
        ],
      ),
    );
  }

  Widget _row(String label, String value) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 2),
    child: Row(
      children: [
        SizedBox(
          width: 90,
          child: Text(
            label,
            style: const TextStyle(fontWeight: FontWeight.w600),
          ),
        ),
        Expanded(child: Text(value)),
      ],
    ),
  );

  String _statusLabel(SyncStatus s) => switch (s) {
    SyncStatus.synced => 'Synced',
    SyncStatus.uploading => 'Uploading',
    SyncStatus.queued => 'Queued',
    SyncStatus.failed => 'Failed',
    SyncStatus.waitingForNetwork => 'Waiting for Internet',
    SyncStatus.localOnly => 'On device (not uploaded)',
    SyncStatus.remoteOnly => 'In cloud only',
    SyncStatus.conflict => 'Conflict',
    SyncStatus.deletedLocal => 'Deleted locally',
    SyncStatus.deletedRemote => 'Deleted in cloud',
  };

  Future<void> _delete(BuildContext context, WidgetRef ref) async {
    final ok = await confirmAction(
      context,
      title: 'Remove photo?',
      message:
          'This removes the photo from this album on the device. If it was '
          'already uploaded, the Google Drive copy is NOT deleted.',
      confirmLabel: 'Remove',
      destructive: true,
    );
    if (!ok) return;
    await ref.read(mediaServiceProvider).removeLocal(item.id);
    if (context.mounted) {
      Navigator.of(context).pop();
      onDeleted?.call();
    }
  }
}
