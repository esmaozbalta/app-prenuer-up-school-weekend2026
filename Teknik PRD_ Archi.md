# 📑 ARCHİ — TEKNİK ÜRÜN GEREKSİNİM DOKÜMANI (Tech PRD)

**Proje:** Archi — Digital Archive & Gallery  
**Faz:** MVP (Faz 1)  
**Platform:** Flutter (iOS & Android)  
**Sürüm:** 1.1  
**Hazırlayan:** Technical Product Owner  
**Son Güncelleme:** Nisan 2026

---

## 1. TPO SORUMLULUK KAPSAMI

Bu doküman, BPO'nun iş gereksinimlerini teknik gerçekliğe dönüştürmek için hazırlanmıştır. TPO'nun sorumlulukları şunlardır:

- Mimari kararlar ve tech stack seçimi
- Veritabanı şeması ve API tasarımı
- Sprint planlaması ve backlog önceliklendirmesi
- Dış API entegrasyon yönetimi
- Performans KPI'larının teknik karşılığını çıkarmak
- QA süreçleri ve canlıya alma kriterleri

TPO, ürün kararlarına müdahale etmez; BPO'nun "ne" istediğini alır, "nasıl" yapılacağına karar verir.

---

## 2. ÜRÜN ÖZETİ

Archi, kullanıcının dijital kültürel tüketimini (Film, Dizi, Kitap, Oyun) estetik bir "sergi" olarak arşivlemesini sağlar.

| Metrik | Hedef |
|--------|-------|
| Kuzey Yıldızı | Kullanıcı başına eklenen toplam içerik (Assets) |
| 10 Saniye Kuralı | İçerik arama → arşive ekleme < 10sn |
| 7. Gün Retention | ≥ %30 |
| Arama Yanıt Süresi | < 500ms (Redis ile) |
| LCP (Sayfa Yükleme) | < 1.5sn |
| Senkronizasyon Başarısı | ≥ %95 |

---

## 3. TEKNOLOJİ YIĞINI (TECH STACK)

