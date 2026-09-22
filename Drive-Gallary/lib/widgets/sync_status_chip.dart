import 'package:flutter/material.dart';

import '../domain/models/enums.dart';

/// Small pill showing an album's aggregate cloud status (spec §35).
class AlbumCloudStatusChip extends StatelessWidget {
  const AlbumCloudStatusChip({super.key, required this.status});

  final AlbumCloudStatus status;

  @override
  Widget build(BuildContext context) {
    final (label, color, icon) = switch (status) {
      AlbumCloudStatus.synced => ('Synced', Colors.green, Icons.cloud_done),
      AlbumCloudStatus.uploading => (
        'Uploading',
        Colors.amber.shade800,
        Icons.cloud_upload,
      ),
      AlbumCloudStatus.waitingForNetwork => (
        'Waiting for Internet',
        Colors.orange,
        Icons.cloud_off,
      ),
      AlbumCloudStatus.failed => ('Failed', Colors.red, Icons.error_outline),
      AlbumCloudStatus.notLinked => ('Not linked', Colors.grey, Icons.link_off),
    };
    return Chip(
      visualDensity: VisualDensity.compact,
      avatar: Icon(icon, size: 18, color: color),
      label: Text(label),
      side: BorderSide(color: color.withValues(alpha: 0.5)),
    );
  }
}
