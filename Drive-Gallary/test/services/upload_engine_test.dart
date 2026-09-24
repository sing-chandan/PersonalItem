import 'dart:typed_data';

import 'package:drift/native.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:drive_linked_gallery/data/repositories/album_repository_impl.dart';
import 'package:drive_linked_gallery/data/repositories/media_repository_impl.dart';
import 'package:drive_linked_gallery/data/repositories/settings_repository.dart';
import 'package:drive_linked_gallery/data/repositories/sync_job_repository_impl.dart';
import 'package:drive_linked_gallery/domain/models/drive_models.dart';
import 'package:drive_linked_gallery/domain/models/enums.dart';
import 'package:drive_linked_gallery/domain/models/picked_media.dart';
import 'package:drive_linked_gallery/domain/models/sync_job.dart';
import 'package:drive_linked_gallery/domain/services/album_service.dart';
import 'package:drive_linked_gallery/domain/services/drive_root_service.dart';
import 'package:drive_linked_gallery/domain/services/linked_album_service.dart';
import 'package:drive_linked_gallery/domain/services/media_bytes_reader.dart';
import 'package:drive_linked_gallery/domain/services/media_service.dart';
import 'package:drive_linked_gallery/domain/services/upload_engine.dart';
import 'package:flutter_test/flutter_test.dart';

import 'drive_root_service_test.dart' show FakeDriveRepository;

class _FailingUploadDrive extends FakeDriveRepository {
  bool failUploads = false;
  int uploadCount = 0;

  @override
  Future<DriveFile> uploadFile({
    required String parentId,
    required String name,
    required String mimeType,
    required Uint8List bytes,
    void Function(int, int)? onProgress,
  }) async {
    if (failUploads) {
      throw Exception('network down');
    }
    uploadCount++;
    return DriveFile(id: 'file$uploadCount', name: name, parentId: parentId);
  }
}

class _FakeReader implements MediaBytesReader {
  @override
  Future<Uint8List> read(String localUri) async =>
      Uint8List.fromList([1, 2, 3]);
}

void main() {
  late AppDatabase db;
  late MediaRepositoryImpl media;
  late SyncJobRepositoryImpl jobs;
  late _FailingUploadDrive drive;
  late LinkedAlbumService linker;
  late MediaService mediaService;
  late UploadEngine engine;

  setUp(() {
    db = AppDatabase.forTesting(NativeDatabase.memory());
    final albums = AlbumRepositoryImpl(db);
    media = MediaRepositoryImpl(db);
    jobs = SyncJobRepositoryImpl(db);
    drive = _FailingUploadDrive();
    final root = DriveRootService(drive, SettingsRepository(db));
    linker = LinkedAlbumService(albums, AlbumService(albums), drive, root);
    mediaService = MediaService(media);
    engine = UploadEngine(
      jobs: jobs,
      media: media,
      albums: albums,
      drive: drive,
      reader: _FakeReader(),
      linker: linker,
    );
  });

  tearDown(() => db.close());

  Future<SyncJob> setupQueuedJob() async {
    final album = (await linker.createAlbum(name: 'A', ownerUserId: 'u')).album;
    final added = await mediaService.addToAlbum(album.id, [
      const PickedMedia(
        localUri: '/tmp/a.jpg',
        fileName: 'a.jpg',
        mimeType: 'image/jpeg',
        sizeBytes: 3,
      ),
    ]);
    final now = DateTime.now();
    return jobs.enqueue(
      SyncJob(
        id: 'job1',
        type: SyncJobType.upload,
        albumId: album.id,
        mediaItemId: added.first.id,
        priority: 0,
        status: SyncJobStatus.queued,
        attemptCount: 0,
        createdAt: now,
        updatedAt: now,
      ),
    );
  }

  test('uploads a queued item and marks it synced', () async {
    final job = await setupQueuedJob();
    final ok = await engine.processJob(job);
    expect(ok, isTrue);
    expect(drive.uploadCount, 1);

    final item = await media.getMediaById(job.mediaItemId!);
    expect(item!.syncStatus, SyncStatus.synced);
    expect(item.driveFileId, isNotNull);

    final stored = await jobs.getById(job.id);
    expect(stored!.status, SyncJobStatus.succeeded);
  });

  test(
    'does not re-upload an already-synced item (duplicate protection)',
    () async {
      final job = await setupQueuedJob();
      await engine.processJob(job);
      expect(drive.uploadCount, 1);

      // Re-run the same job; should short-circuit via driveFileId.
      final again = await jobs.getById(job.id);
      await engine.processJob(again!);
      expect(drive.uploadCount, 1);
    },
  );

  test('failure requeues with backoff and increments attempt count', () async {
    drive.failUploads = true;
    final job = await setupQueuedJob();
    final ok = await engine.processJob(job);
    expect(ok, isFalse);

    final stored = await jobs.getById(job.id);
    expect(stored!.status, SyncJobStatus.queued);
    expect(stored.attemptCount, 1);
    expect(stored.nextAttemptAt, isNotNull);

    final item = await media.getMediaById(job.mediaItemId!);
    expect(item!.syncStatus, SyncStatus.failed);
  });
}
