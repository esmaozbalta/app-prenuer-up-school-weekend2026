import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/ai_suggestion.dart';

class ChatApi {
  ChatApi({required this.baseUrl});

  final String baseUrl;

  Future<ChatApiResult> sendMessage({
    required String accessToken,
    required String message,
  }) async {
    final uri = Uri.parse('$baseUrl/api/v1/chat');
    final response = await http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $accessToken',
      },
      body: jsonEncode({'message': message}),
    );

    if (response.statusCode == 401) {
      throw Exception('Oturum suresi doldu. Lutfen tekrar giris yapin.');
    }

    if (response.statusCode != 200) {
      throw Exception('Asistan yanit veremedi (HTTP ${response.statusCode}).');
    }

    final map = jsonDecode(response.body) as Map<String, dynamic>;
    final type = (map['type'] ?? '') as String;

    if (type == 'suggestion' && map['suggestions'] is List) {
      final list = map['suggestions'] as List;
      final suggestions = list
          .map((e) => AiSuggestion.fromJson(e as Map<String, dynamic>))
          .toList();
      return ChatApiResult.suggestions(suggestions);
    }

    return ChatApiResult.message(
      (map['message'] as String?) ??
          'The assistant is currently taking a short break. Please try again later.',
    );
  }
}

sealed class ChatApiResult {
  const ChatApiResult();
  factory ChatApiResult.suggestions(List<AiSuggestion> suggestions) = ChatSuggestionsResult;
  factory ChatApiResult.message(String message) = ChatTextResult;
}

final class ChatSuggestionsResult extends ChatApiResult {
  const ChatSuggestionsResult(this.suggestions);
  final List<AiSuggestion> suggestions;
}

final class ChatTextResult extends ChatApiResult {
  const ChatTextResult(this.message);
  final String message;
}
