using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Bir ekip: "Kurtarma Ekibi 1" -> [Vesperia Hollow, Solis Ridge]
[Serializable]
public class Team
{
    public TeamType type;
    public int number;
    public List<Region> regions = new List<Region>();

    public string DisplayName => $"{GameNames.Of(type)} {number}";
}

// Her run başında rastgele kurulan dünya.
// Harita, görevlendirme raporu ve laptop bu veriden okur; haberler de buna göre doğru/yanlış olur.
public class WorldState
{
    public readonly Dictionary<Region, HashSet<ServiceType>> services = new Dictionary<Region, HashSet<ServiceType>>();
    public readonly List<Team> teams = new List<Team>();
    public readonly Dictionary<Road, bool> roadOpen = new Dictionary<Road, bool>();

    public static readonly Region[] AllRegions = (Region[])Enum.GetValues(typeof(Region));
    public static readonly Road[] AllRoads = (Road[])Enum.GetValues(typeof(Road));
    public static readonly ServiceType[] AllServices = (ServiceType[])Enum.GetValues(typeof(ServiceType));

    [Serializable]
    public class Settings
    {
        [Tooltip("Her hizmet (su/yemek/çadır) kaç bölgede olsun")]
        public int minRegionsPerService = 2;
        public int maxRegionsPerService = 3;

        [Tooltip("Her türden kaç ekip olsun (Kurtarma 1, Kurtarma 2...)")]
        public int teamsPerType = 2;

        [Tooltip("Her ekip kaç bölgede çalışsın")]
        public int minRegionsPerTeam = 1;
        public int maxRegionsPerTeam = 2;

        [Range(0f, 1f)] public float roadOpenChance = 0.5f;
    }

    public static WorldState Generate(Settings s)
    {
        var w = new WorldState();

        foreach (var r in AllRegions)
            w.services[r] = new HashSet<ServiceType>();

        // Hizmetleri rastgele bölgelere dağıt
        foreach (var service in AllServices)
        {
            int count = UnityEngine.Random.Range(s.minRegionsPerService, s.maxRegionsPerService + 1);
            foreach (var r in Utils.Shuffled(AllRegions).Take(count))
                w.services[r].Add(service);
        }

        // Ekipleri rastgele bölgelere ata
        foreach (TeamType type in Enum.GetValues(typeof(TeamType)))
        {
            for (int i = 1; i <= s.teamsPerType; i++)
            {
                int count = UnityEngine.Random.Range(s.minRegionsPerTeam, s.maxRegionsPerTeam + 1);
                w.teams.Add(new Team
                {
                    type = type,
                    number = i,
                    regions = Utils.Shuffled(AllRegions).Take(count).ToList()
                });
            }
        }

        // Yolları rastgele aç/kapat
        foreach (var road in AllRoads)
            w.roadOpen[road] = UnityEngine.Random.value < s.roadOpenChance;

        return w;
    }

    // ===== OYUN SIRASINDA DEĞİŞİMLER =====

    // Dünyada bir şey değiştiğinde hangi cihazın güncellendiğini bildirir
    public event Action<EvidenceSource> OnChanged;

    public void SetRoad(Road road, bool open)
    {
        if (roadOpen[road] == open) return;
        roadOpen[road] = open;
        OnChanged?.Invoke(EvidenceSource.Laptop);
    }

    public void SetService(Region region, ServiceType service, bool on)
    {
        if (HasService(region, service) == on) return;
        if (on) services[region].Add(service);
        else services[region].Remove(service);
        OnChanged?.Invoke(EvidenceSource.Map);
    }

    // Bu türden rastgele bir ekibe yeni bir bölge ekler
    public void AddTeamToRegion(TeamType type, Region region)
    {
        if (HasTeam(region, type)) return;
        var candidates = teams.Where(t => t.type == type).ToList();
        if (candidates.Count == 0) return;
        Utils.Pick(candidates).regions.Add(region);
        OnChanged?.Invoke(EvidenceSource.Report);
    }

    // Bir ekibi bir bölgeden alıp başka bir bölgeye kaydırır
    bool MoveRandomTeam()
    {
        var team = Utils.Pick(teams);
        var free = AllRegions.Where(r => !team.regions.Contains(r)).ToList();
        if (team.regions.Count == 0 || free.Count == 0) return false;

        team.regions.Remove(Utils.Pick(team.regions));
        team.regions.Add(Utils.Pick(free));
        OnChanged?.Invoke(EvidenceSource.Report);
        return true;
    }

    bool ChangeRandomService()
    {
        var service = Utils.Pick(AllServices);
        var with = RegionsWithService(service, true);
        var without = RegionsWithService(service, false);

        // Her hizmet en az 1 bölgede kalsın
        bool add;
        if (with.Count <= 1) add = true;
        else if (without.Count == 0) add = false;
        else add = UnityEngine.Random.value < 0.5f;

        if (add && without.Count > 0) SetService(Utils.Pick(without), service, true);
        else if (!add) SetService(Utils.Pick(with), service, false);
        else return false;
        return true;
    }

    // Rastgele tek bir değişiklik yapar
    public void RandomChange()
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            switch (UnityEngine.Random.Range(0, 3))
            {
                case 0:
                    var road = Utils.Pick(AllRoads);
                    SetRoad(road, !roadOpen[road]);
                    return;
                case 1:
                    if (ChangeRandomService()) return;
                    break;
                default:
                    if (MoveRandomTeam()) return;
                    break;
            }
        }
    }

    // ===== SORGULAR =====

    public bool HasService(Region r, ServiceType s) => services[r].Contains(s);

    public bool HasTeam(Region r, TeamType t) =>
        teams.Any(team => team.type == t && team.regions.Contains(r));

    public List<Region> RegionsWithService(ServiceType s, bool has) =>
        AllRegions.Where(r => HasService(r, s) == has).ToList();

    public List<Region> RegionsWithTeam(TeamType t, bool has) =>
        AllRegions.Where(r => HasTeam(r, t) == has).ToList();
}
