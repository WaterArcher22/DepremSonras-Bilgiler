using System.Collections;
using TMPro;
using UnityEngine;

// Puan kazanılınca yukarı süzülüp kaybolan "+300" yazısı.
// Bir TMP yazıyı şablon olarak verin; script her puan için onun bir kopyasını oluşturur.
public class PointsPopup : MonoBehaviour
{
    public ScoreManager score;
    [Tooltip("Şablon yazı (oyun başında gizlenir). Yazının çıkacağı yere koyun.")]
    public TMP_Text template;

    public float riseDistance = 24f;
    public float lifetime = 0.9f;
    [Tooltip("Üst üste gelen yazılar çakışmasın diye yatayda rastgele kayma (piksel)")]
    public float horizontalJitter = 12f;

    [Tooltip("Çarpana göre renk: x1, x2, x3, x4")]
    public Color[] multiplierColors =
    {
        new Color(0.9f, 0.9f, 0.9f),
        new Color(1f, 0.9f, 0.3f),
        new Color(1f, 0.6f, 0.15f),
        new Color(1f, 0.3f, 0.3f)
    };

    void Awake()
    {
        if (template) template.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (score) score.OnScoreAdded += HandleScore;
    }

    void OnDisable()
    {
        if (score) score.OnScoreAdded -= HandleScore;
    }

    void HandleScore(int total, int added)
    {
        if (added <= 0 || !template) return;
        StartCoroutine(Spawn(added));
    }

    IEnumerator Spawn(int added)
    {
        var popup = Instantiate(template, template.transform.parent);
        popup.gameObject.SetActive(true);

        int mult = score ? score.Multiplier : 1;
        popup.text = mult > 1 ? $"+{added}  x{mult}" : $"+{added}";
        Color c = multiplierColors.Length > 0
            ? multiplierColors[Mathf.Clamp(mult - 1, 0, multiplierColors.Length - 1)]
            : Color.white;

        var rt = popup.rectTransform;
        Vector2 start = template.rectTransform.anchoredPosition +
                        new Vector2(Mathf.Round(Random.Range(-horizontalJitter, horizontalJitter)), 0f);

        float t = 0f;
        while (t < lifetime)
        {
            t += Time.deltaTime;
            float p = t / lifetime;
            // Önce hızlı yüksel, sonra yavaşla; son yarıda sön
            float rise = 1f - (1f - p) * (1f - p);
            rt.anchoredPosition = start + new Vector2(0f, Mathf.Round(rise * riseDistance));
            c.a = p < 0.5f ? 1f : 1f - (p - 0.5f) * 2f;
            popup.color = c;
            yield return null;
        }

        Destroy(popup.gameObject);
    }
}
