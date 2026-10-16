using System.Collections.Generic;
using UnityEngine;

// ===== Oyundaki sabit kavramlar =====

public enum Region
{
    AethelgardReach,
    VesperiaHollow,
    CinderpineBasin,
    FrostmereExpanse,
    SolisRidge,
    DuskwaterFen,
    IronrootMarch
}

public enum ServiceType { Water, Food, Tent }   // Haritada işaretle görünür
public enum TeamType { Rescue, Fire }           // Görevlendirme raporunda görünür

// Bir bilginin hangi cihazdan doğrulanacağı
public enum EvidenceSource { Map, Report, Laptop }

public enum Road                                // Laptopta görünür
{
    RavenwoodBoulevard,
    GildenscarWay,
    CopperhollowLane
}

// Ekranda görünen isimler
public static class GameNames
{
    static readonly Dictionary<Region, string> regions = new Dictionary<Region, string>
    {
        { Region.AethelgardReach,  "Aethelgard Reach" },
        { Region.VesperiaHollow,   "Vesperia Hollow" },
        { Region.CinderpineBasin,  "Cinderpine Basin" },
        { Region.FrostmereExpanse, "Frostmere Expanse" },
        { Region.SolisRidge,       "Solis Ridge" },
        { Region.DuskwaterFen,     "Duskwater Fen" },
        { Region.IronrootMarch,    "Ironroot March" },
    };

    static readonly Dictionary<Road, string> roads = new Dictionary<Road, string>
    {
        { Road.RavenwoodBoulevard, "Ravenwood Boulevard" },
        { Road.GildenscarWay,      "Gildenscar Way" },
        { Road.CopperhollowLane,   "Copperhollow Lane" },
    };

    public static string Of(Region r) => regions[r];
    public static string Of(Road r) => roads[r];

    public static string Of(ServiceType s)
    {
        switch (s)
        {
            case ServiceType.Water: return "su dağıtımı";
            case ServiceType.Food:  return "yemek dağıtımı";
            default:                return "çadır kampı";
        }
    }

    public static string Of(TeamType t) => t == TeamType.Rescue ? "Kurtarma Ekibi" : "İtfaiye Ekibi";
}

public static class Utils
{
    public static List<T> Shuffled<T>(IEnumerable<T> source)
    {
        var list = new List<T>(source);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    public static T Pick<T>(IList<T> list) => list[Random.Range(0, list.Count)];
}
