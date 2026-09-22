import 'enums.dart';

/// Immutable domain representation of a media item (section 9).
class MediaItem {
  const MediaItem({
    required this.id,
    this.localMediaStoreId,
    required this.albumId,
    required this.localUri,
    required this.fileName,
    required this.mimeType,
    required this.sizeBytes,
    this.width,
    this.height,
    this.capturedAt,
    this.modifiedAt,
    this.contentHash,
    this.category,
    this.sequenceNumber,
    this.driveFileId,
    required this.syncStatus,
    required this.createdAt,
    required this.updatedAt,
  });

  final String id;
  final String? localMediaStoreId;
  final String albumId;
  final String localUri;
  final String fileName;
  final String mimeType;
  final int sizeBytes;
  final int? width;
  final int? height;
  final DateTime? capturedAt;
  final DateTime? modifiedAt;
  final String? contentHash;
  final String? category;
  final int? sequenceNumber;
  final String? driveFileId;
  final SyncStatus syncStatus;
  final DateTime createdAt;
  final DateTime updatedAt;

  bool get isVideo => mimeType.startsWith('video/');

  MediaItem copyWith({
    String? albumId,
    String? fileName,
    String? contentHash,
    String? category,
    int? sequenceNumber,
    String? driveFileId,
    SyncStatus? syncStatus,
    DateTime? updatedAt,
  }) {
    return MediaItem(
      id: id,
      localMediaStoreId: localMediaStoreId,
      albumId: albumId ?? this.albumId,
      localUri: localUri,
      fileName: fileName ?? this.fileName,
      mimeType: mimeType,
      sizeBytes: sizeBytes,
      width: width,
      height: height,
      capturedAt: capturedAt,
      modifiedAt: modifiedAt,
      contentHash: contentHash ?? this.contentHash,
      category: category ?? this.category,
      sequenceNumber: sequenceNumber ?? this.sequenceNumber,
      driveFileId: driveFileId ?? this.driveFileId,
      syncStatus: syncStatus ?? this.syncStatus,
      createdAt: createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }
}
