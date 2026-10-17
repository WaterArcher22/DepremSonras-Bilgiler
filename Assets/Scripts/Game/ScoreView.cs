using TMPro;
using UnityEngine;

// Ekrandaki canlı puan sayacı. Puan geldiğinde sayı hızla yükselerek yeni değere ulaşır.
public class ScoreView : MonoBehaviour
{
    public ScoreManager score;
    public TMP_Text label;
    public string prefix = "PUAN ";

    [Tooltip("Sayının yeni değere ne kadar hızlı ulaşacağı")]
    public float countSpeed = 10f;
    [Tooltip("Puan geldiğinde yazının zıplama büyüklüğü")]
    public float punchScale = 1.2f;
    public float punchTime = 0.15f;

    float shown;
    int target;
    float punchTimer;

    void OnEnable()
    {
        if (score) score.OnScoreAdded += HandleScore;
        Refresh();
    }

    void OnDisable()
    {
        if (score) score.OnScoreAdded -= HandleScore;
    }

    void HandleScore(int total, int added)
    {
        target = total;
        if (added > 0) punchTimer = punchTime;
    }

    void Update()
    {
        if (shown < target)
        {
            float step = Mathf.Max(1f, (target - shown) * countSpeed * Time.deltaTime);
            shown = Mathf.Min(target, shown + step);
            Refresh();
        }

        if (punchTimer > 0f)
        {
            punchTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(punchTimer / punchTime);
            label.transform.localScale = Vector3.one * Mathf.Lerp(1f, punchScale, t);
        }
        else
        {
            label.transform.localScale = Vector3.one;
        }
    }

    void Refresh()
    {
        if (label) label.text = prefix + Mathf.RoundToInt(shown);
    }
}
