# Archi

Archi, film, dizi, kitap ve oyun dünyasını keşfetmenizi, içerikler hakkında detaylı bilgilere ulaşmanızı ve yapay zeka destekli asistanı ile interaktif bir deneyim yaşamanızı sağlayan modern bir içerik keşif platformudur.

---

## 🚀 Hakkında

Archi, kullanıcıların kütüphanelerini yönetebileceği, yeni içerikler keşfedebileceği ve yapay zeka asistanı sayesinde hızlı öneriler alabileceği uçtan uca bir çözümdür.

**Problem:** Kullanıcılar izleyecekleri diziyi, okuyacakları kitabı veya oynayacakları oyunu bulmak için 
saatlerini harcıyor, farklı platformlar arasında gidip geliyor.

**Çözüm:** Archi, tüm içerik türlerini tek platformda topluyor. 
AI asistanı sayesinde saniyeler içinde kişiselleştirilmiş öneri alınabiliyor.

---

## 🛠 Kullanılan Teknolojiler

### Backend
- **ASP.NET Core** — Hızlı ve güvenli API altyapısı
- **Supabase** — PostgreSQL veritabanı yönetimi
- **Entity Framework Core** — ORM çözümü
- **Groq API (LLaMA 3.1)** — Yapay zeka destekli sohbet ve öneriler

### Frontend
- **Flutter** — Mobil ve web platformlarında tek kod tabanı ile yüksek performans

---

## ⚙️ Kurulum Adımları

### 1. Repoyu Klonlayın

```bash
git clone https://github.com/esmaozbalta/app-prenuer-up-school-weekend2026.git
cd app-prenuer-up-school-weekend2026
```

### 2. Backend Yapılandırması

- `backend/Archi.Api` klasörüne gidin.
- `appsettings.Development.json` dosyasını oluşturun.
- `.env.example` dosyasını inceleyerek gerekli değerleri doldurun:

```bash
cp .env.example .env
```

| Değişken | Açıklama |
|----------|----------|
| `ConnectionStrings__DefaultConnection` | Supabase PostgreSQL bağlantı stringi |
| `Jwt__SigningKey` | JWT imzalama anahtarı (min. 32 karakter) |
| `OpenAi__ApiKey` | Groq API anahtarı |
| `RawgApiKey` | RAWG oyun veritabanı API anahtarı |

### 3. Backend'i Başlatın

```bash
cd backend/Archi.Api
dotnet run
```

### 4. Frontend'i Başlatın

```bash
cd frontend/archi_app
flutter pub get
flutter run
```

---

## 🌐 Canlı Yayındaki Proje

| Servis | Link |
|--------|------|
| 🌍 Web Uygulaması | https://archi-frontend-b7l1.onrender.com |
| 📡 API | https://archi-api.onrender.com |
| 📖 Swagger Dokümantasyonu | https://archi-api.onrender.com/swagger |

---

## 📌 Özellikler

- ✅ **Keşfet Ekranı** — Film, dizi, kitap ve oyun dünyasına göz atın
- ✅ **Yapay Zeka Asistanı** — Dizi, film, kitap ve oyun önerileri alın
- ✅ **Arşiv Sistemi** — İçerikleri *Listem*, *Şu Anki Akış* ve *Arşivim* olarak 3 kategori ekleyip takip edin
- ✅ **Kullanıcı Yönetimi** — Kayıt ol, giriş yap, çıkış yap