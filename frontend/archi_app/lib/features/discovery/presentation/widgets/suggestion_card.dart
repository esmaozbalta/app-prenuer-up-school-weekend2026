import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../data/models/ai_suggestion.dart';
import 'package:shared_preferences/shared_preferences.dart';

class SuggestionCard extends StatefulWidget {
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
  State<SuggestionCard> createState() => _SuggestionCardState();
}

class _SuggestionCardState extends State<SuggestionCard> {
  String _realImageUrl = '';

  @override
  void initState() {
    super.initState();
    _realImageUrl = widget.suggestion.imageUrl;
    _fetchRealPosterFromTmdb(); 
  }

  Future<void> _fetchRealPosterFromTmdb() async {
    if (widget.suggestion.archiveCategory == 'movie' || widget.suggestion.archiveCategory == 'series') {
      try {
        final url = Uri.parse('http://localhost:5161/api/v1/search?query=${widget.suggestion.title}');
        final response = await http.get(url);
        
        if (response.statusCode == 200) {
          final List<dynamic> data = jsonDecode(response.body);
          if (data.isNotEmpty) {
            // HATA BURADAYDI: JSON büyük/küçük harf uyuşmazlığını önlemek için ikisine de bakıyoruz
            final fetchedUrl = (data[0]['imageUrl'] ?? data[0]['ImageUrl']) as String?;
            if (fetchedUrl != null && fetchedUrl.isNotEmpty) {
              setState(() {
                _realImageUrl = fetchedUrl; 
              });
            }
          }
        }
      } catch (e) {
        debugPrint('Gerçek afiş bulunamadı: $e');
      }
    }
  }

  void _showSaveBottomSheet(BuildContext context) {
    String finalImageUrl = _realImageUrl;
    if (finalImageUrl.startsWith('/')) {
      finalImageUrl = 'https://image.tmdb.org/t/p/w500$finalImageUrl';
    }

    Future<void> saveToArchive(String status) async {
      Navigator.of(context).pop(); 
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('${widget.suggestion.title} arşive ekleniyor...')),
      );

      try {
        // --- 1. EKSİK OLAN KISIM: KİMLİĞİMİZİ CİHAZDAN ALIYORUZ ---
        final prefs = await SharedPreferences.getInstance();
        final token = prefs.getString('token') ?? '';

        final url = Uri.parse('http://localhost:5161/api/v1/archive');
        final response = await http.post(
          url,
          headers: {
            'Content-Type': 'application/json',
            // --- 2. BÜYÜK ÇÖZÜM: KİMLİĞİ SUNUCUYA SUNUYORUZ ---
            'Authorization': 'Bearer $token', 
          },
          body: jsonEncode({
            // Boşlukları ve özel karakterleri temizleyip eşsiz bir ID yapalım ki çakışmasın
            'externalId': 'ai-${widget.suggestion.title.replaceAll(' ', '-').toLowerCase()}', 
            'title': widget.suggestion.title,
            'category': widget.suggestion.archiveCategory, 
            'imageUrl': finalImageUrl, 
            'year': widget.suggestion.year,
            'status': status,
          }),
        );

        if (response.statusCode == 200 || response.statusCode == 201) {
          if (context.mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Başarıyla eklendi! 🎉'), backgroundColor: Colors.green),
            );
          }
        } else {
          // Eğer hala bir sorun çıkarsa ekranda kırmızı görelim diye
          if (context.mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(content: Text('Hata: ${response.statusCode}'), backgroundColor: Colors.red),
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
                Text(widget.suggestion.title, style: AppTextStyles.heading3),
                const SizedBox(height: 12),
                _BottomSheetActionTile(
                  icon: Icons.playlist_add_check_circle_outlined,
                  title: 'Listeme Ekle',
                  subtitle: 'Okuyacağım / İzleyecegim',
                  onTap: () => saveToArchive("Listem"),
                ),
                _BottomSheetActionTile(
                  icon: Icons.timelapse_rounded,
                  title: 'Şu an Yapıyorum',
                  subtitle: 'Okuyorum / İzliyorum',
                  onTap: () => saveToArchive("Akış"),
                ),
                _BottomSheetActionTile(
                  icon: Icons.inventory_2_outlined,
                  title: 'Bitirdim & Arşivle',
                  subtitle: 'Okuduklarım / İzlediklerim',
                  onTap: () => saveToArchive("Arşivim"),
                ),
              ],
            ),
          ),
        );
      },
    );
  }

@override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      height: 200,
      decoration: BoxDecoration(
        color: AppColors.card,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.border),
      ),
      clipBehavior: Clip.antiAlias,
      // YUKARIDAN AŞAĞIYA DEĞİL (Column), SOLDAN SAĞA (Row) DİZİYORUZ
      child: Row( 
        children: [
          // --- SOL TARAF: AFİŞ ---
          SizedBox(
            width: 110, // Afişin genişliği
            height: double.infinity,
            child: _CoverImage(imageUrl: _realImageUrl),
          ),
          
          // --- SAĞ TARAF: BİLGİLER VE BUTON ---
          Expanded(
            child: Padding(
              padding: const EdgeInsets.all(12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    widget.suggestion.category.toUpperCase(),
                    style: AppTextStyles.label.copyWith(color: AppColors.textMuted, fontSize: 10),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    widget.suggestion.title, 
                    style: AppTextStyles.heading3,
                    maxLines: 1, // Başlık çok uzunsa tek satırda keser
                    overflow: TextOverflow.ellipsis,
                  ),
                  if (widget.suggestion.year.isNotEmpty) ...[
                    const SizedBox(height: 4),
                    Text(widget.suggestion.year, style: AppTextStyles.caption),
                  ],
                  const SizedBox(height: 6),
                  Expanded(
                    child: SingleChildScrollView( // YAZIYI KAYDIRILABİLİR YAPAN SİHİRLİ WIDGET
                      physics: const BouncingScrollPhysics(), // Kaydırırken yumuşak bir his verir
                      child: Text(
                        widget.suggestion.description,
                        style: AppTextStyles.bodySmall.copyWith(height: 1.4, fontSize: 11),
                        // maxLines ve overflow kısımlarını SİLDİK, çünkü artık sığmayan kısım aşağı kayacak
                      ),
                    ),
                  ),
                  const SizedBox(height: 8),
                  
                  // --- KÜÇÜLTÜLMÜŞ EKLE BUTONU ---
                  SizedBox(
                    width: double.infinity,
                    height: 34,
                    child: ElevatedButton(
                      onPressed: widget.isAdding ? null : () => _showSaveBottomSheet(context),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primarySoft,
                        foregroundColor: Colors.white,
                        padding: EdgeInsets.zero,
                        elevation: 0,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: widget.isAdding
                          ? const SizedBox(
                              width: 16,
                              height: 16,
                              child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                            )
                          : Text(
                              'Add to Archi',
                              style: AppTextStyles.caption.copyWith(fontWeight: FontWeight.bold),
                            ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

// --- AFİŞ KISMI (AspectRatio'yu kaldırdık çünkü artık sabit bir kutuya sığıyor) ---
class _CoverImage extends StatelessWidget {
  const _CoverImage({required this.imageUrl});

  final String imageUrl;
  static const _placeholder = 'https://via.placeholder.com/500x750/1B1824/9490B0?text=Archi';

  @override
  Widget build(BuildContext context) {
    String resolved = imageUrl.trim();
    if (resolved.isNotEmpty && resolved.startsWith('/')) {
      resolved = 'https://image.tmdb.org/t/p/w500$resolved';
    } else if (resolved.isEmpty) {
      resolved = _placeholder;
    }

    return CachedNetworkImage(
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