using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Haberin nereden geldiği. Kaynak bir ipucudur, kesin kanıt değil.
public enum NewsSourceType { Official, Media, Unverified }

// Bir haber. Doğruluğu dünyaya bakılarak hesaplanır;
// böylece dünya değişirse ekrandaki haberin doğru cevabı da değişebilir.
public class NewsItem
{
    public string headline;
    public EvidenceSource source;       // Doğrulamak için bakılması gereken cihaz (bilgi haberlerinde önemsiz)
    public string sourceLabel;          // Ekranda görünen kaynak, örn. "@sondakika_haber123"
    public NewsSourceType sourceType;
    public bool isKnowledgeNews;        // Masaya bakmadan, genel bilgiyle çözülen haber mi?

    Func<bool> check;                   // Dünyanın şu anki haline göre doğru mu?
    Func<string> explain;               // Dünyanın şu anki haline göre açıklama
    bool truthAtCreation;
    string explanationAtCreation;

    public NewsItem(string headline, EvidenceSource source, Func<bool> check, Func<string> explain)
    {
        this.headline = headline;
        this.source = source;
        this.check = check;
        this.explain = explain;
        truthAtCreation = check();
        explanationAtCreation = explain();
    }

    // live = true  -> dünya değiştiyse güncel duruma göre
    // live = false -> haber ilk geldiği andaki duruma göre
    public bool IsTrue(bool live) => live ? check() : truthAtCreation;
    public string Explanation(bool live) => live ? explain() : explanationAtCreation;
}

// Dünyaya bakarak doğru veya yanlış haber üretir.
// Haber cümlelerini buradaki listelerden değiştirip çoğaltabilirsiniz ({0} = bölge/yol adı).
public class NewsGenerator
{
    // ================= MASADAN DOĞRULANAN HABERLER =================

    static readonly Dictionary<ServiceType, string[]> serviceTemplates = new Dictionary<ServiceType, string[]>
    {
        { ServiceType.Water, new[] {
            "SON DAKİKA: {0} bölgesinde su dağıtımı başladı!",
            "{0} bölgesindeki depremzedelere temiz su ulaştırılıyor.",
            "{0} bölgesinde içme suyu dağıtım noktası kuruldu." } },
        { ServiceType.Food, new[] {
            "SON DAKİKA: {0} bölgesinde sıcak yemek dağıtılıyor!",
            "{0} bölgesine gıda kolileri ulaştı.",
            "{0} bölgesinde yemek dağıtım noktası açıldı." } },
        { ServiceType.Tent, new[] {
            "SON DAKİKA: {0} bölgesinde çadır kent kuruldu!",
            "Evsiz kalanlar için {0} bölgesinde çadır kampı hazır.",
            "{0} bölgesinde barınma çadırları kuruldu, vatandaşlar yönlendiriliyor." } },
    };

    static readonly Dictionary<TeamType, string[]> teamTemplates = new Dictionary<TeamType, string[]>
    {
        { TeamType.Rescue, new[] {
            "SON DAKİKA: Kurtarma ekipleri {0} bölgesine ulaştı!",
            "{0} bölgesinde enkaz altındakiler için kurtarma çalışması başladı.",
            "Arama kurtarma ekipleri {0} bölgesinde görev yapıyor." } },
        { TeamType.Fire, new[] {
            "SON DAKİKA: İtfaiye ekipleri {0} bölgesinde!",
            "{0} bölgesinde çıkan yangına itfaiye müdahale ediyor.",
            "İtfaiye ekipleri {0} bölgesine konuşlandı." } },
    };

    static readonly string[] roadOpenTemplates = {
        "SON DAKİKA: {0} ulaşıma açıldı!",
        "{0} yolu temizlendi, araç geçişi serbest.",
        "Vatandaşlar tahliye için {0} yolunu kullanabilir." };

    static readonly string[] roadClosedTemplates = {
        "SON DAKİKA: {0} ulaşıma kapandı!",
        "{0} yolunda göçük var, geçiş yapılamıyor.",
        "Uyarı: {0} yolunu kullanmayın, yol kapalı." };

