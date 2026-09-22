import 'package:drift/native.dart';
import 'package:flutter/widgets.dart';
import 'package:drive_linked_gallery/app/app.dart';
import 'package:drive_linked_gallery/app/providers.dart';
import 'package:drive_linked_gallery/data/db/database.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  testWidgets('App boots to the home screen with empty state', (tester) async {
    final db = AppDatabase.forTesting(NativeDatabase.memory());
    addTearDown(db.close);

    await tester.pumpWidget(
      ProviderScope(
        overrides: [databaseProvider.overrideWithValue(db)],
        child: const DriveLinkedGalleryApp(),
      ),
    );
    // Allow the async album stream to emit its first (empty) value. We avoid
    // pumpAndSettle because the live Drift query stream keeps the scheduler
    // busy and never reaches a quiescent state.
    await tester.pump();
    await tester.pump(const Duration(milliseconds: 100));

    expect(find.text('Drive-Linked Gallery'), findsWidgets);
    expect(find.text('No albums yet'), findsOneWidget);

    // Unmount the tree so Riverpod disposes the Drift stream, then pump to
    // flush the (zero-duration) stream-close timer drift schedules on dispose.
    await tester.pumpWidget(const SizedBox());
    await tester.pump(const Duration(milliseconds: 50));
  });
}
