// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'archive_item.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
    'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models');

ArchiveItem _$ArchiveItemFromJson(Map<String, dynamic> json) {
  return _ArchiveItem.fromJson(json);
}

/// @nodoc
mixin _$ArchiveItem {
  String get id => throw _privateConstructorUsedError;
  String get title => throw _privateConstructorUsedError;
  String get subtitle => throw _privateConstructorUsedError;
  ArchiveContentType get type => throw _privateConstructorUsedError;
  String get vibeNote => throw _privateConstructorUsedError;
  String get imageUrl => throw _privateConstructorUsedError;
  int get year => throw _privateConstructorUsedError;

  /// Serializes this ArchiveItem to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of ArchiveItem
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $ArchiveItemCopyWith<ArchiveItem> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $ArchiveItemCopyWith<$Res> {
  factory $ArchiveItemCopyWith(
          ArchiveItem value, $Res Function(ArchiveItem) then) =
      _$ArchiveItemCopyWithImpl<$Res, ArchiveItem>;
  @useResult
  $Res call(
      {String id,
      String title,
      String subtitle,
      ArchiveContentType type,
      String vibeNote,
      String imageUrl,
      int year});
}

/// @nodoc
class _$ArchiveItemCopyWithImpl<$Res, $Val extends ArchiveItem>
    implements $ArchiveItemCopyWith<$Res> {
  _$ArchiveItemCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of ArchiveItem
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? title = null,
    Object? subtitle = null,
    Object? type = null,
    Object? vibeNote = null,
    Object? imageUrl = null,
    Object? year = null,
  }) {
    return _then(_value.copyWith(
      id: null == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as String,
      title: null == title
          ? _value.title
          : title // ignore: cast_nullable_to_non_nullable
              as String,
      subtitle: null == subtitle
          ? _value.subtitle
          : subtitle // ignore: cast_nullable_to_non_nullable
              as String,
      type: null == type
          ? _value.type
          : type // ignore: cast_nullable_to_non_nullable
              as ArchiveContentType,
      vibeNote: null == vibeNote
          ? _value.vibeNote
          : vibeNote // ignore: cast_nullable_to_non_nullable
              as String,
      imageUrl: null == imageUrl
          ? _value.imageUrl
          : imageUrl // ignore: cast_nullable_to_non_nullable
              as String,
      year: null == year
          ? _value.year
          : year // ignore: cast_nullable_to_non_nullable
              as int,
    ) as $Val);
  }
}

/// @nodoc
abstract class _$$ArchiveItemImplCopyWith<$Res>
    implements $ArchiveItemCopyWith<$Res> {
  factory _$$ArchiveItemImplCopyWith(
          _$ArchiveItemImpl value, $Res Function(_$ArchiveItemImpl) then) =
      __$$ArchiveItemImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call(
      {String id,
      String title,
      String subtitle,
      ArchiveContentType type,
      String vibeNote,
      String imageUrl,
      int year});
}

/// @nodoc
class __$$ArchiveItemImplCopyWithImpl<$Res>
    extends _$ArchiveItemCopyWithImpl<$Res, _$ArchiveItemImpl>
    implements _$$ArchiveItemImplCopyWith<$Res> {
  __$$ArchiveItemImplCopyWithImpl(
      _$ArchiveItemImpl _value, $Res Function(_$ArchiveItemImpl) _then)
      : super(_value, _then);

  /// Create a copy of ArchiveItem
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? title = null,
    Object? subtitle = null,
    Object? type = null,
    Object? vibeNote = null,
    Object? imageUrl = null,
    Object? year = null,
  }) {
    return _then(_$ArchiveItemImpl(
      id: null == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as String,
      title: null == title
          ? _value.title
          : title // ignore: cast_nullable_to_non_nullable
              as String,
      subtitle: null == subtitle
          ? _value.subtitle
          : subtitle // ignore: cast_nullable_to_non_nullable
              as String,
      type: null == type
          ? _value.type
          : type // ignore: cast_nullable_to_non_nullable
              as ArchiveContentType,
      vibeNote: null == vibeNote
          ? _value.vibeNote
          : vibeNote // ignore: cast_nullable_to_non_nullable
              as String,
      imageUrl: null == imageUrl
          ? _value.imageUrl
          : imageUrl // ignore: cast_nullable_to_non_nullable
              as String,
      year: null == year
          ? _value.year
          : year // ignore: cast_nullable_to_non_nullable
              as int,
    ));
  }
}

/// @nodoc
@JsonSerializable()
class _$ArchiveItemImpl implements _ArchiveItem {
  const _$ArchiveItemImpl(
      {required this.id,
      required this.title,
      required this.subtitle,
      required this.type,
      required this.vibeNote,
      required this.imageUrl,
      required this.year});

  factory _$ArchiveItemImpl.fromJson(Map<String, dynamic> json) =>
      _$$ArchiveItemImplFromJson(json);

  @override
  final String id;
  @override
  final String title;
  @override
  final String subtitle;
  @override
  final ArchiveContentType type;
  @override
  final String vibeNote;
  @override
  final String imageUrl;
  @override
  final int year;

  @override
  String toString() {
    return 'ArchiveItem(id: $id, title: $title, subtitle: $subtitle, type: $type, vibeNote: $vibeNote, imageUrl: $imageUrl, year: $year)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$ArchiveItemImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.title, title) || other.title == title) &&
            (identical(other.subtitle, subtitle) ||
                other.subtitle == subtitle) &&
            (identical(other.type, type) || other.type == type) &&
            (identical(other.vibeNote, vibeNote) ||
                other.vibeNote == vibeNote) &&
            (identical(other.imageUrl, imageUrl) ||
                other.imageUrl == imageUrl) &&
            (identical(other.year, year) || other.year == year));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
      runtimeType, id, title, subtitle, type, vibeNote, imageUrl, year);

  /// Create a copy of ArchiveItem
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$ArchiveItemImplCopyWith<_$ArchiveItemImpl> get copyWith =>
      __$$ArchiveItemImplCopyWithImpl<_$ArchiveItemImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$ArchiveItemImplToJson(
      this,
    );
  }
}

abstract class _ArchiveItem implements ArchiveItem {
  const factory _ArchiveItem(
      {required final String id,
      required final String title,
      required final String subtitle,
      required final ArchiveContentType type,
      required final String vibeNote,
      required final String imageUrl,
      required final int year}) = _$ArchiveItemImpl;

  factory _ArchiveItem.fromJson(Map<String, dynamic> json) =
      _$ArchiveItemImpl.fromJson;

  @override
  String get id;
  @override
  String get title;
  @override
  String get subtitle;
  @override
  ArchiveContentType get type;
  @override
  String get vibeNote;
  @override
  String get imageUrl;
  @override
  int get year;

  /// Create a copy of ArchiveItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$ArchiveItemImplCopyWith<_$ArchiveItemImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
