import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/repositories/media_repository_impl.dart';
import 'package:drive_linked_gallery/domain/models/picked_media.dart';
import 'package:drive_linked_gallery/domain/services/bulk_media_service.dart';
import 'package:drive_linked_gallery/domain/services/bulk_rename_service.dart';
import 'package:drive_linked_gallery/domain/services/media_service.dart';
import 'package:flutter_test/flutter_test.dart';

import 'drive_root_service_test.dart' show FakeDriveRepository;

void main() {
  late AppDatabase db;
  late MediaRepositoryImpl media;
  late MediaService mediaService;
  late FakeDriveRepository drive;
  late BulkMediaService bulk;

  setUp(() {
    db = AppDatabase.forTesting(NativeDatabase.memory());
    media = MediaRepositoryImpl(db);
    mediaService = MediaService(media);
    drive = FakeDriveRepository();
    bulk = BulkMediaService(media, drive, const BulkRenameService());
  });

  tearDown(() => db.close());

  PickedMedia p(String name) => PickedMedia(
    localUri: '/tmp/$name',
    fileName: name,
    mimeType: 'image/jpeg',
    sizeBytes: 10,
  );

  test('setCategory assigns a category to items', () async {
    final added = await mediaService.addToAlbum('al', [p('a.jpg'), p('b.jpg')]);
    await bulk.setCategory(added, 'Before');
    final refetched = await media.getMediaById(added.first.id);
    expect(refetched!.category, 'Before');
  });

  test('rename renames items sequentially', () async {
    final added = await mediaService.addToAlbum('al', [p('a.jpg'), p('b.jpg')]);
    final renamed = await bulk.rename(
      items: added,
      category: 'After',
      startNumber: 1,
    );
    expect(renamed[0].fileName, 'After_001.jpg');
    expect(renamed[1].fileName, 'After_002.jpg');
    final persisted = await media.getMediaById(added[0].id);
    expect(persisted!.fileName, 'After_001.jpg');
    expect(persisted.category, 'After');
  });
}
