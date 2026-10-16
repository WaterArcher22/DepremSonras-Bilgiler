using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Televizyondaki haber metni.
// Haber gelince önce kısa bir parazit (karışık karakterler + statik sesi), sonra daktilo efektiyle yazar.
// TV yazısına tıklayınca metin anında tamamlanır.
public class TVDisplay : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text headlineText;
    [Tooltip("Saniyede yazılan harf sayısı")]
    public float charsPerSecond = 40f;
    [Tooltip("TV boşken gösterilen yazı")]
    public string idleText = "...";

    [Header("Parazit efekti")]
    public float glitchDuration = 0.35f;
    [Tooltip("Karakterlerin ne sıklıkla karıştırılacağı (saniye)")]
    public float glitchStep = 0.04f;
    public string glitchChars = "#@%&*/\\|<>?!01$=+";

    Coroutine typing;
    string fullText;

    public void Show(string text)
    {
        fullText = text;
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
    }

    IEnumerator Type()
    {
        // 1) Parazit
        headlineText.maxVisibleCharacters = int.MaxValue;
        float g = 0f;
        while (g < glitchDuration)
        {
            headlineText.text = Scramble(fullText);
            yield return new WaitForSeconds(glitchStep);
            g += glitchStep;
        }

        // 2) Daktilo
        headlineText.text = fullText;
        headlineText.maxVisibleCharacters = 0;
        headlineText.ForceMeshUpdate();
        int total = headlineText.textInfo.characterCount;

        float shown = 0f;
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
        if (typing == null || string.IsNullOrEmpty(fullText)) return;
        StopCoroutine(typing);
        typing = null;
        headlineText.text = fullText;
        headlineText.maxVisibleCharacters = int.MaxValue;
    }
}
