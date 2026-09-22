/// A media item selected from the device (picker, camera, or MediaStore),
/// before it is persisted into an album.
class PickedMedia {
  const PickedMedia({
    required this.localUri,
    required this.fileName,
    required this.mimeType,
    required this.sizeBytes,
    this.localMediaStoreId,
    this.width,
    this.height,
    this.capturedAt,
    this.modifiedAt,
  });

  final String localUri;
  final String fileName;
  final String mimeType;
  final int sizeBytes;
  final String? localMediaStoreId;
  final int? width;
  final int? height;
  final DateTime? capturedAt;
  final DateTime? modifiedAt;
}
