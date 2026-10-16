using UnityEngine;
using UnityEngine.UI;

// Barın görseli. Dolgu Image'ının Image Type = Filled, Fill Method = Horizontal olmalı.
// Pixel art'ta yumuşak dolgu bozuk görünmesin diye dolgu 1 piksellik adımlarla ilerler.
// Bar kritik seviyeye inince yanıp söner ve alarm çalar.
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

    [Header("Kritik seviye")]
    [Range(0f, 1f)] public float criticalThreshold = 0.25f;
    public float blinkSpeed = 6f;

    [Header("Yanlış kararda sarsıntı")]
    public RectTransform shakeTarget;
    public float shakeDuration = 0.25f;
    public float shakeStrength = 3f;

    float shakeTimer;
    Vector2 basePos;
    float current = 1f;
    bool alarmOn;

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
        SetAlarm(false);
    }

    void UpdateFill(float t)
    {
        current = t;
        fill.fillAmount = Mathf.Floor(t * pixelWidth) / pixelWidth;
        SetAlarm(t > 0f && t < criticalThreshold);
    }

    void SetAlarm(bool on)
    {
        if (alarmOn == on) return;
        alarmOn = on;
        AudioManager.SetAlarm(on);
    }

    void Shake() => shakeTimer = shakeDuration;

    void Update()
    {
        // Renk ve kritik seviyede yanıp sönme
        Color c = current > 0.5f ? highColor : current > criticalThreshold ? midColor : lowColor;
        if (current < criticalThreshold && Mathf.FloorToInt(Time.time * blinkSpeed) % 2 == 0)
            c = Color.Lerp(c, Color.white, 0.6f);
        fill.color = c;

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
