using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Bir masa penceresi açıldığında odayı karartan siyah, yarı saydam Image.
// - Canvas'ın İLK çocuğu olmalı (pencerelerin ve TV yazısının arkasında, odanın önünde).
// - Image rengi siyah, Alpha ~150 olsun; Raycast Target açık kalsın.
// - Karanlık alana tıklamak açık pencereyi kapatır.
[RequireComponent(typeof(CanvasGroup))]
public class DimOverlay : MonoBehaviour, IPointerClickHandler
{
    public float fadeTime = 0.15f;
    [Tooltip("Karanlık alana tıklayınca pencereler kapansın mı?")]
    public bool clickToClose = true;

    CanvasGroup group;
    Coroutine fade;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    void OnEnable()
    {
        DeskWindow.OnAnyOpenChanged += Apply;
        Apply(DeskWindow.AnyOpen);
    }

    void OnDisable()
    {
        DeskWindow.OnAnyOpenChanged -= Apply;
    }

    void Apply(bool anyOpen)
    {
        // Açıkken arkadaki masa eşyalarına tıklanmasını engeller
        group.blocksRaycasts = anyOpen;

        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeTo(anyOpen ? 1f : 0f));
    }

    IEnumerator FadeTo(float target)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        group.alpha = target;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickToClose) DeskWindow.CloseAll();
    }
}
