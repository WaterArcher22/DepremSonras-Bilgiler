using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

// Bir haber. Doğruluğu dünyaya bakılarak hesaplanır;
// böylece dünya değişirse ekrandaki haberin doğru cevabı da değişebilir.
public class NewsItem
{
    public string headline;
    public EvidenceSource source;   // Doğrulamak için bakılması gereken cihaz

    Func<bool> check;               // Dünyanın şu anki haline göre doğru mu?
    Func<string> explain;           // Dünyanın şu anki haline göre açıklama
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
    // ---- HABER ŞABLONLARI ----
    static readonly Dictionary<ServiceType, string[]> serviceTemplates = new Dictionary<ServiceType, string[]>
    {
        { ServiceType.Water, new[] {
            "SON DAKİKA: {0} bölgesinde su dağıtımı başladı!",
            "{0} bölgesindeki depremzedelere temiz su ulaştırılıyor.",
            "Yetkililer: {0} bölgesinde içme suyu dağıtım noktası kuruldu." } },
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

    // ---- ÜRETİM ----
    readonly WorldState world;
    readonly float trueChance;
    string lastHeadline;

    public NewsGenerator(WorldState world, float trueChance = 0.5f)
    {
        this.world = world;
        this.trueChance = trueChance;
    }

    public NewsItem Next()
    {
        // Aynı haber üst üste gelmesin
        for (int attempt = 0; attempt < 10; attempt++)
        {
            NewsItem item = TryCreate(Random.value < trueChance);
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
        switch (Random.Range(0, 3))
        {
            case 0:  return ServiceNews(makeTrue);
            case 1:  return TeamNews(makeTrue);
            default: return RoadNews(makeTrue);
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
