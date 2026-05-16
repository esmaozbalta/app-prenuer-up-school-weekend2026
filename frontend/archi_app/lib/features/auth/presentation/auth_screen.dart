import 'package:flutter/material.dart';

import '../../../core/config/app_config.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../services/auth_api.dart';
import '../../../services/session_storage.dart';

class AuthScreen extends StatefulWidget {
  const AuthScreen({super.key, required this.onAuthenticated});

  final ValueChanged<String> onAuthenticated;

  @override
  State<AuthScreen> createState() => _AuthScreenState();
}

class _AuthScreenState extends State<AuthScreen> {
  bool _isLoginMode = true;
  bool _isLoading = false;
  String? _message;

  final _api = AuthApi(baseUrl: AppConfig.apiBaseUrl);
  final _sessionStorage = SessionStorage();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _usernameController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    _usernameController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    final email = _emailController.text.trim();
    final password = _passwordController.text.trim();
    final username = _usernameController.text.trim();

    if (email.isEmpty || !email.contains('@')) {
      setState(() => _message = 'Lutfen gecerli bir e-posta gir.');
      return;
    }
    if (password.length < 8) {
      setState(() => _message = 'Sifre en az 8 karakter olmali.');
      return;
    }
    if (!_isLoginMode && username.length < 3) {
      setState(() => _message = 'Kullanici adi en az 3 karakter olmali.');
      return;
    }

    setState(() {
      _isLoading = true;
      _message = null;
    });

