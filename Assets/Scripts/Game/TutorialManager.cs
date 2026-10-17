using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Oyunun ilk açılışında, gerçek masa üzerinde adım adım öğretici.
// Öğretici sırasında bar azalmaz, haber süresi işlemez, dünya değişmez, puan sayılmaz.
// Tamamlanınca ya da atlanınca bir daha gösterilmez (Tekrar Oyna'da çıkmaz).
//
// Oyuncunun o an bakması gereken yer her zaman yanıp söner:
//   yeni haber -> TV, cihaz adımı -> cihaz, karar anı -> Paylaş / Sil, metin sayfası -> Devam.
// Karardan sonra açıklama okunurken öğretici kutusu gizlenir.
public class TutorialManager : MonoBehaviour
{
    [Header("Arayüz")]
    [Tooltip("Öğretici kutusunun tamamı (metin + butonlar)")]
    public GameObject root;
    public TMP_Text messageText;
    public Button continueButton;
    [Tooltip("Devam butonunun yazısı (isteğe bağlı)")]
    public TMP_Text continueLabel;
    public Button skipButton;
    [Tooltip("'ADIM 2/4' yazısı (isteğe bağlı)")]
    public TMP_Text stepText;

    [Header("Dikkat sırası")]
    [Tooltip("Haber yazıldıktan sonra, öğretici kutusu belirmeden önce haberin okunması için bekleme")]
    public float newsReadTime = 1.5f;
    [Tooltip("Öğretici kutusunun belirirken yaptığı zıplama")]
    public float popTime = 0.15f;
    [Range(0.5f, 1f)] public float popStartScale = 0.85f;

    [Header("Ayarlar")]
    [Tooltip("Açık: öğretici sadece ilk oyunda gösterilir")]
    public bool onlyFirstTime = true;
    [Tooltip("Karardan sonra en az bu kadar beklenir; açıklama hâlâ açıksa kapanması da beklenir")]
    public float delayAfterDecision = 1f;

    [Header("Bakılması gereken yerin yanıp sönmesi")]
    public Color pulseColor = new Color(1f, 0.85f, 0.3f);
    [Tooltip("Butonun en fazla ne kadar büyüyeceği (1 = büyümez)")]
    public float pulseScale = 1.1f;
    [Tooltip("Saniyedeki yanıp sönme sayısı")]
    public float pulseSpeed = 2f;
    [Tooltip("Pencerenin X butonu renk de değiştirsin mi? Kapalıyken sadece büyüyüp küçülür.")]
    public bool closeButtonColorPulse = false;

    [Header("Metinler")]
    [Tooltip("Öğreticiden önce, oyunun anlatmak istediğini söyleyen sayfalar. Boş bırakılırsa atlanır.")]
    [TextArea(3, 5)] public string[] introPages =
    {
        "Afetlerden sonra yalnızca binalar değil, bilgi de yıkılır.\n" +
        "Doğrulanmamış tek bir paylaşım; paniğe, yanlış yere giden yardıma ve kaybedilen zamana yol açabilir.",

        "Bu oyunda bir kriz iletişim merkezindesin.\n" +
        "Görevin, halka yalnızca DOĞRULANMIŞ bilgiyi ulaştırmak."
    };

    [TextArea(2, 4)] public string welcomeText =
        "TV'ye gelen her haberi masandaki bilgilerle doğrula:\ndoğruysa PAYLAŞ, yanlışsa SİL.";

    [TextArea(2, 4)] public string laptopOpenText =
        "TV'deki haber bir YOLDAN bahsediyor.\nYolların durumunu yanıp sönen LAPTOPTAN kontrol et.";
    [TextArea(2, 4)] public string laptopDecideText =
        "Ünlem = yol kapalı.\nPencereyi kapat ve kararını ver: PAYLAŞ ya da SİL.";

