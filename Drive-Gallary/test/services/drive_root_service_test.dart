import 'dart:typed_data';

import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/repositories/settings_repository.dart';
import 'package:drive_linked_gallery/domain/models/drive_models.dart';
import 'package:drive_linked_gallery/domain/models/enums.dart';
import 'package:drive_linked_gallery/domain/repositories/drive_repository.dart';
import 'package:drive_linked_gallery/domain/services/drive_root_service.dart';
import 'package:flutter_test/flutter_test.dart';

/// In-memory fake Drive used to verify root-folder logic without network.
class FakeDriveRepository implements DriveRepository {
  final Map<String, DriveFolder> folders = {};
  int _seq = 0;
  int createCalls = 0;

  @override
  Future<DriveFolder> createFolder(String name, {String? parentId}) async {
    createCalls++;
    final folder =
        DriveFolder(id: 'id${_seq++}', name: name, parentId: parentId);
    folders[folder.id] = folder;
    return folder;
  }

  @override
  Future<DriveFolder?> getFolderById(String id) async => folders[id];

  @override
  Future<DriveFolder?> findFolder(String name, {String? parentId}) async {
    for (final f in folders.values) {
      if (f.name == name && f.parentId == parentId) return f;
    }
    return null;
  }

  @override
  Future<void> renameFolder(String id, String newName) async {}
  @override
  Future<void> deleteFolder(String id) async {}
  @override
  Future<DriveFile> uploadFile({
    required String parentId,
    required String name,
    required String mimeType,
    required Uint8List bytes,
    void Function(int, int)? onProgress,
  }) async =>
      throw UnimplementedError();
  @override
  Future<DriveFile?> getFileById(String id) async => null;
  @override
  Future<void> renameFile(String id, String newName) async {}
  @override
  Future<List<DriveFile>> listChildren(String parentId) async => [];
  @override
  Future<Uint8List> downloadFile(String id) async => Uint8List(0);
  @override
  Future<void> createPermission(
      String fileId, String email, MemberRole role) async {}
  @override
  Future<void> deletePermission(String fileId, String permissionId) async {}
}

void main() {
  late AppDatabase db;
  late FakeDriveRepository drive;
  late DriveRootService service;

  setUp(() {
    db = AppDatabase.forTesting(NativeDatabase.memory());
    drive = FakeDriveRepository();
    service = DriveRootService(drive, SettingsRepository(db));
  });

  tearDown(() => db.close());

  test('ensureRoot creates and persists a root folder', () async {
    expect(await service.isConfigured, isFalse);
    final root = await service.ensureRoot(name: 'Gallery Cloud');
    expect(root.name, 'Gallery Cloud');
    expect(await service.isConfigured, isTrue);
    expect(await service.getRootFolderId(), root.id);
  });

  test('ensureRoot reuses persisted root without creating again', () async {
    final first = await service.ensureRoot(name: 'Gallery Cloud');
    final second = await service.ensureRoot(name: 'Gallery Cloud');
    expect(first.id, second.id);
    expect(drive.createCalls, 1);
  });

  test('ensureRoot reuses an existing same-named folder', () async {
    await drive.createFolder('Gallery Cloud');
    drive.createCalls = 0;
    final root = await service.ensureRoot(name: 'Gallery Cloud');
    expect(drive.createCalls, 0);
    expect(root.name, 'Gallery Cloud');
  });
}
