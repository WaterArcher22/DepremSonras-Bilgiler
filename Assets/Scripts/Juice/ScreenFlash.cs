using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Ekranın tamamını kaplayan, kısa süre renkli parlayıp sönen Image.
// Canvas'ın EN ALTINA (en öne) koyun, anchor'ı stretch-stretch olsun.
[RequireComponent(typeof(Image))]
public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance { get; private set; }

    [Range(0f, 1f)] public float maxAlpha = 0.3f;
    public float duration = 0.3f;

    Image image;

    void Awake()
    {
        Instance = this;
        image = GetComponent<Image>();
        image.raycastTarget = false;   // Tıklamaları engellemesin
        SetAlpha(0f);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Flash(Color color)
    {
        StopAllCoroutines();
        StartCoroutine(Run(color));
    }

    IEnumerator Run(Color color)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            color.a = maxAlpha * (1f - t / duration);
            image.color = color;
            yield return null;
        }
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        var c = image.color;
        c.a = a;
        image.color = c;
    }
}