    [TextArea(2, 4)] public string mapOpenText =
        "TV'deki haber bir DAĞITIMDAN bahsediyor.\nSu, yemek ve çadır dağıtımları yanıp sönen HARİTADA görünür.";
    [TextArea(2, 4)] public string mapDecideText =
        "Haberdeki bölgenin üstünde ilgili işaret var mı?\nPencereyi kapat ve kararını ver.";

    [TextArea(2, 4)] public string reportOpenText =
        "TV'deki haber bir EKİPTEN bahsediyor.\nEkiplerin nerede çalıştığı yanıp sönen GÖREVLENDİRME RAPORUNDA yazar.";
    [TextArea(2, 4)] public string reportDecideText =
        "Ekip bu bölgede çalışıyor mu?\nPencereyi kapat ve kararını ver.";

    [Tooltip("Pencere kapatıldıktan sonra, karar beklenirken gösterilir")]
    [TextArea(2, 4)] public string chooseText =
        "Şimdi kararını ver:\nhaber doğruysa PAYLAŞ, yanlışsa SİL.";

    [TextArea(2, 4)] public string knowledgeText =
        "Bazı haberler masaya bakmadan çözülür.\nTV'deki haberin KAYNAĞINA ve içeriğine dikkat et: depremler önceden tahmin edilebilir mi?";

    [TextArea(3, 5)] public string finalText =
        "Bilgiler oyun boyunca DEĞİŞİR; değişen cihaz turuncu yanıp söner.\n" +
        "Her haberin bir süresi var. Bazen odaya biri gelip soru sorar; cevabı yine masada.\n" +
        "DOĞRULUK BARI biterse oyun biter. Hazır mısın?";

    const string DoneKey = "TutorialDone";
    const int TotalSteps = 4;

    // Yanıp sönen öğeler ve orijinal görünümleri
    class Highlight
    {
        public Graphic graphic;
        public Transform transform;
        public Vector3 scale;
        public Color color;
        public bool scalePulse;
        public bool colorPulse;
    }
    readonly List<Highlight> highlights = new List<Highlight>();

    GameManager game;
    bool continuePressed;
    bool decisionMade;
    DeskWindow awaitedWindow;
    bool windowOpened;
    bool finished;

    public bool ShouldRun() => !onlyFirstTime || PlayerPrefs.GetInt(DoneKey, 0) == 0;

    void Awake()
    {
        // Yazı, büyük olsa bile butonların tıklamasını yutmasın
        if (messageText) messageText.raycastTarget = false;
        if (stepText) stepText.raycastTarget = false;

        if (root) root.SetActive(false);
        continueButton.onClick.AddListener(() => continuePressed = true);
        if (skipButton) skipButton.onClick.AddListener(Finish);
    }

    void OnDestroy()
    {
        Unsubscribe();
        DeskItem.CanInteract = null;   // Sahne değişirse kilit kalmasın
    }

    public void Begin(GameManager gameManager)
    {
        game = gameManager;
        finished = false;
        LockDesk();
        game.OnTutorialDecision += HandleDecision;
        DeskWindow.OnWindowOpened += HandleWindowOpened;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        SetStep(0);

        if (introPages != null)
            foreach (var page in introPages)
                if (!string.IsNullOrWhiteSpace(page))
                    yield return Say(page, "DEVAM");

        yield return Say(welcomeText, "BAŞLAYALIM");

        SetStep(1);
        yield return DeskStep(EvidenceSource.Laptop, true,  laptopOpenText, laptopDecideText);
        SetStep(2);
        yield return DeskStep(EvidenceSource.Map,    false, mapOpenText,    mapDecideText);
        SetStep(3);
        yield return DeskStep(EvidenceSource.Report, true,  reportOpenText, reportDecideText);

        // Masaya bakmadan çözülen haber: deprem tahmini (her zaman yanlış)
        SetStep(4);
        decisionMade = false;
        game.ShowTutorialNews(game.Generator.CreateKnowledgeNews(0));
        yield return LetPlayerReadNews();
        ShowMessage(knowledgeText, false);
        game.SetDecisionButtons(true);
        HighlightDecisionButtons();
        while (!decisionMade) yield return null;
        ClearHighlights();
        yield return AfterDecision();

        SetStep(0);
        yield return Say(finalText, "BAŞLA");
        Finish();
    }

