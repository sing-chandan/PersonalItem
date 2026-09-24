import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/db/mappers.dart';
import 'package:drive_linked_gallery/data/repositories/media_repository_impl.dart';
import 'package:drive_linked_gallery/domain/models/enums.dart';
import 'package:drive_linked_gallery/domain/models/media_item.dart';
import 'package:flutter_test/flutter_test.dart';

/// Performance smoke test (spec §40, §52 #27): a large library must stay fast
/// with paginated queries and indexed lookups.
void main() {
  test('handles 10,000 media rows with fast paginated queries', () async {
    final db = AppDatabase.forTesting(NativeDatabase.memory());
    final repo = MediaRepositoryImpl(db);
    const albumId = 'big-album';
    const total = 10000;

    final now = DateTime.now();
    await db.batch((batch) {
      for (var i = 0; i < total; i++) {
        batch.insert(
          db.mediaItems,
          MediaItem(
            id: 'm$i',
            albumId: albumId,
            localUri: '/tmp/$i.jpg',
            fileName: '$i.jpg',
            mimeType: 'image/jpeg',
            sizeBytes: 1000,
            capturedAt: now.subtract(Duration(seconds: i)),
            syncStatus: i.isEven ? SyncStatus.synced : SyncStatus.localOnly,
            createdAt: now,
            updatedAt: now,
          ).toCompanion(),
        );
      }
    });

    final sw = Stopwatch()..start();
    expect(await repo.countInAlbum(albumId), total);
    final page = await repo.getMediaInAlbum(albumId, limit: 120, offset: 0);
    expect(page.length, 120);
    final deepPage = await repo.getMediaInAlbum(
      albumId,
      limit: 120,
      offset: 9000,
    );
    expect(deepPage.length, 120);
    final pending = await repo.countPendingInAlbum(albumId);
    expect(pending, total ~/ 2);
    sw.stop();

    // Generous bound; indexed queries should be far below this.
    expect(sw.elapsedMilliseconds, lessThan(3000));

    await db.close();
  });
}
