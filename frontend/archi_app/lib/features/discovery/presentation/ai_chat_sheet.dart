import 'package:flutter/material.dart';

import '../../../core/config/app_config.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../services/session_storage.dart';
import '../data/models/chat_message.dart';
import '../data/services/archive_api.dart';
import '../data/services/chat_api.dart';
import 'widgets/suggestion_card.dart';
import 'widgets/thinking_indicator.dart';

Future<void> showAiChatSheet(BuildContext context) {
  return showModalBottomSheet<void>(
    context: context,
    isScrollControlled: true,
    backgroundColor: Colors.transparent,
    builder: (context) => const AiChatSheet(),
  );
}

class AiChatSheet extends StatefulWidget {
  const AiChatSheet({super.key});

  @override
  State<AiChatSheet> createState() => _AiChatSheetState();
}

class _AiChatSheetState extends State<AiChatSheet> {
  final _inputController = TextEditingController();
  final _scrollController = ScrollController();
  final _sessionStorage = SessionStorage();
  final _chatApi = ChatApi(baseUrl: AppConfig.apiBaseUrl);
  final _archiveApi = ArchiveApi(baseUrl: AppConfig.apiBaseUrl);

  final List<ChatMessage> _messages = [
    ChatMessage.assistantText(
      'Bana bir mod, tür veya tema söyle — sana bir film, dizi, kitap veya oyun önereyim.',
    ),
  ];

  bool _isLoading = false;
  String? _addingSuggestionId;

  @override
  void dispose() {
    _inputController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  Future<void> _sendMessage() async {
    final text = _inputController.text.trim();
    if (text.isEmpty || _isLoading) return;

    setState(() {
      _messages.add(ChatMessage.user(text));
      _messages.add(ChatMessage.thinking());
      _isLoading = true;
      _inputController.clear();
    });
    _scrollToBottom();

    try {
      final token = await _sessionStorage.readToken();
      if (token == null || token.isEmpty) {
        throw Exception('Oturum bulunamadi.');
      }

      final result = await _chatApi.sendMessage(accessToken: token, message: text);

      if (!mounted) return;

      setState(() {
        _messages.removeWhere((m) => m.kind == ChatMessageKind.thinking);
        switch (result) {
          case ChatSuggestionResult(:final suggestion):
            _messages.add(ChatMessage.suggestion(suggestion));
          case ChatTextResult(:final message):
            _messages.add(ChatMessage.assistantText(message));
        }
        _isLoading = false;
      });
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _messages.removeWhere((m) => m.kind == ChatMessageKind.thinking);
        _messages.add(
          ChatMessage.assistantText(
            'The assistant is currently taking a short break. Please try again later.',
          ),
        );
        _isLoading = false;
      });
    }

