using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Televizyondaki haber metni. Daktilo efektiyle yazar; TV'ye tıklayınca metin anında tamamlanır.
public class TVDisplay : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text headlineText;
    [Tooltip("Saniyede yazılan harf sayısı")]
    public float charsPerSecond = 40f;
    [Tooltip("TV boşken gösterilen yazı")]
    public string idleText = "...";

    Coroutine typing;
    string fullText;

    public void Show(string text)
    {
        fullText = text;
        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(Type());
    }

    public void Clear()
    {
        if (typing != null) StopCoroutine(typing);
        headlineText.text = idleText;
        headlineText.maxVisibleCharacters = int.MaxValue;
    }

    IEnumerator Type()
    {
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (typing == null) return;
        StopCoroutine(typing);
        typing = null;
        headlineText.maxVisibleCharacters = int.MaxValue;
    }
}
