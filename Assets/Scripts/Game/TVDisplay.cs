using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Televizyondaki haber metni ve kaynağı.
// Haber gelince önce kısa bir parazit (karışık karakterler + statik sesi), sonra daktilo efektiyle yazar.
// TV yazısına tıklayınca metin anında tamamlanır.
public class TVDisplay : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text headlineText;
    [Tooltip("Kaynak için ayrı bir yazı (isteğe bağlı). Boşsa kaynak, haberin üstünde küçük harflerle gösterilir.")]
    public TMP_Text sourceText;
    [Tooltip("Kaynağın önüne yazılan ifade")]
    public string sourcePrefix = "KAYNAK: ";

    [Tooltip("Saniyede yazılan harf sayısı")]
    public float charsPerSecond = 40f;
    [Tooltip("TV boşken gösterilen yazı")]
    public string idleText = "...";

    [Header("Kaynak renkleri")]
    public Color officialColor = new Color(0.45f, 0.85f, 0.55f);
    public Color mediaColor = new Color(0.8f, 0.8f, 0.8f);
    public Color unverifiedColor = new Color(1f, 0.65f, 0.25f);

    [Header("Parazit efekti")]
    public float glitchDuration = 0.35f;
    [Tooltip("Karakterlerin ne sıklıkla karıştırılacağı (saniye)")]
    public float glitchStep = 0.04f;
    public string glitchChars = "#@%&*/\\|<>?!01$=+";

    Coroutine typing;
    string fullHeadline;

    // Haber hâlâ yazılıyor mu (öğretici bunu bekler)
    public bool IsTyping => typing != null;
    string sourceLine;   // Rich text ile hazırlanmış kaynak satırı (haberle aynı yazıdaysa)

    public void Show(string headline) => Show(headline, null, NewsSourceType.Media);

    public void Show(string headline, string sourceLabel, NewsSourceType sourceType)
    {
        fullHeadline = headline;
        sourceLine = "";

        if (!string.IsNullOrEmpty(sourceLabel))
        {
            string hex = ColorUtility.ToHtmlStringRGB(ColorFor(sourceType));
            if (sourceText)
            {
                sourceText.text = sourcePrefix + sourceLabel;
                sourceText.color = ColorFor(sourceType);
            }
            else
            {
                sourceLine = $"<size=70%><color=#{hex}>{sourcePrefix}{sourceLabel}</color></size>\n";
            }
        }
        else if (sourceText) sourceText.text = "";

        if (typing != null) StopCoroutine(typing);
        AudioManager.Play(Sfx.NewsStatic);
        typing = StartCoroutine(Type());
    }

    public void Clear()
    {
        if (typing != null) StopCoroutine(typing);
        typing = null;
        headlineText.text = idleText;
        headlineText.maxVisibleCharacters = int.MaxValue;
        if (sourceText) sourceText.text = "";
    }

    Color ColorFor(NewsSourceType type)
    {
        switch (type)
        {
            case NewsSourceType.Official:   return officialColor;
            case NewsSourceType.Unverified: return unverifiedColor;
            default:                        return mediaColor;
        }
    }

    IEnumerator Type()
    {
        // 1) Parazit (kaynak satırı sabit kalır, sadece haber karışır)
        headlineText.maxVisibleCharacters = int.MaxValue;
        float g = 0f;
        while (g < glitchDuration)
        {
            // noparse: karışık karakterlerdeki < > işaretleri TMP etiketi sanılmasın
            headlineText.text = sourceLine + "<noparse>" + Scramble(fullHeadline) + "</noparse>";
            yield return new WaitForSeconds(glitchStep);
            g += glitchStep;
        }

        // 2) Daktilo (kaynak satırı hemen görünür, haber harf harf yazılır)
        headlineText.text = sourceLine + fullHeadline;
        headlineText.ForceMeshUpdate();
        int total = headlineText.textInfo.characterCount;
        int sourceChars = total - fullHeadline.Length;
        if (sourceChars < 0) sourceChars = 0;

        float shown = sourceChars;
        headlineText.maxVisibleCharacters = sourceChars;
        while (shown < total)
        {
            shown += charsPerSecond * Time.deltaTime;
            headlineText.maxVisibleCharacters = Mathf.Min((int)shown, total);
            yield return null;
        }
        typing = null;
    }

    string Scramble(string source)
    {
        var sb = new StringBuilder(source.Length);
        foreach (char c in source)
            sb.Append(char.IsWhiteSpace(c) ? c : glitchChars[Random.Range(0, glitchChars.Length)]);
        return sb.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (typing == null || string.IsNullOrEmpty(fullHeadline)) return;
        StopCoroutine(typing);
        typing = null;
        headlineText.text = sourceLine + fullHeadline;
        headlineText.maxVisibleCharacters = int.MaxValue;
    }
}
