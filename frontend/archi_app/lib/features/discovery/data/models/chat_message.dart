import '../models/ai_suggestion.dart';

enum ChatMessageKind { user, assistantText, suggestion, thinking }

class ChatMessage {
  const ChatMessage({
    required this.id,
    required this.kind,
    this.text,
    this.suggestions, // <-- BURASI DÜZELTİLDİ (this.suggestion'dı)
  });

  final String id;
  final ChatMessageKind kind;
  final String? text;
  final List<AiSuggestion>? suggestions; // <-- BURASI LİSTE OLARAK GÜNCELLENDİ

  factory ChatMessage.user(String text) => ChatMessage(
    id: DateTime.now().microsecondsSinceEpoch.toString(),
    kind: ChatMessageKind.user,
    text: text,
  );

  factory ChatMessage.assistantText(String text) => ChatMessage(
    id: DateTime.now().microsecondsSinceEpoch.toString(),
    kind: ChatMessageKind.assistantText,
    text: text,
  );

  factory ChatMessage.suggestions(List<AiSuggestion> suggestions) => ChatMessage(
    id: DateTime.now().microsecondsSinceEpoch.toString(),
    kind: ChatMessageKind.suggestion,
    suggestions: suggestions,
  );

  factory ChatMessage.thinking() => ChatMessage(
    id: 'thinking',
    kind: ChatMessageKind.thinking,
  );
}