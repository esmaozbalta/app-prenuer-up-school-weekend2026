# PRD: Archi - Kişisel Kültür Arşivi (MVP v1.0)

## 1. Ürün Vizyonu
**Archi**, kullanıcıların tükettiği kültürel içerikleri (Film, Kitap, Oyun, Podcast) "less is more" felsefesiyle takip edebildiği, hem kişisel bir günlük hem de opsiyonel olarak dış dünyaya açık bir vitrin işlevi gören mobil platformdur.

## 2. Teknik Stack (Tech Stack)
* **Mobil:** Flutter (Dart)
* **Backend:** .NET 8 Web API
* **Veritabanı:** PostgreSQL
* **ORM:** Entity Framework Core (veya Dapper)
* **Auth:** JWT tabanlı E-posta/Şifre (PostgreSQL Identity)

---

## 3. Fonksiyonel Gereksinimler

### 3.1. Kimlik Doğrulama ve Güvenlik (Auth)
* **Kayıt Olma:** E-posta, Benzersiz Kullanıcı Adı ve Şifre ile hesap oluşturma.
* **Giriş:** JWT (Json Web Token) ile güvenli oturum yönetimi.
* **Profil Gizliliği:** Kullanıcı profilini istediği zaman "Herkes Görebilir" (Public) veya "Sadece Ben" (Private) moduna alabilmelidir.

### 3.2. İçerik Yönetimi (CAS & DMS)
* **CAS (Content Acquisition System):** * **Hızlı Arama:** TMDB (Film/Dizi) ve Google Books (Kitap) API entegrasyonu.
    * **Manuel Giriş:** API'da bulunmayan içerikler (yerel podcastler, bağımsız oyunlar vb.) için manuel veri girişi.
* **DMS (Data Management System):**
    * **Statü Atama:** "Tüketiliyor", "Tamamlandı", "Sonra Bakılacak" etiketleri.
    * **Rating:** 5 yıldız üzerinden hızlı oylama.
    * **Taste Notes:** Her içerik için 280 karakterlik "Neler hissettirdi?" alanı.

### 3.3. Sosyal ve Profil Özellikleri
* **Keşif:** Diğer kullanıcıları kullanıcı adına göre aratabilme.
* **Profil İzleme:** Public profillerin arşiv özetlerini (toplam içerik sayısı) ve listelerini görüntüleyebilme.
* **Favoriler:** En sevilen içerikleri profilin en üstünde "Öne Çıkanlar" olarak sabitleme.

---

## 4. Kullanıcı Deneyimi (UX/UI)
* **Minimalist Tasarım:** Temiz boşluklar, pastel tonlar ve yüksek okunabilirlik (COS ve Massimo Dutti çizgisinde).
* **Karanlık Mod Desteği:** Sistem ayarlarına duyarlı arayüz.
* **Hızlı Erişim:** Ana ekranda "Şu an devam edilenler" (In-Progress) bölümü.

---

## 5. Veritabanı Mimarisi (PostgreSQL Şeması)

### **Table: Users**
| Kolon | Tip | Özellik |
| :--- | :--- | :--- |
| `id` | `UUID` | Primary Key |
| `username` | `VARCHAR(50)` | Unique, Index |
| `is_private` | `BOOLEAN` | Default: `false` |
| `password_hash`| `TEXT` | Hash |

### **Table: Contents**
| Kolon | Tip | Özellik |
| :--- | :--- | :--- |
| `id` | `UUID` | Primary Key |
| `user_id` | `UUID` | Foreign Key |
| `category` | `ENUM` | Film, Kitap, Oyun, Podcast |
| `status` | `INT` | 0, 1, 2 |
| `note` | `VARCHAR(280)`| Taste Notes |

---

## 6. Bildirimler ve Hatırlatıcılar
* **Weekly Recap:** Haftalık bitirilen içeriklerin özeti.
* **Nudge (Dürtme):** 1 haftadan uzun süredir "Tüketiliyor" modunda kalan içerikler için "Hala buna devam ediyor musun?" hatırlatması.

---

## 7. MVP Başarı Metrikleri
1.  **Günlük Aktif Kullanıcı (DAU):** Kullanıcıların uygulamayı bir "alışkanlık günlüğü" olarak kullanma oranı.
2.  **Katalog Hızı:** Kullanıcının API üzerinden bir içeriği bulup listesine ekleme süresinin 15 saniyenin altında olması.
