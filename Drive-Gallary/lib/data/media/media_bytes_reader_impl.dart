import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

import '../../core/errors/app_error.dart';
import '../../domain/services/media_bytes_reader.dart';

/// Reads local media bytes. On native platforms the URI is a file path; on web
/// it is a blob/object URL fetched via HTTP.
class MediaBytesReaderImpl implements MediaBytesReader {
  const MediaBytesReaderImpl();

  @override
  Future<Uint8List> read(String localUri) async {
    try {
      if (kIsWeb) {
        final response = await http.get(Uri.parse(localUri));
        if (response.statusCode != 200) {
          throw AppError.fileNotFound('blob read ${response.statusCode}');
        }
        return response.bodyBytes;
      }
      final file = File(localUri);
      if (!await file.exists()) {
        throw AppError.fileNotFound(localUri);
      }
      return await file.readAsBytes();
    } on AppError {
      rethrow;
    } catch (e) {
      throw AppError.fileNotFound('read failed: $e');
    }
  }
}
