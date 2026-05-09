// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'archive_item.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ArchiveItemImpl _$$ArchiveItemImplFromJson(Map<String, dynamic> json) =>
    _$ArchiveItemImpl(
      id: json['id'] as String,
      title: json['title'] as String,
      subtitle: json['subtitle'] as String,
      type: $enumDecode(_$ArchiveContentTypeEnumMap, json['type']),
      vibeNote: json['vibeNote'] as String,
      imageUrl: json['imageUrl'] as String,
      year: (json['year'] as num).toInt(),
    );

Map<String, dynamic> _$$ArchiveItemImplToJson(_$ArchiveItemImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'title': instance.title,
      'subtitle': instance.subtitle,
      'type': _$ArchiveContentTypeEnumMap[instance.type]!,
      'vibeNote': instance.vibeNote,
      'imageUrl': instance.imageUrl,
      'year': instance.year,
    };

const _$ArchiveContentTypeEnumMap = {
  ArchiveContentType.movie: 'movie',
  ArchiveContentType.book: 'book',
  ArchiveContentType.game: 'game',
  ArchiveContentType.series: 'series',
  ArchiveContentType.album: 'album',
};
