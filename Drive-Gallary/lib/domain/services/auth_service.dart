import 'package:http/http.dart' as http;

import '../models/app_user.dart';

/// Authentication + authorization abstraction over Google Sign-In (spec §18).
/// Tokens are never exposed via logs; only an authorized HTTP client is handed
/// to the Drive layer.
abstract class AuthService {
  /// Whether programmatic sign-in is available on this platform. On web,
  /// Google Sign-In requires a rendered button and returns false here.
  bool get supportsInteractiveSignIn;

  /// Emits the current user (or null when signed out) over time.
  Stream<AppUser?> authState();

  AppUser? get currentUser;

  /// Prepares the underlying SDK. Safe to call before any credentials exist.
  Future<void> initialize();

  /// Attempts to restore a previously signed-in session without UI.
  Future<AppUser?> restoreSession();

  /// Interactive sign-in + Drive authorization. Throws [AppError] on failure.
  Future<AppUser> signIn();

  Future<void> signOut();

  /// Fully revokes access (spec §18: clear auth state on disconnect).
  Future<void> disconnect();

  /// An HTTP client that injects Drive authorization headers, or null when no
  /// authorized session exists.
  Future<http.Client?> authorizedClient();
}
