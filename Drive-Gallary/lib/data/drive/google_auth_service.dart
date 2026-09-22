import 'dart:async';

import 'package:google_sign_in/google_sign_in.dart';
import 'package:http/http.dart' as http;

import '../../core/constants/app_constants.dart';
import '../../core/errors/app_error.dart';
import '../../core/logging/app_logger.dart';
import '../../domain/models/app_user.dart';
import '../../domain/services/auth_service.dart';

/// [AuthService] backed by google_sign_in 7.x. Authentication (identity) and
/// authorization (Drive scopes) are separate steps in v7.
class GoogleAuthService implements AuthService {
  GoogleAuthService();

  final _log = const AppLogger('GoogleAuthService');
  final _signIn = GoogleSignIn.instance;
  final _userController = StreamController<AppUser?>.broadcast();

  GoogleSignInAccount? _account;
  bool _initialized = false;

  @override
  bool get supportsInteractiveSignIn => _signIn.supportsAuthenticate();

  @override
  Stream<AppUser?> authState() => _userController.stream;

  @override
  AppUser? get currentUser => _account == null ? null : _mapUser(_account!);

  @override
  Future<void> initialize() async {
    if (_initialized) return;
    _initialized = true;
    try {
      await _signIn.initialize(
        clientId: AppConstants.googleWebClientId.isEmpty
            ? null
            : AppConstants.googleWebClientId,
        serverClientId: AppConstants.googleServerClientId.isEmpty
            ? null
            : AppConstants.googleServerClientId,
      );
      _signIn.authenticationEvents.listen(
        _onAuthEvent,
        onError: (Object e) {
          _log.w('auth event error', e);
        },
      );
    } catch (e) {
      _log.w(
        'Google Sign-In initialize failed (credentials may be missing)',
        e,
      );
    }
  }

  void _onAuthEvent(GoogleSignInAuthenticationEvent event) {
    switch (event) {
      case GoogleSignInAuthenticationEventSignIn(:final user):
        _account = user;
        _userController.add(_mapUser(user));
      case GoogleSignInAuthenticationEventSignOut():
        _account = null;
        _userController.add(null);
    }
  }

  @override
  Future<AppUser?> restoreSession() async {
    await initialize();
    try {
      final account = await _signIn.attemptLightweightAuthentication();
      if (account != null) {
        _account = account;
        final user = _mapUser(account);
        _userController.add(user);
        return user;
      }
    } catch (e) {
      _log.w('Lightweight authentication failed', e);
    }
    return null;
  }

  @override
  Future<AppUser> signIn() async {
    await initialize();
    if (!_signIn.supportsAuthenticate()) {
      throw AppError.authRequired(
        'Interactive sign-in is not supported on this platform (web uses a '
        'rendered Google button). Test sign-in on an Android device.',
      );
    }
    try {
      final account = await _signIn.authenticate(
        scopeHint: AppConstants.googleSignInScopes,
      );
      // Ensure Drive authorization is granted (may show a consent screen).
      final authz = await account.authorizationClient.authorizeScopes(
        AppConstants.googleSignInScopes,
      );
      if (authz.accessToken.isEmpty) {
        throw AppError.authRequired('Drive authorization was not granted.');
      }
      _account = account;
      final user = _mapUser(account);
      _userController.add(user);
      return user;
    } on AppError {
      rethrow;
    } catch (e) {
      _log.e('Sign-in failed', e);
      throw AppError.authRequired('Google sign-in failed.');
    }
  }

  @override
  Future<void> signOut() async {
    try {
      await _signIn.signOut();
    } finally {
      _account = null;
      _userController.add(null);
    }
  }

  @override
  Future<void> disconnect() async {
    try {
      await _signIn.disconnect();
    } finally {
      _account = null;
      _userController.add(null);
    }
  }

  @override
  Future<http.Client?> authorizedClient() async {
    final account = _account;
    if (account == null) return null;
    return _AuthorizedClient(account, http.Client());
  }

  AppUser _mapUser(GoogleSignInAccount a) {
    final now = DateTime.now();
    return AppUser(
      id: a.id,
      googleAccountId: a.id,
      email: a.email,
      displayName: a.displayName ?? a.email,
      photoUrl: a.photoUrl,
      createdAt: now,
      updatedAt: now,
    );
  }
}

/// HTTP client that injects Drive authorization headers on each request.
class _AuthorizedClient extends http.BaseClient {
  _AuthorizedClient(this._account, this._inner);

  final GoogleSignInAccount _account;
  final http.Client _inner;

  @override
  Future<http.StreamedResponse> send(http.BaseRequest request) async {
    final headers = await _account.authorizationClient.authorizationHeaders(
      AppConstants.googleSignInScopes,
      promptIfNecessary: false,
    );
    if (headers == null) {
      throw AppError.authRequired('Drive authorization expired.');
    }
    request.headers.addAll(headers);
    return _inner.send(request);
  }

  @override
  void close() => _inner.close();
}
