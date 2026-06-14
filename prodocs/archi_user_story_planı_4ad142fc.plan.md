---
name: Archi User Story Planı
overview: "`ims_prd.md` icindeki MVP kapsamini user story bazinda ayrıştırıp, Flutter + .NET + PostgreSQL stack icin uygulanabilir gelistirme adimlarina donusturecegim. Plan, backend ve frontendin tamamen ayrik ilerlemesini ve backendin Android/iOS istemcilerinden bagimsiz deploy edilmesini esas alir."
todos:
  - id: story-breakdown
    content: ims_prd.md icindeki gereksinimleri user story bazinda kesinlestir ve kapsamlandir
    status: completed
  - id: foundation-setup
    content: backend/frontend temel proje iskeletini, auth altyapisini ve veritabani migration yapisini olustur
    status: completed
  - id: content-flows
    content: icerik edinim + manuel ekleme + dms alanlarinin tum endpoint ve UI akislarini tamamla
    status: pending
  - id: social-profile
    content: kesif, public/private profil ve one cikanlar ozelliklerini implement et
    status: pending
  - id: ux-retention
    content: ana ekran in-progress, dark mode ve bildirim/nudge/weekly recap akislarini teslim et
    status: pending
  - id: quality-gate
    content: tum storyler icin test, performans, erisilebilirlik ve metric dogrulamasini tamamla
    status: pending
isProject: false
---

# Archi MVP - User Story Bazli Gelistirme Plani

Referans dokumanlar:
- [IMS PRD](c:\Users\Esma\Desktop\Archi\ims_prd.md)
- [Flutter kurallari](c:\Users\Esma\Desktop\Archi\.cursor\rules\frontend\flutter.mdc)
- [.NET kurallari](c:\Users\Esma\Desktop\Archi\.cursor\rules\backend\Dotnet.mdc)
- [Design system enforcement](c:\Users\Esma\Desktop\Archi\.cursor\rules\frontend\design-system-enforcement.mdc)
- [Design system](c:\Users\Esma\Desktop\Archi\.cursor\rules\frontend\design-system.md)

