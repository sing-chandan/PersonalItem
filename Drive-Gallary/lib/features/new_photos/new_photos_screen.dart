import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/providers.dart';
import '../../domain/models/album.dart';
import '../../domain/models/picked_media.dart';
import '../../widgets/picked_thumbnail.dart';

/// New Photos — the father's core workflow (spec §14, §48):
/// pick photos → choose linked album → (optional) category + bulk rename →
/// Add & Upload → background sync.
class NewPhotosScreen extends ConsumerStatefulWidget {
  const NewPhotosScreen({super.key, this.initialAlbumId});

  final String? initialAlbumId;

  @override
  ConsumerState<NewPhotosScreen> createState() => _NewPhotosScreenState();
}

class _NewPhotosScreenState extends ConsumerState<NewPhotosScreen> {
  final List<PickedMedia> _picked = [];
  final Set<int> _selected = {};
  String? _albumId;
  String? _category;
  bool _rename = false;
  int _startNumber = 1;
  bool _busy = false;

  @override
  void initState() {
    super.initState();
    _albumId = widget.initialAlbumId;
    WidgetsBinding.instance.addPostFrameCallback((_) => _pick());
  }

  Future<void> _pick() async {
    final picked = await ref.read(mediaSourceProvider).pickImages();
    if (picked.isEmpty) return;
    setState(() {
      final base = _picked.length;
      _picked.addAll(picked);
      for (var i = 0; i < picked.length; i++) {
        _selected.add(base + i);
      }
    });
  }

  List<PickedMedia> get _selectedMedia =>
      _selected.map((i) => _picked[i]).toList();

