# Hayati Bilgi

> **Afete hazırlık ve farkındalık** temalı game jam için geliştirildi.
> Alt başlık: **Doğru bilgiye erişim**

Büyük bir depremin ardından bir kriz iletişim merkezindesin. Televizyona durmadan haber düşüyor: bir kısmı doğru, bir kısmı söylenti, bir kısmı bilerek yayılan dezenformasyon. Görevin her haberi masandaki kaynaklarla doğrulamak ve halka yalnızca **doğru bilgiyi** ulaştırmak.

Doğrulanmamış tek bir paylaşım; paniğe, yanlış yere giden yardıma ve kaybedilen zamana yol açabilir.

---

## Nasıl oynanır

1. **TV'deki haberi oku.** Kaynağına da dikkat et: resmi kurum mu, haber kanalı mı, anonim bir hesap mı?
2. **Masadaki cihazlarla doğrula:**
   - **Tablet (harita):** Bölgelere göre su, yemek ve çadır dağıtımları
   - **Görevlendirme raporu:** Kurtarma ve itfaiye ekiplerinin nerede çalıştığı
   - **Laptop:** Yolların açık / kapalı durumu
3. **Karar ver:** Haber doğruysa **PAYLAŞ**, yanlışsa **SİL**.

Bazı haberler masaya bakmadan, genel bilgiyle çözülür. Örneğin "Yarın saat 15:00'te deprem olacak!" gibi bir haber her zaman yanlıştır, çünkü depremler önceden tahmin edilemez.

### Dikkat
- **Doğruluk barı** zamanla azalır. Doğru kararlar barı doldurur, yanlış kararlar boşaltır. Bar biterse oyun biter.
- Her haberin bir **süresi** vardır. Süre dolarsa haber kendiliğinden yayılır.
- Dünya oyun boyunca **değişir**. Bilgisi güncellenen cihaz turuncu yanıp söner.
- Arka arkaya doğru kararlar **kombo** yapar ve puanı katlar.
- Her kararın insanlar üzerindeki etkisi **Saha Raporu**'nda gösterilir.

### Kontroller
Oyun tamamen **fare** ile oynanır.

---

## Özellikler

- Her oyunda rastgele oluşturulan dünya: bölgeler, dağıtımlar, ekipler ve yollar
- Gerçek hayattaki afet dezenformasyonu örneklerine dayanan "bilgi haberleri"
- Haber kaynakları: resmi kurum, medya, doğrulanmamış hesap
- Kararların insanlara etkisini gösteren Saha Raporu ve bitiş ekranında "Yanlış bilginin bedeli" listesi
- Ana menü ve bitiş ekranında gerçek afete hazırlık ipuçları ("Biliyor muydun?")
- İlk oyunda adım adım öğretici
- Kombo, puan, unvanlar ve en yüksek skor

---

## Teknik bilgiler

| | |
|---|---|
| Motor | Unity 6 (URP 2D) |
| Dil | C# |
| Çözünürlük | 320×180 tabanlı pixel art (Pixel Perfect Camera, 16 PPU) |
| Yazı | TextMeshPro |
| Platform | [WebGL / Windows] |

### Sahneler
`StartScene` → `IntroScene` → `GameScene` → `EndScene`

### Klasör yapısı (Assets/Scripts)
| Klasör | İçerik |
|---|---|
| `Core` | Oyun verisi, dünya durumu, haber üretici, doğruluk barı, kararların etkisi, sahne geçişleri |
| `Game` | Oyun yöneticisi, TV, geri bildirim, puan/kombo, öğretici, saha raporu |
| `Desk` | Masadaki eşyalar ve pencereleri (harita, rapor, laptop) |
| `Visitor` | Odaya gelen ziyaretçiler ve istekleri |
| `Audio` | Ses yöneticisi, sahne müziği, ortam sesleri |
| `Juice` | Ekran sarsıntısı, flaş, damga, ortam ışıkları, kahve dumanı |
| `UI` / `Menus` | Menüler, giriş sahnesi, ipuçları |
| `Editor` | Editör araçları (toplu font değiştirici) |

### Projeyi açma
1. Repoyu klonlayın.
2. Unity Hub'dan **Unity 6** ile açın.
3. `Assets/Scenes/StartScene` sahnesini açıp Play'e basın.

---

## Ekip

| İsim | Rol |
|---|---|
| Deniz Murat, Efe Özgür| [Programlama / Tasarım / Ses] |
| Ömür Çamurcu | [Pixel art] |

---

## Kaynaklar ve lisanslar

### Görseller
Oyundaki **tüm görseller ve pixel art ekibimiz tarafından çizilmiştir.**

### Fontlar
- **Press Start 2P**, CodeMan38. SIL Open Font License 1.1
- **Pixelify Sans**, Stefie Justprince. SIL Open Font License 1.1



### Sesler ve müzik
kaynak: Freesound / Pixabay

---

## Yapay zekâ kullanımı



**Yapay zekâ kullanılmayan kısımlar:**
- Oyundaki **tüm görseller ve pixel art** ekibimiz tarafından elle çizilmiştir. Hiçbir görsel yapay zekâ ile üretilmemiştir.
- Oyun fikri, tema yorumu, oyun tasarımı ve tasarım kararları ekibimize aittir.
- Sahnelerin kurulumu, Unity içindeki yerleşim, arayüz düzeni, ses seçimi ve oyun dengesi ekibimiz tarafından yapılmıştır.

**Yapay zekânın kullanıldığı kısımlar:**
- **Kodlama:** Yapay zekâ (Anthropic Claude) bir kodlama asistanı olarak kullanılmıştır. C# script'lerinin önemli bir kısmının ilk taslakları yapay zekâ ile yazılmış; ekibimiz bu kodları projeye entegre etmiş, Unity'de bağlamış, test etmiş ve isteklerimiz doğrultusunda defalarca düzenlemiştir.
- **Hata ayıklama:** Hata mesajlarının ve beklenmeyen davranışların çözümünde yardımcı olarak kullanılmıştır.
- **Metinler:** Oyun içi haber metinlerinin ve afete hazırlık ipuçlarının bir kısmının taslağı yapay zekâ ile hazırlanmış, ekibimiz tarafından gözden geçirilmiştir.

Oyundaki afete hazırlık bilgileri genel kabul görmüş tavsiyelere dayanır. Gerçek bir afet durumunda lütfen **resmi kurumların** (AFAD, 112 vb.) duyurularını takip edin.

---

## Lisans
Görseller ekibe aittir ve izinsiz kullanılamaz.
