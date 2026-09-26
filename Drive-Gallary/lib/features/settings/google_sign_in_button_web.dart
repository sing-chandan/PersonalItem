import 'package:flutter/widgets.dart';
import 'package:google_sign_in_web/web_only.dart' as web;

/// On web, Google Sign-In requires the official rendered button; programmatic
/// `authenticate()` is not supported. Clicking it opens Google's account
/// chooser and, on success, fires an authentication event the app listens to.
Widget googleRenderedSignInButton() => web.renderButton();
