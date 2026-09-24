/// Generates collision-free sequential file names for bulk rename (spec §26).
///
/// Rules: preserve the original extension, zero-pad the number, and keep names
/// unique. Ordering is decided by the caller (capture time by default, or
/// selection order).
class BulkRenameService {
  const BulkRenameService();

  /// Produces one new name per entry in [originalNames], of the form
  /// `Category_001.ext`. [startNumber] is the first index (default 1) and
  /// [padWidth] the minimum digit count (default 3).
  List<String> generateNames({
    required List<String> originalNames,
    required String category,
    int startNumber = 1,
    int padWidth = 3,
  }) {
    final prefix = _sanitize(category);
    final used = <String>{};
    final result = <String>[];
    var n = startNumber;
    for (final original in originalNames) {
      final ext = _extension(original);
      String candidate;
      do {
        final number = n.toString().padLeft(padWidth, '0');
        candidate = ext.isEmpty
            ? '${prefix}_$number'
            : '${prefix}_$number.$ext';
        n++;
      } while (used.contains(candidate));
      used.add(candidate);
      result.add(candidate);
    }
    return result;
  }

  String _extension(String fileName) {
    final dot = fileName.lastIndexOf('.');
    if (dot <= 0 || dot == fileName.length - 1) return '';
    return fileName.substring(dot + 1);
  }

  String _sanitize(String category) {
    final trimmed = category.trim();
    if (trimmed.isEmpty) return 'File';
    // Replace path/space-unfriendly characters.
    return trimmed.replaceAll(RegExp(r'[^A-Za-z0-9_\-]+'), '_');
  }
}