| Katman | Teknoloji | Gerekçe |
|--------|-----------|---------|
| Mobil İstemci | Flutter (Dart) | Tek codebase, yüksek performans |
| Backend API | .NET 8 Web API (C#) | Aggregator pattern için olgun ekosistem |
| Veritabanı | PostgreSQL (Render Managed) | Relational yapı + JSONB esnekliği |
| Cache | Redis | Arama sonuçlarını 500ms altında tutmak için |
| Sunucu | Render.com (Docker) | MVP için düşük operasyonel yük |
| Local Persistence | Isar DB | Offline-first deneyim |
| Auth | Firebase Auth (Google/Apple) | Hızlı entegrasyon, güvenilirlik |

---

## 4. MİMARİ GENEL GÖRÜNÜM

```
Flutter App
    │
    ▼
.NET 8 Web API  ──▶  Redis Cache
    │
    ├──▶ Aggregation Layer
    │       ├── TMDB API         (Film/Dizi)
    │       ├── Google Books API (Kitap)
    │       └── IGDB API         (Oyun)
    │
    └──▶ PostgreSQL
```

**Temel prensipler:**
- Aggregation Layer, dış API'ları normalize eder; istemci tek bir `/search/omni` endpoint'i çağırır.
- Feed ve arama Redis üzerinden döner; PostgreSQL'e sadece yazma ve kritik okumalar gider.
- Tüm dış API anahtarları backend'de tutulur, istemciye sızdırılmaz.

---

## 5. VERİTABANI ŞEMASI (PostgreSQL)

### `users`
| Sütun | Tip | Özellik |
|-------|-----|---------|
| id | UUID | PK |
| oauth_id | VARCHAR | Unique, Not Null |
| email | VARCHAR | Unique |
| is_vault_member | BOOLEAN | Default: false |
| created_at | TIMESTAMPTZ | Default: now() |

### `archive_items`
| Sütun | Tip | Özellik |
|-------|-----|---------|
| id | UUID | PK |
| user_id | UUID | FK → users |
| external_id | VARCHAR | Index |
| category | VARCHAR | movie / book / game |
| title | VARCHAR | Not Null |
| metadata | JSONB | Kapak, yıl, yazar vb. |
| status | SMALLINT | 0:Wishlist, 1:In Progress, 2:Done |
| referral_url | VARCHAR | Affiliate redirect (placeholder) |
| created_at | TIMESTAMPTZ | Default: now() |

### `vibe_tags`
| Sütun | Tip | Özellik |
|-------|-----|---------|
| id | UUID | PK |
| item_id | UUID | FK → archive_items |
| tag_name | VARCHAR(30) | Index |
| created_at | TIMESTAMPTZ | Default: now() |

---

## 6. API UÇ NOKTALARI

### Arama
```
GET /api/v1/search/omni?q={query}
```
TMDB, Google Books ve IGDB paralel sorgulanır, normalize edilir, Redis'e yazılır.

### Arşiv
```
POST /api/v1/archive/add
Body: { external_id, category, status, tags[] }

GET /api/v1/archive/{userId}
```

### Feed
```
GET /api/v1/feed/global
```
Dapper ile son 20 eklemeyi döner. Redis TTL: 60sn.

### Senkronizasyon
```
POST /api/v1/sync/steam
Body: { steam_profile_url }
→ Background Job ile kütüphane çekilir, bulk insert yapılır.

POST /api/v1/sync/goodreads
Body: { csv_file }
→ CSV parse → normalize → bulk insert.
```

### Share Card
```
GET /api/v1/share-card/{itemId}
→ Sunucu tarafında PNG render (SkiaSharp veya benzeri).
```

---

## 7. USER STORIES & ACCEPTANCE CRITERIA

### EPIC 1: Onboarding & Auth

**US-101 — Hızlı Giriş**  
Kullanıcı olarak Google veya Apple hesabımla 3 adımda giriş yapmak istiyorum.

- AC1: Auth akışı 10sn altında tamamlanmalı.
- AC2: Hata durumunda kullanıcıya anlamlı mesaj gösterilmeli.
- AC3: Token güvenli local storage'a yazılmalı.

---

### EPIC 2: Omni-Search & Hızlı Arşiv

**US-201 — Merkezi Arama**  
Tek bir arama çubuğundan Film, Kitap ve Oyun aramak istiyorum.

- AC1: Sonuçlar < 800ms içinde gelmeli.
- AC2: Aynı sorgu Redis'te 5dk cache'lenmeli.
- AC3: Sonuçlar kategori bazlı gruplandırılmış gelsin.

**US-202 — Hızlı Ekleme**  
Bir içeriği 10 saniye içinde arşivime ekleyebilmek istiyorum.

- AC1: Arama → detay → arşiv ekleme maksimum 3 tap.
- AC2: Ekleme sonrası optimistic UI ile anlık feedback.
- AC3: Offline iken ekleme kuyruğa alınmalı, bağlantı dönünce senkronize olmalı.

---

### EPIC 3: Senkronizasyon

**US-301 — Steam Senkronizasyonu**  
Steam hesabımı bağladığımda kütüphanem 10sn içinde Archi'ye aktarılsın.

- AC1: Background job < 10sn içinde tamamlanmalı (ilk 500 oyun için).
- AC2: Hata durumunda kullanıcıya retry seçeneği sunulmalı.
- AC3: Senkronizasyon başarı oranı ≥ %95.

**US-302 — CSV Import (Goodreads/Letterboxd)**  
CSV yükleyerek mevcut kitap/film listemimi aktarmak istiyorum.

- AC1: CSV parse + insert < 30sn (500 kayıt için).
- AC2: Duplicate item'lar sessizce atlanmalı.

---

### EPIC 4: Sosyal & Vibe

**US-401 — Global Feed**  
Topluluğun son 24 saatte ne eklediğini görmek istiyorum.

- AC1: Feed < 300ms yüklenmeli (Redis cache).
- AC2: Sonsuz scroll desteklenmeli (pagination: cursor-based).

**US-402 — Vibe Etiketleri**  
İçeriğe duygu etiketi ekleyip diğerlerinin etiketlerini görmek istiyorum.

- AC1: Etiket ekleme 1 tap.
- AC2: İçerik sayfasında en popüler 5 etiket görünmeli.

**US-403 — Share Card**  
Bitirdiğim içerik için estetik bir kart üretip paylaşmak istiyorum.

- AC1: Kart 2sn içinde render edilmeli.
- AC2: Instagram Story formatında (1080x1920) export.

---

## 8. EKRANLAR & KULLANICI AKIŞI

| # | Ekran | Kritik Notlar |
|---|-------|---------------|
| 1 | Splash & Auth | True Black tema, tek buton giriş |
| 2 | Progressive Onboarding | 3 soru (ne okuyorsun / izliyorsun / oynuyorsun) |
| 3 | Digital Gallery (Dashboard) | Bento-box grid, büyük kapak görselleri |
| 4 | Omni-Search | Full-screen, yazarken canlı sonuçlar |
| 5 | Item Detail & Vibe Selector | Durum seçimi + duygu etiketleri |
| 6 | Global Feed | Kronolojik, sonsuz scroll |
| 7 | Share Card Creator | Preview + export |
| 8 | Profil & Ayarlar | Hesap bağlama (Steam, CSV) |

---

## 9. SPRINT PLANI (6 HAFTA MVP)

| Sprint | Süre | Çıktı |
|--------|------|-------|
| S1 | Hf 1-2 | Auth, DB schema, Aggregation Layer (TMDB + Books + IGDB) |
| S2 | Hf 3 | Omni-Search + Redis cache + Arşiv ekleme akışı |
| S3 | Hf 4 | Dashboard UI, Feed, Vibe Tags |
| S4 | Hf 5 | Steam sync, CSV import, Share Card |
| S5 | Hf 6 | QA, performans testleri, beta canlıya alma |

---

## 10. KAPSAM DIŞI (FAZ 1)

- Podcast / Spotify entegrasyonu
- Manuel içerik girişi (sadece API desteği)
- Premium özellikler (Vault Member)
- Push notification
- Dark/Light tema geçişi

---

## 11. BAĞIMLILIKLAR & RİSKLER

| Risk | Olasılık | Etki | Önlem |
|------|----------|------|-------|
| IGDB API rate limit | Orta | Yüksek | Redis cache + request throttle |
| Steam scraping engellemesi | Yüksek | Orta | Resmi API'ya geçiş planı hazır tut |
| CSV import format tutarsızlığı | Yüksek | Düşük | Esnek parser + hata logları |
| Render.com cold start gecikmesi | Düşük | Orta | Keep-alive ping mekanizması |

---

## 12. DEFINITION OF DONE

Bir story'nin tamamlanmış sayılması için:

1. Tüm AC'ler geçiyor.
2. Unit test yazıldı (kritik business logic için).
3. Postman/Swagger ile API dokümante edildi.
4. Performans KPI'ları karşılanıyor (load test).
5. Kod review tamamlandı ve merge edildi.

---