    // ================= GENEL BİLGİYLE ÇÖZÜLEN HABERLER =================
    // Masaya bakmaya gerek yok; gerçek hayatta yanlış bilgiyi tanımayı öğretir.

    class KnowledgeNews
    {
        public string headline;
        public bool isTrue;
        public string explanation;
        public KnowledgeNews(string h, bool t, string e) { headline = h; isTrue = t; explanation = e; }
    }

    static readonly KnowledgeNews[] knowledgeNews =
    {
        // ---- YANLIŞ: tipik afet dezenformasyonu ----
        new KnowledgeNews(
            "Uzmanlar uyardı: Yarın saat 15:00'te 7 büyüklüğünde yeni bir deprem olacak!", false,
            "Depremler önceden tahmin edilemez. Saat ve tarih veren 'deprem tahmini' paylaşımları asılsızdır ve panik yaratır."),
        new KnowledgeNews(
            "Depremzedeler için acil bağış! Resmi değil ama hızlı: paranızı şu kişisel hesaba gönderin!", false,
            "Bağışlar yalnızca resmi kurumlar ve tanınmış yardım kuruluşları üzerinden yapılmalı. Kişisel hesaplarla toplanan 'bağışlar' sıkça dolandırıcılıktır."),
        new KnowledgeNews(
            "Arkadaşımın kuzeni söyledi: baraj çatladı, herkes hemen şehri terk etsin!", false,
            "Kaynağı belirsiz, 'bir tanıdık söyledi' diye başlayan haberler doğrulanmadan paylaşılmamalı. Toplu kaçış paniği yolları tıkar, kurtarma ekiplerini engeller."),
        new KnowledgeNews(
            "Bu mesajı 10 kişiye iletmezseniz yardım listesinden çıkarılırsınız!", false,
            "Zincir mesajlar dezenformasyonun en yaygın biçimidir. Yardım listeleri mesaj iletmeye bağlı değildir."),
        new KnowledgeNews(
            "Deprem gecesi gökyüzünde garip ışıklar görüldü, depremi gizli bir silah yaptı!", false,
            "Depremler yer kabuğundaki doğal hareketlerden kaynaklanır. Komplo teorileri dikkati gerçek ihtiyaçlardan uzaklaştırır."),
        new KnowledgeNews(
            "Tehlike tamamen geçti, herkes evine dönebilir!", false,
            "Artçı sarsıntılar günlerce sürebilir. Binalara dönmek için resmi hasar tespitinin sonucunu beklemek gerekir."),
        new KnowledgeNews(
            "Enkaz altında kimse kalmadı, arama çalışmaları bitti!", false,
            "Arama kurtarma çalışmalarının durumu yalnızca yetkili kurumlarca açıklanır. Erken 'bitti' haberleri yardımı engelleyebilir."),

        // ---- DOĞRU: resmi kurumlardan gelen gerçek uyarılar ----
        new KnowledgeNews(
            "Artçı sarsıntılar sürebilir; hasarlı binalara kesinlikle girmeyin.", true,
            "Doğru ve hayati bir uyarı. Büyük depremlerden sonra artçılar günlerce, hatta haftalarca sürebilir."),
        new KnowledgeNews(
            "Acil durumlar dışında telefonla arama yapmayın, hatları meşgul etmeyin; mesaj kullanın.", true,
            "Doğru. Afet sonrası hatlar kolayca tıkanır; kısa mesaj, acil aramalara yer açar."),
        new KnowledgeNews(
            "Enkaz altında kalanlar: sesinizi korumak için bağırmak yerine düdük çalın ya da borulara vurun.", true,
            "Doğru. Düdük ve vurma sesleri daha uzağa ulaşır, enerjinizi korumanızı sağlar."),
        new KnowledgeNews(
            "Gaz kaçağı riskine karşı vanaları kapalı tutun, açık ateş yakmayın.", true,
            "Doğru. Deprem sonrası yangınların önemli bir kısmı gaz kaçaklarından çıkar."),
        new KnowledgeNews(
            "Yardım bağışları yalnızca resmi kanallar üzerinden kabul ediliyor.", true,
            "Doğru. Resmi kanallar, yardımın gerçekten ihtiyaç sahiplerine ulaşmasını sağlar."),
    };

