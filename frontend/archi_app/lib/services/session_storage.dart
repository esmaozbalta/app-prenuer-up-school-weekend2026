import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class SessionStorage {
  static const _tokenKey = 'archi_access_token';
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  Future<void> saveToken(String token) async {
    try {
      await _storage.write(key: _tokenKey, value: token);
    } catch (_) {
      // Ignore plugin-level storage errors in unsupported environments.
    }
  }

  Future<String?> readToken() async {
    try {
      return _storage
          .read(key: _tokenKey)
          .timeout(const Duration(milliseconds: 200), onTimeout: () => null);
    } catch (_) {
      return null;
    }
  }

  Future<void> clearToken() async {
    try {
      await _storage.delete(key: _tokenKey);
    } catch (_) {
      // Ignore plugin-level storage errors in unsupported environments.
    }
  }
}
