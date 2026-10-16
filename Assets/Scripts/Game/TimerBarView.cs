using UnityEngine;
using UnityEngine.UI;

// Basit süre çubuğu (haber süresi, ziyaretçinin sabrı).
// Dolgu Image'ının Image Type = Filled, Fill Method = Horizontal olmalı.
public class TimerBarView : MonoBehaviour
{
    public Image fill;
    [Tooltip("Çubuğun iç genişliği (piksel). Dolgu piksel piksel azalır.")]
    public int pixelWidth = 60;

    public Color normalColor = new Color(0.9f, 0.9f, 0.9f);
    public Color warningColor = new Color(0.95f, 0.3f, 0.3f);
    [Range(0f, 1f)] public float warningThreshold = 0.3f;
    public bool blinkWhenLow = true;

    public void SetNormalized(float t)
    {
        if (!fill) return;
        t = Mathf.Clamp01(t);
        fill.fillAmount = Mathf.Floor(t * pixelWidth) / pixelWidth;

        bool low = t < warningThreshold;
        Color c = low ? warningColor : normalColor;
        if (low && blinkWhenLow && Mathf.FloorToInt(Time.time * 6f) % 2 == 0) c.a = 0.35f;
        fill.color = c;
    }
}
