using System.Collections;
using UnityEngine;

// Herhangi bir masa penceresi açıkken bu objeyi gizler (örn. Paylaş/Sil butonları, ziyaretçi butonları).
// Obje kapanmaz, sadece görünmez ve tıklanmaz olur; bu yüzden direkt butonun üstüne ekleyebilirsiniz.
[RequireComponent(typeof(CanvasGroup))]
public class HideWhenWindowOpen : MonoBehaviour
{
    public float fadeTime = 0.1f;

    CanvasGroup group;
    Coroutine fade;

    void Awake() => group = GetComponent<CanvasGroup>();

    void OnEnable()
    {
        DeskWindow.OnAnyOpenChanged += Apply;
        Apply(DeskWindow.AnyOpen, true);
    }

    void OnDisable()
    {
        DeskWindow.OnAnyOpenChanged -= Apply;
    }

    void Apply(bool anyOpen) => Apply(anyOpen, false);

    void Apply(bool anyOpen, bool instant)
    {
        group.interactable = !anyOpen;
        group.blocksRaycasts = !anyOpen;

        float target = anyOpen ? 0f : 1f;
        if (fade != null) StopCoroutine(fade);

        if (instant || !isActiveAndEnabled) group.alpha = target;
        else fade = StartCoroutine(FadeTo(target));
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
}