    // Bir cihazı açtırıp karar verdiren adım
    IEnumerator DeskStep(EvidenceSource source, bool makeTrue, string openText, string decideText)
    {
        decisionMade = false;
        windowOpened = false;
        awaitedWindow = game.WindowFor(source);

        // 1) Önce sadece TV: kutu gizli, oyuncu haberi okur
        game.ShowTutorialNews(game.Generator.CreateDeskNews(source, makeTrue));
        yield return LetPlayerReadNews();

        // 2) Sonra kutu belirir ve cihazı gösterir; sadece o cihaz tıklanabilir
        ShowMessage(openText, false);
        game.BlinkDevice(source);
        AllowOnlyDevice(source);

        // Oyuncu cihazı açana kadar bekle (Paylaş / Sil bu sırada kilitli)
        while (!windowOpened) yield return null;

        // 3) Pencere açıkken X yanıp söner; kapanınca Paylaş / Sil yanıp söner
        game.SetDecisionButtons(true);
        int phase = 0;   // 1 = pencere açık, 2 = pencere kapalı
        while (!decisionMade)
        {
            bool open = awaitedWindow && awaitedWindow.IsOpen;

            if (open && phase != 1)
            {
                phase = 1;
                ClearHighlights();
                ShowMessage(decideText, false);
                AddHighlight(awaitedWindow.closeButton, closeButtonColorPulse);
            }
            else if (!open && phase != 2)
            {
                phase = 2;
                ClearHighlights();
                ShowMessage(chooseText, false);
                HighlightDecisionButtons();
            }
            yield return null;
        }
        LockDesk();

        ClearHighlights();
        awaitedWindow = null;
        yield return AfterDecision();
    }

    // Kutu gizlenir; haber yazılıp okunana kadar beklenir (oyuncu bu sırada karar verirse beklemeyi keser)
    IEnumerator LetPlayerReadNews()
    {
        HidePanel();
        while (game.tv && game.tv.IsTyping && !decisionMade) yield return null;
        for (float t = 0f; t < newsReadTime && !decisionMade; t += Time.deltaTime) yield return null;
    }

