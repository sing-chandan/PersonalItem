import 'dart:typed_data';

/// Reads the raw bytes of a local media item for upload. Implementations adapt
/// to the platform (native file path vs. web blob/object URL).
abstract class MediaBytesReader {
  Future<Uint8List> read(String localUri);
}
