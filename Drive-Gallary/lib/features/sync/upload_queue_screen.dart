import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/providers.dart';
import '../../domain/models/enums.dart';
import '../../domain/models/sync_job.dart';

/// Upload Queue (spec §36): shows active/failed jobs and queue actions. Note
/// that clearing completed history never deletes Drive files.
class UploadQueueScreen extends ConsumerWidget {
  const UploadQueueScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final jobsAsync = ref.watch(activeJobsProvider);
    final controller = ref.read(syncControllerProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Upload Queue'),
        actions: [
          PopupMenuButton<String>(
            onSelected: (v) async {
              switch (v) {
                case 'retry':
                  await controller.retryFailed();
                case 'pause':
                  await controller.pause();
                case 'resume':
                  await controller.resume();
                case 'clear':
                  await controller.clearCompleted();
              }
            },
            itemBuilder: (context) => const [
              PopupMenuItem(value: 'retry', child: Text('Retry failed')),
              PopupMenuItem(value: 'pause', child: Text('Pause')),
              PopupMenuItem(value: 'resume', child: Text('Resume')),
              PopupMenuItem(value: 'clear', child: Text('Clear completed')),
            ],
          ),
        ],
      ),
      body: jobsAsync.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(child: Text('Could not load queue.\n$e')),
        data: (jobs) {
          if (jobs.isEmpty) {
            return const Center(
              child: Text('No pending uploads. All caught up.'),
            );
          }
          return ListView.separated(
            itemCount: jobs.length,
            separatorBuilder: (_, _) => const Divider(height: 1),
            itemBuilder: (context, i) => _JobTile(job: jobs[i]),
          );
        },
      ),
    );
  }
}

class _JobTile extends ConsumerWidget {
  const _JobTile({required this.job});
  final SyncJob job;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final mediaAsync = job.mediaItemId == null
        ? null
        : ref.watch(mediaByIdProvider(job.mediaItemId!));
    final name = mediaAsync?.value?.fileName ?? job.type.name;

    final (label, icon, color) = switch (job.status) {
      SyncJobStatus.queued => ('Waiting', Icons.schedule, Colors.orange),
      SyncJobStatus.running => ('Uploading', Icons.cloud_upload, Colors.blue),
      SyncJobStatus.paused => ('Paused', Icons.pause_circle, Colors.grey),
      SyncJobStatus.succeeded => ('Synced', Icons.cloud_done, Colors.green),
      SyncJobStatus.failed => ('Failed', Icons.error_outline, Colors.red),
      SyncJobStatus.cancelled => ('Cancelled', Icons.cancel, Colors.grey),
    };

    return ListTile(
      leading: Icon(icon, color: color),
      title: Text(name, overflow: TextOverflow.ellipsis),
      subtitle: Text(
        job.status == SyncJobStatus.failed && job.lastError != null
            ? '$label · attempt ${job.attemptCount} · ${job.lastError}'
            : label,
      ),
      trailing: job.status == SyncJobStatus.running
          ? const SizedBox(
              width: 20,
              height: 20,
              child: CircularProgressIndicator(strokeWidth: 2),
            )
          : null,
    );
  }
}
