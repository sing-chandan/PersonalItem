import 'package:uuid/uuid.dart';

/// Generates stable unique identifiers for domain entities.
class IdGenerator {
  const IdGenerator();
  static const _uuid = Uuid();
  String newId() => _uuid.v4();
}
