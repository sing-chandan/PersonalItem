import '../../core/errors/app_error.dart';
import '../../core/logging/app_logger.dart';
import '../../core/utils/id_generator.dart';
import '../models/album.dart';
import '../models/album_member.dart';
import '../models/enums.dart';
import '../repositories/album_member_repository.dart';
import '../repositories/drive_repository.dart';

// ignore_for_file: prefer_initializing_formals

/// Album sharing/collaboration (spec §29–31). Creates/updates Drive permissions
/// and keeps application-level membership metadata in sync. Validates sharing
/// before changing Drive permissions (spec §39).
class SharingService {
  SharingService(
    this._members,
    this._drive, {
    IdGenerator ids = const IdGenerator(),
  }) : _ids = ids;

  final AlbumMemberRepository _members;
  final DriveRepository _drive;
  final IdGenerator _ids;

  final _log = const AppLogger('SharingService');

  Stream<List<AlbumMember>> watchMembers(String albumId) =>
      _members.watchMembers(albumId);

  /// Shares [album] with [email] at [role]. Requires the album to be linked to
  /// a Drive folder. Creates the Drive permission first, then persists the
  /// membership so local state never claims access that Drive doesn't grant.
  Future<AlbumMember> shareAlbum(
    Album album,
    String email,
    MemberRole role,
  ) async {
    final normalized = email.trim().toLowerCase();
    if (normalized.isEmpty || !normalized.contains('@')) {
      throw AppError.unknown('Please enter a valid email address.');
    }
    if (album.driveFolderId == null) {
      throw AppError.driveNotFound(
        'Link this album to Drive before sharing it.',
      );
    }
    if (role == MemberRole.owner) {
      throw AppError.permissionDenied('Ownership cannot be transferred here.');
    }

    await _drive.createPermission(album.driveFolderId!, normalized, role);

    final existing = await _members.findByEmail(album.id, normalized);
    final now = DateTime.now();
    final member = AlbumMember(
      id: existing?.id ?? _ids.newId(),
      albumId: album.id,
      userId: existing?.userId,
      email: normalized,
      role: role,
      createdAt: existing?.createdAt ?? now,
      updatedAt: now,
    );
    return _members.upsert(member);
  }

  /// Removes [member] from [album]: revokes the Drive permission (best effort)
  /// then removes the membership record.
  Future<void> removeMember(Album album, AlbumMember member) async {
    if (album.driveFolderId != null) {
      try {
        final perms = await _drive.listPermissions(album.driveFolderId!);
        for (final p in perms) {
          if (p.email?.toLowerCase() == member.email.toLowerCase()) {
            await _drive.deletePermission(album.driveFolderId!, p.id);
          }
        }
      } catch (e) {
        _log.w('Failed to revoke Drive permission for ${member.email}', e);
      }
    }
    await _members.remove(member.id);
  }

  /// Changes a member's role: updates the Drive permission and local metadata.
  Future<AlbumMember> changeRole(
    Album album,
    AlbumMember member,
    MemberRole newRole,
  ) async {
    if (newRole == MemberRole.owner) {
      throw AppError.permissionDenied('Ownership cannot be transferred here.');
    }
    if (album.driveFolderId != null) {
      // Re-create the permission with the new role (create is idempotent per
      // email on Drive and updates the existing grant).
      await _drive.createPermission(
        album.driveFolderId!,
        member.email,
        newRole,
      );
    }
    final updated = AlbumMember(
      id: member.id,
      albumId: member.albumId,
      userId: member.userId,
      email: member.email,
      role: newRole,
      createdAt: member.createdAt,
      updatedAt: DateTime.now(),
    );
    return _members.upsert(updated);
  }
}
