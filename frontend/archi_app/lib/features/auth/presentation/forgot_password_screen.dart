import 'package:flutter/material.dart';
import '../../../core/config/app_config.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../services/auth_api.dart';

class ForgotPasswordScreen extends StatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  State<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends State<ForgotPasswordScreen> {
  final _emailController = TextEditingController();
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();

  final _authApi = AuthApi(baseUrl: AppConfig.apiBaseUrl);

  bool _isLoading = false;
  String _errorMessage = '';

  Future<void> _resetPassword() async {
    final email = _emailController.text.trim();
    final username = _usernameController.text.trim();
    final newPassword = _passwordController.text.trim();

    if (email.isEmpty || !email.contains('@')) {
      setState(() => _errorMessage = 'Geçerli bir e-posta adresi giriniz.');
      return;
    }

    if (username.isEmpty) {
      setState(() => _errorMessage = 'Kullanıcı adınızı giriniz.');
      return;
    }

    if (newPassword.length < 8) {
      setState(() => _errorMessage = 'Yeni şifreniz en az 8 karakter olmalıdır.');
      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = '';
    });

    try {
      await _authApi.resetPassword(
        email: email, 
        username: username, 
        newPassword: newPassword,
      );
      
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Şifreniz başarıyla yenilendi! 🎉 Giriş yapabilirsiniz.'), backgroundColor: Colors.green),
        );
        Navigator.of(context).pop(); // Başarılıysa Login ekranına geri dön
      }
    } catch (e) {
      setState(() => _errorMessage = e.toString().replaceAll('Exception: ', ''));
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  void dispose() {
    _emailController.dispose();
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Şifre Sıfırlama'),
        backgroundColor: Colors.transparent,
        elevation: 0,
      ),
      body: SafeArea(
        child: SingleChildScrollView( // Klavye açılınca taşma olmasın diye eklendi
          padding: const EdgeInsets.all(24.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Şifreni mi Unuttun?',
                style: AppTextStyles.heading1,
              ),
              const SizedBox(height: 8),
              Text(
                'Hesabını doğrulamak için kayıtlı e-posta adresini ve kullanıcı adını girerek yeni şifreni belirleyebilirsin.',
                style: AppTextStyles.body.copyWith(color: AppColors.textMuted),
              ),
              const SizedBox(height: 32),

              if (_errorMessage.isNotEmpty) ...[
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.redAccent.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.redAccent),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.error_outline, color: Colors.redAccent),
                      const SizedBox(width: 8),
                      Expanded(child: Text(_errorMessage, style: AppTextStyles.bodySmall.copyWith(color: Colors.redAccent))),
                    ],
                  ),
                ),
                const SizedBox(height: 24),
              ],

              // --- E-POSTA GİRİŞİ ---
              TextField(
                controller: _emailController,
                style: AppTextStyles.body,
                decoration: InputDecoration(
                  labelText: 'E-posta Adresi',
                  labelStyle: AppTextStyles.bodySmall.copyWith(color: AppColors.textMuted),
                  filled: true,
                  fillColor: AppColors.surfaceElevated,
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                ),
                keyboardType: TextInputType.emailAddress,
              ),
              const SizedBox(height: 16),

              // --- KULLANICI ADI GİRİŞİ ---
              TextField(
                controller: _usernameController,
                style: AppTextStyles.body,
                decoration: InputDecoration(
                  labelText: 'Kullanıcı Adı',
                  labelStyle: AppTextStyles.bodySmall.copyWith(color: AppColors.textMuted),
                  filled: true,
                  fillColor: AppColors.surfaceElevated,
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                ),
              ),
              const SizedBox(height: 16),

              // --- YENİ ŞİFRE GİRİŞİ ---
              TextField(
                controller: _passwordController,
                style: AppTextStyles.body,
                obscureText: true,
                decoration: InputDecoration(
                  labelText: 'Yeni Şifre (En az 8 karakter)',
                  labelStyle: AppTextStyles.bodySmall.copyWith(color: AppColors.textMuted),
                  filled: true,
                  fillColor: AppColors.surfaceElevated,
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                ),
              ),
              const SizedBox(height: 32),

              // --- BUTON ---
              SizedBox(
                width: double.infinity,
                height: 52,
                child: FilledButton(
                  onPressed: _isLoading ? null : _resetPassword,
                  style: FilledButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                  ),
                  child: _isLoading 
                      ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                      : Text('Şifremi Yenile', style: AppTextStyles.body.copyWith(color: Colors.white, fontWeight: FontWeight.bold)),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}