import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/ai_suggestion.dart';
import 'package:flutter/foundation.dart';

class ArchiveApi {
  ArchiveApi({required this.baseUrl});

  final String baseUrl;

  Future<void> addFromSuggestion({
    required String accessToken,
    required AiSuggestion suggestion,
  }) async {
    final uri = Uri.parse('$baseUrl/api/v1/archive');
    
    // Rastgele değil, benzersiz bir ID oluşturuyoruz
    final externalId = 'ai-${suggestion.title.toLowerCase().replaceAll(RegExp(r'[^a-z0-9]+'), '-')}-${DateTime.now().millisecondsSinceEpoch}';

    final payload = {
      'externalId': externalId,
      'title': suggestion.title,
      'category': suggestion.archiveCategory, // 'movie', 'book', 'game'
      'imageUrl': suggestion.imageUrl,
      'year': suggestion.parsedYear?.toString() ?? '',
      'description': suggestion.description,
      'status': 0, // ÖNEMLİ: Backend'e 'Listem' yerine 0 gönderiyoruz!
    };

    debugPrint('--- GÖNDERİLEN JSON: $payload ---');

    final response = await http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $accessToken',
      },
      body: jsonEncode(payload),
    );

    debugPrint('--- SUNUCU CEVABI: ${response.statusCode} - ${response.body} ---');

    if (response.statusCode == 200 || response.statusCode == 201) {
      return;
    }

    throw Exception('Hata: ${response.statusCode} - ${response.body}');
  }
}
