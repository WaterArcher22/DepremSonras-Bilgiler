using System.Collections;
using TMPro;
using UnityEngine;

// "BİLİYOR MUYDUN?" kutusu: gerçek hayatta işe yarayan afet hazırlığı ve bilgi doğrulama ipuçları.
// StartScene ve EndScene'e koyun. Her açılışta rastgele bir ipucu gösterir;
// Belli aralıklarla yenisine geçer ve hafifçe "nefes alır" (büyüyüp küçülür).
public class TipView : MonoBehaviour
{
    public TMP_Text tipText;
    [Tooltip("Başlık yazısı (isteğe bağlı), örn. 'BİLİYOR MUYDUN?'")]
    public TMP_Text headerText;
    public string header = "BİLİYOR MUYDUN?";

    [Tooltip("0 = sadece bir ipucu göster. Değer girilirse o kadar saniyede bir yenisine geçer.")]
    public float changeInterval = 6f;
    public float fadeTime = 0.3f;

    [Header("Nefes alma efekti")]
    [Tooltip("Büyüyüp küçülecek obje. Boşsa ipucu yazısı kullanılır. Başlıkla birlikte büyümesi için ikisinin ortak ebeveynini verin.")]
    public Transform breatheTarget;
    [Tooltip("En fazla ne kadar büyüsün (1 = efekt yok)")]
    public float breatheScale = 1.06f;
    [Tooltip("Bir nefesin süresi (saniye)")]
    public float breathePeriod = 2.5f;
    [Tooltip("Yeni ipucu gelince küçük bir zıplama")]
    public float changePop = 1.12f;
    public float changePopTime = 0.25f;

    [TextArea(2, 4)]
    public string[] tips =
    {
        "Depremler önceden tahmin edilemez. Saat ve tarih veren 'deprem tahmini' paylaşımlarına inanma.",
        "Bölgendeki acil toplanma alanını e-Devlet üzerinden öğrenebilirsin.",
        "Afet çantanda su, bozulmayan yiyecek, el feneri, yedek pil, düdük, ilk yardım malzemesi ve önemli belgelerinin kopyaları bulunsun.",
        "Acil durumlarda 112'yi ara.",
        "Afet sonrası telefon hatlarını meşgul etme; mümkünse arama yerine kısa mesaj kullan.",
        "Deprem anında Çök - Kapan - Tutun: yere çök, başını ve enseni koru, sağlam bir yere tutun.",
        "Doğrulanmamış bir haberi paylaşmak, iyi niyetli olsa bile paniği büyütür. Önce kaynağını kontrol et.",
        "Bağışlarını yalnızca resmi kurumlar ve tanınmış yardım kuruluşları aracılığıyla yap.",
        "Ailenle bir afet planı yapın: bir buluşma noktası ve şehir dışından bir iletişim kişisi belirleyin.",
        "Dolap ve raf gibi ağır eşyaları duvara sabitle; deprem anında devrilen eşyalar ciddi yaralanmalara yol açar.",
        "Enkaz altında kalırsan, sesini korumak için bağırmak yerine düdük çal ya da borulara vur.",
        "Hasar görmüş binalara, yetkililer güvenli olduğunu açıklamadan girme.",
        "'Bir arkadaşım söyledi' ile başlayan, kaynağı belli olmayan mesajlar çoğunlukla yanlış bilgidir.",
        "Zincir mesajları iletme. Resmi duyurular 'herkese iletin' diye bitmez.",
    };

    int lastIndex = -1;
    CanvasGroup group;
    Vector3 baseScale;
    float pop;   // 0..1, yeni ipucu gelince 1 olur ve söner

    void Start()
    {
        if (headerText) headerText.text = header;

        group = tipText.GetComponent<CanvasGroup>();
        if (!group) group = tipText.gameObject.AddComponent<CanvasGroup>();

        if (!breatheTarget) breatheTarget = tipText.transform;
        baseScale = breatheTarget.localScale;

        // Yazılar tıklamayı engellemesin
        tipText.raycastTarget = false;
        if (headerText) headerText.raycastTarget = false;

        ShowRandom();
        if (changeInterval > 0f) StartCoroutine(Cycle());
    }

    void Update()
    {
        if (!breatheTarget) return;

        // Yavaş nefes: 0..1 arasında yumuşakça gidip gelir
        float b = (1f - Mathf.Cos(Time.unscaledTime / Mathf.Max(0.1f, breathePeriod) * Mathf.PI * 2f)) * 0.5f;
        float scale = Mathf.Lerp(1f, breatheScale, b);

        // Yeni ipucu zıplaması
        if (pop > 0f)
        {
            scale *= Mathf.Lerp(1f, changePop, pop);
            pop = Mathf.Max(0f, pop - Time.unscaledDeltaTime / Mathf.Max(0.01f, changePopTime));
        }

        breatheTarget.localScale = baseScale * scale;
    }

    void ShowRandom()
    {
        if (tips == null || tips.Length == 0) return;

        int index;
        do index = Random.Range(0, tips.Length);
        while (tips.Length > 1 && index == lastIndex);
        lastIndex = index;

        tipText.text = tips[index];
    }

    IEnumerator Cycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(changeInterval);
            yield return Fade(1f, 0f);
            ShowRandom();
            pop = 1f;
            yield return Fade(0f, 1f);
        }
    }

    IEnumerator Fade(float from, float to)
    {
        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        group.alpha = to;
    }
}
