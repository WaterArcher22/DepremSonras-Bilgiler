using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Harita penceresi: her bölgenin üstündeki küçük su / yemek / çadır işaretlerini açıp kapatır
// ve bölge adlarını yazar.
// Her bölge için Inspector'da bir satır doldurun ve haritadaki objeleri sürükleyin.
public class MapWindow : DeskWindow
{
    [Serializable]
    public class RegionMarkers
    {
        public Region region;
        [Tooltip("Bölge adının yazılacağı yazı (isteğe bağlı). Kod doldurur.")]
        public TMP_Text nameLabel;
        public GameObject waterIcon;
        public GameObject foodIcon;
        public GameObject tentIcon;
    }

    public List<RegionMarkers> regions = new List<RegionMarkers>();
    [Tooltip("Uzun isimleri iki satıra böl (örn. 'Frostmere\\nExpanse')")]
    public bool splitNamesIntoTwoLines = true;

    public override void Setup(WorldState world)
    {
        foreach (var m in regions)
        {
            if (m.nameLabel)
            {
                string name = GameNames.Of(m.region);
                m.nameLabel.text = splitNamesIntoTwoLines ? name.Replace(' ', '\n') : name;
            }

            if (m.waterIcon) m.waterIcon.SetActive(world.HasService(m.region, ServiceType.Water));
            if (m.foodIcon)  m.foodIcon.SetActive(world.HasService(m.region, ServiceType.Food));
            if (m.tentIcon)  m.tentIcon.SetActive(world.HasService(m.region, ServiceType.Tent));
        }
    }
}