## Mimari Prensip (Ayrik Kod ve Ayrik Deploy)
- Backend ve frontend iki ayri kod tabani gibi yonetilir: `/backend` ve `/frontend`.
- Backend API-first gelistirilir; mobil uygulama sadece HTTP API sozlesmesine bagli olur.
- Backend bagimsiz pipeline ile deploy edilir (frontend release'inden bagimsiz versiyonlanir).
- Android ve iOS ayni backend servisini kullanir; mobil tarafta platforma ozel is sadece UI/cihaz entegrasyonunda kalir.
- Ortak gelisim ritmi: once API sozlesmesi (OpenAPI), sonra mobil entegrasyon.

## US-01 - Kayit Olma (E-posta + Kullanici Adi + Sifre)
- Durum: Tamamlandi
- Backend: `POST /api/v1/auth/register` endpointi, username unique kontrolu, sifre hashleme, JWT donusu.
- DB: `users` tablosu migration (id, email, username, password_hash, is_private).
- Frontend: Kayit formu (validasyon, hata durumlari, loading state), basarili kayit sonrasi oturum acma.
- Guvenlik: Minimum sifre politikasi, rate limit/lockout temel koruma, standart hata modeli.
- Test: Backend unit+integration (register success/fail), Flutter widget test (form validasyon).

## US-02 - Giris ve JWT Oturum Yonetimi
- Durum: Tamamlandi
- Backend: `POST /api/v1/auth/login`, refresh token stratejisi (MVP’de basit sliding session veya re-login).
- Frontend: Login ekrani, token secure storage, app acilisinda session restore.
- Altyapi: Auth middleware ve protected endpoint erisimi.
- Hata senaryolari: Hatali sifre, bulunamayan kullanici, token suresi doldu.
- Test: Auth middleware testleri, token expiry senaryolari.

## US-03 - Profil Gizliligi (Public/Private)
- Durum: Tamamlandi
- Backend: `PATCH /api/v1/profile/privacy` ile `is_private` guncelleme; `GET /api/v1/profile` (auth); `GET /api/v1/users/{id}/profile` (public/private kurallari).
- Yetkilendirme: Private profilde sadece sahip goruntuleyebilir; public profiller herkese acik.
- Frontend: Profil ayarlari ekraninda privacy toggle, durum degisince anlik UI guncelleme.
- Test: Profil goruntuleme izin kurallari (public/private/owner) + entegrasyon testleri.

## US-04 - Icerik Arama (TMDB + Google Books)
- Backend: Harici servis adapter katmani (`tmdb`, `googleBooks`) ve normalize edilmis arama response modeli.
- Caching: Kisa sureli sorgu cache’i (performans ve API kota optimizasyonu).
- Frontend: Arama ekrani (kategori filtreleri, loading/skeleton, empty state).
- UX KPI: Sonuc bulup ekleme akisinin 15 sn altina inmesi icin debounce + hizli aksiyon butonu.
- Test: Adapter testleri (mock API), timeout/fallback senaryolari.

## US-05 - Manuel Icerik Girisi
- Backend: `POST /api/v1/contents/manual` ile serbest icerik ekleme.
- Kurallar: Zorunlu alanlar (baslik, kategori), maksimum karakter ve temiz veri validasyonu.
- Frontend: Manuel ekleme formu + API’dan gelen icerikten ayristirilmis etiketleme.
- Test: Validasyon ve edge-case (uzun metin, gecersiz kategori).

## US-06 - Icerik Durumu, Puanlama ve Taste Notes
- Backend: `contents` kaydi icin status enum (InProgress/Completed/Later), 5 yildiz rating, 280 karakter note.
- Endpointler: Ekleme, guncelleme, listeleme, silme (`/api/v1/contents`).
- Frontend: Icerik detay/kartinda hizli status degistirme, yildiz secimi, note editor.
- Veri butunlugu: Kullanici sadece kendi icerigini guncelleyebilir.
- Test: CRUD + authorization + not limit testi.

## US-07 - Diger Kullanicilari Arama
- Backend: `GET /api/v1/users/search?username=` (prefix/contains, pagination).
- Gizlilik filtresi: Private kullanicilarin listeleme kurali PRD’ye uygun sekilde netlestirilip uygulanir.
- Frontend: Kesif ekraninda username arama, sonuc listesi, bos/hata durumlari.
- Test: Arama performansi ve indeks dogrulamasi.

## US-08 - Public Profil ve Arsiv Goruntuleme
- Backend: Public profil endpointleri (ozet metrik + liste).
- Kurallar: Private profilde `403/404` davranisi netlestirilip tutarli uygulanir.
- Frontend: Profil sayfasi (toplam icerik sayisi, listeler, basit filtreler).
- Test: Public/Private gorunurluk regresyon testleri.

## US-09 - One Cikanlar (Favori Sabitleme)
- Backend: Favori alanlari (`is_featured`, `featured_order`) ve max pinned limit (ornegin 3).
- Endpoint: Favori ekle/kaldir/sirala.
- Frontend: Profilde “One Cikanlar” alani, surukle-birak yerine MVP icin sirali secim butonlari.
- Test: Limit asimi ve siralama tutarliligi.

## US-10 - Ana Ekranda In-Progress Hizli Erisim
- Backend: `GET /api/v1/home/in-progress` (kullaniciya ozel kisa liste).
- Frontend: Home ekraninda “Su an devam edilenler” bolumu.
- UX: Empty state ve hizli “tamamlandi/sonra” aksiyonlari.
- Test: Home veri cekme, UI state gecisleri.

## US-11 - Karanlik Mod ve Minimalist UI Uyumu
- Frontend: Theme token yapisi, light/dark mapleme, sistem temasini otomatik takip.
- Tasarim uyumu: Tum ekranlar [Design system](c:\Users\Esma\Desktop\Archi\.cursor\rules\frontend\design-system.md) kurallarina gore normalize edilir.
- Erisilebilirlik: Minimum font/dokunma alani, kontrast ve hata/empty/skeleton durumlari.
- Test: Golden/snapshot testleri (light-dark), temel erisilebilirlik kontrolleri.

## US-12 - Haftalik Ozet ve Nudge Hatirlatma
- Backend: Haftalik recap hesaplama gorevi (BackgroundService/Cron), in-progress >7 gun nudge kurali.
- Bildirim stratejisi: MVP’de uygulama ici inbox veya push-ready soyutlama.
- Frontend: Bildirim listesi + recap karti + nudge aksiyonlari.
- Test: Zaman tabanli job testleri, duplicate bildirim korumasi.

## Fazlama ve Onceliklendirme
- Faz 1 (temel): US-01, US-02, US-06 + backend/frontend repo ayrimi + bagimsiz deploy pipeline temeli.
- Faz 2 (kesif/profil): US-03, US-07, US-08, US-09.
- Faz 3 (icerik edinim): US-04, US-05.
- Faz 4 (deneyim ve retention): US-10, US-11, US-12.

## Teknik Cikislar (Cross-cutting)
- Kod organizasyonu: `/backend` (.NET Web API), `/frontend` (Flutter) ayrik moduller.
- Deploy: Backend icin ayri CI/CD (build-test-migrate-deploy), frontend icin ayri mobil release pipeline.
- CI: Build + test pipeline (backend test, flutter test) ve contract check.
- API sozlesmesi: OpenAPI/Swagger ve frontend DTO uyumu.
- Gozlemlenebilirlik: Merkezi loglama, temel hata takip ve performans metriği (arama->ekleme suresi).