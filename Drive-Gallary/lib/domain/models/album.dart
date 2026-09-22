import 'enums.dart';

/// Immutable domain representation of an album/folder (section 8.2).
class Album {
  const Album({
    required this.id,
    this.parentAlbumId,
    required this.name,
    required this.type,
    required this.ownerUserId,
    this.driveFolderId,
    this.driveParentFolderId,
    required this.isDriveLinked,
    required this.autoSyncEnabled,
    required this.createdAt,
    required this.updatedAt,
    this.deletedAt,
  });

  final String id;
  final String? parentAlbumId;
  final String name;
  final AlbumType type;
  final String ownerUserId;
  final String? driveFolderId;
  final String? driveParentFolderId;
  final bool isDriveLinked;
  final bool autoSyncEnabled;
  final DateTime createdAt;
  final DateTime updatedAt;
  final DateTime? deletedAt;

  Album copyWith({
    String? name,
    String? parentAlbumId,
    String? driveFolderId,
    String? driveParentFolderId,
    bool? isDriveLinked,
    bool? autoSyncEnabled,
    DateTime? updatedAt,
    DateTime? deletedAt,
  }) {
    return Album(
      id: id,
      parentAlbumId: parentAlbumId ?? this.parentAlbumId,
      name: name ?? this.name,
      type: type,
      ownerUserId: ownerUserId,
      driveFolderId: driveFolderId ?? this.driveFolderId,
      driveParentFolderId: driveParentFolderId ?? this.driveParentFolderId,
      isDriveLinked: isDriveLinked ?? this.isDriveLinked,
      autoSyncEnabled: autoSyncEnabled ?? this.autoSyncEnabled,
      createdAt: createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
      deletedAt: deletedAt ?? this.deletedAt,
    );
  }
}
