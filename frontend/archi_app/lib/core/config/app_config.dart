import 'package:flutter/foundation.dart';

/// Ortak API yapılandırması — login ve diğer backend çağrıları için taban URL.
abstract final class AppConfig {
  static const String _envBaseUrl = String.fromEnvironment('API_BASE_URL');

  /// Backend kök adresi (sonunda `/` olmadan).
  ///
  /// Gerçek cihaz veya farklı makine için:
  /// `flutter run --dart-define=API_BASE_URL=http://192.168.1.10:5161`
  static String get apiBaseUrl {
    final trimmed = _envBaseUrl.trim();
    if (trimmed.isNotEmpty) {
      return trimmed.endsWith('/') ? trimmed.substring(0, trimmed.length - 1) : trimmed;
    }
    return _defaultApiBaseUrl();
  }

  static String _defaultApiBaseUrl() {
    if (kIsWeb) {
      return 'http://localhost:5161';
    }
    switch (defaultTargetPlatform) {
      case TargetPlatform.android:
        // Android emülatör: makinedeki localhost → 10.0.2.2
        return 'http://10.0.2.2:5161';
      default:
        return 'http://localhost:5161';
    }
  }
}
