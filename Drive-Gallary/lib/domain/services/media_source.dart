import '../models/picked_media.dart';

/// Abstraction over the device's media capabilities. Isolates the UI/services
/// from platform specifics (Android MediaStore vs. web file selection).
///
/// - Android (device stage): backed by MediaStore for [recentMedia] and
///   `image_picker`/camera for [pickImages]/[capturePhoto].
/// - Web (current dev target): [pickImages]/[capturePhoto] use the browser
///   file/camera dialog; [recentMedia] is unsupported and returns an empty
///   list (see TECHNICAL_LIMITATIONS.md).
abstract class MediaSource {
  /// Whether this platform can enumerate recently added device media.
  bool get supportsRecentEnumeration;

  Future<List<PickedMedia>> pickImages({bool multiple = true});

  Future<PickedMedia?> capturePhoto();

  /// Recently added device media, newest first. Empty when unsupported.
  Future<List<PickedMedia>> recentMedia({int limit = 120});
}
