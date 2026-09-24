import 'package:drive_linked_gallery/domain/services/bulk_rename_service.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  const service = BulkRenameService();

  test('generates padded sequential names preserving extension', () {
    final names = service.generateNames(
      originalNames: ['a.jpg', 'b.png', 'c.jpeg'],
      category: 'Before',
      startNumber: 1,
    );
    expect(names, ['Before_001.jpg', 'Before_002.png', 'Before_003.jpeg']);
  });

  test('honors custom start number', () {
    final names = service.generateNames(
      originalNames: ['x.jpg', 'y.jpg'],
      category: 'After',
      startNumber: 10,
    );
    expect(names, ['After_010.jpg', 'After_011.jpg']);
  });

  test('handles files without extension', () {
    final names = service.generateNames(
      originalNames: ['noext'],
      category: 'WIP',
    );
    expect(names, ['WIP_001']);
  });

  test('sanitizes unsafe category characters', () {
    final names = service.generateNames(
      originalNames: ['a.jpg'],
      category: 'My Photos!',
    );
    expect(names.first, startsWith('My_Photos'));
    expect(names.first, endsWith('.jpg'));
  });

  test('produces unique collision-free names', () {
    final names = service.generateNames(
      originalNames: List.filled(50, 'img.jpg'),
      category: 'Meeting',
    );
    expect(names.toSet().length, 50);
  });
}
