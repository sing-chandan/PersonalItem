import 'dart:typed_data';

import 'package:googleapis/drive/v3.dart' as drive;
import 'package:http/http.dart' as http;

import '../../core/constants/app_constants.dart';
import '../../core/errors/app_error.dart';
import '../../domain/models/drive_models.dart';
import '../../domain/models/enums.dart';
import '../../domain/repositories/drive_repository.dart';
import '../../domain/services/auth_service.dart';

/// [DriveRepository] backed by Google Drive API v3 (googleapis). Obtains an
/// authorized client from [AuthService] per operation so token refresh is
/// handled transparently.
class DriveRepositoryImpl implements DriveRepository {
  DriveRepositoryImpl(this._auth);

  final AuthService _auth;

  static const _fileFields =
      'id,name,mimeType,size,md5Checksum,modifiedTime,parents';

  Future<drive.DriveApi> _api() async {
    final client = await _auth.authorizedClient();
    if (client == null) {
      throw AppError.authRequired('Not connected to Google Drive.');
    }
    return drive.DriveApi(client);
  }

  DriveFolder _toFolder(drive.File f) => DriveFolder(
    id: f.id!,
    name: f.name ?? '',
    parentId: (f.parents != null && f.parents!.isNotEmpty)
        ? f.parents!.first
        : null,
  );

  DriveFile _toFile(drive.File f) => DriveFile(
    id: f.id!,
    name: f.name ?? '',
    mimeType: f.mimeType,
    size: f.size == null ? null : int.tryParse(f.size!),
    md5Checksum: f.md5Checksum,
    modifiedTime: f.modifiedTime,
    parentId: (f.parents != null && f.parents!.isNotEmpty)
        ? f.parents!.first
        : null,
  );

  @override
  Future<DriveFolder> createFolder(String name, {String? parentId}) async {
    final api = await _api();
    final file = drive.File()
      ..name = name
      ..mimeType = AppConstants.driveFolderMimeType
      ..parents = parentId == null ? null : [parentId];
    final created = await api.files.create(file, $fields: _fileFields);
    return _toFolder(created);
  }

  @override
  Future<DriveFolder?> getFolderById(String id) async {
    final api = await _api();
    try {
      final f = await api.files.get(id, $fields: _fileFields) as drive.File;
      return _toFolder(f);
    } on drive.DetailedApiRequestError catch (e) {
      if (e.status == 404) return null;
      rethrow;
    }
  }

  @override
  Future<DriveFolder?> findFolder(String name, {String? parentId}) async {
    final api = await _api();
    final safeName = name.replaceAll("'", r"\'");
    final buffer = StringBuffer()
      ..write("mimeType='${AppConstants.driveFolderMimeType}'")
      ..write(" and trashed=false")
      ..write(" and name='$safeName'");
    if (parentId != null) buffer.write(" and '$parentId' in parents");
    final result = await api.files.list(
      q: buffer.toString(),
      $fields: 'files($_fileFields)',
      spaces: 'drive',
    );
    final files = result.files ?? const [];
    return files.isEmpty ? null : _toFolder(files.first);
  }

  @override
  Future<void> renameFolder(String id, String newName) async {
    final api = await _api();
    await api.files.update(drive.File()..name = newName, id);
  }

  @override
  Future<void> deleteFolder(String id) async {
    final api = await _api();
    await api.files.delete(id);
  }

  @override
  Future<DriveFile> uploadFile({
    required String parentId,
    required String name,
    required String mimeType,
    required Uint8List bytes,
    void Function(int sent, int total)? onProgress,
  }) async {
    final api = await _api();
    final media = drive.Media(
      Stream<List<int>>.value(bytes),
      bytes.length,
      contentType: mimeType,
    );
    final file = drive.File()
      ..name = name
      ..parents = [parentId];
    final created = await api.files.create(
      file,
      uploadMedia: media,
      $fields: _fileFields,
    );
    onProgress?.call(bytes.length, bytes.length);
    return _toFile(created);
  }

  @override
  Future<DriveFile?> getFileById(String id) async {
    final api = await _api();
    try {
      final f = await api.files.get(id, $fields: _fileFields) as drive.File;
      return _toFile(f);
    } on drive.DetailedApiRequestError catch (e) {
      if (e.status == 404) return null;
      rethrow;
    }
  }

  @override
  Future<void> renameFile(String id, String newName) async {
    final api = await _api();
    await api.files.update(drive.File()..name = newName, id);
  }

  @override
  Future<List<DriveFile>> listChildren(String parentId) async {
    final api = await _api();
    final files = <DriveFile>[];
    String? pageToken;
    do {
      final result = await api.files.list(
        q: "'$parentId' in parents and trashed=false",
        $fields: 'nextPageToken,files($_fileFields)',
        spaces: 'drive',
        pageToken: pageToken,
        pageSize: 200,
      );
      files.addAll((result.files ?? const []).map(_toFile));
      pageToken = result.nextPageToken;
    } while (pageToken != null);
    return files;
  }

  @override
  Future<Uint8List> downloadFile(String id) async {
    final api = await _api();
    final media = await api.files.get(
      id,
      downloadOptions: drive.DownloadOptions.fullMedia,
    ) as drive.Media;
    final builder = BytesBuilder(copy: false);
    await for (final chunk in media.stream) {
      builder.add(chunk);
    }
    return builder.takeBytes();
  }

  @override
  Future<void> createPermission(
    String fileId,
    String email,
    MemberRole role,
  ) async {
    final api = await _api();
    final permission = drive.Permission()
      ..type = 'user'
      ..role = _driveRole(role)
      ..emailAddress = email;
    await api.permissions.create(
      permission,
      fileId,
      sendNotificationEmail: true,
    );
  }

  @override
  Future<void> deletePermission(String fileId, String permissionId) async {
    final api = await _api();
    await api.permissions.delete(fileId, permissionId);
  }

  String _driveRole(MemberRole role) => switch (role) {
    MemberRole.owner => 'writer',
    MemberRole.editor => 'writer',
    MemberRole.contributor => 'writer',
    MemberRole.viewer => 'reader',
  };
}

/// Convenience so callers can pass a plain client for testing.
typedef DriveClientFactory = Future<http.Client?> Function();
