import 'dart:convert';
import 'package:http/http.dart' as http;

import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_staggered_grid_view/flutter_staggered_grid_view.dart';
import 'package:shimmer/shimmer.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import 'package:shared_preferences/shared_preferences.dart';

// Kategorileri Türkçeye çeviren yardımcı fonksiyon
String getTurkishCategory(String category) {
  final cat = category.toLowerCase();
  if (cat == 'movie') return 'FİLM';
  if (cat == 'tv show' || cat == 'series') return 'DİZİ';
  if (cat == 'book') return 'KİTAP';
  if (cat == 'game') return 'OYUN';
  return category.toUpperCase();
}

class DiscoveryItem {
  final String externalId; 
  final String title;
  final String category;
  final String imageUrl;
  final String year;
  final String description; 

  DiscoveryItem({
    required this.externalId,
    required this.title,
    required this.category,
    required this.imageUrl,
    required this.year,
    required this.description,
  });

  factory DiscoveryItem.fromJson(Map<String, dynamic> json) {
    return DiscoveryItem(
      externalId: json['externalId']?.toString() ?? json['ExternalId']?.toString() ?? '', 
      title: json['title'] ?? json['Title'] ?? '',
      category: json['category'] ?? json['Category'] ?? '',
      imageUrl: json['imageUrl'] ?? json['ImageUrl'] ?? '',
      year: json['year']?.toString() ?? json['Year']?.toString() ?? '',
      description: json['description'] ?? json['Description'] ?? '',
    );
  }
}

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> with AutomaticKeepAliveClientMixin {
  
  @override
  bool get wantKeepAlive => true; 

  final TextEditingController _searchController = TextEditingController();
  String _selectedCategory = 'Hepsi';
  
  List<DiscoveryItem> _searchResults = [];
  bool _isLoading = false;

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
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _performSearch("");
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }
  
  Future<void> _performSearch(String query) async {
    String searchQuery = query.trim();
    
    setState(() {
      _isLoading = true;
      _searchResults = []; 
    });

    try {
      if (searchQuery.isNotEmpty) {
        final url = Uri.parse('${AppConfig.apiBaseUrl}/api/v1/search?query=$searchQuery');
        final response = await http.get(url);
        if (response.statusCode == 200) {
          final List<dynamic> data = jsonDecode(response.body);
          setState(() {
            _searchResults = data.map((json) => DiscoveryItem.fromJson(json)).toList();
          });
        }
      } 
      else {
        // --- DEVASA VE ÇEŞİTLİ POPÜLER KELİME & KATEGORİ HAVUZU ---
        final popularKeywords = [
          // Seriler ve Evrenler
          'Harry Potter', 'Star Wars', 'Marvel', 'Batman', 'Spider-Man',
          'Lord of the Rings', 'Matrix', 'Game of Thrones', 'Avatar', 
          'Breaking Bad', 'Stranger Things', 'The Walking Dead', 'Jurassic Park',
          
          // Popüler Oyunlar ve Evrenler
          'GTA', 'Witcher', 'Minecraft', 'Cyberpunk', 'Elden Ring', 
          'God of War', 'The Last of Us', 'Red Dead Redemption', 'Zelda',
          'Super Mario', 'Resident Evil', 'Fallout', 'Assassin\'s Creed',
          
          // Bilindik Yönetmenler / Yazarlar
          'Nolan', 'Tarantino', 'Pixar', 'Studio Ghibli', 'Anime',
          'Stephen King', 'Agatha Christie', 'Tolkien', 'George R. R. Martin',
          
          // --- YENİ: GENEL TÜRLER VE KATEGORİLER (Sürpriz sonuçlar için) ---
          'Sci-Fi', 'Fantasy', 'Zombies', 'Vampires', 'Alien',
          'RPG', 'Open World', 'Comedy', 'Thriller', 'Romance', 
          'Biography', 'Strategy', 'Indie', 'Arcade', 'Classic',
          'Horror', 'Mystery', 'Documentary', 'Action', 'Adventure'
        ];
        
        popularKeywords.shuffle();
        
        // Bu devasa havuzdan sadece 3 tanesini alıyoruz
        final top3Keywords = popularKeywords.take(3);
        
        List<DiscoveryItem> combinedResults = [];

        for (var keyword in top3Keywords) {
          final url = Uri.parse('${AppConfig.apiBaseUrl}/api/v1/search?query=$keyword');
          final response = await http.get(url);
          
          if (response.statusCode == 200) {
            final List<dynamic> data = jsonDecode(response.body);
            final items = data.map((json) => DiscoveryItem.fromJson(json)).toList();
            combinedResults.addAll(items);
          }
        }

        // Bulunan tüm sonuçları güzelce birbirine karıştır
        combinedResults.shuffle();

        setState(() {
          _searchResults = combinedResults;
        });
      }
    } catch (e) {
      debugPrint('Arama Hatası: $e');
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);
    final filteredItems = _searchResults.where((item) {
      final String cat = item.category.toLowerCase(); 
      
      if (_selectedCategory == 'Hepsi') return true;
      if (_selectedCategory == 'Filmler') return cat == 'movie';
      if (_selectedCategory == 'Diziler') return cat == 'tv show' || cat == 'series';
      if (_selectedCategory == 'Kitaplar') return cat == 'book';
      if (_selectedCategory == 'Oyunlar') return cat == 'game'; 
      return false; 
    }).toList();

    return Scaffold(
      appBar: AppBar(
        title: const Text('Archi'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Keşfet & Arşivle', style: AppTextStyles.heading1),
              const SizedBox(height: 4),
              Text(
                'Filmleri, dizileri, kitapları ve oyunları ara, arşivine ekle',
                style: AppTextStyles.bodySmall,
              ),
              const SizedBox(height: 16),
              
              TextField(
                controller: _searchController,
                onSubmitted: (value) => _performSearch(value),
                style: AppTextStyles.body,
                decoration: InputDecoration(
                  hintText: 'Film, dizi, kitap veya oyun ara (ör: Matrix)...',
                  hintStyle: AppTextStyles.bodySmall,
                  prefixIcon: const Icon(Icons.search_rounded),
                  suffixIcon: IconButton(
                    icon: const Icon(Icons.arrow_forward),
                    onPressed: () => _performSearch(_searchController.text),
                  ),
                  filled: true,
                  fillColor: AppColors.surfaceElevated,
                  contentPadding: const EdgeInsets.symmetric(vertical: 14),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: AppColors.border),
                  ),
                  enabledBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: AppColors.border),
                  ),
                  focusedBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: AppColors.primary),
                  ),
                ),
              ),
              const SizedBox(height: 12),
              
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
                    ? const _DashboardLoadingGrid() 
                    : filteredItems.isEmpty
                        ? Center(
                            child: Text(
                              _searchController.text.isEmpty
                                  ? 'Aramaya başlamak için bir şeyler yazın.'
                                  : 'Sonuç bulunamadı.',
                              style: AppTextStyles.bodySmall,
                            ),
                          )
                        : MasonryGridView.count(
                            itemCount: filteredItems.length,
                            crossAxisCount: 2,
                            mainAxisSpacing: 12,
                            crossAxisSpacing: 12,
                            itemBuilder: (context, index) =>
                                _ArchiveBentoCard(item: filteredItems[index], index: index),
                          ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _ArchiveBentoCard extends StatelessWidget {
  const _ArchiveBentoCard({required this.item, required this.index});

  final DiscoveryItem item; 
  final int index;

  void _showDetails(BuildContext context) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      // BURADAKİ context İSMİNİ sheetContext OLARAK DEĞİŞTİRDİK:
      builder: (sheetContext) => _ItemDetailsSheet(
        item: item,
        onAddTapped: () {
          // Önce detay sayfasının bağlamını (sheetContext) kapatıyoruz
          Navigator.pop(sheetContext); 
          // Sonra kartın kendi ana bağlamını (context) kullanarak yeni menüyü açıyoruz
          _openArchiveActions(context, item); 
        },
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final isLarge = index % 3 == 0;

    return Card(
      clipBehavior: Clip.antiAlias, // Tıklama efektinin kart sınırlarından taşmaması için
      child: InkWell(
        onTap: () => _showDetails(context), // KARTA TIKLANDIĞINDA DETAYLAR AÇILIR
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            CachedNetworkImage(
              imageUrl: item.imageUrl.isNotEmpty ? item.imageUrl : 'https://via.placeholder.com/500x750?text=Afiş+Yok',
              fit: BoxFit.cover, // Bento kartlarındaki afişler kaplasın diye cover bırakıyoruz
              width: double.infinity,
              height: isLarge ? 190 : 132,
              placeholder: (context, _) => _ShimmerCover(
                height: isLarge ? 190 : 132,
              ),
              errorWidget: (context, url, error) => Container(
                height: isLarge ? 190 : 132,
                color: AppColors.surfaceElevated,
                alignment: Alignment.center,
                child: const Icon(Icons.broken_image_outlined),
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    getTurkishCategory(item.category), // Türkçe yaptık
                    style: AppTextStyles.label.copyWith(
                      color: Colors.deepPurpleAccent, // Hepsi mor oldu
                    ),
                  ),
                  const SizedBox(height: 6),
                  
                  Text(item.title, style: AppTextStyles.heading3, maxLines: 2, overflow: TextOverflow.ellipsis),
                  
                  if (item.category.toLowerCase() == 'book' && item.description.isNotEmpty)
                    Padding(
                      padding: const EdgeInsets.only(top: 4),
                      child: Text(
                        item.description, 
                        style: AppTextStyles.caption.copyWith(color: AppColors.textMuted),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),

                  const SizedBox(height: 10),
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 8,
                          vertical: 4,
                        ),
                        decoration: BoxDecoration(
                          color: AppColors.surfaceElevated,
                          borderRadius: BorderRadius.circular(999),
                          border: Border.all(color: AppColors.border),
                        ),
                        child: Text(
                          item.year.isNotEmpty 
                            ? (item.year.length >= 4 ? item.year.substring(0, 4) : item.year) 
                            : 'Bilinmiyor', 
                          style: AppTextStyles.caption
                        ),
                      ),
                      const Spacer(),
                      
                      InkWell(
                        borderRadius: BorderRadius.circular(999),
                        onTap: () => _openArchiveActions(context, item),
                        child: Container(
                          width: 30,
                          height: 30,
                          decoration: const BoxDecoration(
                            color: AppColors.primary,
                            shape: BoxShape.circle,
                          ),
                          child: const Icon(
                            Icons.add_rounded,
                            size: 18,
                            color: Colors.white,
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _openArchiveActions(BuildContext context, DiscoveryItem item) {
    Future<void> saveToArchive(String status) async {
      Navigator.of(context).pop(); 
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('${item.title} arşive ekleniyor...')),
      );

      try {
        final prefs = await SharedPreferences.getInstance();
        final token = prefs.getString('token') ?? '';

        final url = Uri.parse('${AppConfig.apiBaseUrl}/api/v1/archive');
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
            'status': status,
          }),
        );

        if (response.statusCode == 200 || response.statusCode == 201) {
          if (context.mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Başarıyla eklendi! 🎉'), backgroundColor: Colors.green),
            );
          }
        }
      } catch (e) {
        debugPrint('Kaydetme hatası: $e');
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
                  subtitle: 'Okuyacağım / İzleyecegim / Oynayacağım',
                  onTap: () => saveToArchive("Listem"),
                ),
                _BottomSheetActionTile(
                  icon: Icons.timelapse_rounded,
                  title: 'Şu an Yapıyorum',
                  subtitle: 'Okuyorum / İzliyorum / Oynuyorum',
                  onTap: () => saveToArchive("Akış"),
                ),
                _BottomSheetActionTile(
                  icon: Icons.inventory_2_outlined,
                  title: 'Bitirdim & Arşivle',
                  subtitle: 'Okuduklarım / İzlediklerim / Oynadıklarım',
                  onTap: () => saveToArchive("Arşivim"),
                ),
              ],
            ),
          ),
        );
      },
    );
  }
}

