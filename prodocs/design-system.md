**CAS + DMS PLATFORM**

**Design System**


İçerik Toplama Sistemi ve Belge Yönetim Sistemi için birleşik, soft mor tonlu tasarım rehberi. Bu doküman; renk paleti, tipografi, bileşen kütüphanesi, boşluk skalası ve mobil ekran tasarım ilkelerini kapsamaktadır.

| **📡 CAS Modülü** | **📁 DMS Modülü** | **📱 Mobile First** |
| ----------------- | ----------------- | ------------------- |

Versiyon: 1.0.0

Tarih: Nisan 2025

# **1\. Giriş ve Tasarım Felsefesi**

Bu tasarım sistemi, CAS (Content Aggregation System - İçerik Toplama Sistemi) ve DMS (Document Management System - Belge Yönetim Sistemi) platformu için geliştirilmiştir. Her iki modül ortak bir tasarım dili ve bileşen kütüphanesi paylaşırken, modüle özgü renk kimlikleriyle ayrışır.

## **1.1 Temel Prensipler**

- Mobile First: Tüm bileşenler önce 375px genişlikte tasarlanır, büyük ekranlara doğru ölçeklenir.
- Soft & Accessible: Yumuşak mor tonları, yüksek kontrast olmaksızın hiyerarşi kurar. WCAG AA uyumluluğu hedeflenir.
- Unified yet Distinct: CAS ve DMS aynı temel sistemden beslenir; CAS lavender/violet, DMS sage/teal aksanlarıyla ayrışır.
- Component-Based: Her UI elemanı bağımsız, yeniden kullanılabilir bileşenlerden oluşur.
- Consistent Spacing: 4px taban birimi üzerinde kurulu matematiksel boşluk skalası.

## **1.2 Modül Renk Kimlikleri**

| **📡 CAS - Lavender** | **📁 DMS - Sage Teal** |
| --------------------- | ---------------------- |

CAS modülü, içerik keşfi ve bilgi akışını simgeleyen lavender-violet tonları kullanır. DMS modülü, güven, düzen ve doğayı çağrıştıran soft teal-sage tonları ile ayrışır.

# **2\. Renk Paleti**

Palet; soft mor ağırlıklı birincil renkler, CAS ve DMS modül aksanları, semantik renkler ve nötr tonlardan oluşmaktadır. Tüm renkler light/dark mode uyumlu olacak şekilde tasarlanmıştır.

## **2.1 Primary Purple - Temel Mor Skalası**

| **Purple 900**<br><br>#2D1B69 | **Purple 800**<br><br>#3D2B82 | **Purple 700**<br><br>#5B3FA8 | **Purple 600**<br><br>#7B5CCC |
| ----------------------------- | ----------------------------- | ----------------------------- | ----------------------------- |

| **Purple 500**<br><br>#9B7EE0 | **Purple 400**<br><br>#B8A5ED | **Purple 300**<br><br>#D4C6F5 | **Purple 200**<br><br>#E8E0FA |
| ----------------------------- | ----------------------------- | ----------------------------- | ----------------------------- |

| **Purple 100**<br><br>#F3EFFD | **Purple 50**<br><br>#FAF8FF | #FFFFFF | #FFFFFF |
| ----------------------------- | ---------------------------- | ------- | ------- |

_Kullanım Kuralı: 800-900 → başlıklar ve güçlü vurgu. 600-700 → interaktif elemanlar, linkler, seçili durum. 300-500 → sınırlar, ayırıcılar. 50-200 → yüzey, kart arka planı._

## **2.2 CAS Aksanı - Lavender Violet**

| **CAS 600 - Ana**<br><br>#7B5CCC | **CAS 400 - Orta**<br><br>#B8A5ED | **CAS 100 - Soft BG**<br><br>#EDE8FA | **White**<br><br>#FFFFFF |
| -------------------------------- | --------------------------------- | ------------------------------------ | ------------------------ |

## **2.3 DMS Aksanı - Sage Teal**

| **DMS 600 - Ana**<br><br>#3B8C7A | **DMS 400 - Orta**<br><br>#72C5B5 | **DMS 100 - Soft BG**<br><br>#E0F5F1 | **White**<br><br>#FFFFFF |
| -------------------------------- | --------------------------------- | ------------------------------------ | ------------------------ |