    // ================= KAYNAKLAR =================
    // Oyundaki kurgusal kurumlar. İstediğiniz gibi değiştirebilirsiniz.

    static readonly string[] officialSources = {
        "Afet Koordinasyon Merkezi (Resmi)",
        "Valilik Açıklaması (Resmi)",
        "Rasathane (Resmi)",
        "Belediye Basın Ofisi (Resmi)" };

    static readonly string[] mediaSources = {
        "Ulusal Haber Kanalı",
        "Yerel Radyo",
        "Şehir Gazetesi" };

    static readonly string[] unverifiedSources = {
        "@sondakika_haber123",
        "WhatsApp'tan iletildi",
        "Anonim sosyal medya hesabı",
        "Bir vatandaşın paylaşımı",
        "@gercekler_burada" };

    // ================= ÜRETİM =================

    readonly WorldState world;
    readonly float trueChance;
    readonly float knowledgeChance;
    readonly float sourceReliability;
    string lastHeadline;
    int lastKnowledgeIndex = -1;

    // knowledgeChance: haberlerin ne kadarı genel bilgiyle çözülen türden olsun
    // sourceReliability: masadan doğrulanan haberlerde kaynağın doğrulukla ne kadar uyumlu olduğu
    //   (0.5 = hiç ipucu vermez, 1 = kaynak her zaman doğruyu söyler)
    public NewsGenerator(WorldState world, float trueChance = 0.5f, float knowledgeChance = 0.25f, float sourceReliability = 0.7f)
    {
        this.world = world;
        this.trueChance = trueChance;
        this.knowledgeChance = knowledgeChance;
        this.sourceReliability = sourceReliability;
    }

    public NewsItem Next()
    {
        // Aynı haber üst üste gelmesin
        for (int attempt = 0; attempt < 10; attempt++)
        {
            NewsItem item = Random.value < knowledgeChance
                ? CreateKnowledgeNews()
                : TryCreate(Random.value < trueChance);

            if (item != null && item.headline != lastHeadline)
            {
                lastHeadline = item.headline;
                return item;
            }
        }
        return TryCreate(true) ?? TryCreate(false);
    }

    NewsItem TryCreate(bool makeTrue)
    {
        NewsItem item;
        switch (Random.Range(0, 3))
        {
            case 0:  item = ServiceNews(makeTrue); break;
            case 1:  item = TeamNews(makeTrue); break;
            default: item = RoadNews(makeTrue); break;
        }
        if (item != null) AssignDeskNewsSource(item, makeTrue);
        return item;
    }

    // Masadan doğrulanan haberlerde kaynak bir ipucu: doğru haberler çoğunlukla güvenilir kaynaktan,
    // yanlışlar çoğunlukla doğrulanmamış kaynaktan gelir. Ama her zaman değil, yine de kontrol etmek gerekir.
    // Resmi kaynaklar hiçbir zaman yanlış haber vermez; medya ise bazen hata yapabilir.
    void AssignDeskNewsSource(NewsItem item, bool isTrue)
    {
        if (isTrue)
        {
            if (Random.value < sourceReliability)
                SetSource(item, Random.value < 0.5f ? NewsSourceType.Official : NewsSourceType.Media);
            else
                SetSource(item, NewsSourceType.Unverified);   // Doğru ama kaynağı belirsiz
        }
        else
        {
            if (Random.value < 1f - sourceReliability)
                SetSource(item, NewsSourceType.Media);         // Medya da yanılabilir
            else
                SetSource(item, NewsSourceType.Unverified);
        }
    }

    // ---- Öğretici gibi belirli bir haber türü isteyen yerler için ----

