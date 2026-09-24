/// Minimal, transport-agnostic representations of Google Drive resources.
/// The UI and services depend on these, never on googleapis types directly.
class DriveFolder {
  const DriveFolder({required this.id, required this.name, this.parentId});

  final String id;
  final String name;
  final String? parentId;
}

class DrivePermission {
  const DrivePermission({required this.id, this.email, this.role});

  final String id;
  final String? email;
  final String? role;
}

class DriveFile {
  const DriveFile({
    required this.id,
    required this.name,
    this.mimeType,
    this.size,
    this.md5Checksum,
    this.modifiedTime,
    this.parentId,
  });

  final String id;
  final String name;
  final String? mimeType;
  final int? size;
  final String? md5Checksum;
  final DateTime? modifiedTime;
  final String? parentId;
}