## **2.4 Nötr Renk Skalası**

| **Neutral 800**<br><br>#2A2440 | **Neutral 600**<br><br>#5C5480 | **Neutral 400**<br><br>#9490B0 | **Neutral 200**<br><br>#D8D4EC |
| ------------------------------ | ------------------------------ | ------------------------------ | ------------------------------ |

| **Neutral 100**<br><br>#EEEAF8 | **Neutral 50**<br><br>#F8F7FD | **White**<br><br>#FFFFFF | #FFFFFF |
| ------------------------------ | ----------------------------- | ------------------------ | ------- |

_Not: Nötr renkler hafif mor-tinted yapıdadır; saf gri değil. Bu sayede tüm ekran soğuk ton bütünlüğünü korur._

## **2.5 Semantik Renkler**

| **Warning BG**<br><br>#FEF3E2 | **Warning Text**<br><br>#C4720A | **Danger BG**<br><br>#FDEAEA | **Danger Text**<br><br>#B03030 |
| ----------------------------- | ------------------------------- | ---------------------------- | ------------------------------ |

| **Success BG**<br><br>#E0F5EC | **Success Text**<br><br>#2E7D5E | **Info BG**<br><br>#F3EFFD | **Info Text**<br><br>#5B3FA8 |
| ----------------------------- | ------------------------------- | -------------------------- | ---------------------------- |

## **2.6 Renk Kullanım Tablosu**

| **Token**               | **Değer** | **Kullanım**                       |
| ----------------------- | --------- | ---------------------------------- |
| \--color-primary        | #5B3FA8   | Ana eylem butonu, navigasyon aktif |
| \--color-primary-soft   | #E8E0FA   | Buton hover bg, chip arka planı    |
| \--color-cas            | #7B5CCC   | CAS modülü aksanı, badge, link     |
| \--color-dms            | #3B8C7A   | DMS modülü aksanı, badge, link     |
| \--color-surface        | #FAF8FF   | Sayfa arka planı                   |
| \--color-card           | #FFFFFF   | Kart arka planı                    |
| \--color-border         | #D8D4EC   | Standart sınır                     |
| \--color-text-primary   | #2A2440   | Başlık, gövde metni                |
| \--color-text-secondary | #5C5480   | Meta, açıklama                     |
| \--color-text-muted     | #9490B0   | Zaman damgası, placeholder         |

# **3\. Tipografi**

İki font ailesi kullanılır: Syne (başlık/display) ve DM Sans (gövde). Bu ikili; modern, soft ve okunabilir bir ton yaratır.

## **3.1 Font Aileleri**

| **Font Ailesi**                   | **Kullanım Alanı**                        |
| --------------------------------- | ----------------------------------------- |
| Syne (Bold 700, SemiBold 600)     | Ekran başlıkları, section header, app adı |
| DM Sans (Regular 400, Medium 500) | Gövde metni, etiket, caption, buton       |
| JetBrains Mono                    | Kod parçacıkları, token adları, API       |

## **3.2 Tipografi Skalası - Mobile**

| **Stil**   | **Boyut / Ağırlık**   | **Kullanım**                     |
| ---------- | --------------------- | -------------------------------- |
| Display    | 28px / Syne 700       | Karşılama ekranı, hero           |
| Heading 1  | 22px / Syne 700       | Ekran başlığı                    |
| Heading 2  | 18px / Syne 600       | Bölüm başlığı                    |
| Heading 3  | 15px / DM Sans 500    | Alt bölüm, kart başlığı          |
| Body       | 14px / DM Sans 400    | Temel metin, 1.6 satır aralığı   |
| Body Small | 13px / DM Sans 400    | Yardımcı açıklama                |
| Caption    | 12px / DM Sans 400    | Zaman damgası, meta bilgi        |
| Label      | 10px / DM Sans 600    | Uppercase sekme, rozet, kategori |
| Code       | 12px / JetBrains Mono | Kod, token adı                   |

_Minimum font boyutu 12px'dir (erişilebilirlik). Label / uppercase elemanlar letter-spacing: 0.08em kullanır._

# **4\. Boşluk Skalası**

