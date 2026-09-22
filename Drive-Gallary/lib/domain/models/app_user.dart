/// Immutable domain representation of a signed-in user (section 8.1).
class AppUser {
  const AppUser({
    required this.id,
    required this.googleAccountId,
    required this.email,
    required this.displayName,
    this.photoUrl,
    required this.createdAt,
    required this.updatedAt,
  });

  final String id;
  final String googleAccountId;
  final String email;
  final String displayName;
  final String? photoUrl;
  final DateTime createdAt;
  final DateTime updatedAt;
}
