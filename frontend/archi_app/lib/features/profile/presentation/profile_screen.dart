import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_staggered_grid_view/flutter_staggered_grid_view.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../services/session_storage.dart';

// --- GERÇEK VERİ MODELİMİZ ---
class ArchiveItemModel {
  final String externalId;
  final String title;
  final String category;
  final String imageUrl;
  final String year;
  final String status;

  ArchiveItemModel({
    required this.externalId,
    required this.title,
    required this.category,
    required this.imageUrl,
    required this.year,
    required this.status,
  });

  factory ArchiveItemModel.fromJson(Map<String, dynamic> json) {
    String parsedImageUrl = json['imageUrl'] ?? json['ImageUrl'] ?? '';

    if (parsedImageUrl.isNotEmpty && parsedImageUrl.startsWith('/')) {
      parsedImageUrl = 'https://image.tmdb.org/t/p/w500$parsedImageUrl';
    }

    return ArchiveItemModel(
      externalId: json['externalId']?.toString() ?? '',
      title: json['title'] ?? '',
      category: json['category'] ?? '',
      imageUrl: parsedImageUrl, 
      year: json['year']?.toString() ?? '', 
      status: json['status']?.toString() ?? '', 
    );
  }
}

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  String _username = 'Yükleniyor...';
  String _email = '';
  List<ArchiveItemModel> _allArchives = [];
  bool _isLoading = true;
  bool _isLoggedIn = false;

  String _selectedCategory = 'Hepsi';
  static const _categories = <String>[
    'Hepsi',
    'Filmler',
    'Kitaplar', 
    'Diziler',
    'Oyunlar',  
  ];

  @override
  void initState() {
    super.initState();
    _loadUserProfile();
  }

  Future<void> _loadUserProfile() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token'); 
    final savedEmail = prefs.getString('email');
    
    setState(() {
      _isLoggedIn = token != null && token.isNotEmpty;
      _email = savedEmail ?? ''; 
      _username = 'Yükleniyor...'; 
    });

    if (_isLoggedIn) {
      // --- BACKEND'DEN GERÇEK PROFİL BİLGİLERİNİ ÇEKİYORUZ ---
      try {
        final url = Uri.parse('http://localhost:5161/api/v1/profile');
        final response = await http.get(
          url,
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer $token', 
          },
        );

        if (response.statusCode == 200) {
          final data = jsonDecode(response.body);
          
          // Backend'in döndüğü veriye göre gerçek username'i yakala
          final realUsername = data['username'] ?? data['Username'] ?? data['userName'];
          
          if (realUsername != null) {
            setState(() {
              _username = realUsername.toString();
            });
            // Yanlış olan eski veriyi (esmaozbalta333) silip GERÇEK olanı (esmacik) telefona kaydedelim
            await prefs.setString('Username', _username);
          } else {
            // Eğer backend username dönmezse lokalde ne varsa onu göster
            setState(() => _username = prefs.getString('Username') ?? 'Kullanıcı');
          }
        } else {
          setState(() => _username = prefs.getString('Username') ?? 'Kullanıcı');
        }
      } catch (e) {
        debugPrint('Profil çekme hatası: $e');
        setState(() => _username = prefs.getString('Username') ?? 'Kullanıcı');
      }

      // Profili çektikten sonra arşivleri çekmeye başla
      _fetchArchives();
    } else {
      setState(() => _isLoading = false);
    }
  }

  Future<void> _fetchArchives() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final token = prefs.getString('token') ?? ''; 

      final url = Uri.parse('http://localhost:5161/api/v1/archive');
      final response = await http.get(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token', 
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        setState(() {
          _allArchives = data.map((json) => ArchiveItemModel.fromJson(json)).toList();
          _isLoading = false;
        });
      } else {
        setState(() => _isLoading = false);
      }
    } catch (e) {
      setState(() => _isLoading = false);
    }
  }

  void _showUpdateBottomSheet(BuildContext context, ArchiveItemModel item) {
    Future<void> updateStatus(String newStatus) async {
      Navigator.of(context).pop(); 
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('${item.title} taşınıyor...')),
      );

      try {
        final prefs = await SharedPreferences.getInstance();
        final token = prefs.getString('token') ?? '';

        final url = Uri.parse('http://localhost:5161/api/v1/archive');
        final response = await http.post(
          url,
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer $token', 
          },
          body: jsonEncode({
            'externalId': item.externalId,
            'title': item.title,
            'category': item.category,
            'imageUrl': item.imageUrl,
            'year': item.year,
            'status': newStatus, 
          }),
        );

        if (response.statusCode == 200 || response.statusCode == 201) {
          if (context.mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Başarıyla güncellendi! 🎉'), backgroundColor: Colors.green),
            );
            _fetchArchives();
          }
        }
      } catch (e) {
        debugPrint('Güncelleme hatası: $e');
      }
    }

    showModalBottomSheet<void>(
      context: context,
      backgroundColor: AppColors.surfaceElevated,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(18)),
      ),
      builder: (context) {
        return SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 16),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(item.title, style: AppTextStyles.heading3),
                const SizedBox(height: 12),
                _BottomSheetActionTile(
                  icon: Icons.playlist_add_check_circle_outlined,
                  title: 'Listeme Ekle',
                  subtitle: 'Okuyacağım / İzleyecegim',
                  onTap: () => updateStatus("Listem"),
                ),
                _BottomSheetActionTile(
                  icon: Icons.timelapse_rounded,
                  title: 'Şu an Yapıyorum',
                  subtitle: 'Okuyorum / İzliyorum',
                  onTap: () => updateStatus("Akış"),
                ),
                _BottomSheetActionTile(
                  icon: Icons.inventory_2_outlined,
                  title: 'Bitirdim & Arşivle',
                  subtitle: 'Okuduklarım / İzlediklerim',
                  onTap: () => updateStatus("Arşivim"),
                ),
                const Divider(color: AppColors.border),
              ],
            ),
          ),
        );
      },
    );
  }

  bool _categoryFilter(ArchiveItemModel item) {
    final String cat = item.category.toLowerCase();
    
    if (_selectedCategory == 'Hepsi') return true;
    if (_selectedCategory == 'Filmler') return cat == 'movie';
    if (_selectedCategory == 'Diziler') return cat == 'tv show' || cat == 'series';
    if (_selectedCategory == 'Kitaplar') return cat == 'book';
    if (_selectedCategory == 'Oyunlar') return cat == 'game';
    return false; 
  }

  @override
  Widget build(BuildContext context) {
    final listemItems = _allArchives.where((i) => (i.status == "Listem" || i.status == "0") && _categoryFilter(i)).toList();
    final akisItems = _allArchives.where((i) => (i.status == "Akış" || i.status == "1") && _categoryFilter(i)).toList();
    final arsivimItems = _allArchives.where((i) => (i.status == "Arşivim" || i.status == "2") && _categoryFilter(i)).toList();

    if (!_isLoggedIn) {
      return Scaffold(
        body: SafeArea(
          child: Center(
            child: Padding(
              padding: const EdgeInsets.all(24.0),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.person_outline_rounded, size: 80, color: AppColors.textMuted),
                  const SizedBox(height: 16),
                  Text('Kendi Arşivini Oluştur', style: AppTextStyles.heading2),
                  const SizedBox(height: 12),
                  Text(
                    'Filmleri, dizileri ve kitapları takip etmek için giriş yap veya yeni bir hesap oluştur.',
                    textAlign: TextAlign.center,
                    style: AppTextStyles.body.copyWith(color: AppColors.textMuted),
                  ),
                  const SizedBox(height: 32),
                  SizedBox(
                    width: double.infinity,
                    height: 48,
                    child: FilledButton(
                      style: FilledButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                      ),
                      onPressed: () {
                        Navigator.of(context).pushNamedAndRemoveUntil('/login', (route) => false);
                      },
                      child: Text('Giriş Yap / Kayıt Ol', style: AppTextStyles.body.copyWith(color: Colors.white, fontWeight: FontWeight.bold)),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      );
    }

    return DefaultTabController(
      length: 3,
      child: Scaffold(
        body: SafeArea(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              children: [
                Container(
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: AppColors.surfaceElevated,
                    borderRadius: BorderRadius.circular(16),
                    border: Border.all(color: AppColors.border),
                  ),
                  child: Row(
                    children: [
                      const CircleAvatar(
                        radius: 28,
                        backgroundColor: AppColors.primary,
                        child: Icon(Icons.person_outline, color: Colors.white),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(_username, style: AppTextStyles.heading2),
                            if (_email.isNotEmpty) 
                              Text(_email, style: AppTextStyles.bodySmall),
                          ],
                        ),
                      ),
                      IconButton(
                        icon: const Icon(Icons.logout_rounded, color: Colors.redAccent),
                        tooltip: 'Çıkış Yap',
                        onPressed: () async {
                          final prefs = await SharedPreferences.getInstance();
                          await prefs.clear(); 
                          await SessionStorage().clearToken(); 
   
                          if (mounted) {
                            setState(() {
                              _isLoggedIn = false;
                              _username = 'Kullanıcı';
                              _email = '';
                            });
                          }
                        },
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 14),
                
                TabBar(
                  indicator: BoxDecoration(
                    color: AppColors.primary,
                    borderRadius: BorderRadius.circular(10),
                  ),
                  indicatorSize: TabBarIndicatorSize.tab,
                  dividerColor: Colors.transparent,
                  labelColor: Colors.white,
                  unselectedLabelColor: AppColors.textMuted,
                  tabs: const [
                    Tab(text: 'Listem'),
                    Tab(text: 'Şu an ki Akış'),
                    Tab(text: 'Arşivim'),
                  ],
                ),
                const SizedBox(height: 14),

                SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: _categories
                        .map(
                          (category) => Padding(
                            padding: const EdgeInsets.only(right: 8),
                            child: ChoiceChip(
                              label: Text(category),
                              selected: _selectedCategory == category,
                              onSelected: (_) {
                                setState(() {
                                  _selectedCategory = category;
                                });
                              },
                              selectedColor: AppColors.primary,
                              backgroundColor: AppColors.surfaceElevated,
                              side: const BorderSide(color: AppColors.border),
                              labelStyle: (_selectedCategory == category
                                      ? AppTextStyles.caption
                                      : AppTextStyles.bodySmall)
                                  .copyWith(
                                color: _selectedCategory == category
                                    ? Colors.white
                                    : AppColors.textMuted,
                              ),
                            ),
                          ),
                        )
                        .toList(),
                  ),
                ),
                const SizedBox(height: 14),

                Expanded(
                  child: _isLoading 
                    ? const Center(child: CircularProgressIndicator())
                    : TabBarView(
                        children: [
                          _CompactBentoGrid(items: listemItems, onItemTapped: (item) => _showUpdateBottomSheet(context, item)),
                          _CompactBentoGrid(items: akisItems, onItemTapped: (item) => _showUpdateBottomSheet(context, item)),
                          _CompactBentoGrid(items: arsivimItems, onItemTapped: (item) => _showUpdateBottomSheet(context, item)),
                        ],
                      ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _CompactBentoGrid extends StatelessWidget {
  const _CompactBentoGrid({required this.items, required this.onItemTapped});

  final List<ArchiveItemModel> items;
  final Function(ArchiveItemModel) onItemTapped;

  @override
  Widget build(BuildContext context) {
    if (items.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.inbox_rounded, size: 48, color: AppColors.textMuted.withValues(alpha: 0.5)),
            const SizedBox(height: 12),
            Text('Burada henüz bir şey yok.', style: AppTextStyles.bodySmall),
          ],
        ),
      );
    }

    return MasonryGridView.count(
      itemCount: items.length,
      crossAxisCount: 2,
      mainAxisSpacing: 10,
      crossAxisSpacing: 10,
      itemBuilder: (context, index) {
        final item = items[index];
        return Card(
          clipBehavior: Clip.antiAlias, 
          child: InkWell(
            onTap: () => onItemTapped(item),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                CachedNetworkImage(
                  imageUrl: item.imageUrl.isNotEmpty ? item.imageUrl : 'https://via.placeholder.com/500x750?text=Afiş+Yok',
                  width: double.infinity,
                  height: index.isEven ? 160 : 120,
                  fit: BoxFit.cover,
                  errorWidget: (context, url, error) => Container(
                    color: AppColors.surfaceElevated,
                    height: index.isEven ? 160 : 120,
                    child: const Icon(Icons.broken_image_outlined),
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.all(10),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(item.title, style: AppTextStyles.body, maxLines: 2, overflow: TextOverflow.ellipsis),
                      const SizedBox(height: 4),
                      Text(item.category.toUpperCase(), style: AppTextStyles.caption.copyWith(color: AppColors.textMuted, fontSize: 10)),
                    ],
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }
}

class _BottomSheetActionTile extends StatelessWidget {
  const _BottomSheetActionTile({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return ListTile(
      contentPadding: EdgeInsets.zero,
      leading: CircleAvatar(
        radius: 18,
        backgroundColor: AppColors.card,
        child: Icon(icon, color: AppColors.primarySoft, size: 18),
      ),
      title: Text(title, style: AppTextStyles.body.copyWith(color: AppColors.textPrimary)),
      subtitle: Text(subtitle, style: AppTextStyles.caption),
      trailing: const Icon(Icons.chevron_right_rounded, color: AppColors.textMuted),
      onTap: onTap,
    );
  }
}