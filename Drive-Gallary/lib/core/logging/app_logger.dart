import 'package:flutter/foundation.dart';

/// Severity levels for application logs.
enum LogLevel { debug, info, warning, error }

/// A tiny, dependency-free logger.
///
/// SECURITY: never pass OAuth tokens, credentials, or full request headers to
/// this logger. Callers are responsible for redacting sensitive values.
class AppLogger {
  const AppLogger(this._tag);

  final String _tag;

  static LogLevel minLevel = kReleaseMode ? LogLevel.warning : LogLevel.debug;

  void d(String message) => _log(LogLevel.debug, message);
  void i(String message) => _log(LogLevel.info, message);
  void w(String message, [Object? error]) =>
      _log(LogLevel.warning, message, error);
  void e(String message, [Object? error, StackTrace? stack]) =>
      _log(LogLevel.error, message, error, stack);

  void _log(
    LogLevel level,
    String message, [
    Object? error,
    StackTrace? stack,
  ]) {
    if (level.index < minLevel.index) return;
    final ts = DateTime.now().toIso8601String();
    final line =
        '[$ts] [${level.name.toUpperCase()}] [$_tag] $message'
        '${error != null ? ' :: $error' : ''}';
    debugPrint(line);
    if (stack != null && level == LogLevel.error) {
      debugPrint(stack.toString());
    }
  }
}