    _scrollToBottom();
  }

  Future<void> _addToArchive(ChatMessage message) async {
    final suggestion = message.suggestion;
    if (suggestion == null || _addingSuggestionId != null) return;

    setState(() => _addingSuggestionId = message.id);

    try {
      final token = await _sessionStorage.readToken();
      if (token == null || token.isEmpty) {
        throw Exception('Oturum bulunamadi.');
      }

      await _archiveApi.addFromSuggestion(
        accessToken: token,
        suggestion: suggestion,
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('${suggestion.title} arsive eklendi.'),
          behavior: SnackBarBehavior.floating,
          backgroundColor: AppColors.surfaceElevated,
        ),
      );
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(error.toString().replaceFirst('Exception: ', '')),
          behavior: SnackBarBehavior.floating,
          backgroundColor: AppColors.card,
        ),
      );
    } finally {
      if (mounted) {
        setState(() => _addingSuggestionId = null);
      }
    }
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!_scrollController.hasClients) return;
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent,
        duration: const Duration(milliseconds: 280),
        curve: Curves.easeOut,
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.viewInsetsOf(context).bottom;

    return Padding(
      padding: EdgeInsets.only(bottom: bottomInset),
      child: DraggableScrollableSheet(
        initialChildSize: 0.88,
        minChildSize: 0.55,
        maxChildSize: 0.95,
        expand: false,
        builder: (context, sheetController) {
          return Container(
            decoration: const BoxDecoration(
              color: AppColors.surface,
              borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
              border: Border(top: BorderSide(color: AppColors.border)),
            ),
            child: Column(
              children: [
                const SizedBox(height: 10),
                Container(
                  width: 36,
                  height: 4,
                  decoration: BoxDecoration(
                    color: AppColors.border,
                    borderRadius: BorderRadius.circular(999),
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.fromLTRB(20, 16, 12, 8),
                  child: Row(
                    children: [
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text('Archi Asistanı', style: AppTextStyles.heading2),
                            const SizedBox(height: 2),
                            Text(
                              'Akıllı Keşif',
                              style: AppTextStyles.caption,
                            ),
                          ],
                        ),
                      ),
                      IconButton(
                        onPressed: () => Navigator.of(context).pop(),
                        icon: const Icon(Icons.close_rounded, color: AppColors.textMuted),
                      ),
                    ],
                  ),
                ),
                const Divider(height: 1, color: AppColors.border),
                Expanded(
                  child: ListView.builder(
                    controller: sheetController,
                    padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
                    itemCount: _messages.length,
                    itemBuilder: (context, index) {
                      final message = _messages[index];
                      return switch (message.kind) {
                        ChatMessageKind.user => _UserBubble(text: message.text ?? ''),
                        ChatMessageKind.assistantText => _AssistantBubble(text: message.text ?? ''),
                        ChatMessageKind.thinking => const ThinkingIndicator(),
                        ChatMessageKind.suggestion => SuggestionCard(
                          suggestion: message.suggestion!,
                          isAdding: _addingSuggestionId == message.id,
                          onAdd: () => _addToArchive(message),
                        ),
                      };
                    },
                  ),
                ),
                _ChatInputBar(
                  controller: _inputController,
                  isLoading: _isLoading,
                  onSend: _sendMessage,
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}

class _UserBubble extends StatelessWidget {
  const _UserBubble({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: Alignment.centerRight,
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        constraints: BoxConstraints(maxWidth: MediaQuery.sizeOf(context).width * 0.78),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 11),
        decoration: BoxDecoration(
          color: AppColors.primary.withValues(alpha: 0.18),
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: AppColors.primary.withValues(alpha: 0.35)),
        ),
        child: Text(text, style: AppTextStyles.body),
      ),
    );
  }
}

class _AssistantBubble extends StatelessWidget {
  const _AssistantBubble({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: Alignment.centerLeft,
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        constraints: BoxConstraints(maxWidth: MediaQuery.sizeOf(context).width * 0.82),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 11),
        decoration: BoxDecoration(
          color: AppColors.surfaceElevated,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: AppColors.border),
        ),
        child: Text(
          text,
          style: AppTextStyles.bodySmall.copyWith(color: AppColors.textSecondary),
        ),
      ),
    );
  }
}

class _ChatInputBar extends StatelessWidget {
  const _ChatInputBar({
    required this.controller,
    required this.isLoading,
    required this.onSend,
  });

  final TextEditingController controller;
  final bool isLoading;
  final VoidCallback onSend;

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      top: false,
      child: Padding(
        padding: const EdgeInsets.fromLTRB(20, 8, 20, 14),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Expanded(
              child: TextField(
                controller: controller,
                minLines: 1,
                maxLines: 4,
                style: AppTextStyles.body,
                textInputAction: TextInputAction.send,
                onSubmitted: (_) => onSend(),
                decoration: InputDecoration(
                  hintText: 'Nasıl bir öneri istediğini anlat...',
                  hintStyle: AppTextStyles.bodySmall,
                  filled: false,
                  contentPadding: const EdgeInsets.symmetric(vertical: 12),
                  border: const UnderlineInputBorder(
                    borderSide: BorderSide(color: AppColors.border),
                  ),
                  enabledBorder: const UnderlineInputBorder(
                    borderSide: BorderSide(color: AppColors.border),
                  ),
                  focusedBorder: const UnderlineInputBorder(
                    borderSide: BorderSide(color: AppColors.textPrimary, width: 1.2),
                  ),
                ),
              ),
            ),
            const SizedBox(width: 8),
            Material(
              color: Colors.transparent,
              child: InkWell(
                onTap: isLoading ? null : onSend,
                borderRadius: BorderRadius.circular(999),
                child: Container(
                  width: 44,
                  height: 44,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    border: Border.all(
                      color: isLoading ? AppColors.border : AppColors.textPrimary,
                    ),
                  ),
                  child: Icon(
                    Icons.arrow_upward_rounded,
                    size: 20,
                    color: isLoading ? AppColors.textMuted : AppColors.textPrimary,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
