import '../models/ai_suggestion.dart';

enum ChatMessageKind { user, assistantText, suggestion, thinking }

class ChatMessage {
  const ChatMessage({
    required this.id,
    required this.kind,
    this.text,
    this.suggestion,
  });

  final String id;
  final ChatMessageKind kind;
  final String? text;
  final AiSuggestion? suggestion;

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

  factory ChatMessage.suggestion(AiSuggestion suggestion) => ChatMessage(
    id: DateTime.now().microsecondsSinceEpoch.toString(),
    kind: ChatMessageKind.suggestion,
    suggestion: suggestion,
  );

  factory ChatMessage.thinking() => ChatMessage(
    id: 'thinking',
    kind: ChatMessageKind.thinking,
  );
}
