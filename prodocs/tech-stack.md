# Archi — Teknoloji Yığını ve Kullanım Gerekçeleri (Tech Stack)

Bu doküman, Archi MVP (Faz 1) sürecinde Ürün Gereksinim Dokümanı'na (PRD) sadık kalınarak alınan mimari kararları, teknoloji seçimlerinin arkasındaki mühendislik gerekçelerini ve yapay zeka (LLM) entegrasyonunu açıklamaktadır.

---

# 1. Mobil İstemci (Frontend)

## Framework: Flutter (Dart)

**Gerekçe:**  
Tek bir kod tabanı (*single codebase*) ile hem iOS hem de Android cihazlarda "Digital Gallery" konseptine uygun, yüksek performanslı ve estetik arayüzler (Bento-box Grid) sunabilmek.

## Lokal Depolama: Isar DB & Shared Preferences

**Gerekçe:**  
Isar DB, uygulamanın *Offline-first* deneyimini desteklemek ve internet bağlantısının kesildiği durumlarda arşive ekleme işlemlerini kuyruklayabilmek için hızlı ve verimli bir NoSQL çözümü olarak seçilmiştir.

## Kimlik Doğrulama (Authentication): Firebase Auth

**Gerekçe:**  
MVP aşamasında Google ve Apple ile güvenilir, hızlı ve kullanıcı dostu bir onboarding deneyimi sunmak. Hedef, kullanıcıların 10 saniyeden kısa sürede kayıt ve giriş işlemlerini tamamlayabilmesidir.

---

# 2. Sunucu ve API Katmanı (Backend)

## Framework: .NET 8 Web API (C#)

**Gerekçe:**  
Uygulama mimarisi, *Aggregation Layer* (Toplama Katmanı) deseni üzerine kurulmuştur.

İstemci yalnızca tek bir API isteği gönderirken, backend aşağıdaki servisleri paralel olarak sorgular:

- TMDB (Film & Dizi)
- Google Books (Kitap)
- IGDB (Oyun)

.NET 8'in güçlü asenkron programlama modeli ve yüksek performanslı çalışma zamanı, bu çoklu entegrasyon trafiğini verimli şekilde yönetmek için tercih edilmiştir.

## Veritabanı: PostgreSQL (Render Managed)

**Gerekçe:**  
Kullanıcılar, arşivler ve etiketler (*vibe_tags*) gibi ilişkisel verilerin yanında farklı içerik türlerine ait değişken alanların da saklanması gerekmektedir.

PostgreSQL'in:

- Güçlü ilişkisel veri modeli
- JSONB desteği
- Yüksek performansı

sayesinde hem yapılandırılmış hem de yarı yapılandırılmış veriler etkin şekilde yönetilebilmektedir.

## Önbellekleme (Cache): Redis

**Gerekçe:**  
Redis aşağıdaki amaçlarla kullanılmaktadır:

- Omni-Search (Merkezi Arama) performansını artırmak
- Global Feed yanıt sürelerini **500 ms'nin altında** tutmak
- Özellikle IGDB gibi üçüncü parti servislerin rate limit kısıtlamalarına takılmamak
- Sık erişilen verileri önbelleğe almak

---

# 3. Yapay Zeka (AI) Entegrasyonu

Proje teslimat kriterleri doğrultusunda yapay zeka, Archi'nin çekirdek özelliklerinden biri olarak tasarlanmıştır.

## Kullanılan Servis

- OpenRouter
- Gemini API

## Ürün İçi Entegrasyon (Archi Asistanı – Akıllı Keşif)

Kullanıcılar aşağıdaki gibi doğal dil sorguları oluşturabilir:

> "Bugün yağmurlu bir gün, bana melankolik bir kitap veya film öner."

Bu istekler uygulama içerisindeki sohbet asistanı tarafından işlenir:

- `ai_chat_sheet.dart`

LLM tarafından dönen yapılandırılmış JSON çıktıları, uygulamanın içerik modeliyle doğrudan eşleşecek şekilde tasarlanmıştır.

Bu sayede kullanıcılar önerilen içerikleri:

- Görüntüleyebilir
- İnceleyebilir
- Tek dokunuşla **Arşivime Ekle** işlemini gerçekleştirebilir

## Geliştirme Sürecinde Yapay Zeka Kullanımı

Kodlama, hata ayıklama ve mimari karar süreçlerinde:

- Cursor IDE
- AI Pair Programming yaklaşımları

aktif olarak kullanılmıştır.

Bu sayede geliştirme hızı artırılmış, tekrarlayan görevlerde verimlilik sağlanmış ve teknik karar süreçleri desteklenmiştir.

---

# 4. Canlıya Alma ve DevOps (Deployment)

## Sunucu Altyapısı: Render.com (Docker)

**Gerekçe:**  
MVP aşamasında minimum operasyonel yük ile hızlı ve güvenilir dağıtım süreçleri oluşturmak amaçlanmıştır.

### Kullanılan Bileşenler

- Docker
- Render.com
- GitHub Actions / Git tabanlı CI-CD
- render.yaml

### Sağlanan Avantajlar

- Otomatik deployment
- GitHub üzerinden tetiklenen CI/CD süreçleri
- 7/24 erişilebilir backend altyapısı
- Düşük DevOps maliyeti
- Kolay ölçeklenebilirlik

Backend servisi, PostgreSQL/Supabase altyapısıyla güvenli bağlantılar kurarak veri yönetimini gerçekleştirmektedir.

---

# Teknoloji Özeti

| Katman | Teknoloji |
|----------|------------|
| Mobil Uygulama | Flutter (Dart) |
| Lokal Depolama | Isar DB, Shared Preferences |
| Authentication | Firebase Auth |
| Backend | .NET 8 Web API |
| Veritabanı | PostgreSQL |
| Cache | Redis |
| Yapay Zeka | OpenRouter, Gemini API |
| Containerization | Docker |
| Hosting | Render.com |
| CI/CD | GitHub + Render |