import 'package:freezed_annotation/freezed_annotation.dart';

part 'archive_item.freezed.dart';
part 'archive_item.g.dart';

enum ArchiveContentType { movie, book, game, series, album }

@freezed
class ArchiveItem with _$ArchiveItem {
  const factory ArchiveItem({
    required String id,
    required String title,
    required String subtitle,
    required ArchiveContentType type,
    required String vibeNote,
    required String imageUrl,
    required int year,
  }) = _ArchiveItem;

  factory ArchiveItem.fromJson(Map<String, dynamic> json) =>
      _$ArchiveItemFromJson(json);
}
