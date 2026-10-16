using System;
using System.Collections.Generic;
using UnityEngine;

// Harita penceresi: her bölgenin üstündeki küçük su / yemek / çadır işaretlerini açıp kapatır.
// Her bölge için Inspector'da bir satır doldurun ve haritadaki ikon objelerini sürükleyin.
public class MapWindow : DeskWindow
{
    [Serializable]
    public class RegionMarkers
    {
        public Region region;
        public GameObject waterIcon;
        public GameObject foodIcon;
        public GameObject tentIcon;
    }

    public List<RegionMarkers> regions = new List<RegionMarkers>();

    public override void Setup(WorldState world)
    {
        foreach (var m in regions)
        {
            if (m.waterIcon) m.waterIcon.SetActive(world.HasService(m.region, ServiceType.Water));
            if (m.foodIcon)  m.foodIcon.SetActive(world.HasService(m.region, ServiceType.Food));
            if (m.tentIcon)  m.tentIcon.SetActive(world.HasService(m.region, ServiceType.Tent));
        }
    }
}
