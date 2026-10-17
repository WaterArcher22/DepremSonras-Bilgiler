using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Oyunun ilk açılışında, gerçek masa üzerinde adım adım öğretici.
// Öğretici sırasında bar azalmaz, haber süresi işlemez, dünya değişmez, puan sayılmaz.
// Tamamlanınca ya da atlanınca bir daha gösterilmez (Tekrar Oyna'da çıkmaz).
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

    [Header("Ayarlar")]
    [Tooltip("Açık: öğretici sadece ilk oyunda gösterilir")]
    public bool onlyFirstTime = true;
    [Tooltip("Karardan sonra en az bu kadar beklenir; açıklama hâlâ açıksa kapanması da beklenir")]
    public float delayAfterDecision = 1f;

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
        "Bu haber bir YOLDAN bahsediyor.\nYolların durumunu yanıp sönen LAPTOPTAN kontrol et.";
    [TextArea(2, 4)] public string laptopDecideText =
        "YEŞİL = açık, KIRMIZI = kapalı.\nPencereyi kapat ve kararını ver: PAYLAŞ ya da SİL.";

    [TextArea(2, 4)] public string mapOpenText =
        "Su, yemek ve çadır dağıtımları HARİTADA görünür.\nYanıp sönen haritayı aç, haberdeki bölgeye bak.";
    [TextArea(2, 4)] public string mapDecideText =
        "Bölgenin üstünde ilgili işaret var mı?\nPencereyi kapat ve kararını ver.";

    [TextArea(2, 4)] public string reportOpenText =
        "Kurtarma ve itfaiye ekiplerinin nerede çalıştığı GÖREVLENDİRME RAPORUNDA yazar.\nYanıp sönen raporu aç.";
    [TextArea(2, 4)] public string reportDecideText =
        "Ekip bu bölgede çalışıyor mu?\nPencereyi kapat ve kararını ver.";

    [TextArea(2, 4)] public string knowledgeText =
        "Bazı haberler masaya bakmadan çözülür.\nHABERİN KAYNAĞINA ve içeriğine dikkat et: depremler önceden tahmin edilebilir mi?";

    [TextArea(3, 5)] public string finalText =
        "Bilgiler oyun boyunca DEĞİŞİR; değişen cihaz turuncu yanıp söner.\n" +
        "Her haberin bir süresi var. Bazen odaya biri gelip soru sorar; cevabı yine masada.\n" +
        "DOĞRULUK BARI biterse oyun biter. Hazır mısın?";

    const string DoneKey = "TutorialDone";

    GameManager game;
    bool continuePressed;
    bool decisionMade;
    DeskWindow awaitedWindow;
    bool windowOpened;
    bool finished;

    public bool ShouldRun() => !onlyFirstTime || PlayerPrefs.GetInt(DoneKey, 0) == 0;

    void Awake()
    {
        if (root) root.SetActive(false);
        continueButton.onClick.AddListener(() => continuePressed = true);
        if (skipButton) skipButton.onClick.AddListener(Finish);
    }

    void OnDestroy()
    {
        Unsubscribe();
    }

    public void Begin(GameManager gameManager)
    {
        game = gameManager;
        finished = false;
        root.SetActive(true);
        game.OnTutorialDecision += HandleDecision;
        DeskWindow.OnWindowOpened += HandleWindowOpened;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        if (introPages != null)
            foreach (var page in introPages)
                if (!string.IsNullOrWhiteSpace(page))
                    yield return Say(page, "DEVAM");

        yield return Say(welcomeText, "DEVAM");

        yield return DeskStep(EvidenceSource.Laptop, true,  laptopOpenText, laptopDecideText);
        yield return DeskStep(EvidenceSource.Map,    false, mapOpenText,    mapDecideText);
        yield return DeskStep(EvidenceSource.Report, true,  reportOpenText, reportDecideText);

        // Masaya bakmadan çözülen haber: deprem tahmini (her zaman yanlış)
        decisionMade = false;
        game.ShowTutorialNews(game.Generator.CreateKnowledgeNews(0));
        ShowMessage(knowledgeText, false);
        while (!decisionMade) yield return null;
        yield return AfterDecision();

        yield return Say(finalText, "BAŞLA");
        Finish();
    }

    // Bir cihazı açtırıp karar verdiren adım
    IEnumerator DeskStep(EvidenceSource source, bool makeTrue, string openText, string decideText)
    {
        decisionMade = false;
        windowOpened = false;
        awaitedWindow = game.WindowFor(source);

        game.ShowTutorialNews(game.Generator.CreateDeskNews(source, makeTrue));
        game.BlinkDevice(source);
        ShowMessage(openText, false);

        // Oyuncu cihazı açana (ya da açmadan karar verene) kadar bekle
        while (!windowOpened && !decisionMade) yield return null;

        if (!decisionMade)
        {
            ShowMessage(decideText, false);
            while (!decisionMade) yield return null;
        }

        awaitedWindow = null;
        yield return AfterDecision();
    }

    // Açıklama kutusu kapanana kadar (ya da oyuncu tıklayıp kapatana kadar) bekler
    IEnumerator AfterDecision()
    {
        ShowMessage("", false);
        yield return new WaitForSeconds(delayAfterDecision);
        while (game.Feedback && game.Feedback.IsVisible) yield return null;
        game.ClearNews();
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator Say(string text, string buttonText)
    {
        continuePressed = false;
        ShowMessage(text, true, buttonText);
        while (!continuePressed) yield return null;
    }

    void ShowMessage(string text, bool showContinue, string buttonText = "DEVAM")
    {
        messageText.text = text;
        messageText.gameObject.SetActive(!string.IsNullOrEmpty(text));
        continueButton.gameObject.SetActive(showContinue);
        if (continueLabel) continueLabel.text = buttonText;
    }

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
