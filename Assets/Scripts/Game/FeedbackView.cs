using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Karardan sonra "DOĞRU / YANLIŞ" ve nedenini gösterir.
// Ekranda kalma süresi açıklamanın uzunluğuna göre ayarlanır; kutuya tıklayınca hemen kapanır.
public class FeedbackView : MonoBehaviour, IPointerClickHandler
{
    public GameObject root;
    public TMP_Text titleText;
    public TMP_Text explanationText;

    [Header("Ekranda kalma süresi")]
    [Tooltip("Açıklama ne kadar kısa olursa olsun en az bu kadar kalır")]
    public float minDuration = 2.5f;
    [Tooltip("Açıklama ne kadar uzun olursa olsun en fazla bu kadar kalır")]
    public float maxDuration = 8f;
    [Tooltip("Okuma hızı: saniyede kaç karakter (düşük = daha uzun kalır)")]
    public float readingCharsPerSecond = 18f;
    [Tooltip("Kutuya tıklayınca kapansın mı?")]
    public bool clickToClose = true;

    [Header("Renkler")]
    public Color correctColor = new Color(0.3f, 0.85f, 0.4f);
    public Color wrongColor = new Color(0.95f, 0.3f, 0.3f);

    // Son açıklamanın ekranda kalacağı süre (GameManager yeni haberi buna göre geciktirir)
    public float LastDuration { get; private set; }
    public bool IsVisible => root && root.activeSelf;

    Coroutine hideRoutine;

    void Awake() => root.SetActive(false);

    public void Show(bool correct, string explanation) =>
        Show(correct ? "DOĞRU KARAR!" : "YANLIŞ KARAR!", correct ? correctColor : wrongColor, explanation);

    public void Show(string title, Color color, string explanation)
    {
        titleText.text = title;
        titleText.color = color;
        explanationText.text = explanation;
        root.SetActive(true);

        int length = string.IsNullOrEmpty(explanation) ? 0 : explanation.Length;
        LastDuration = Mathf.Clamp(minDuration + length / Mathf.Max(1f, readingCharsPerSecond), minDuration, maxDuration);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideLater(LastDuration));
    }

    public void Hide()
    {
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = null;
        root.SetActive(false);
    }

    IEnumerator HideLater(float duration)
    {
        yield return new WaitForSeconds(duration);
        root.SetActive(false);
        hideRoutine = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickToClose) Hide();
    }
}
