using UnityEngine;
using UnityEngine.UI;

// Barın görseli. Dolgu Image'ının Image Type = Filled, Fill Method = Horizontal olmalı.
// Pixel art'ta yumuşak dolgu bozuk görünmesin diye dolgu 1 piksellik adımlarla ilerler.
public class AccuracyBarView : MonoBehaviour
{
    public AccuracyBar bar;
    public Image fill;
    [Tooltip("Barın iç genişliği (piksel). Örn. 120x12'lik barda 116")]
    public int pixelWidth = 116;

    [Header("Renkler")]
    public Color highColor = new Color(0.3f, 0.85f, 0.4f);
    public Color midColor = new Color(0.95f, 0.8f, 0.2f);
    public Color lowColor = new Color(0.95f, 0.3f, 0.3f);

    [Header("Yanlış kararda sarsıntı")]
    public RectTransform shakeTarget;
    public float shakeDuration = 0.25f;
    public float shakeStrength = 3f;

    float shakeTimer;
    Vector2 basePos;

    void OnEnable()
    {
        bar.OnChanged += UpdateFill;
        bar.OnBurst += Shake;
        if (shakeTarget) basePos = shakeTarget.anchoredPosition;
    }

    void OnDisable()
    {
        bar.OnChanged -= UpdateFill;
        bar.OnBurst -= Shake;
    }

    void UpdateFill(float t)
    {
        fill.fillAmount = Mathf.Floor(t * pixelWidth) / pixelWidth;
        fill.color = t > 0.5f ? highColor : t > 0.25f ? midColor : lowColor;
    }

    void Shake() => shakeTimer = shakeDuration;

    void Update()
    {
        if (!shakeTarget) return;

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            // Tam piksel kaydır, yarım piksel bulanıklık olmasın
            var offset = new Vector2(Mathf.Round(Random.Range(-shakeStrength, shakeStrength)),
                                     Mathf.Round(Random.Range(-shakeStrength, shakeStrength)));
            shakeTarget.anchoredPosition = basePos + offset;
        }
        else
        {
            shakeTarget.anchoredPosition = basePos;
        }
    }
}