  @override
  Widget build(BuildContext context) {
    final albumsAsync = ref.watch(allAlbumsProvider);
    final categories = ref.watch(categoriesProvider);
    final canSubmit = _albumId != null && _selected.isNotEmpty && !_busy;

    return Scaffold(
      appBar: AppBar(
        title: const Text('New Photos'),
        actions: [
          TextButton.icon(
            onPressed: _busy ? null : _pick,
            icon: const Icon(Icons.add),
            label: const Text('Add more'),
          ),
        ],
      ),
      body: _picked.isEmpty
          ? const Center(child: Text('No photos selected yet.'))
          : Column(
              children: [
                Expanded(child: _grid()),
                _optionsPanel(albumsAsync, categories),
              ],
            ),
      bottomNavigationBar: Padding(
        padding: EdgeInsets.only(
          left: 16,
          right: 16,
          top: 8,
          bottom: MediaQuery.of(context).padding.bottom + 12,
        ),
        child: FilledButton.icon(
          onPressed: canSubmit ? _addAndUpload : null,
          icon: _busy
              ? const SizedBox(
                  width: 18,
                  height: 18,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : const Icon(Icons.cloud_upload),
          label: Text(
            _busy ? 'Adding...' : 'Add & Upload (${_selected.length})',
          ),
        ),
      ),
    );
  }

  Widget _grid() {
    return GridView.builder(
      padding: const EdgeInsets.all(8),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 3,
        crossAxisSpacing: 6,
        mainAxisSpacing: 6,
      ),
      itemCount: _picked.length,
      itemBuilder: (context, i) {
        final isSel = _selected.contains(i);
        return GestureDetector(
          onTap: () => setState(() {
            if (isSel) {
              _selected.remove(i);
            } else {
              _selected.add(i);
            }
          }),
          child: Stack(
            fit: StackFit.expand,
            children: [
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: PickedThumbnail(media: _picked[i]),
              ),
              Positioned(
                right: 4,
                top: 4,
                child: Icon(
                  isSel ? Icons.check_circle : Icons.circle_outlined,
                  color: isSel ? Colors.lightBlueAccent : Colors.white70,
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _optionsPanel(
    AsyncValue<List<Album>> albumsAsync,
    List<String> categories,
  ) {
    return Material(
      elevation: 8,
      child: Padding(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Add to album', style: Theme.of(context).textTheme.labelLarge),
            albumsAsync.when(
              loading: () => const LinearProgressIndicator(),
              error: (e, _) => Text('Could not load albums: $e'),
              data: (albums) => _albumDropdown(albums),
            ),
            const SizedBox(height: 8),
            Wrap(
              spacing: 8,
              crossAxisAlignment: WrapCrossAlignment.center,
              children: [
                const Text('Category:'),
                ChoiceChip(
                  label: const Text('None'),
                  selected: _category == null,
                  onSelected: (_) => setState(() => _category = null),
                ),
                for (final c in categories)
                  ChoiceChip(
                    label: Text(c),
                    selected: _category == c,
                    onSelected: (_) => setState(() => _category = c),
                  ),
              ],
            ),
            SwitchListTile(
              contentPadding: EdgeInsets.zero,
              title: const Text('Bulk rename'),
              subtitle: Text(
                _rename && _category != null
                    ? _renamePreview()
                    : 'Rename selected as Category_001, 002, ...',
              ),
              value: _rename,
              onChanged: _category == null
                  ? null
                  : (v) => setState(() => _rename = v),
            ),
            if (_rename && _category != null)
              Row(
                children: [
                  const Text('Start #: '),
                  SizedBox(
                    width: 80,
                    child: TextFormField(
                      initialValue: '$_startNumber',
                      keyboardType: TextInputType.number,
                      onChanged: (v) =>
                          setState(() => _startNumber = int.tryParse(v) ?? 1),
                    ),
                  ),
                ],
              ),
          ],
        ),
      ),
    );
  }

  Widget _albumDropdown(List<Album> albums) {
    if (albums.isEmpty) {
      return const Padding(
        padding: EdgeInsets.symmetric(vertical: 8),
        child: Text('No albums yet. Create one from Home first.'),
      );
    }
    final byId = {for (final a in albums) a.id: a};
    String pathOf(Album a) {
      final parts = <String>[a.name];
      var cur = a;
      while (cur.parentAlbumId != null && byId[cur.parentAlbumId!] != null) {
        cur = byId[cur.parentAlbumId!]!;
        parts.insert(0, cur.name);
      }
      return parts.join(' / ');
    }

    return DropdownButton<String>(
      isExpanded: true,
      value: _albumId,
      hint: const Text('Choose an album'),
      items: [
        for (final a in albums)
          DropdownMenuItem(
            value: a.id,
            child: Text(
              '${pathOf(a)}${a.isDriveLinked ? '' : '  (not linked)'}',
              overflow: TextOverflow.ellipsis,
            ),
          ),
      ],
      onChanged: (v) => setState(() => _albumId = v),
    );
  }

  String _renamePreview() {
    final names = ref
        .read(bulkRenameServiceProvider)
        .generateNames(
          originalNames: _selectedMedia.map((m) => m.fileName).toList(),
          category: _category!,
          startNumber: _startNumber,
        );
    if (names.isEmpty) return '';
    final head = names.take(3).join(', ');
    return names.length > 3 ? '$head ...' : head;
  }

  Future<void> _addAndUpload() async {
    final albumId = _albumId!;
    final media = _selectedMedia;
    setState(() => _busy = true);
    final messenger = ScaffoldMessenger.of(context);
    final navigator = Navigator.of(context);
    try {
      List<String>? names;
      if (_rename && _category != null) {
        names = ref
            .read(bulkRenameServiceProvider)
            .generateNames(
              originalNames: media.map((m) => m.fileName).toList(),
              category: _category!,
              startNumber: _startNumber,
            );
      }
      final added = await ref
          .read(mediaServiceProvider)
          .addToAlbum(albumId, media, category: _category, newNames: names);
      await ref.read(syncControllerProvider).enqueueUploads(added);
      messenger.showSnackBar(
        SnackBar(
          content: Text(
            'Added ${added.length} photo(s). Uploading in the '
            'background.',
          ),
        ),
      );
      navigator.pop();
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }
}
