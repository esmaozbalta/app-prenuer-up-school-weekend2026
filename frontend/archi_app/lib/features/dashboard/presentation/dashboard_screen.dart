import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_staggered_grid_view/flutter_staggered_grid_view.dart';
import 'package:shimmer/shimmer.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../archive/data/models/archive_item.dart';
import '../../archive/data/services/archive_mock_service.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  final _service = const ArchiveMockService();
  late Future<List<ArchiveItem>> _itemsFuture;
  final TextEditingController _searchController = TextEditingController();
  String _selectedCategory = 'Hepsi';

  static const _categories = <String>[
    'Hepsi',
    'Filmler',
    'Kitaplar',
    'Diziler',
    'Oyunlar',
  ];

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  void initState() {
    super.initState();
    _itemsFuture = _service.fetchMatches();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Archi'),
        actions: [
          IconButton(
            onPressed: () {
              setState(() {
                _itemsFuture = _service.fetchMatches();
              });
            },
            icon: const Icon(Icons.refresh_rounded),
          ),
        ],
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: FutureBuilder<List<ArchiveItem>>(
            future: _itemsFuture,
            builder: (context, snapshot) {
              if (snapshot.connectionState != ConnectionState.done) {
                return const _DashboardLoadingGrid();
              }
              if (snapshot.hasError) {
                return Center(
                  child: Text(
                    'Veri yuklenemedi: ${snapshot.error}',
                    style: AppTextStyles.bodySmall,
                  ),
                );
              }

              final allItems = snapshot.data ?? [];
              final query = _searchController.text.trim().toLowerCase();
              final items = allItems.where((item) {
                final categoryMatches = switch (_selectedCategory) {
                  'Filmler' => item.type == ArchiveContentType.movie,
                  'Kitaplar' => item.type == ArchiveContentType.book,
                  'Diziler' => item.type == ArchiveContentType.series,
                  'Oyunlar' => item.type == ArchiveContentType.game,
                  _ => true,
                };
                final textMatches =
                    query.isEmpty ||
                    item.title.toLowerCase().contains(query) ||
                    item.vibeNote.toLowerCase().contains(query);
                return categoryMatches && textMatches;
              }).toList();

              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Vibe Match Archive', style: AppTextStyles.heading1),
                  const SizedBox(height: 4),
                  Text(
                    'Zevkine uygun içerikleri arşivle',
                    style: AppTextStyles.bodySmall,
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _searchController,
                    onChanged: (_) => setState(() {}),
                    style: AppTextStyles.body,
                    decoration: InputDecoration(
                      hintText: 'Film, kitap, dizi ya da vibe ara...',
                      hintStyle: AppTextStyles.bodySmall,
                      prefixIcon: const Icon(Icons.search_rounded),
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
                    child: MasonryGridView.count(
                      itemCount: items.length,
                      crossAxisCount: 2,
                      mainAxisSpacing: 12,
                      crossAxisSpacing: 12,
                      itemBuilder: (context, index) =>
                          _ArchiveBentoCard(item: items[index], index: index),
                    ),
                  ),
                ],
              );
            },
          ),
        ),
      ),
    );
  }
}

class _ArchiveBentoCard extends StatelessWidget {
  const _ArchiveBentoCard({required this.item, required this.index});

  final ArchiveItem item;
  final int index;

  @override
  Widget build(BuildContext context) {
    final isLarge = index % 3 == 0;

    return Card(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          ClipRRect(
            borderRadius: const BorderRadius.vertical(top: Radius.circular(14)),
            child: CachedNetworkImage(
              imageUrl: item.imageUrl,
              fit: BoxFit.cover,
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
          ),
          Padding(
            padding: const EdgeInsets.all(12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item.subtitle.toUpperCase(),
                  style: AppTextStyles.label,
                ),
                const SizedBox(height: 6),
                Text(item.title, style: AppTextStyles.heading3),
                const SizedBox(height: 6),
                Text(item.vibeNote, style: AppTextStyles.caption),
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
                      child: Text('${item.year}', style: AppTextStyles.caption),
                    ),
                    const Spacer(),
                    InkWell(
                      borderRadius: BorderRadius.circular(999),
                      onTap: () => _openArchiveActions(context, item),
                      child: Container(
                        width: 30,
                        height: 30,
                        decoration: BoxDecoration(
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
    );
  }

  void _openArchiveActions(BuildContext context, ArchiveItem item) {
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
                  subtitle: 'Okuyacagim / Izleyecegim',
                ),
                _BottomSheetActionTile(
                  icon: Icons.timelapse_rounded,
                  title: 'Su an Yapiyorum',
                  subtitle: 'Okuyorum / Izliyorum',
                ),
                _BottomSheetActionTile(
                  icon: Icons.inventory_2_outlined,
                  title: 'Bitirdim & Arsivle',
                  subtitle: 'Okuduklarim / Izlediklerim',
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
  });

  final IconData icon;
  final String title;
  final String subtitle;

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
      onTap: () => Navigator.of(context).pop(),
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
