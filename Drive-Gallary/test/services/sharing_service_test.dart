import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/repositories/album_member_repository_impl.dart';
import 'package:drive_linked_gallery/data/repositories/album_repository_impl.dart';
import 'package:drive_linked_gallery/data/repositories/settings_repository.dart';
import 'package:drive_linked_gallery/domain/models/enums.dart';
import 'package:drive_linked_gallery/domain/services/album_service.dart';
import 'package:drive_linked_gallery/domain/services/drive_root_service.dart';
import 'package:drive_linked_gallery/domain/services/linked_album_service.dart';
import 'package:drive_linked_gallery/domain/services/sharing_service.dart';
import 'package:flutter_test/flutter_test.dart';

import 'drive_root_service_test.dart' show FakeDriveRepository;

void main() {
  late AppDatabase db;
  late FakeDriveRepository drive;
  late AlbumMemberRepositoryImpl members;
  late LinkedAlbumService linker;
  late SharingService sharing;

  setUp(() {
    db = AppDatabase.forTesting(NativeDatabase.memory());
    final albums = AlbumRepositoryImpl(db);
    drive = FakeDriveRepository();
    members = AlbumMemberRepositoryImpl(db);
    final root = DriveRootService(drive, SettingsRepository(db));
    linker = LinkedAlbumService(albums, AlbumService(albums), drive, root);
    sharing = SharingService(members, drive);
  });

  tearDown(() => db.close());

  test(
    'shares a linked album, creating a Drive permission and a member',
    () async {
      final album = (await linker.createAlbum(
        name: 'A',
        ownerUserId: 'u',
      )).album;
      final member = await sharing.shareAlbum(
        album,
        'friend@gmail.com',
        MemberRole.contributor,
      );
      expect(member.role, MemberRole.contributor);
      expect(drive.permissions[album.driveFolderId]!.length, 1);
      final stored = await members.getMembers(album.id);
      expect(stored.single.email, 'friend@gmail.com');
    },
  );

  test('rejects an invalid email', () async {
    final album = (await linker.createAlbum(name: 'A', ownerUserId: 'u')).album;
    expect(
      () => sharing.shareAlbum(album, 'not-an-email', MemberRole.viewer),
      throwsA(isA<Object>()),
    );
  });

  test('removing a member revokes the Drive permission', () async {
    final album = (await linker.createAlbum(name: 'A', ownerUserId: 'u')).album;
    final member = await sharing.shareAlbum(
      album,
      'x@gmail.com',
      MemberRole.viewer,
    );
    await sharing.removeMember(album, member);
    expect(drive.permissions[album.driveFolderId], isEmpty);
    expect(await members.getMembers(album.id), isEmpty);
  });

  test('changeRole updates the member role', () async {
    final album = (await linker.createAlbum(name: 'A', ownerUserId: 'u')).album;
    final member = await sharing.shareAlbum(
      album,
      'x@gmail.com',
      MemberRole.viewer,
    );
    final updated = await sharing.changeRole(album, member, MemberRole.editor);
    expect(updated.role, MemberRole.editor);
    final stored = await members.findByEmail(album.id, 'x@gmail.com');
    expect(stored!.role, MemberRole.editor);
  });
}
