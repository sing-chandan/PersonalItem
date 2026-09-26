import 'package:flutter/widgets.dart';

/// Non-web platforms use the interactive `authenticate()` flow via a normal
/// button, so this rendered-button placeholder is empty.
Widget googleRenderedSignInButton() => const SizedBox.shrink();
