using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Laptop penceresi: her yol için ayrı çizilmiş yeşil (açık) / kırmızı (kapalı) resmi gösterir.
public class LaptopWindow : DeskWindow
{
    [Serializable]
    public class RoadView
    {
        public Road road;
        public Image image;
        public Sprite openSprite;    // yeşil
        public Sprite closedSprite;  // kırmızı
    }

    public List<RoadView> roads = new List<RoadView>();

    public override void Setup(WorldState world)
    {
        foreach (var r in roads)
        {
            if (!r.image) continue;
            r.image.sprite = world.roadOpen[r.road] ? r.openSprite : r.closedSprite;
            r.image.SetNativeSize(); // pixel art ölçeği bozulmasın
        }
    }
}
