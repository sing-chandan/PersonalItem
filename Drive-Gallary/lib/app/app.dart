import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/constants/app_constants.dart';
import 'providers.dart';
import 'router.dart';
import 'theme.dart';

class DriveLinkedGalleryApp extends ConsumerStatefulWidget {
  const DriveLinkedGalleryApp({super.key});

  @override
  ConsumerState<DriveLinkedGalleryApp> createState() =>
      _DriveLinkedGalleryAppState();
}

class _DriveLinkedGalleryAppState extends ConsumerState<DriveLinkedGalleryApp> {
  @override
  void initState() {
    super.initState();
    // Recover the persisted upload queue and begin processing (spec §22, §4).
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(syncControllerProvider).start();
    });
  }

  @override
  Widget build(BuildContext context) {
    final router = ref.watch(routerProvider);
    return MaterialApp.router(
      title: AppConstants.appName,
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      darkTheme: AppTheme.dark,
      routerConfig: router,
    );
  }
}