    // Açıklama okunurken öğretici kutusu gizlenir; açıklama kapanınca devam edilir
    IEnumerator AfterDecision()
    {
        HidePanel();
        yield return new WaitForSeconds(delayAfterDecision);
        while (game.Feedback && game.Feedback.IsVisible) yield return null;
        game.ClearNews();
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator Say(string text, string buttonText)
    {
        continuePressed = false;
        ShowMessage(text, true, buttonText);
        AddHighlight(continueButton);
        while (!continuePressed) yield return null;
        ClearHighlights();
    }

    // ---------- Kutu ----------

    void ShowMessage(string text, bool showContinue, string buttonText = "DEVAM")
    {
        bool wasHidden = !root.activeSelf;
        root.SetActive(true);
        messageText.text = text;
        continueButton.gameObject.SetActive(showContinue);
        if (continueLabel) continueLabel.text = buttonText;

        // Kutu yeni beliriyorsa ya da mesaj değiştiyse küçük bir zıplamayla dikkat çeksin
        if (popRoutine != null) StopCoroutine(popRoutine);
        popRoutine = StartCoroutine(Pop(wasHidden ? popStartScale : Mathf.Lerp(popStartScale, 1f, 0.5f)));
    }

    Coroutine popRoutine;

    IEnumerator Pop(float fromScale)
    {
        var t = root.transform;
        for (float time = 0f; time < popTime; time += Time.unscaledDeltaTime)
        {
            t.localScale = Vector3.one * Mathf.Lerp(fromScale, 1f, time / popTime);
            yield return null;
        }
        t.localScale = Vector3.one;
        popRoutine = null;
    }

    void HidePanel()
    {
        if (popRoutine != null) StopCoroutine(popRoutine);
        popRoutine = null;
        root.transform.localScale = Vector3.one;
        root.SetActive(false);
    }

    void SetStep(int step)
    {
        if (!stepText) return;
        stepText.gameObject.SetActive(step > 0);
        stepText.text = $"ADIM {step}/{TotalSteps}";
    }

    // ---------- Yanıp sönme ----------

    void HighlightDecisionButtons()
    {
        AddHighlight(game.shareButton);
        AddHighlight(game.deleteButton);
    }

    void AddHighlight(Button button, bool colorPulse = true)
    {
        if (button) AddHighlight(button.targetGraphic, true, button.transform, colorPulse);
    }

    void AddHighlight(Graphic graphic, bool scalePulse, Transform scaleTarget = null, bool colorPulse = true)
    {
        if (!graphic || highlights.Exists(h => h.graphic == graphic)) return;
        var t = scaleTarget ? scaleTarget : graphic.transform;
        highlights.Add(new Highlight
        {
            graphic = graphic,
            transform = t,
            scale = t.localScale,
            color = graphic.color,
            scalePulse = scalePulse,
            colorPulse = colorPulse
        });
    }

    void ClearHighlights()
    {
        foreach (var h in highlights)
        {
            if (h.transform) h.transform.localScale = h.scale;
            if (h.graphic) h.graphic.color = h.color;
        }
        highlights.Clear();
    }

    void Update()
    {
        if (highlights.Count == 0) return;

        // 0 ile 1 arasında gidip gelen değer
        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

        foreach (var h in highlights)
        {
            if (h.scalePulse && h.transform) h.transform.localScale = h.scale * Mathf.Lerp(1f, pulseScale, t);
            if (h.graphic && h.colorPulse)
            {
                // Rengi değiştirirken orijinal saydamlığı koru (TV zemini yarı saydam olabilir)
                Color c = Color.Lerp(h.color, pulseColor, t);
                c.a = Mathf.Lerp(h.color.a, Mathf.Max(h.color.a, 0.6f), t);
                h.graphic.color = c;
            }
        }
    }

    // ---------- Masa kilitleri ----------

    // Hiçbir masa eşyası tıklanamaz
    void LockDesk() => DeskItem.CanInteract = _ => false;

    // Sadece istenen cihaz tıklanabilir
    void AllowOnlyDevice(EvidenceSource source) => DeskItem.CanInteract = item => item.source == source;

    // ---------- Olaylar ----------

    void HandleDecision(bool correct) => decisionMade = true;

    void HandleWindowOpened(DeskWindow window)
    {
        if (awaitedWindow && window == awaitedWindow) windowOpened = true;
    }

    void Finish()
    {
        if (finished) return;
        finished = true;

        StopAllCoroutines();
        Unsubscribe();
        ClearHighlights();
        DeskItem.CanInteract = null;   // Bütün kilitler kalkar

        PlayerPrefs.SetInt(DoneKey, 1);
        PlayerPrefs.Save();

        root.SetActive(false);
        DeskWindow.CloseAll();
        game.StartGameplay();
    }

    void Unsubscribe()
    {
        if (game) game.OnTutorialDecision -= HandleDecision;
        DeskWindow.OnWindowOpened -= HandleWindowOpened;
    }

    // Test için: Inspector'da script başlığına sağ tıklayıp çalıştırın
    [ContextMenu("Öğreticiyi sıfırla (tekrar göster)")]
    void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(DoneKey);
        PlayerPrefs.Save();
        Debug.Log("Öğretici sıfırlandı; bir sonraki oyunda tekrar gösterilecek.");
    }
}
