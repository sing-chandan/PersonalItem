import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/providers.dart';
import '../../core/errors/app_error.dart';
import '../../domain/models/album_member.dart';
import '../../domain/models/enums.dart';

/// Members / Permissions screen for an album (spec §29–31).
class MembersScreen extends ConsumerWidget {
  const MembersScreen({super.key, required this.albumId});

  final String albumId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final albumAsync = ref.watch(albumByIdProvider(albumId));
    final membersAsync = ref.watch(albumMembersProvider(albumId));

    return Scaffold(
      appBar: AppBar(title: const Text('Members & Sharing')),
      body: albumAsync.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(child: Text('$e')),
        data: (album) {
          if (album == null) {
            return const Center(child: Text('Album not found.'));
          }
          return membersAsync.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (e, _) => Center(child: Text('$e')),
            data: (members) {
              return ListView(
                children: [
                  if (!album.isDriveLinked)
                    const Padding(
                      padding: EdgeInsets.all(16),
                      child: Text(
                        'Link this album to Drive before sharing.',
                        style: TextStyle(color: Colors.orange),
                      ),
                    ),
                  if (members.isEmpty)
                    const Padding(
                      padding: EdgeInsets.all(24),
                      child: Text(
                        'No members yet. Share to add collaborators.',
                      ),
                    ),
                  for (final m in members)
                    ListTile(
                      leading: const CircleAvatar(child: Icon(Icons.person)),
                      title: Text(m.email),
                      subtitle: Text(_roleLabel(m.role)),
                      trailing: PopupMenuButton<String>(
                        onSelected: (v) => _onMemberAction(context, ref, m, v),
                        itemBuilder: (context) => [
                          const PopupMenuItem(
                            value: 'role',
                            child: Text('Change role'),
                          ),
                          const PopupMenuItem(
                            value: 'remove',
                            child: Text('Remove'),
                          ),
                        ],
                      ),
                    ),
                ],
              );
            },
          );
        },
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _share(context, ref),
        icon: const Icon(Icons.person_add),
        label: const Text('Share'),
      ),
    );
  }

  String _roleLabel(MemberRole role) => switch (role) {
    MemberRole.owner => 'Owner',
    MemberRole.editor => 'Editor',
    MemberRole.contributor => 'Contributor',
    MemberRole.viewer => 'Viewer',
  };

  Future<void> _share(BuildContext context, WidgetRef ref) async {
    final album = await ref.read(albumByIdProvider(albumId).future);
    if (album == null || !context.mounted) return;
    final result = await showDialog<(String, MemberRole)>(
      context: context,
      builder: (context) => const _ShareDialog(),
    );
    if (result == null || !context.mounted) return;
    final messenger = ScaffoldMessenger.of(context);
    try {
      await ref
          .read(sharingServiceProvider)
          .shareAlbum(album, result.$1, result.$2);
      messenger.showSnackBar(
        SnackBar(content: Text('Shared with ${result.$1}.')),
      );
    } on AppError catch (e) {
      messenger.showSnackBar(SnackBar(content: Text(e.message)));
    }
  }

  Future<void> _onMemberAction(
    BuildContext context,
    WidgetRef ref,
    AlbumMember member,
    String action,
  ) async {
    final album = await ref.read(albumByIdProvider(albumId).future);
    if (album == null || !context.mounted) return;
    final messenger = ScaffoldMessenger.of(context);
    if (action == 'remove') {
      await ref.read(sharingServiceProvider).removeMember(album, member);
      messenger.showSnackBar(
        SnackBar(content: Text('Removed ${member.email}.')),
      );
    } else if (action == 'role') {
      final role = await showDialog<MemberRole>(
        context: context,
        builder: (context) => _RolePickerDialog(current: member.role),
      );
      if (role == null) return;
      await ref.read(sharingServiceProvider).changeRole(album, member, role);
    }
  }
}

class _ShareDialog extends StatefulWidget {
  const _ShareDialog();

  @override
  State<_ShareDialog> createState() => _ShareDialogState();
}

class _ShareDialogState extends State<_ShareDialog> {
  final _email = TextEditingController();
  MemberRole _role = MemberRole.contributor;

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Share album'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextField(
            controller: _email,
            keyboardType: TextInputType.emailAddress,
            decoration: const InputDecoration(
              labelText: 'Google account email',
              hintText: 'name@gmail.com',
            ),
          ),
          const SizedBox(height: 12),
          DropdownButtonFormField<MemberRole>(
            initialValue: _role,
            decoration: const InputDecoration(labelText: 'Role'),
            items: const [
              DropdownMenuItem(value: MemberRole.viewer, child: Text('Viewer')),
              DropdownMenuItem(
                value: MemberRole.contributor,
                child: Text('Contributor'),
              ),
              DropdownMenuItem(value: MemberRole.editor, child: Text('Editor')),
            ],
            onChanged: (v) => setState(() => _role = v ?? _role),
          ),
        ],
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Cancel'),
        ),
        FilledButton(
          onPressed: () => Navigator.pop(context, (_email.text.trim(), _role)),
          child: const Text('Share'),
        ),
      ],
    );
  }
}

class _RolePickerDialog extends StatelessWidget {
  const _RolePickerDialog({required this.current});
  final MemberRole current;

  @override
  Widget build(BuildContext context) {
    return SimpleDialog(
      title: const Text('Change role'),
      children: [
        for (final role in const [
          MemberRole.viewer,
          MemberRole.contributor,
          MemberRole.editor,
        ])
          ListTile(
            leading: Icon(
              role == current
                  ? Icons.radio_button_checked
                  : Icons.radio_button_unchecked,
            ),
            title: Text(role.name),
            onTap: () => Navigator.pop(context, role),
          ),
      ],
    );
  }
}
