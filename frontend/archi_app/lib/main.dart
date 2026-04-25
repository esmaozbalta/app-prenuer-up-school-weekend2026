import 'package:flutter/material.dart';

import 'services/auth_api.dart';
import 'services/session_storage.dart';

const apiBaseUrl = 'http://localhost:5161';

void main() => runApp(const ArchiApp());

class ArchiApp extends StatelessWidget {
  const ArchiApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Archi',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF5B3FA8)),
        useMaterial3: true,
      ),
      home: const SessionBootstrapScreen(),
    );
  }
}

class SessionBootstrapScreen extends StatefulWidget {
  const SessionBootstrapScreen({super.key});

  @override
  State<SessionBootstrapScreen> createState() => _SessionBootstrapScreenState();
}

class _SessionBootstrapScreenState extends State<SessionBootstrapScreen> {
  final _sessionStorage = SessionStorage();

  bool _isLoading = true;
  String? _token;

  @override
  void initState() {
    super.initState();
    _restoreSession();
  }

  Future<void> _restoreSession() async {
    final token = await _sessionStorage.readToken();
    if (!mounted) {
      return;
    }
    setState(() {
      _token = token;
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    if (_token != null && _token!.isNotEmpty) {
      return HomeScreen(
        token: _token!,
        onLogout: () async {
          await _sessionStorage.clearToken();
          if (!mounted) {
            return;
          }
          setState(() {
            _token = null;
          });
        },
      );
    }

    return AuthScreen(
      onAuthenticated: (token) {
        setState(() {
          _token = token;
        });
      },
    );
  }
}

class AuthScreen extends StatefulWidget {
  const AuthScreen({super.key, required this.onAuthenticated});

  final ValueChanged<String> onAuthenticated;

  @override
  State<AuthScreen> createState() => _AuthScreenState();
}

class _AuthScreenState extends State<AuthScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  final _authApi = AuthApi(baseUrl: apiBaseUrl);
  final _sessionStorage = SessionStorage();

  bool _isLoading = false;
  String? _message;
  bool _isLoginMode = false;

  @override
  void dispose() {
    _emailController.dispose();
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() {
      _isLoading = true;
      _message = null;
    });

    try {
      final token = _isLoginMode
          ? await _authApi.login(
              email: _emailController.text.trim(),
              password: _passwordController.text,
            )
          : await _authApi.register(
              email: _emailController.text.trim(),
              username: _usernameController.text.trim(),
              password: _passwordController.text,
            );
      await _sessionStorage.saveToken(token);
      final tokenPreview = token.length > 16 ? '${token.substring(0, 16)}...' : token;
      setState(() {
        _message = _isLoginMode
            ? 'Giris basarili. Token: $tokenPreview'
            : 'Kayit basarili. Token: $tokenPreview';
      });
      widget.onAuthenticated(token);
    } catch (error) {
      setState(() {
        _message = 'Hata: $error';
      });
    } finally {
      setState(() {
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(_isLoginMode ? 'Archi Giris' : 'Archi Kayit')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              TextFormField(
                controller: _emailController,
                decoration: const InputDecoration(labelText: 'E-posta'),
                keyboardType: TextInputType.emailAddress,
                validator: (value) {
                  if (value == null || !value.contains('@')) {
                    return 'Gecerli bir e-posta girin.';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 12),
              if (!_isLoginMode) ...[
                TextFormField(
                  controller: _usernameController,
                  decoration: const InputDecoration(labelText: 'Kullanici adi'),
                  validator: (value) {
                    if (value == null || value.trim().length < 3) {
                      return 'En az 3 karakter olmali.';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 12),
              ],
              TextFormField(
                controller: _passwordController,
                decoration: const InputDecoration(labelText: 'Sifre'),
                obscureText: true,
                validator: (value) {
                  if (value == null || value.length < 8) {
                    return 'Sifre en az 8 karakter olmali.';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 20),
              FilledButton(
                onPressed: _isLoading ? null : _submit,
                child: _isLoading
                    ? const SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : Text(_isLoginMode ? 'Giris Yap' : 'Kayit Ol'),
              ),
              const SizedBox(height: 8),
              TextButton(
                onPressed: _isLoading
                    ? null
                    : () {
                        setState(() {
                          _isLoginMode = !_isLoginMode;
                          _message = null;
                        });
                      },
                child: Text(
                  _isLoginMode
                      ? 'Hesabin yok mu? Kayit ol'
                      : 'Zaten hesabin var mi? Giris yap',
                ),
              ),
              const SizedBox(height: 12),
              if (_message != null)
                SelectableText.rich(
                  TextSpan(
                    text: _message!,
                    style: TextStyle(
                      color: _message!.startsWith('Hata')
                          ? Colors.red
                          : Colors.green,
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }
}

class HomeScreen extends StatefulWidget {
  const HomeScreen({
    super.key,
    required this.token,
    required this.onLogout,
  });

  final String token;
  final Future<void> Function() onLogout;

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final _api = AuthApi(baseUrl: apiBaseUrl);
  bool _isLoading = true;
  bool _saving = false;
  String? _error;
  UserProfile? _profile;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });
    try {
      final profile = await _api.fetchProfile(accessToken: widget.token);
      if (!mounted) {
        return;
      }
      setState(() {
        _profile = profile;
        _isLoading = false;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _error = 'Hata: $error';
        _isLoading = false;
      });
    }
  }

  Future<void> _onPrivacyChanged(bool value) async {
    setState(() {
      _saving = true;
      _error = null;
    });
    try {
      final profile = await _api.updatePrivacy(
        accessToken: widget.token,
        isPrivate: value,
      );
      if (!mounted) {
        return;
      }
      setState(() {
        _profile = profile;
        _saving = false;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _error = 'Hata: $error';
        _saving = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final tokenPreview = widget.token.length > 16
        ? '${widget.token.substring(0, 16)}...'
        : widget.token;
    return Scaffold(
      appBar: AppBar(title: const Text('Archi Home')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: _isLoading
            ? const Center(child: CircularProgressIndicator())
            : Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text('Hosgeldin, ${_profile?.username ?? ''}'),
                  const SizedBox(height: 8),
                  Text('E-posta: ${_profile?.email ?? ''}'),
                  const SizedBox(height: 8),
                  Text('Session. Token: $tokenPreview'),
                  const SizedBox(height: 12),
                  SwitchListTile(
                    title: const Text('Profil gizliligi (Private)'),
                    subtitle: const Text('Acik: Herkes, Kapali: Sadece sen'),
                    value: _profile?.isPrivate ?? false,
                    onChanged: _saving ? null : _onPrivacyChanged,
                  ),
                  if (_saving)
                    const Padding(
                      padding: EdgeInsets.only(top: 8),
                      child: LinearProgressIndicator(),
                    ),
                  if (_error != null) ...[
                    const SizedBox(height: 8),
                    SelectableText.rich(
                      TextSpan(
                        text: _error!,
                        style: const TextStyle(color: Colors.red),
                      ),
                    ),
                  ],
                  const Spacer(),
                  FilledButton(
                    onPressed: widget.onLogout,
                    child: const Text('Cikis Yap'),
                  ),
                ],
              ),
      ),
    );
  }
}
