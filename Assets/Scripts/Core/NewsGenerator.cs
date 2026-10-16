using System.Collections.Generic;
using UnityEngine;

public enum EvidenceSource { Map, Report, Laptop }

public class NewsItem
{
    public string headline;
    public bool isTrue;            // Doğruysa PAYLAŞ, yanlışsa SİL doğru karardır
    public string explanation;     // Karardan sonra gösterilen öğretici açıklama
    public EvidenceSource source;  // Doğrulamak için bakılması gereken eşya
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
        var service = Utils.Pick(WorldState.AllServices);
        var candidates = world.RegionsWithService(service, makeTrue);
        if (candidates.Count == 0) return null;

        var region = Utils.Pick(candidates);
        string name = GameNames.Of(region);

        return new NewsItem
        {
            headline = string.Format(Utils.Pick(serviceTemplates[service]), name),
            isTrue = makeTrue,
            source = EvidenceSource.Map,
            explanation = makeTrue
                ? $"Haritada {name} bölgesinde {GameNames.Of(service)} işareti var."
                : $"Haritada {name} bölgesinde {GameNames.Of(service)} işareti yok."
        };
    }

    NewsItem TeamNews(bool makeTrue)
    {
        var type = Random.value < 0.5f ? TeamType.Rescue : TeamType.Fire;
        var candidates = world.RegionsWithTeam(type, makeTrue);
        if (candidates.Count == 0) return null;

        var region = Utils.Pick(candidates);
        string name = GameNames.Of(region);

        return new NewsItem
        {
            headline = string.Format(Utils.Pick(teamTemplates[type]), name),
            isTrue = makeTrue,
            source = EvidenceSource.Report,
            explanation = makeTrue
                ? $"Görevlendirme raporuna göre {name} bölgesinde {GameNames.Of(type)} çalışıyor."
                : $"Görevlendirme raporunda {name} bölgesine {GameNames.Of(type)} atanmamış."
        };
    }

    NewsItem RoadNews(bool makeTrue)
    {
        var road = Utils.Pick(WorldState.AllRoads);
        bool actuallyOpen = world.roadOpen[road];
        bool claimOpen = makeTrue ? actuallyOpen : !actuallyOpen;
        string name = GameNames.Of(road);

        return new NewsItem
        {
            headline = string.Format(Utils.Pick(claimOpen ? roadOpenTemplates : roadClosedTemplates), name),
            isTrue = makeTrue,
            source = EvidenceSource.Laptop,
            explanation = $"Laptopta {name} {(actuallyOpen ? "yeşil, yani açık" : "kırmızı, yani kapalı")}."
        };
    }
}
