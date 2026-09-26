import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/providers.dart';
import '../../core/errors/app_error.dart';
import '../../data/repositories/settings_repository.dart';
import 'google_sign_in_button.dart';

/// Settings: Google account connection and Drive root setup (spec §37, §17, §18).
class SettingsScreen extends ConsumerWidget {
  const SettingsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authState = ref.watch(authStateProvider);
    final auth = ref.watch(authServiceProvider);
    final rootName = ref.watch(
      settingWatchProvider(SettingsRepository.kDriveRootFolderName),
    );

    return Scaffold(
      appBar: AppBar(title: const Text('Settings')),
      body: ListView(
        children: [
          const _SectionHeader('Google Account'),
          authState.when(
            loading: () => const ListTile(
              leading: CircularProgressIndicator(),
              title: Text('Checking sign-in...'),
            ),
            error: (e, _) => ListTile(
              leading: const Icon(Icons.error_outline),
              title: const Text('Sign-in unavailable'),
              subtitle: Text('$e'),
            ),
            data: (user) {
              if (user == null) {
                return Column(
                  children: [
                    const ListTile(
                      leading: Icon(Icons.account_circle_outlined),
                      title: Text('Not connected'),
                      subtitle: Text(
                        'Connect Google to enable Drive backup and sync.',
                      ),
                    ),
                    if (auth.supportsInteractiveSignIn)
                      Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        child: FilledButton.icon(
                          onPressed: () => _connect(context, ref),
                          icon: const Icon(Icons.login),
                          label: const Text('Connect Google Account'),
                        ),
                      )
                    else
                      // Web: use Google's official rendered sign-in button.
                      Padding(
                        padding: const EdgeInsets.all(16),
                        child: Align(
                          alignment: Alignment.centerLeft,
                          child: googleRenderedSignInButton(),
                        ),
                      ),
                  ],
                );
              }
              return Column(
                children: [
                  ListTile(
                    leading: user.photoUrl != null
                        ? CircleAvatar(
                            backgroundImage: NetworkImage(user.photoUrl!),
                          )
                        : const CircleAvatar(child: Icon(Icons.person)),
                    title: Text(user.displayName),
                    subtitle: Text(user.email),
                  ),
                  OverflowBar(
                    alignment: MainAxisAlignment.center,
                    children: [
                      TextButton.icon(
                        onPressed: () =>
                            ref.read(authServiceProvider).signOut(),
                        icon: const Icon(Icons.logout),
                        label: const Text('Sign out'),
                      ),
                      TextButton.icon(
                        onPressed: () =>
                            ref.read(authServiceProvider).disconnect(),
                        icon: const Icon(Icons.link_off),
                        label: const Text('Disconnect'),
                      ),
                    ],
                  ),
                ],
              );
            },
          ),
          const Divider(),
          const _SectionHeader('Google Drive'),
          rootName.when(
            loading: () => const SizedBox.shrink(),
            error: (_, _) => const SizedBox.shrink(),
            data: (name) => ListTile(
              leading: const Icon(Icons.cloud_outlined),
              title: Text(
                name == null ? 'Root folder not set up' : 'Root: $name',
              ),
              subtitle: Text(
                name == null
                    ? 'Create or link the top-level "Gallery Cloud" folder.'
                    : 'New albums are created inside this folder.',
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: FilledButton.icon(
              onPressed: authState.value == null
                  ? null
                  : () => _setupRoot(context, ref),
              icon: const Icon(Icons.create_new_folder),
              label: const Text('Set up Drive root'),
            ),
          ),
          const SizedBox(height: 8),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: OutlinedButton.icon(
              onPressed: authState.value == null
                  ? null
                  : () => _importDrive(context, ref),
              icon: const Icon(Icons.download_for_offline_outlined),
              label: const Text('Import folders & files from Drive'),
            ),
          ),
          const Padding(
            padding: EdgeInsets.fromLTRB(16, 4, 16, 0),
            child: Text(
              'Mirrors your existing Drive folder structure into the app as '
              'albums (files load on demand).',
              style: TextStyle(color: Colors.grey, fontSize: 12),
            ),
          ),
          const SizedBox(height: 24),
        ],
      ),
    );
  }

  Future<void> _importDrive(BuildContext context, WidgetRef ref) async {
    final messenger = ScaffoldMessenger.of(context);
    messenger.showSnackBar(
      const SnackBar(content: Text('Importing from Drive...')),
    );
    try {
      final result = await ref
          .read(driveMirrorServiceProvider)
          .importTree(ownerUserId: ref.read(currentOwnerIdProvider));
      ref.invalidate(childAlbumsProvider);
      ref.invalidate(allAlbumsProvider);
      messenger.showSnackBar(
        SnackBar(
          content: Text(
            'Imported ${result.albumsCreated} folder(s) and '
            '${result.filesImported} file(s) from Drive.',
          ),
        ),
      );
    } on AppError catch (e) {
      messenger.showSnackBar(SnackBar(content: Text(e.message)));
    } catch (e) {
      messenger.showSnackBar(
        const SnackBar(content: Text('Could not import from Drive.')),
      );
    }
  }

  Future<void> _connect(BuildContext context, WidgetRef ref) async {
    try {
      await ref.read(authServiceProvider).signIn();
    } on AppError catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(e.message)));
      }
    }
  }

  Future<void> _setupRoot(BuildContext context, WidgetRef ref) async {
    final messenger = ScaffoldMessenger.of(context);
    try {
      final folder = await ref.read(driveRootServiceProvider).ensureRoot();
      ref.invalidate(settingWatchProvider);
      messenger.showSnackBar(
        SnackBar(content: Text('Drive root ready: ${folder.name}')),
      );
    } on AppError catch (e) {
      messenger.showSnackBar(SnackBar(content: Text(e.message)));
    } catch (e) {
      messenger.showSnackBar(
        const SnackBar(content: Text('Could not set up Drive root.')),
      );
    }
  }
}

class _SectionHeader extends StatelessWidget {
  const _SectionHeader(this.title);
  final String title;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 4),
      child: Text(
        title,
        style: TextStyle(
          fontWeight: FontWeight.bold,
          color: Theme.of(context).colorScheme.primary,
        ),
      ),
    );
  }
}
