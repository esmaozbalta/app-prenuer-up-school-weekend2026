import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_staggered_grid_view/flutter_staggered_grid_view.dart';
import 'package:shared_preferences/shared_preferences.dart'; // <-- Dinamik veriler için eklendi

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../archive/data/models/archive_item.dart';
import '../../archive/data/services/archive_mock_service.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _service = const ArchiveMockService();
  late Future<List<ArchiveItem>> _itemsFuture;

  // <-- 1. Dinamik kullanıcı verilerini tutacak değişkenler
  String _username = 'Yükleniyor...';
  String _email = '';

  @override
  void initState() {
    super.initState();
    _itemsFuture = _service.fetchMatches();
    _loadUserProfile(); // <-- 2. Sayfa açıldığında bilgileri çekecek fonksiyonu çağırıyoruz
  }

  // <-- 3. Cihaz hafızasından giriş yapan kullanıcının bilgilerini çeken fonksiyon
  Future<void> _loadUserProfile() async {
    final prefs = await SharedPreferences.getInstance();
    setState(() {
      // Login olurken verileri kaydettiğin key isimlerine göre ('username', 'email') verileri alıyoruz
      _username = prefs.getString('username') ?? 'Kullanıcı'; 
      _email = prefs.getString('email') ?? ''; 
    });
  }

  @override
  Widget build(BuildContext context) {
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
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // <-- 4. Sabit "Esma" yazısı yerine dinamik _username değişkeni
                          Text(_username, style: AppTextStyles.heading2),
                          
                          // <-- 5. Sabit alt başlık yerine dinamik _email (boş değilse gösterir)
                          if (_email.isNotEmpty) 
                            Text(_email, style: AppTextStyles.bodySmall),
                        ],
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
                const SizedBox(height: 10),
                Expanded(
                  child: FutureBuilder<List<ArchiveItem>>(
                    future: _itemsFuture,
                    builder: (context, snapshot) {
                      if (snapshot.connectionState != ConnectionState.done) {
                        return const Center(child: CircularProgressIndicator());
                      }
                      final items = snapshot.data ?? [];
                      return TabBarView(
                        children: [
                          _CompactBentoGrid(items: items.take(6).toList()),
                          _CompactBentoGrid(items: items.skip(2).take(6).toList()),
                          _CompactBentoGrid(items: items.reversed.take(6).toList()),
                        ],
                      );
                    },
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
  const _CompactBentoGrid({required this.items});

  final List<ArchiveItem> items;

  @override
  Widget build(BuildContext context) {
    return MasonryGridView.count(
      itemCount: items.length,
      crossAxisCount: 2,
      mainAxisSpacing: 10,
      crossAxisSpacing: 10,
      itemBuilder: (context, index) {
        final item = items[index];
        return Card(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              ClipRRect(
                borderRadius: const BorderRadius.vertical(top: Radius.circular(14)),
                child: CachedNetworkImage(
                  imageUrl: item.imageUrl,
                  width: double.infinity,
                  height: index.isEven ? 110 : 88,
                  fit: BoxFit.cover,
                  placeholder: (context, url) => Container(
                    color: AppColors.surfaceElevated,
                    height: index.isEven ? 110 : 88,
                  ),
                  errorWidget: (context, url, error) => Container(
                    color: AppColors.surfaceElevated,
                    height: index.isEven ? 110 : 88,
                    child: const Icon(Icons.broken_image_outlined),
                  ),
                ),
              ),
              Padding(
                padding: const EdgeInsets.all(10),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(item.title, style: AppTextStyles.body),
                    const SizedBox(height: 4),
                    Text(item.subtitle, style: AppTextStyles.caption),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}