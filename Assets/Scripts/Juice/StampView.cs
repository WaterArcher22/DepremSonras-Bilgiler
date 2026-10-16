using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Karar verilince TV'nin üstüne basılan "PAYLAŞILDI / SİLİNDİ" damgası.
// Bu script her zaman açık duran bir objede; root ise damganın kendisi (TMP yazı + isteğe bağlı çerçeve).
public class StampView : MonoBehaviour
{
    public GameObject root;
    public TMP_Text label;
    [Tooltip("Damganın çerçevesi / arka planı (isteğe bağlı)")]
    public Image frame;

    public Color shareColor = new Color(0.3f, 0.85f, 0.4f);
    public Color deleteColor = new Color(0.95f, 0.3f, 0.3f);

    [Header("Animasyon")]
    public float startScale = 1.8f;
    public float punchTime = 0.1f;
    public float holdTime = 0.6f;
    public float fadeTime = 0.25f;
    [Tooltip("Damganın rastgele eğim açısı (0 = düz)")]
    public float maxRotation = 8f;

    CanvasGroup group;
    Coroutine routine;

    void Awake()
    {
        group = root.GetComponent<CanvasGroup>();
        if (!group) group = root.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        root.SetActive(false);
    }

    public void ShowDecision(bool share) =>
        Show(share ? "PAYLAŞILDI" : "SİLİNDİ", share ? shareColor : deleteColor);

    public void Show(string text, Color color)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Play(text, color));
    }

    IEnumerator Play(string text, Color color)
    {
        label.text = text;
        label.color = color;
        if (frame) frame.color = color;

        var rt = root.transform;
        rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-maxRotation, maxRotation));
        group.alpha = 1f;
        root.SetActive(true);
        AudioManager.Play(Sfx.Stamp);

        // Büyükten küçüğe "pat" diye basılma
        float t = 0f;
        while (t < punchTime)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, t / punchTime);
            yield return null;
        }
        rt.localScale = Vector3.one;

        yield return new WaitForSeconds(holdTime);

        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            group.alpha = 1f - t / fadeTime;
            yield return null;
        }

        root.SetActive(false);
        routine = null;
    }
}
