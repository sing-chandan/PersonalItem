import 'package:drift/drift.dart';

import '../../domain/models/album_member.dart';
import '../../domain/repositories/album_member_repository.dart';
import '../db/database.dart';
import '../db/mappers.dart';

class AlbumMemberRepositoryImpl implements AlbumMemberRepository {
  AlbumMemberRepositoryImpl(this._db);

  final AppDatabase _db;

  @override
  Future<List<AlbumMember>> getMembers(String albumId) async {
    final rows = await (_db.select(
      _db.albumMembers,
    )..where((t) => t.albumId.equals(albumId))).get();
    return rows.map(albumMemberFromRow).toList();
  }

  @override
  Stream<List<AlbumMember>> watchMembers(String albumId) {
    final query = _db.select(_db.albumMembers)
      ..where((t) => t.albumId.equals(albumId))
      ..orderBy([(t) => OrderingTerm(expression: t.email)]);
    return query.watch().map((rows) => rows.map(albumMemberFromRow).toList());
  }

  @override
  Future<AlbumMember> upsert(AlbumMember member) async {
    await _db
        .into(_db.albumMembers)
        .insertOnConflictUpdate(
          AlbumMembersCompanion(
            id: Value(member.id),
            albumId: Value(member.albumId),
            userId: Value(member.userId),
            email: Value(member.email),
            role: Value(member.role.name),
            createdAt: Value(member.createdAt),
            updatedAt: Value(member.updatedAt),
          ),
        );
    return member;
  }

  @override
  Future<AlbumMember?> findByEmail(String albumId, String email) async {
    final row =
        await (_db.select(_db.albumMembers)
              ..where((t) => t.albumId.equals(albumId) & t.email.equals(email)))
            .getSingleOrNull();
    return row == null ? null : albumMemberFromRow(row);
  }

  @override
  Future<void> remove(String id) async {
    await (_db.delete(_db.albumMembers)..where((t) => t.id.equals(id))).go();
  }
}
