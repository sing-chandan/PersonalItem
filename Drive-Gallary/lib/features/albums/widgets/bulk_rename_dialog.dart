import 'package:flutter/material.dart';

/// Options chosen in the bulk-rename dialog (spec §26).
class BulkRenameOptions {
  const BulkRenameOptions(this.category, this.startNumber);
  final String category;
  final int startNumber;
}

/// Prompts for a category and starting number for bulk rename.
Future<BulkRenameOptions?> promptBulkRename(
  BuildContext context, {
  required List<String> categories,
  required int photoCount,
}) {
  String category = categories.isNotEmpty ? categories.first : 'File';
  final startController = TextEditingController(text: '1');

  return showDialog<BulkRenameOptions>(
    context: context,
    builder: (context) {
      return StatefulBuilder(
        builder: (context, setState) {
          final start = int.tryParse(startController.text) ?? 1;
          final example = '${category}_${start.toString().padLeft(3, '0')}.jpg';
          return AlertDialog(
            title: const Text('Bulk rename'),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Renames $photoCount photo(s), ordered by capture time.'),
                const SizedBox(height: 12),
                DropdownButtonFormField<String>(
                  initialValue: category,
                  decoration: const InputDecoration(labelText: 'Category'),
                  items: [
                    for (final c in categories)
                      DropdownMenuItem(value: c, child: Text(c)),
                  ],
                  onChanged: (v) => setState(() => category = v ?? category),
                ),
                const SizedBox(height: 8),
                TextField(
                  controller: startController,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    labelText: 'Starting number',
                  ),
                  onChanged: (_) => setState(() {}),
                ),
                const SizedBox(height: 12),
                Text(
                  'Preview: $example',
                  style: const TextStyle(fontStyle: FontStyle.italic),
                ),
              ],
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('Cancel'),
              ),
              FilledButton(
                onPressed: () =>
                    Navigator.of(context)
                        .pop(BulkRenameOptions(category, start)),
                child: const Text('Rename'),
              ),
            ],
          );
        },
      );
    },
  );
}
