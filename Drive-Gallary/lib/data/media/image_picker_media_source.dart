import 'package:image_picker/image_picker.dart';

import '../../domain/models/picked_media.dart';
import '../../domain/services/media_source.dart';

/// [MediaSource] implementation backed by `image_picker`. Works on web and
/// Android for picking and camera capture. Recent-media enumeration is not
/// available through image_picker, so [recentMedia] returns an empty list on
/// platforms without a dedicated MediaStore implementation.
class ImagePickerMediaSource implements MediaSource {
  ImagePickerMediaSource([ImagePicker? picker])
    : _picker = picker ?? ImagePicker();

  final ImagePicker _picker;

  @override
  bool get supportsRecentEnumeration => false;

  @override
  Future<List<PickedMedia>> pickImages({bool multiple = true}) async {
    final files = multiple
        ? await _picker.pickMultiImage()
        : <XFile>[
            if (await _picker.pickImage(source: ImageSource.gallery)
                case final XFile f)
              f,
          ];
    return _toPicked(files);
  }

  @override
  Future<PickedMedia?> capturePhoto() async {
    final file = await _picker.pickImage(source: ImageSource.camera);
    if (file == null) return null;
    final list = await _toPicked([file]);
    return list.isEmpty ? null : list.first;
  }

  @override
  Future<List<PickedMedia>> recentMedia({int limit = 120}) async => const [];

  Future<List<PickedMedia>> _toPicked(List<XFile> files) async {
    final result = <PickedMedia>[];
    for (final f in files) {
      final size = await f.length();
      result.add(
        PickedMedia(
          localUri: f.path,
          fileName: f.name,
          mimeType: f.mimeType ?? _guessMime(f.name),
          sizeBytes: size,
          capturedAt: DateTime.now(),
          modifiedAt: DateTime.now(),
        ),
      );
    }
    return result;
  }

  String _guessMime(String name) {
    final lower = name.toLowerCase();
    if (lower.endsWith('.png')) return 'image/png';
    if (lower.endsWith('.gif')) return 'image/gif';
    if (lower.endsWith('.webp')) return 'image/webp';
    if (lower.endsWith('.heic')) return 'image/heic';
    if (lower.endsWith('.mp4')) return 'video/mp4';
    if (lower.endsWith('.mov')) return 'video/quicktime';
    return 'image/jpeg';
  }
}
