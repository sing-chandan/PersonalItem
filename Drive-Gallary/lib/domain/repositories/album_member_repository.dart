import '../models/album_member.dart';

/// Persistence for album membership metadata (spec §10, §31). Application-level
/// membership is kept independently of Drive permissions.
abstract class AlbumMemberRepository {
  Future<List<AlbumMember>> getMembers(String albumId);

  Stream<List<AlbumMember>> watchMembers(String albumId);

  Future<AlbumMember> upsert(AlbumMember member);

  Future<AlbumMember?> findByEmail(String albumId, String email);

  Future<void> remove(String id);
}
