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

    public bool HasService(Region r, ServiceType s) => services[r].Contains(s);

    public bool HasTeam(Region r, TeamType t) =>
        teams.Any(team => team.type == t && team.regions.Contains(r));

    public List<Region> RegionsWithService(ServiceType s, bool has) =>
        AllRegions.Where(r => HasService(r, s) == has).ToList();

    public List<Region> RegionsWithTeam(TeamType t, bool has) =>
        AllRegions.Where(r => HasTeam(r, t) == has).ToList();
}