// --- YENİ: ESER DETAY EKRANI (BOTTOM SHEET) ---
class _ItemDetailsSheet extends StatelessWidget {
  final DiscoveryItem item;
  final VoidCallback onAddTapped;

  const _ItemDetailsSheet({
    required this.item,
    required this.onAddTapped,
  });

  @override
  Widget build(BuildContext context) {
    return DraggableScrollableSheet(
      initialChildSize: 0.85, 
      minChildSize: 0.5,
      maxChildSize: 0.95,
      builder: (_, controller) {
        return Container(
          decoration: const BoxDecoration(
            color: AppColors.surfaceElevated,
            borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
          ),
          child: ListView(
            controller: controller,
            padding: const EdgeInsets.all(20),
            children: [
              // Üstteki küçük tutamaç
              Center(
                child: Container(
                  width: 40,
                  height: 5,
                  decoration: BoxDecoration(
                    color: AppColors.border,
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
              ),
              const SizedBox(height: 24),
              
              // Büyük Afiş (Tamamen gözüksün diye contain yapıldı)
              ClipRRect(
                borderRadius: BorderRadius.circular(16),
                child: CachedNetworkImage(
                  imageUrl: item.imageUrl.isNotEmpty ? item.imageUrl : 'https://via.placeholder.com/500x750?text=Afiş+Yok',
                  fit: BoxFit.contain, // COVER yerine CONTAIN kullandık, artık kesilmeyecek
                  height: 350,
                  width: double.infinity,
                  placeholder: (context, _) => _ShimmerCover(height: 350),
                  errorWidget: (context, url, error) => Container(
                    height: 350,
                    color: AppColors.card,
                    alignment: Alignment.center,
                    child: const Icon(Icons.broken_image_outlined, size: 48, color: AppColors.textMuted),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              
              // Kategori ve Yıl Çipleri
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                    decoration: BoxDecoration(
                      color: Colors.deepPurpleAccent.withValues(alpha: 0.2), // Hepsi mor oldu
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Text(
                      getTurkishCategory(item.category), // Türkçe yaptık
                      style: AppTextStyles.label.copyWith(
                        color: Colors.deepPurpleAccent, // Hepsi mor oldu
                      ),
                    ),
                  ),
                  const SizedBox(width: 8),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                    decoration: BoxDecoration(
                      color: AppColors.card,
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(color: AppColors.border),
                    ),
                    child: Text(
                      item.year.isNotEmpty 
                        ? (item.year.length >= 4 ? item.year.substring(0, 4) : item.year) 
                        : 'Bilinmiyor',
                      style: AppTextStyles.label.copyWith(color: AppColors.textMuted),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              
              // Başlık
              Text(item.title, style: AppTextStyles.heading1),
              const SizedBox(height: 16),
              const Divider(color: AppColors.border),
              const SizedBox(height: 12),
              
              // Açıklama Metni
              Text(
                item.description.isNotEmpty && item.description != 'Video Oyunu'
                    ? item.description
                    : 'Bu içerik için henüz detaylı bir açıklama bulunmuyor.',
                style: AppTextStyles.body.copyWith(
                  color: AppColors.textMuted, 
                  height: 1.6, // Satır arası boşluğu (okunabilirliği artırır)
                ),
              ),
              const SizedBox(height: 32),
              
              // Dev "Arşive Ekle" Butonu
              SizedBox(
                width: double.infinity,
                height: 54,
                child: FilledButton.icon(
                  onPressed: onAddTapped,
                  icon: const Icon(Icons.add_circle_outline_rounded),
                  label: Text('Arşivime Ekle', style: AppTextStyles.body.copyWith(fontWeight: FontWeight.bold, color: Colors.white)),
                  style: FilledButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  ),
                ),
              ),
              const SizedBox(height: 20),
            ],
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

class _ShimmerCover extends StatelessWidget {
  const _ShimmerCover({required this.height});

  final double height;

  @override
  Widget build(BuildContext context) {
    return Shimmer.fromColors(
      baseColor: AppColors.surfaceElevated,
      highlightColor: AppColors.border,
      child: Container(
        width: double.infinity,
        height: height,
        color: AppColors.surfaceElevated,
      ),
    );
  }
}

class _DashboardLoadingGrid extends StatelessWidget {
  const _DashboardLoadingGrid();

  @override
  Widget build(BuildContext context) {
    return MasonryGridView.count(
      itemCount: 6,
      crossAxisCount: 2,
      mainAxisSpacing: 12,
      crossAxisSpacing: 12,
      itemBuilder: (context, index) => Card(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _ShimmerCover(height: index.isEven ? 190 : 132),
            const Padding(
              padding: EdgeInsets.all(12),
              child: _ShimmerTextBlock(),
            ),
          ],
        ),
      ),
    );
  }
}

class _ShimmerTextBlock extends StatelessWidget {
  const _ShimmerTextBlock();

  @override
  Widget build(BuildContext context) {
    return Shimmer.fromColors(
      baseColor: AppColors.surfaceElevated,
      highlightColor: AppColors.border,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(height: 10, width: 80, color: AppColors.surfaceElevated),
          const SizedBox(height: 8),
          Container(height: 14, width: 140, color: AppColors.surfaceElevated),
          const SizedBox(height: 8),
          Container(height: 10, width: 160, color: AppColors.surfaceElevated),
        ],
      ),
    );
  }
}