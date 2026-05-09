import '../models/archive_item.dart';

class ArchiveMockService {
  const ArchiveMockService();

  Future<List<ArchiveItem>> fetchMatches() async {
    await Future<void>.delayed(const Duration(milliseconds: 600));

    return const [
      ArchiveItem(
        id: '1',
        title: 'Interstellar',
        subtitle: 'Film',
        type: ArchiveContentType.movie,
        vibeNote: 'Cosmic wonder + existential sci-fi',
        imageUrl:
            'https://images.unsplash.com/photo-1446776811953-b23d57bd21aa?auto=format&fit=crop&w=900&q=80',
        year: 2014,
      ),
      ArchiveItem(
        id: '2',
        title: 'Dune',
        subtitle: 'Roman',
        type: ArchiveContentType.book,
        vibeNote: 'Desert prophecy and layered politics',
        imageUrl:
            'https://images.unsplash.com/photo-1462331940025-496dfbfc7564?auto=format&fit=crop&w=900&q=80',
        year: 1965,
      ),
      ArchiveItem(
        id: '3',
        title: 'Cyberpunk 2077',
        subtitle: 'Oyun',
        type: ArchiveContentType.game,
        vibeNote: 'Neon noir + high-tech rebellion',
        imageUrl:
            'https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=900&q=80',
        year: 2020,
      ),
      ArchiveItem(
        id: '4',
        title: 'Blade Runner 2049',
        subtitle: 'Film',
        type: ArchiveContentType.movie,
        vibeNote: 'Melancholic futurism and identity',
        imageUrl:
            'https://images.unsplash.com/photo-1478720568477-152d9b164e26?auto=format&fit=crop&w=900&q=80',
        year: 2017,
      ),
      ArchiveItem(
        id: '5',
        title: 'Foundation',
        subtitle: 'Dizi',
        type: ArchiveContentType.series,
        vibeNote: 'Grand-scale empire and math destiny',
        imageUrl:
            'https://images.unsplash.com/photo-1451187580459-43490279c0fa?auto=format&fit=crop&w=900&q=80',
        year: 2021,
      ),
      ArchiveItem(
        id: '6',
        title: 'Mass Effect 2',
        subtitle: 'Oyun',
        type: ArchiveContentType.game,
        vibeNote: 'Crew loyalty and galaxy stakes',
        imageUrl:
            'https://images.unsplash.com/photo-1534423861386-85a16f5d13fd?auto=format&fit=crop&w=900&q=80',
        year: 2010,
      ),
      ArchiveItem(
        id: '7',
        title: 'Arrival',
        subtitle: 'Film',
        type: ArchiveContentType.movie,
        vibeNote: 'Quiet tension and linguistic mystery',
        imageUrl:
            'https://images.unsplash.com/photo-1518639192441-8fce0a366e2e?auto=format&fit=crop&w=900&q=80',
        year: 2016,
      ),
      ArchiveItem(
        id: '8',
        title: 'Silo',
        subtitle: 'Dizi',
        type: ArchiveContentType.series,
        vibeNote: 'Claustrophobic future and hidden truth',
        imageUrl:
            'https://images.unsplash.com/photo-1469474968028-56623f02e42e?auto=format&fit=crop&w=900&q=80',
        year: 2023,
      ),
      ArchiveItem(
        id: '9',
        title: 'The Dark Side of the Moon',
        subtitle: 'Albüm',
        type: ArchiveContentType.album,
        vibeNote: 'Spacey atmosphere and introspection',
        imageUrl:
            'https://images.unsplash.com/photo-1514525253161-7a46d19cd819?auto=format&fit=crop&w=900&q=80',
        year: 1973,
      ),
      ArchiveItem(
        id: '10',
        title: 'Hyperion',
        subtitle: 'Roman',
        type: ArchiveContentType.book,
        vibeNote: 'Pilgrimage structure with epic scale',
        imageUrl:
            'https://images.unsplash.com/photo-1526662092594-e98c1e356d6a?auto=format&fit=crop&w=900&q=80',
        year: 1989,
      ),
    ];
  }
}
