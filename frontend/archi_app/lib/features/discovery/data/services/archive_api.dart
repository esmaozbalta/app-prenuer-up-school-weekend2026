import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/ai_suggestion.dart';

class ArchiveApi {
  ArchiveApi({required this.baseUrl});

  final String baseUrl;

  Future<void> addFromSuggestion({
    required String accessToken,
    required AiSuggestion suggestion,
  }) async {
    final uri = Uri.parse('$baseUrl/api/v1/archive/add');
    final externalId =
        'ai-${suggestion.title.toLowerCase().replaceAll(RegExp(r'[^a-z0-9]+'), '-')}-${DateTime.now().millisecondsSinceEpoch}';

    final metadata = <String, dynamic>{
      'description': suggestion.description,
      'year': suggestion.parsedYear,
      'imageUrl': suggestion.imageUrl.isNotEmpty ? suggestion.imageUrl : null,
    }..removeWhere((_, value) => value == null);

    final response = await http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $accessToken',
      },
      body: jsonEncode({
        'externalId': externalId,
        'category': suggestion.archiveCategory,
        'title': suggestion.title,
        'metadata': metadata,
        'status': 0,
        'tags': <String>[],
      }),
    );

    if (response.statusCode == 201) {
      return;
    }

    if (response.statusCode == 401) {
      throw Exception('Oturum suresi doldu.');
    }

    if (response.statusCode == 409) {
      throw Exception('Bu icerik zaten arsivinde.');
    }

    final fallback = 'Arsive eklenemedi (HTTP ${response.statusCode}).';
    if (response.body.trim().startsWith('{')) {
      try {
        final error = jsonDecode(response.body) as Map<String, dynamic>;
        throw Exception((error['message'] as String?) ?? fallback);
      } catch (_) {
        throw Exception(fallback);
      }
    }

    throw Exception(fallback);
  }
}
