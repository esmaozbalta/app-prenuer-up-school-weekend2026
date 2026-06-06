import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../data/models/ai_suggestion.dart';

class SuggestionCard extends StatelessWidget {
  const SuggestionCard({
    super.key,
    required this.suggestion,
    required this.onAdd,
    this.isAdding = false,
  });

  final AiSuggestion suggestion;
  final VoidCallback onAdd;
  final bool isAdding;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      decoration: BoxDecoration(
        color: AppColors.card,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.border),
      ),
      clipBehavior: Clip.antiAlias,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _CoverImage(imageUrl: suggestion.imageUrl),
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 14, 16, 16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  suggestion.category.toUpperCase(),
                  style: AppTextStyles.label.copyWith(color: AppColors.textMuted),
                ),
                const SizedBox(height: 6),
                Text(suggestion.title, style: AppTextStyles.heading2),
                if (suggestion.year.isNotEmpty) ...[
                  const SizedBox(height: 8),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      border: Border.all(color: AppColors.border),
                      borderRadius: BorderRadius.circular(999),
                    ),
                    child: Text(suggestion.year, style: AppTextStyles.caption),
                  ),
                ],
                const SizedBox(height: 12),
                Text(
                  suggestion.description,
                  style: AppTextStyles.bodySmall.copyWith(height: 1.55),
                  maxLines: 5,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 16),
                SizedBox(
                  width: double.infinity,
                  child: OutlinedButton(
                    onPressed: isAdding ? null : onAdd,
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppColors.textPrimary,
                      side: const BorderSide(color: AppColors.border),
                      padding: const EdgeInsets.symmetric(vertical: 14),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10),
                      ),
                    ),
                    child: isAdding
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : Text(
                            'Add to Archi',
                            style: AppTextStyles.body.copyWith(
                              fontWeight: FontWeight.w500,
                              letterSpacing: 0.3,
                            ),
                          ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _CoverImage extends StatelessWidget {
  const _CoverImage({required this.imageUrl});

  final String imageUrl;

  static const _placeholder = 'https://via.placeholder.com/600x340/1B1824/9490B0?text=Archi';

  @override
  Widget build(BuildContext context) {
    final resolved = imageUrl.trim().isEmpty ? _placeholder : imageUrl;

    return AspectRatio(
      aspectRatio: 16 / 9,
      child: CachedNetworkImage(
        imageUrl: resolved,
        fit: BoxFit.cover,
        placeholder: (context, url) => Container(color: AppColors.surfaceElevated),
        errorWidget: (context, url, error) => Container(
          color: AppColors.surfaceElevated,
          alignment: Alignment.center,
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(Icons.image_outlined, color: AppColors.textMuted.withValues(alpha: 0.7)),
              const SizedBox(height: 6),
              Text('No image', style: AppTextStyles.caption),
            ],
          ),
        ),
      ),
    );
  }
}
