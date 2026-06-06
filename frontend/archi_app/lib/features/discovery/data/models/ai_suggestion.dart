class AiSuggestion {
  const AiSuggestion({
    required this.title,
    required this.category,
    required this.description,
    required this.year,
    required this.imageUrl,
  });

  final String title;
  final String category;
  final String description;
  final String year;
  final String imageUrl;

  factory AiSuggestion.fromJson(Map<String, dynamic> json) {
    return AiSuggestion(
      title: (json['title'] ?? '') as String,
      category: (json['category'] ?? '') as String,
      description: (json['description'] ?? '') as String,
      year: (json['year'] ?? '').toString(),
      imageUrl: (json['imageUrl'] ?? '') as String,
    );
  }

  /// Maps AI categories to backend archive categories (movie / book / game).
  String get archiveCategory {
    final normalized = category.trim().toLowerCase();
    if (normalized.contains('book')) return 'book';
    if (normalized.contains('game')) return 'game';
    return 'movie';
  }

  int? get parsedYear => int.tryParse(year.replaceAll(RegExp(r'[^0-9]'), ''));
}