    try {
      if (_isLoginMode) {
        final token = await _api.login(email: email, password: password);
        await _sessionStorage.saveToken(token);
        if (!mounted) {
          return;
        }
        widget.onAuthenticated(token);
      } else {
        await _api.register(
          email: email,
          username: username,
          password: password,
        );
        if (!mounted) {
          return;
        }
        await _sessionStorage.clearToken();
        if (!mounted) {
          return;
        }
        _showRegistrationSuccessSnackBar(context);
        setState(() {
          _isLoginMode = true;
          _message = null;
          _passwordController.clear();
          _usernameController.clear();
        });
      }
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _message = 'Hata: $error';
      });
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }

  void _showRegistrationSuccessSnackBar(BuildContext context) {
    final messenger = ScaffoldMessenger.maybeOf(context);
    if (messenger == null) {
      return;
    }
    messenger.hideCurrentSnackBar();
    messenger.showSnackBar(
      SnackBar(
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.fromLTRB(16, 0, 16, 24),
        elevation: 8,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
        backgroundColor: AppColors.surfaceElevated,
        duration: const Duration(seconds: 4),
        content: Row(
          children: [
            Icon(Icons.check_circle_rounded, color: AppColors.success, size: 22),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                'Kayıt başarılı! Lütfen giriş yapın.',
                style: AppTextStyles.bodySmall.copyWith(color: AppColors.textPrimary),
              ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.deepSurface,
      body: Stack(
        children: [
          Positioned(
            top: -180,
            left: -60,
            right: -60,
            child: IgnorePointer(
              child: Container(
                height: 430,
                decoration: BoxDecoration(
                  gradient: RadialGradient(
                    colors: [
                      AppColors.primary.withValues(alpha: 0.28),
                      AppColors.primary.withValues(alpha: 0.10),
                      Colors.transparent,
                    ],
                    stops: const [0.0, 0.45, 1],
                  ),
                ),
              ),
            ),
          ),
          SafeArea(
            child: Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
                child: ConstrainedBox(
                  constraints: const BoxConstraints(maxWidth: 420),
                  child: AnimatedSwitcher(
                    duration: const Duration(milliseconds: 260),
                    switchInCurve: Curves.easeOutCubic,
                    switchOutCurve: Curves.easeInCubic,
                    transitionBuilder: (child, animation) {
                      final slide = Tween<Offset>(
                        begin: const Offset(0.08, 0),
                        end: Offset.zero,
                      ).animate(animation);
                      return FadeTransition(
                        opacity: animation,
                        child: SlideTransition(position: slide, child: child),
                      );
                    },
                    child: _AuthFormCard(
                      key: ValueKey<bool>(_isLoginMode),
                      isLoginMode: _isLoginMode,
                      isLoading: _isLoading,
                      message: _message,
                      emailController: _emailController,
                      passwordController: _passwordController,
                      usernameController: _usernameController,
                      onSubmit: _submit,
                      onToggleMode: () {
                        setState(() {
                          _isLoginMode = !_isLoginMode;
                          _message = null;
                        });
                      },
                    ),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _AuthFormCard extends StatelessWidget {
  const _AuthFormCard({
    super.key,
    required this.isLoginMode,
    required this.isLoading,
    required this.message,
    required this.emailController,
    required this.passwordController,
    required this.usernameController,
    required this.onSubmit,
    required this.onToggleMode,
  });

  final bool isLoginMode;
  final bool isLoading;
  final String? message;
  final TextEditingController emailController;
  final TextEditingController passwordController;
  final TextEditingController usernameController;
  final VoidCallback onSubmit;
  final VoidCallback onToggleMode;

  @override
  Widget build(BuildContext context) {
    return Column(
      key: key,
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(
          'Archi',
          textAlign: TextAlign.center,
          style: AppTextStyles.display.copyWith(
            fontSize: 32,
            color: Colors.white,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Kişisel kütüphaneni inşa etmeye başla',
          textAlign: TextAlign.center,
          style: AppTextStyles.body.copyWith(color: AppColors.textSecondary),
        ),
        const SizedBox(height: 24),
        _AuthField(
          controller: emailController,
          hintText: 'E-posta',
          keyboardType: TextInputType.emailAddress,
        ),
        const SizedBox(height: 12),
        if (!isLoginMode) ...[
          _AuthField(controller: usernameController, hintText: 'Kullanici adi'),
          const SizedBox(height: 12),
        ],
        _AuthField(
          controller: passwordController,
          hintText: 'Şifre',
          obscureText: true,
        ),
        const SizedBox(height: 10),
        Align(
          alignment: Alignment.centerRight,
          child: TextButton(
            onPressed: isLoading ? null : () {},
            child: Text(
              'Şifremi Unuttum',
              style: AppTextStyles.bodySmall.copyWith(color: AppColors.primaryStrong),
            ),
          ),
        ),
        const SizedBox(height: 8),
        SizedBox(
          height: 48,
          child: FilledButton(
            style: FilledButton.styleFrom(
              backgroundColor: AppColors.primary,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(14),
              ),
            ),
            onPressed: isLoading ? null : onSubmit,
            child: isLoading
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : Text(
                    isLoginMode ? 'Giriş Yap' : 'Kayıt Ol',
                    style: AppTextStyles.body.copyWith(color: Colors.white),
                  ),
          ),
        ),
        const SizedBox(height: 12),
        TextButton(
          onPressed: isLoading ? null : onToggleMode,
          child: Text(
            isLoginMode ? 'Hesabın yok mu? Kayıt ol' : 'Zaten hesabın var mi? Giriş yap',
            style: AppTextStyles.bodySmall.copyWith(color: AppColors.primaryStrong),
          ),
        ),
        if (message != null) ...[
          const SizedBox(height: 8),
          Text(
            message!,
            textAlign: TextAlign.center,
            style: AppTextStyles.caption.copyWith(
              color: message!.startsWith('Hata') ? Colors.red.shade300 : AppColors.success,
            ),
          ),
        ],
      ],
    );
  }
}

class _AuthField extends StatefulWidget {
  const _AuthField({
    required this.controller,
    required this.hintText,
    this.keyboardType,
    this.obscureText = false,
  });

  final TextEditingController controller;
  final String hintText;
  final TextInputType? keyboardType;
  final bool obscureText;

  @override
  State<_AuthField> createState() => _AuthFieldState();
}

class _AuthFieldState extends State<_AuthField> {
  bool _isFocused = false;

  @override
  Widget build(BuildContext context) {
    return Focus(
      onFocusChange: (focused) {
        setState(() {
          _isFocused = focused;
        });
      },
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 180),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(14),
          boxShadow: _isFocused
              ? [
                  BoxShadow(
                    color: AppColors.primary.withValues(alpha: 0.35),
                    blurRadius: 16,
                    spreadRadius: 1,
                  ),
                ]
              : null,
        ),
        child: TextField(
          controller: widget.controller,
          keyboardType: widget.keyboardType,
          obscureText: widget.obscureText,
          style: AppTextStyles.body.copyWith(color: AppColors.textPrimary),
          decoration: InputDecoration(
            hintText: widget.hintText,
            hintStyle: AppTextStyles.bodySmall,
            filled: true,
            fillColor: AppColors.surfaceElevated,
            contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(14),
              borderSide: const BorderSide(color: AppColors.border),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(14),
              borderSide: const BorderSide(color: AppColors.border),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(14),
              borderSide: const BorderSide(color: AppColors.primary, width: 1.4),
            ),
          ),
        ),
      ),
    );
  }
}
