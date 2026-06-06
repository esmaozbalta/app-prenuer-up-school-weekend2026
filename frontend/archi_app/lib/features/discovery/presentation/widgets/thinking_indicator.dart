import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class ThinkingIndicator extends StatefulWidget {
  const ThinkingIndicator({super.key});

  @override
  State<ThinkingIndicator> createState() => _ThinkingIndicatorState();
}

class _ThinkingIndicatorState extends State<ThinkingIndicator>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1400),
    )..repeat();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: Alignment.centerLeft,
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        decoration: BoxDecoration(
          color: AppColors.surfaceElevated,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: AppColors.border),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            AnimatedBuilder(
              animation: _controller,
              builder: (context, child) {
                return Row(
                  children: List.generate(3, (index) {
                    final phase = (_controller.value + index * 0.2) % 1.0;
                    final opacity = 0.25 + (phase < 0.5 ? phase : 1 - phase) * 1.5;
                    return Container(
                      margin: EdgeInsets.only(right: index == 2 ? 10 : 5),
                      width: 5,
                      height: 5,
                      decoration: BoxDecoration(
                        color: AppColors.textMuted.withValues(alpha: opacity.clamp(0.2, 1.0)),
                        shape: BoxShape.circle,
                      ),
                    );
                  }),
                );
              },
            ),
            Text(
              'Archi is thinking...',
              style: AppTextStyles.bodySmall.copyWith(
                color: AppColors.textMuted,
                letterSpacing: 0.2,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