4px taban birimli matematiksel skala. Tüm margin, padding ve gap değerleri bu değerlerin katlarından oluşur.

## **4.1 Temel Değerler**

| **space-1** | 4px | Mikro boşluk - ikon-metin arası |
| ----------- | --- | ------------------------------- |

| **space-2** | 8px | Küçük gap - badge içi, liste öğe arası |
| ----------- | --- | -------------------------------------- |

| **space-3** | 12px | Standart gap - kartlar arası |
| ----------- | ---- | ---------------------------- |

| **space-4** | 16px | Sayfa padding - mobile standart kenar boşluğu |
| ----------- | ---- | --------------------------------------------- |

| **space-5** | 20px | Orta boşluk - section içi |
| ----------- | ---- | ------------------------- |

| **space-6** | 24px | Büyük boşluk - card padding dikey |
| ----------- | ---- | --------------------------------- |

| **space-8** | 32px | Section gap - bölümler arası |
| ----------- | ---- | ---------------------------- |

| **space-10** | 40px | Sayfa üst/alt boşluk |
| ------------ | ---- | -------------------- |

| **space-12** | 48px | Header yüksekliği |
| ------------ | ---- | ----------------- |

## **4.2 Border Radius Skalası**

| **Token**      | **Değer - Kullanım**           |
| -------------- | ------------------------------ |
| \--radius-sm   | 6px - Input, buton, chip       |
| \--radius-md   | 10px - Kart, dialog küçük      |
| \--radius-lg   | 14px - Ana kart, bottom sheet  |
| \--radius-xl   | 20px - Modal, tam ekran panel  |
| \--radius-full | 9999px - Avatar, tag pill, FAB |

# **5\. Bileşen Kütüphanesi**

## **5.1 Butonlar**

