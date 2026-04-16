# **📑 TEKNİK ÜRÜN GEREKSİNİM DÖKÜMANI (TECH PRD)**

**Proje Adı:** Archi (Digital Archive & Gallery)
**Aşama:** Faz 1 - MVP (Minimum Uygulanabilir Ürün)
**Platform:** Cross-Platform (iOS & Android - Flutter)
**Hazırlayan:** Technical Product Owner

---

## **1. Ürün Vizyonu ve İş Hedefleri (Özet)**
Archi, kullanıcının dijital dünyadaki kültürel ayak izlerini (Film, Dizi, Kitap, Oyun) tek bir estetik merkezde toplamasını sağlayan bir "Dijital Hafıza" uygulamasıdır. Karmaşık listeler yerine şık bir "sergi" deneyimi sunar.

* **Kuzey Yıldızı Metriği:** Kullanıcı başına eklenen toplam dijital varlık (Assets) sayısı.
* **10 Saniye Kuralı:** Bir içeriği bulup arşive ekleme süresi < 10sn olmalıdır.
* **Viralite Metriği:** Paylaşılan "Archi Share Card" başına gelen yeni kullanıcı dönüşümü.

---

## **2. Teknoloji Yığını (Tech Stack) & Mimari**
MVP aşamasında yüksek performans ve ölçeklenebilirlik için aşağıdaki yapı kullanılacaktır:

* **Mobil İstemci (Frontend):** Flutter (Dart). Tek kod tabanı ile yüksek performanslı UI.
* **Backend (API Katmanı):** .NET 8 Web API (C#). Aggregator Pattern ile dış API entegrasyonu.
* **Sunucu & Dağıtım:** Render.com (Dockerize edilmiş servisler).
* **Veritabanı:** PostgreSQL (Render Managed).
* **Hız Katmanı (Cache):** Redis. Arama sonuçlarını 500ms altında tutmak için.
* **Local Persistence:** Isar Database (Offline-first yaklaşımı için).

---

## **3. VERİTABANI ŞEMASI (POSTGRESQL)**

| **Tablo Adı** | **Sütun (Column)** | **Veri Tipi** | **Özellikler** | **Açıklama** |
| :--- | :--- | :--- | :--- | :--- |
| **users** | id | UUID | Primary Key | Kullanıcının eşsiz kimliği |
| | oauth_id | VARCHAR | Unique, Not Null | Google/Apple Sign-in ID |
| | email | VARCHAR | Unique | İletişim e-postası |
| **archive_items** | id | UUID | Primary Key | Kaydın sistemdeki ID'si |
| | user_id | UUID | Foreign Key | Hangi kullanıcıya ait? |
| | external_id | VARCHAR | Index | Kaynak API'daki ID (TMDB_ID vb.) |
| | category | VARCHAR | Movie, Book, Game | İçeriğin türü |
| | title | VARCHAR | Not Null | İçerik başlığı |
| | metadata | JSONB | Not Null | Kapak görseli, yıl, yazar vb. |
| | status | INT | 0:Wishlist, 1:Done | İzleme/Okuma durumu |
| **vibe_tags** | id | UUID | Primary Key | Etiket ID'si |
| | item_id | UUID | Foreign Key | Hangi içeriğe eklendi? |
| | tag_name | VARCHAR(30) | Index | #Karanlık, #UmutVerici vb. |

---

## **4. API UÇ NOKTALARI (ASP.NET CORE - REST/JSON)**

* **GET /api/v1/search/omni?q={query}**
    * **İşlem:** TMDB, Google Books ve IGDB API'ları aynı anda sorgulanır ve normalize edilir.
* **POST /api/v1/archive/add**
    * **Payload:** `{ "external_id": "string", "category": "string", "status": 1, "tags": ["#vibe"] }`
* **POST /api/v1/sync/steam**
    * **İşlem:** Background Job ile oyunlar çekilir ve bulk insert yapılır.
* **GET /api/v1/feed/global**
    * **İşlem:** Dapper ile en son eklenen 20 içeriği hızlıca döner.

---

## **5. EKRANLAR (SCREENS) VE KULLANICI AKIŞI (UI/UX)**

1.  **Splash & Minimal Auth:** "True Black" tema. Tek butonla hızlı giriş.
2.  **The Omni-Search:** Minimalist arama barı. Yazarken beliren görsel grid yapısı.
3.  **Digital Gallery (Dashboard):** Bento-box stili dizilim. Büyük görseller, akıcı kaydırma.
4.  **Item Detail & Vibe Selector:** "Nasıl hissettirdi?" sorusuyla seçilebilir duygu etiketleri.
5.  **Share Card Creator:** Instagram Story formatında estetik görsel üretimi.

---

## **6. USER STORIES & ACCEPTANCE CRITERIA**

### **EPIC 1: Omni-Search & Hızlı Arşiv**
**US 1.1 - Merkezi Arama**
* **AC 1:** Arama sonuçları < 800ms içinde gelmeli.
* **AC 2:** Sonuçlar Redis'te cache'lenmeli.
* **AC 3:** Tek aramada Film, Kitap ve Oyun sonuçları dikey olarak listelenmeli.

---

## **7. KAPSAM DIŞI (FAZ 1 - MVP)**
* Podcast ve Spotify müzik listesi entegrasyonu.
* Manuel veri girişi (Sadece API desteği).

---
*Bu döküman Archi projesinin teknik standartlarını belirler.*