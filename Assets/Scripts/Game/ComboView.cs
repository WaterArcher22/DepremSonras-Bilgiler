using System.Collections;
using TMPro;
using UnityEngine;

// "SERİ 7  x3" sayacı. Seri arttıkça zıplar, çarpan yükselince büyük zıplar ve renk değiştirir.
// Seri bozulunca "SERİ BOZULDU" yazıp sarsılır ve kaybolur.
// Script her zaman açık duran bir objede; root ise görünüp kaybolan yazı.
public class ComboView : MonoBehaviour
{
    public ScoreManager score;
    public GameObject root;
    public TMP_Text label;

    [Tooltip("Sayaç bu seri uzunluğundan itibaren görünür")]
    public int showFromStreak = 2;

    [Tooltip("Çarpana göre renk: x1, x2, x3, x4")]
    public Color[] tierColors =
    {
        new Color(0.9f, 0.9f, 0.9f),
        new Color(1f, 0.9f, 0.3f),
        new Color(1f, 0.6f, 0.15f),
        new Color(1f, 0.3f, 0.3f)
    };

    [Header("Animasyon")]
    public float popScale = 1.3f;
    public float tierUpScale = 1.8f;
    public float popTime = 0.15f;

    [Header("Seri bozulunca")]
    public string breakText = "SERİ BOZULDU";
    public Color breakColor = new Color(0.95f, 0.3f, 0.3f);
    public float breakShowTime = 0.9f;
    public float shakePixels = 3f;

    Coroutine routine;
    Vector2 basePos;

    void Awake()
    {
        basePos = ((RectTransform)root.transform).anchoredPosition;
        root.SetActive(false);
    }

    void OnEnable()
    {
        if (!score) return;
        score.OnComboChanged += HandleCombo;
        score.OnComboBroken += HandleBreak;
    }

    void OnDisable()
    {
        if (!score) return;
        score.OnComboChanged -= HandleCombo;
        score.OnComboBroken -= HandleBreak;
    }

    void HandleCombo(int streak, int multiplier, bool tierUp)
    {
        if (streak < showFromStreak) return;

        label.text = multiplier > 1 ? $"SERİ {streak}  x{multiplier}" : $"SERİ {streak}";
        label.color = tierColors[Mathf.Clamp(multiplier - 1, 0, tierColors.Length - 1)];
        Restart(Pop(tierUp ? tierUpScale : popScale));
    }

    void HandleBreak(int lostStreak)
    {
        if (lostStreak < showFromStreak) return;
        Restart(Break());
    }

    void Restart(IEnumerator next)
    {
        if (routine != null) StopCoroutine(routine);
        root.SetActive(true);
        ((RectTransform)root.transform).anchoredPosition = basePos;
        routine = StartCoroutine(next);
    }

    IEnumerator Pop(float scale)
    {
        var t = root.transform;
        for (float time = 0f; time < popTime; time += Time.deltaTime)
        {
            t.localScale = Vector3.one * Mathf.Lerp(scale, 1f, time / popTime);
            yield return null;
        }
        t.localScale = Vector3.one;
        routine = null;
    }

    IEnumerator Break()
    {
        label.text = breakText;
        var rt = (RectTransform)root.transform;
        rt.localScale = Vector3.one;

        for (float time = 0f; time < breakShowTime; time += Time.deltaTime)
        {
            // Sarsıntı (ilk yarı) ve yanıp sönme
            float shake = time < breakShowTime * 0.5f ? shakePixels : 0f;
            rt.anchoredPosition = basePos + new Vector2(
                Mathf.Round(Random.Range(-shake, shake)),
                Mathf.Round(Random.Range(-shake, shake)));

            Color c = breakColor;
            if (Mathf.FloorToInt(time * 12f) % 2 == 0) c.a = 0.4f;
            label.color = c;
            yield return null;
        }

        rt.anchoredPosition = basePos;
        root.SetActive(false);
        routine = null;
    }
}
