using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// IntroScene: Başla'ya basınca oynayan kısa giriş.
// 1) Sakin ekran (karanlıktan açılır)
// 2) Sakin ekran giderek artan şiddetle titrer (deprem uğultusu)
// 3) Beyaz flaş → SON DAKİKA ekranı
// 4) Kararır → GameScene
// Oyuncu ekrana tıklayarak geçebilir.
// Bu script Canvas objesine eklenir: ekran resimlerine yapılan tıklamalar Canvas'a ulaşır
// (sahnede EventSystem olmalı).
public class IntroSequence : MonoBehaviour, IPointerClickHandler
{
    [Header("Ekranlar (Canvas içindeki tam ekran Image'lar)")]
    public RectTransform calmScreen;
    public RectTransform breakingScreen;
    [Tooltip("Tam ekran siyah Image (en üstte). Kararma / açılma için.")]
    public Image fadeImage;
    [Tooltip("Tam ekran beyaz Image (fadeImage'ın altında). Flaş için. İsteğe bağlı.")]
    public Image flashImage;
    [Tooltip("'Geçmek için tıkla' yazısı. İsteğe bağlı.")]
    public TMP_Text skipHint;

    [Header("Süreler (saniye)")]
    public float fadeInTime = 0.8f;
    public float calmTime = 2f;
    public float shakeTime = 2.5f;
    public float flashTime = 0.4f;
    public float breakingTime = 3.5f;
    public float fadeOutTime = 0.6f;

    [Header("Titreme")]
    [Tooltip("Titremenin başlangıçtaki ve en sondaki gücü (UI pikseli)")]
    public float shakeStartStrength = 1f;
    public float shakeEndStrength = 8f;
    [Tooltip("Saniyede kaç kez konum değişsin (pikselli hissiyat için düşük tutun)")]
    public float shakeRate = 30f;
    [Tooltip("SON DAKİKA ekranı gelince de kısa bir sarsıntı")]
    public float breakingShakeTime = 0.4f;
    public float breakingShakeStrength = 4f;

    [Header("Sesler (isteğe bağlı)")]
    public AudioSource audioSource;
    [Tooltip("Titreme başlarken (deprem uğultusu)")]
    public AudioClip rumbleSound;
    [Tooltip("SON DAKİKA ekranı gelince (haber jeneriği / alarm)")]
    public AudioClip breakingSound;
    [Range(0f, 1f)] public float rumbleVolume = 1f;
    [Range(0f, 1f)] public float breakingVolume = 1f;

    [Header("Geçiş")]
    [Tooltip("Bu kadar saniye geçmeden tıklamayla atlanamaz (yanlışlıkla çift tıklamayı önler)")]
    public float skipLockTime = 0.5f;

    float startTime;
    bool leaving;
    Vector2 calmPos, breakingPos;

    void Start()
    {
        startTime = Time.unscaledTime;
        calmPos = calmScreen.anchoredPosition;
        breakingPos = breakingScreen.anchoredPosition;

        calmScreen.gameObject.SetActive(true);
        breakingScreen.gameObject.SetActive(false);
        SetAlpha(fadeImage, 1f);
        SetAlpha(flashImage, 0f);
        if (skipHint) skipHint.gameObject.SetActive(false);

        StartCoroutine(Run());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (leaving || Time.unscaledTime - startTime < skipLockTime) return;
        Leave();
    }

    IEnumerator Run()
    {
        // 1) Sakin ekran karanlıktan açılır
        yield return Fade(fadeImage, 1f, 0f, fadeInTime);
        if (skipHint) skipHint.gameObject.SetActive(true);
        yield return new WaitForSeconds(calmTime);

        // 2) Titreme giderek artar
        PlaySound(rumbleSound, rumbleVolume);
        yield return Shake(calmScreen, calmPos, shakeTime, shakeStartStrength, shakeEndStrength);

        // 3) Flaş ve SON DAKİKA
        calmScreen.gameObject.SetActive(false);
        breakingScreen.gameObject.SetActive(true);
        PlaySound(breakingSound, breakingVolume);
        if (flashImage) StartCoroutine(Fade(flashImage, 1f, 0f, flashTime));
        yield return Shake(breakingScreen, breakingPos, breakingShakeTime, breakingShakeStrength, 0f);
        yield return new WaitForSeconds(breakingTime);

        Leave();
    }

    void Leave()
    {
        if (leaving) return;
        leaving = true;
        StopAllCoroutines();
        StartCoroutine(LeaveRoutine());
    }

    IEnumerator LeaveRoutine()
    {
        calmScreen.anchoredPosition = calmPos;
        breakingScreen.anchoredPosition = breakingPos;
        if (skipHint) skipHint.gameObject.SetActive(false);

        float from = fadeImage ? fadeImage.color.a : 0f;
        float startVolume = audioSource ? audioSource.volume : 0f;
        for (float t = 0f; t < fadeOutTime; t += Time.unscaledDeltaTime)
        {
            float k = t / fadeOutTime;
            SetAlpha(fadeImage, Mathf.Lerp(from, 1f, k));
            if (audioSource) audioSource.volume = Mathf.Lerp(startVolume, 0f, k);
            yield return null;
        }
        SetAlpha(fadeImage, 1f);
        GameSession.LoadGame();
    }

    // ---------- Yardımcılar ----------

    IEnumerator Shake(RectTransform target, Vector2 basePos, float duration, float fromStrength, float toStrength)
    {
        float step = 1f / Mathf.Max(1f, shakeRate);
        float nextMove = 0f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            if (t >= nextMove)
            {
                nextMove = t + step;
                float s = Mathf.Lerp(fromStrength, toStrength, t / duration);
                // Tam sayı adımlarla kayar: pikseller bulanıklaşmaz
                var offset = new Vector2(Mathf.Round(Random.Range(-s, s)), Mathf.Round(Random.Range(-s, s)));
                target.anchoredPosition = basePos + offset;
            }
            yield return null;
        }
        target.anchoredPosition = basePos;
    }

    IEnumerator Fade(Image img, float from, float to, float duration)
    {
        if (!img) yield break;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            SetAlpha(img, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(img, to);
    }

    static void SetAlpha(Image img, float a)
    {
        if (!img) return;
        var c = img.color;
        c.a = a;
        img.color = c;
        img.raycastTarget = false;
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip, volume);
    }
}
