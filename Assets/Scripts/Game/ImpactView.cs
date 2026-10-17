using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// "SAHA RAPORU": kararın insanlar üzerindeki etkisini büyük ve net bir şeritte gösterir.
// Örn. "Vesperia Hollow: 120 kişi temiz suya ulaştı."
// İsteğe bağlı olarak toplam sayaçları da günceller (Ulaşılan / Etkilenen).
public class ImpactView : MonoBehaviour, IPointerClickHandler
{
    [Header("Şerit")]
    public GameObject root;
    public TMP_Text messageText;
    [Tooltip("Şeridin başlığı (örn. 'SAHA RAPORU'). İsteğe bağlı.")]
    public TMP_Text headerText;
    [Tooltip("Rengi iyi / kötü duruma göre değişen arka plan. İsteğe bağlı.")]
    public Image background;
    [Tooltip("İyi / kötü simgesi. İsteğe bağlı.")]
    public Image icon;
    public Sprite goodIcon;
    public Sprite badIcon;

    [Header("Metinler")]
    public string goodHeader = "SAHA RAPORU";
    public string badHeader = "SAHA RAPORU";

    [Header("Renkler")]
    public Color goodTextColor = new Color(0.55f, 1f, 0.6f);
    public Color badTextColor = new Color(1f, 0.5f, 0.45f);
    public Color goodBackground = new Color(0.05f, 0.25f, 0.1f, 0.9f);
    public Color badBackground = new Color(0.3f, 0.05f, 0.05f, 0.9f);

    [Header("Zamanlama")]
    [Tooltip("Karardan sonra şeridin belirmesi için bekleme (önce damga görünsün)")]
    public float showDelay = 0.35f;
    public float minDuration = 3f;
    public float maxDuration = 6f;
    [Tooltip("Okuma hızı: saniyede kaç karakter")]
    public float readingCharsPerSecond = 22f;
    public float popTime = 0.15f;
    public float popStartScale = 0.8f;

    [Header("Toplam sayaçlar (isteğe bağlı)")]
    public TMP_Text helpedText;
    public TMP_Text harmedText;
    public string helpedFormat = "ULAŞILAN: {0}";
    public string harmedFormat = "ETKİLENEN: {0}";
    public float counterCountTime = 0.6f;
    [Tooltip("Sayaç artınca ne kadar büyüsün")]
    public float counterPunch = 1.3f;

    Coroutine showRoutine, helpedRoutine, harmedRoutine;
    int shownHelped, shownHarmed;

    public bool IsVisible => root && root.activeSelf;

    void Awake()
    {
        if (root) root.SetActive(false);
        if (messageText) messageText.raycastTarget = false;
        if (headerText) headerText.raycastTarget = false;
        if (helpedText) helpedText.raycastTarget = false;
        if (harmedText) harmedText.raycastTarget = false;
        SetCounter(helpedText, helpedFormat, 0);
        SetCounter(harmedText, harmedFormat, 0);
    }

    public void Show(ImpactResult result)
    {
        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowRoutine(result));
    }

    public void Hide()
    {
        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = null;
        if (root) root.SetActive(false);
    }

    // Toplamları günceller (GameManager çağırır)
    public void UpdateTotals(int helped, int harmed)
    {
        if (helped != shownHelped)
        {
            if (helpedRoutine != null) StopCoroutine(helpedRoutine);
            helpedRoutine = StartCoroutine(CountTo(helpedText, helpedFormat, shownHelped, helped));
            shownHelped = helped;
        }
        if (harmed != shownHarmed)
        {
            if (harmedRoutine != null) StopCoroutine(harmedRoutine);
            harmedRoutine = StartCoroutine(CountTo(harmedText, harmedFormat, shownHarmed, harmed));
            shownHarmed = harmed;
        }
    }

    IEnumerator ShowRoutine(ImpactResult r)
    {
        if (root) root.SetActive(false);
        yield return new WaitForSeconds(showDelay);

        if (messageText)
        {
            messageText.text = r.text;
            messageText.color = r.good ? goodTextColor : badTextColor;
        }
        if (headerText)
        {
            headerText.text = r.good ? goodHeader : badHeader;
            headerText.color = r.good ? goodTextColor : badTextColor;
        }
        if (background) background.color = r.good ? goodBackground : badBackground;
        if (icon)
        {
            var s = r.good ? goodIcon : badIcon;
            icon.enabled = s != null;
            if (s) icon.sprite = s;
        }

        root.SetActive(true);

        // Küçük bir zıplama
        var t = root.transform;
        for (float time = 0f; time < popTime; time += Time.deltaTime)
        {
            t.localScale = Vector3.one * Mathf.Lerp(popStartScale, 1f, time / popTime);
            yield return null;
        }
        t.localScale = Vector3.one;

        int length = string.IsNullOrEmpty(r.text) ? 0 : r.text.Length;
        float duration = Mathf.Clamp(length / Mathf.Max(1f, readingCharsPerSecond), minDuration, maxDuration);
        yield return new WaitForSeconds(duration);

        root.SetActive(false);
        showRoutine = null;
    }

    IEnumerator CountTo(TMP_Text text, string format, int from, int to)
    {
        if (!text) yield break;
        var t = text.transform;
        for (float time = 0f; time < counterCountTime; time += Time.deltaTime)
        {
            float p = time / counterCountTime;
            SetCounter(text, format, Mathf.RoundToInt(Mathf.Lerp(from, to, p)));
            t.localScale = Vector3.one * Mathf.Lerp(counterPunch, 1f, p);
            yield return null;
        }
        SetCounter(text, format, to);
        t.localScale = Vector3.one;
    }

    static void SetCounter(TMP_Text text, string format, int value)
    {
        if (text) text.text = string.Format(format, Impact.FormatNumber(value));
    }

    public void OnPointerClick(PointerEventData eventData) => Hide();
}
