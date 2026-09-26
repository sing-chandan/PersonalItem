// Chooses the platform implementation: a rendered Google button on web, an
// empty placeholder elsewhere (Android uses the interactive authenticate flow).
export 'google_sign_in_button_stub.dart'
    if (dart.library.js_interop) 'google_sign_in_button_web.dart';
