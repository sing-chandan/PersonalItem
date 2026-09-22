import '../../core/constants/app_constants.dart';
import '../../data/repositories/settings_repository.dart';
import '../models/drive_models.dart';
import '../repositories/drive_repository.dart';

/// Manages the single Drive root folder (spec §17). The root's folder id is
/// persisted so the app never searches by name during normal operation.
class DriveRootService {
  DriveRootService(this._drive, this._settings);

  final DriveRepository _drive;
  final SettingsRepository _settings;

  Future<String?> getRootFolderId() =>
      _settings.get(SettingsRepository.kDriveRootFolderId);

  Future<bool> get isConfigured async =>
      (await getRootFolderId())?.isNotEmpty ?? false;

  /// Ensures a root folder exists: reuses an existing folder with the given
  /// name if present, otherwise creates it. Persists the resulting id.
  Future<DriveFolder> ensureRoot({
    String name = AppConstants.defaultDriveRootName,
  }) async {
    final existingId = await getRootFolderId();
    if (existingId != null && existingId.isNotEmpty) {
      final folder = await _drive.getFolderById(existingId);
      if (folder != null) return folder;
    }
    final found = await _drive.findFolder(name);
    final folder = found ?? await _drive.createFolder(name);
    await _persist(folder);
    return folder;
  }

  /// Creates a brand-new root folder even if one with the name exists.
  Future<DriveFolder> createNewRoot(String name) async {
    final folder = await _drive.createFolder(name);
    await _persist(folder);
    return folder;
  }

  Future<void> _persist(DriveFolder folder) async {
    await _settings.set(SettingsRepository.kDriveRootFolderId, folder.id);
    await _settings.set(SettingsRepository.kDriveRootFolderName, folder.name);
  }
}