| **Varyant** | **Açıklama**                     | **Kullanım**           |
| ----------- | -------------------------------- | ---------------------- |
| Primary     | Solid mor (#5B3FA8), beyaz metin | Ana eylem, form gönder |
| Secondary   | Soft mor BG (#E8E0FA), mor metin | İkincil eylem          |
| CAS Accent  | Lavender solid (#7B5CCC)         | CAS modülü eylemleri   |
| DMS Accent  | Sage teal solid (#3B8C7A)        | DMS modülü eylemleri   |
| Outline     | Şeffaf BG, mor border            | Nötr eylem             |
| Ghost       | Şeffaf, mor metin                | Geri / atla eylemleri  |
| Destructive | Kırmızı solid                    | Silme, geri al         |
| Disabled    | Neutral 200 BG                   | Kullanım dışı durum    |

## **5.2 Buton Boyutları**

| **Boyut**   | **Özellikler**                                            |
| ----------- | --------------------------------------------------------- |
| Large (LG)  | 48px yükseklik, 16px yatay padding, font 15px - CTA       |
| Medium (MD) | 40px yükseklik, 16px yatay padding, font 14px - standart  |
| Small (SM)  | 32px yükseklik, 12px yatay padding, font 12px - liste içi |
| Icon Button | 40x40px kare/tam yuvarlak - yalnız ikon                   |

## **5.3 Rozet ve Etiketler (Badges)**

| **Tip**  | **Renk**                      | **Kullanım**                   |
| -------- | ----------------------------- | ------------------------------ |
| CAS      | Lavender (#cas100 / #cas600)  | İçerik kaynağı etiketi         |
| DMS      | Teal (#dms100 / #dms600)      | Belge tipi etiketi             |
| Aktif    | Yeşil (#successBg / #success) | Kaynak durumu - aktif          |
| Bekliyor | Amber (#warningBg / #warning) | Onay bekliyor                  |
| Hata     | Kırmızı (#dangerBg / #danger) | Sync hatası, yükleme başarısız |
| Nötr     | Neutral 100 / Neutral 600     | Kategori, etiket, versiyon     |

## **5.4 Form Alanları**

| **Durum** | **Görünüm**                   | **Açıklama**       |
| --------- | ----------------------------- | ------------------ |
| Default   | Neutral 200 border            | Normal boş alan    |
| Focused   | Purple 600 border (2px)       | Aktif yazma durumu |
| Filled    | Neutral 200 border, mor metin | Dolu alan          |
| Error     | Danger kırmızı border         | Validasyon hatası  |
| Disabled  | Neutral 100 bg, gri metin     | Kullanım dışı      |
| Search    | Sol ikon + Neutral 100 bg     | Arama alanı        |

## **5.5 Kart Bileşenleri**

| **Kart Tipi**       | **Açıklama ve Kullanım**                                          |
| ------------------- | ----------------------------------------------------------------- |
| Content Card (CAS)  | Thumbnail + kaynak etiketi + başlık + meta. Feed akışı için.      |
| Document Card (DMS) | Dosya ikonu + isim + boyut/tarih + eylem. Liste görünümü.         |
| Stat Card           | Büyük sayı + küçük etiket. Dashboard özet metriği.                |
| Source Card         | Kaynak logosu + sync durumu + içerik sayısı. CAS kaynak yönetimi. |
| Folder Card (DMS)   | Klasör ikonu + ad + belge sayısı. DMS klasör yapısı.              |
| Notification Card   | Sol renkli borderlı bildirim. Info, warning, danger varyantları.  |

# **6\. Mobil UX İlkeleri**

## **6.1 Navigasyon Yapısı**

| **Bölge**          | **İçerik**                                                |
| ------------------ | --------------------------------------------------------- |
| Bottom Tab Bar     | 5 sekme: Ana Sayfa / CAS / DMS / Bildirim / Profil        |
| Status Bar         | Platform native - koyu mor (primary #2D1B69) arka plan    |
| App Header         | Kullanıcı selamı + avatar + arama çubuğu - koyu mor yüzey |
| Content Area       | Soft mor / beyaz arka plan, scroll area                   |
| FAB (İsteğe bağlı) | İçerik ekle / belge yükle - mor solid, sağ alt köşe       |

## **6.2 Dokunma Hedefleri**

- Minimum dokunma alanı: 44x44px (Apple HIG & Material 3 standardı).
- Liste öğeleri minimum 56px yüksekliğinde olmalıdır.
- Butonlar arası minimum 8px boşluk bırakılmalıdır.
- Kritik eylemler (sil, yayınla) confirm dialog gerektirir.

## **6.3 Unified Dashboard - İki Modül Birlikte**

| **Bileşen**           | **Açıklama**                                                        |
| --------------------- | ------------------------------------------------------------------- |
| Üst İstatistik Şeridi | 3 chip: Yeni İçerik (mor), Belge (teal), Görev (amber)              |
| Sekme Navigasyonu     | Akış / Belgeler / Kaynaklar - ince alt çizgi vurgusu                |
| Birleşik Feed         | CAS içerikleri ve DMS olayları tek akışta, modül rozeti ile ayrışır |
| Hızlı Eylem Kartı     | Son eklenen belge veya içerik öne çıkar                             |

## **6.4 Erişilebilirlik**

- Kontrast oranı: Tüm metin/arka plan kombinasyonları WCAG AA (4.5:1) hedefi.
- Mor 700 (#5B3FA8) beyaz üzerinde: 4.9:1 - AA uyumlu.
- Mor 800 (#3D2B82) beyaz üzerinde: 7.2:1 - AAA uyumlu.
- Semantik HTML ve ARIA etiketleri zorunludur.
- Metin boyutu minimum 12px, tercih edilen 14px+.

# **7\. İkon Sistemi**

Phosphor Icons (MIT lisanslı) önerilen ikon kütüphanesidir. Alternatif olarak Lucide Icons kullanılabilir.

## **7.1 Boyut Skalası**

| **Boyut** | **Kullanım**                   |
| --------- | ------------------------------ |
| 16px      | Buton içi, satır içi ikon      |
| 20px      | Liste öğesi, input ikon        |
| 24px      | Navigasyon (bottom tab)        |
| 32px      | Kart başlığı, section ikon     |
| 48px      | Boş durum (empty state) ikonu  |
| 64px+     | Onboarding, büyük illüstrasyon |

## **7.2 Modül İkon Renkleri**

| **Bağlam**  | **Renk**              |
| ----------- | --------------------- |
| CAS - aktif | #7B5CCC (CAS 600)     |
| CAS - pasif | #9490B0 (Neutral 400) |
| DMS - aktif | #3B8C7A (DMS 600)     |
| DMS - pasif | #9490B0 (Neutral 400) |
| Genel aktif | #5B3FA8 (Purple 700)  |
| Genel pasif | #9490B0 (Neutral 400) |
| Tehlike     | #B03030 (Danger)      |
| Uyarı       | #C4720A (Warning)     |

# **8\. Light / Dark Mode**

Sistem, kullanıcının cihaz tercihini (prefers-color-scheme) otomatik algılar. Manuel geçiş de desteklenir.

## **8.1 Dark Mode Token Eşleştirmesi**

| **Token**               | **Light Mode** | **Dark Mode** |
| ----------------------- | -------------- | ------------- |
| \--color-surface        | #FAF8FF        | #1A1628       |
| \--color-card           | #FFFFFF        | #241E38       |
| \--color-border         | #D8D4EC        | #3D3560       |
| \--color-text-primary   | #2A2440        | #EDE9FC       |
| \--color-text-secondary | #5C5480        | #A49BC8       |
| \--color-primary        | #5B3FA8        | #9B7EE0       |
| \--color-cas            | #7B5CCC        | #B8A5ED       |
| \--color-dms            | #3B8C7A        | #72C5B5       |

_Dark modda yüzey renkleri koyu mor-tinted yapıya döner; saf siyah kullanılmaz. Bu sayede gece kullanımında göz yorgunluğu azalır._

# **9\. Animasyon ve Geçişler**

| **Geçiş Tipi** | **Değer - Kullanım**                                  |
| -------------- | ----------------------------------------------------- |
| Hızlı (fast)   | 100ms ease-out - buton basma, toggle                  |
| Standart (std) | 200ms ease-in-out - renk değişimi, hover              |
| Orta (mid)     | 300ms ease-in-out - kart aç/kapat, dropdown           |
| Yavaş (slow)   | 400ms ease-in-out - sayfa geçişi, modal açılış        |
| Spring         | cubic-bezier(0.34,1.56,0.64,1) - FAB, toast, bildirim |

- Reduced motion medya sorgusu desteklenmeli; animasyonlar gerektiğinde kapatılabilmeli.
- Parallax ve döngüsel animasyon kullanılmaz (erişilebilirlik).
- Skeleton loading her içerik kartında zorunludur (şimşek efekti, mor toned).

# **10\. Proje Dosya Organizasyonu**

| **Klasör / Dosya**      | **İçerik**                             |
| ----------------------- | -------------------------------------- |
| /tokens/colors.json     | Tüm renk token'ları JSON formatında    |
| /tokens/typography.json | Font ailesi, boyut, ağırlık token'ları |
| /tokens/spacing.json    | Boşluk ve radius değerleri             |
| /components/common/     | Paylaşılan buton, input, badge, kart   |
| /components/cas/        | CAS'a özgü content card, feed, source  |
| /components/dms/        | DMS'e özgü doc card, folder, upload    |
| /screens/shared/        | Login, onboarding, profil, ayarlar     |
| /screens/cas/           | CAS akış, kaynak yönetimi, arama       |
| /screens/dms/           | DMS klasör, belge detay, yükleme       |
| /assets/icons/          | Phosphor icon set SVG'leri             |
| /assets/illustrations/  | Boş durum, onboarding SVG              |

# **11\. Tasarım Kontrol Listesi**

Her yeni ekran veya bileşen tasarlanmadan önce aşağıdaki maddeler kontrol edilmelidir:

- Minimum dokunma hedefi 44x44px sağlandı mı?
- Font boyutları ve satır aralıkları tipografi skalasına uygun mu?
- Renk kontrast oranları WCAG AA seviyesinde mi?
- Dark mode token'ları tanımlandı mı?
- Skeleton loading durumu tasarlandı mı?
- Boş durum (empty state) tasarlandı mı?
- Hata durumu tasarlandı mı?
- Animasyon reduced-motion ile uyumlu mu?
- ARIA etiketleri ve erişilebilir metin tanımlandı mı?
- Bileşen component kütüphanesine eklendi mi?

_CAS + DMS Design System v1.0 - Tüm hakları saklıdır._