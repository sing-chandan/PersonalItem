import 'enums.dart';

/// Immutable domain representation of an album membership (section 10).
class AlbumMember {
  const AlbumMember({
    required this.id,
    required this.albumId,
    this.userId,
    required this.email,
    required this.role,
    required this.createdAt,
    required this.updatedAt,
  });

  final String id;
  final String albumId;
  final String? userId;
  final String email;
  final MemberRole role;
  final DateTime createdAt;
  final DateTime updatedAt;
}
