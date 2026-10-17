using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Laptop penceresi: her yolun durumunu gösterir.
// İki şekilde kullanılabilir:
//  - Açık ve kapalı için ayrı resimler (örn. yeşil / kırmızı yol çizimi)
//  - Sadece kapalı için bir işaret (örn. ünlem): Open Sprite boş bırakılırsa açık yolda hiçbir şey görünmez
public class LaptopWindow : DeskWindow
{
    [Serializable]
    public class RoadView
    {
        public Road road;
        public Image image;
        [Tooltip("Yol açıkken gösterilecek resim. Boş bırakılırsa açıkken hiçbir şey görünmez.")]
        public Sprite openSprite;
        [Tooltip("Yol kapalıyken gösterilecek resim (örn. ünlem). Boş bırakılırsa kapalıyken hiçbir şey görünmez.")]
        public Sprite closedSprite;
    }

    public List<RoadView> roads = new List<RoadView>();

    [Tooltip("Açık: resim kendi piksel boyutuna getirilir. Kapalı: Image'a sizin verdiğiniz boyut korunur.")]
    public bool useNativeSize = false;

    public override void Setup(WorldState world)
    {
        foreach (var r in roads)
        {
            if (!r.image) continue;

            Sprite sprite = world.roadOpen[r.road] ? r.openSprite : r.closedSprite;

            // Sprite yoksa beyaz kare görünmesin diye Image'ı tamamen gizle
            r.image.enabled = sprite != null;
            if (!sprite) continue;

            r.image.sprite = sprite;
            var c = r.image.color;
            if (c.a <= 0.01f) { c.a = 1f; r.image.color = c; }   // Saydam bırakıldıysa görünür yap
            if (useNativeSize) r.image.SetNativeSize();
        }
    }
}
