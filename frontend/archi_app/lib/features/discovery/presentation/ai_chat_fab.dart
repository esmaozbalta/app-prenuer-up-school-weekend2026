import 'package:flutter/material.dart';

import '../../../core/theme/app_colors.dart';
import 'ai_chat_sheet.dart';

class AiChatFab extends StatelessWidget {
  const AiChatFab({super.key});

  @override
  Widget build(BuildContext context) {
    return FloatingActionButton(
      onPressed: () => showAiChatSheet(context),
      elevation: 0,
      highlightElevation: 0,
      backgroundColor: AppColors.surfaceElevated,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
        side: const BorderSide(color: AppColors.border),
      ),
      child: const Icon(
        Icons.auto_awesome_outlined,
        color: AppColors.textPrimary,
        size: 22,
      ),
    );
  }
}
