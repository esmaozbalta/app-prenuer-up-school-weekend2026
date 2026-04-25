import 'dart:convert';

import 'package:http/http.dart' as http;

class AuthApi {
  AuthApi({required this.baseUrl});

  final String baseUrl;

  String _readErrorMessage(http.Response response, String fallback) {
    final body = response.body;
    if (body.trim().startsWith('{')) {
      try {
        final error = jsonDecode(body) as Map<String, dynamic>;
        return (error['message'] as String?) ?? fallback;
      } catch (_) {
        return fallback;
      }
    }
    return 'Sunucu hatasi (HTTP ${response.statusCode})';
  }

  Map<String, String> _authHeaders(String token) => {
    'Content-Type': 'application/json',
    'Authorization': 'Bearer $token',
  };

  Future<UserProfile> fetchProfile({required String accessToken}) async {
    final uri = Uri.parse('$baseUrl/api/v1/profile');
    final response = await http.get(
      uri,
      headers: _authHeaders(accessToken),
    );

    if (response.statusCode == 200) {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      return UserProfile.fromJson(map);
    }

    if (response.statusCode == 401) {
      throw Exception('Oturum suresi doldu veya yetki yok.');
    }

    throw Exception(_readErrorMessage(response, 'Profil yuklenemedi'));
  }

  Future<UserProfile> updatePrivacy({
    required String accessToken,
    required bool isPrivate,
  }) async {
    final uri = Uri.parse('$baseUrl/api/v1/profile/privacy');
    final response = await http.patch(
      uri,
      headers: _authHeaders(accessToken),
      body: jsonEncode({'isPrivate': isPrivate}),
    );

    if (response.statusCode == 200) {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      return UserProfile.fromJson(map);
    }

    if (response.statusCode == 401) {
      throw Exception('Oturum suresi doldu veya yetki yok.');
    }

    throw Exception(_readErrorMessage(response, 'Gizlilik guncellenemedi'));
  }

  Future<String> register({
    required String email,
    required String username,
    required String password,
  }) async {
    final uri = Uri.parse('$baseUrl/api/v1/auth/register');
    final response = await http.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'email': email,
        'username': username,
        'password': password,
      }),
    );

    if (response.statusCode == 201) {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      return (map['accessToken'] as String?) ?? '';
    }

    throw Exception(_readErrorMessage(response, 'Registration failed'));
  }

  Future<String> login({
    required String email,
    required String password,
  }) async {
    final uri = Uri.parse('$baseUrl/api/v1/auth/login');
    final response = await http.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'email': email,
        'password': password,
      }),
    );

    if (response.statusCode == 200) {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      return (map['accessToken'] as String?) ?? '';
    }

    if (response.statusCode == 401) {
      throw Exception('Email veya sifre hatali.');
    }

    throw Exception(_readErrorMessage(response, 'Login failed'));
  }
}

class UserProfile {
  UserProfile({
    required this.userId,
    required this.email,
    required this.username,
    required this.isPrivate,
  });

  final String userId;
  final String email;
  final String username;
  final bool isPrivate;

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      userId: (json['userId'] ?? '').toString(),
      email: (json['email'] ?? '') as String,
      username: (json['username'] ?? '') as String,
      isPrivate: json['isPrivate'] as bool? ?? false,
    );
  }
}
