import 'dart:typed_data';

import '../models/drive_models.dart';
import '../models/enums.dart';

/// Abstraction over Google Drive (spec §19). The UI must never call the Drive
/// API directly; everything goes through this interface so the app is not
/// coupled to a specific Drive client package.
abstract class DriveRepository {
  /// Creates a folder under [parentId] (or Drive root when null).
  Future<DriveFolder> createFolder(String name, {String? parentId});

  Future<DriveFolder?> getFolderById(String id);

  /// Finds a folder by exact name under a parent. Used only for initial root
  /// discovery, never as a permanent identifier.
  Future<DriveFolder?> findFolder(String name, {String? parentId});

  Future<void> renameFolder(String id, String newName);

  Future<void> deleteFolder(String id);

  /// Uploads [bytes] as a file into [parentId]. Returns the created file.
  Future<DriveFile> uploadFile({
    required String parentId,
    required String name,
    required String mimeType,
    required Uint8List bytes,
    void Function(int sent, int total)? onProgress,
  });

  Future<DriveFile?> getFileById(String id);

  Future<void> renameFile(String id, String newName);

  Future<List<DriveFile>> listChildren(String parentId);

  Future<Uint8List> downloadFile(String id);

  /// Grants [email] access to [fileId] with [role]. Returns the permission id.
  Future<String> createPermission(String fileId, String email, MemberRole role);

  Future<List<DrivePermission>> listPermissions(String fileId);

  Future<void> deletePermission(String fileId, String permissionId);
}