    // Belirli bir cihazdan doğrulanan, istenen doğrulukta bir haber
    public NewsItem CreateDeskNews(EvidenceSource type, bool makeTrue)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            NewsItem item;
            switch (type)
            {
                case EvidenceSource.Map:    item = ServiceNews(makeTrue); break;
                case EvidenceSource.Report: item = TeamNews(makeTrue); break;
                default:                    item = RoadNews(makeTrue); break;
            }
            if (item != null)
            {
                AssignDeskNewsSource(item, makeTrue);
                return item;
            }
        }
        return null;
    }

    // Listedeki belirli bir bilgi haberi (0 = deprem tahmini)
    public NewsItem CreateKnowledgeNews(int index)
    {
        index = Mathf.Clamp(index, 0, knowledgeNews.Length - 1);
        lastKnowledgeIndex = index;
        return BuildKnowledgeNews(knowledgeNews[index]);
    }

    NewsItem CreateKnowledgeNews()
    {
        int index;
        do index = Random.Range(0, knowledgeNews.Length);
        while (knowledgeNews.Length > 1 && index == lastKnowledgeIndex);
        lastKnowledgeIndex = index;
        return BuildKnowledgeNews(knowledgeNews[index]);
    }

    NewsItem BuildKnowledgeNews(KnowledgeNews k)
    {
        var item = new NewsItem(k.headline, EvidenceSource.Map, () => k.isTrue, () => k.explanation)
        {
            isKnowledgeNews = true
        };
        // Doğru uyarılar resmi kurumlardan, dezenformasyon doğrulanmamış kaynaklardan gelir
        SetSource(item, k.isTrue ? NewsSourceType.Official : NewsSourceType.Unverified);
        return item;
    }

    static void SetSource(NewsItem item, NewsSourceType type)
    {
        item.sourceType = type;
        switch (type)
        {
            case NewsSourceType.Official: item.sourceLabel = Utils.Pick(officialSources); break;
            case NewsSourceType.Media:    item.sourceLabel = Utils.Pick(mediaSources); break;
            default:                      item.sourceLabel = Utils.Pick(unverifiedSources); break;
        }
    }

    NewsItem ServiceNews(bool makeTrue)
    {
        var w = world;
        var service = Utils.Pick(WorldState.AllServices);
        var candidates = w.RegionsWithService(service, makeTrue);
        if (candidates.Count == 0) return null;

        var region = Utils.Pick(candidates);
        string name = GameNames.Of(region);
        string serviceName = GameNames.Of(service);

        return new NewsItem(
            string.Format(Utils.Pick(serviceTemplates[service]), name),
            EvidenceSource.Map,
            () => w.HasService(region, service),
            () => w.HasService(region, service)
                ? $"Haritada {name} bölgesinde {serviceName} işareti var."
                : $"Haritada {name} bölgesinde {serviceName} işareti yok.");
    }

    NewsItem TeamNews(bool makeTrue)
    {
        var w = world;
        var type = Random.value < 0.5f ? TeamType.Rescue : TeamType.Fire;
        var candidates = w.RegionsWithTeam(type, makeTrue);
        if (candidates.Count == 0) return null;

        var region = Utils.Pick(candidates);
        string name = GameNames.Of(region);
        string teamName = GameNames.Of(type);

        return new NewsItem(
            string.Format(Utils.Pick(teamTemplates[type]), name),
            EvidenceSource.Report,
            () => w.HasTeam(region, type),
            () => w.HasTeam(region, type)
                ? $"Görevlendirme raporuna göre {name} bölgesinde {teamName} çalışıyor."
                : $"Görevlendirme raporunda {name} bölgesine {teamName} atanmamış.");
    }

    NewsItem RoadNews(bool makeTrue)
    {
        var w = world;
        var road = Utils.Pick(WorldState.AllRoads);
        bool claimOpen = makeTrue ? w.roadOpen[road] : !w.roadOpen[road];
        string name = GameNames.Of(road);

        return new NewsItem(
            string.Format(Utils.Pick(claimOpen ? roadOpenTemplates : roadClosedTemplates), name),
            EvidenceSource.Laptop,
            () => w.roadOpen[road] == claimOpen,
            () => $"Laptopta {name} {(w.roadOpen[road] ? "yeşil, yani açık" : "kırmızı, yani kapalı")}.");
    }
}
