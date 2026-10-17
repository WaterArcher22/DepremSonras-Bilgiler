using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// GameScene'in beyni: dünyayı kurar ve değiştirir, haberleri getirir, kararları değerlendirir, oyunu bitirir.
public class GameManager : MonoBehaviour
{
    [Header("Dünya ayarları (her run rastgele)")]
    public WorldState.Settings worldSettings = new WorldState.Settings();
    [Range(0f, 1f)] public float trueNewsChance = 0.5f;

    [Header("Haber türleri ve kaynaklar")]
    [Tooltip("Haberlerin ne kadarı masaya bakmadan, genel bilgiyle çözülen türden olsun (deprem tahmini, sahte bağış vb.)")]
    [Range(0f, 1f)] public float knowledgeNewsChance = 0.25f;
    [Tooltip("Masadan doğrulanan haberlerde kaynağın ne kadar ipucu verdiği (0.5 = hiç, 1 = her zaman)")]
    [Range(0.5f, 1f)] public float sourceReliability = 0.7f;

    [Header("Haber süresi")]
    [Tooltip("İlk haberin süresi (saniye)")]
    public float newsDurationStart = 12f;
    [Tooltip("Sürenin inebileceği en düşük değer")]
    public float newsDurationMin = 6f;
    [Tooltip("Her yeni haberde süreden düşülen miktar")]
    public float newsDurationDecrease = 0.25f;
    [Tooltip("Açık: süre dolan haber kendiliğinden yayılır (Paylaş sayılır). Kapalı: sadece ceza.")]
    public bool timeoutCountsAsShare = true;
    [Tooltip("timeoutCountsAsShare kapalıyken süre dolunca verilen ceza")]
    public float missPenalty = 10f;

    [Header("Dünya değişimleri")]
    public bool enableWorldChanges = true;
    [Tooltip("Açık: dünya değişince ekrandaki haberin doğru cevabı da değişir")]
    public bool liveTruth = true;
    [Tooltip("İlk değişikliğe kadar geçen süre")]
    public float firstChangeDelay = 20f;
    [Tooltip("Başlangıçta iki değişiklik arası süre")]
    public float changeIntervalStart = 30f;
    [Tooltip("Oyun ilerledikçe inilen en kısa aralık")]
    public float changeIntervalEnd = 12f;
    [Tooltip("Aralığın en kısaya inmesi için geçen süre (saniye)")]
    public float changeRampDuration = 180f;

    [Header("Bağlantılar")]
    public AccuracyBar accuracyBar;
    public TVDisplay tv;
    public Button shareButton;
    public Button deleteButton;
    public FeedbackView feedback;
    [Tooltip("Haberin altındaki süre çubuğu (isteğe bağlı)")]
    public TimerBarView newsTimer;
    [Tooltip("PAYLAŞILDI / SİLİNDİ damgası (isteğe bağlı)")]
    public StampView stamp;
    [Tooltip("Odaya gelen kişi (isteğe bağlı)")]
    public VisitorManager visitorManager;
    [Tooltip("Puan ve kombo (isteğe bağlı)")]
    public ScoreManager scoreManager;

    [Header("Masa pencereleri")]
    public MapWindow mapWindow;
    public ReportWindow reportWindow;
    public LaptopWindow laptopWindow;

    [Header("Akış")]
    [Tooltip("Karardan sonra yeni haber gelene kadar bekleme")]
    public float delayBetweenNews = 1.2f;
    [Tooltip("Bar bittikten sonra bitiş sahnesine geçmeden önce bekleme")]
    public float delayBeforeEnd = 1.5f;

    public WorldState World { get; private set; }
    public float ElapsedTime { get; private set; }
    public bool IsGameOver => gameOver;
    public AccuracyBar Bar => accuracyBar;
    public FeedbackView Feedback => feedback;
    public ScoreManager Score => scoreManager;

    NewsGenerator generator;
    NewsItem currentNews;
    float newsTimeLeft, currentNewsDuration;
    int newsCount;
    float nextChangeTime;
    DeskItem[] deskItems;
    int correct, wrong;
    bool gameOver;

    void Start()
    {
        World = WorldState.Generate(worldSettings);
        World.OnChanged += HandleWorldChanged;
        generator = new NewsGenerator(World, trueNewsChance, knowledgeNewsChance, sourceReliability);

        RefreshWindows();
        deskItems = FindObjectsByType<DeskItem>(FindObjectsSortMode.None);

        shareButton.onClick.AddListener(() => Decide(true));
        deleteButton.onClick.AddListener(() => Decide(false));
        SetButtons(false);

        accuracyBar.OnDepleted += EndGame;
        accuracyBar.ResetBar();
        accuracyBar.IsRunning = true;

        nextChangeTime = firstChangeDelay;
        if (visitorManager) visitorManager.Begin(this);
        if (scoreManager) scoreManager.Running = true;

        ShowNextNews();
    }

    void OnDestroy()
    {
        if (World != null) World.OnChanged -= HandleWorldChanged;
    }

    void Update()
    {
        if (gameOver) return;
        ElapsedTime += Time.deltaTime;
        UpdateNewsTimer();
        UpdateWorldChanges();
    }

    // ---------- Haberler ----------

    void ShowNextNews()
    {
        if (gameOver) return;
        currentNews = generator.Next();
        currentNewsDuration = Mathf.Max(newsDurationMin, newsDurationStart - newsDurationDecrease * newsCount);
        newsTimeLeft = currentNewsDuration;
        newsCount++;

        tv.Show(currentNews.headline, currentNews.sourceLabel, currentNews.sourceType);
        SetButtons(true);
    }

    void UpdateNewsTimer()
    {
        if (currentNews == null)
        {
            if (newsTimer) newsTimer.SetNormalized(0f);
            return;
        }

        newsTimeLeft -= Time.deltaTime;
        if (newsTimer) newsTimer.SetNormalized(newsTimeLeft / currentNewsDuration);
        if (newsTimeLeft <= 0f) NewsTimedOut();
    }

    // share = true -> PAYLAŞ, false -> SİL
    void Decide(bool share)
    {
        if (gameOver || currentNews == null) return;

        bool isTrue = currentNews.IsTrue(liveTruth);
        string explanation = currentNews.Explanation(liveTruth);
        bool isCorrect = share == isTrue;

        if (stamp) stamp.ShowDecision(share);

        if (isCorrect)
        {
            correct++;
            if (scoreManager) scoreManager.NewsCorrect(newsTimeLeft);
            // Kombo çarpanı barı da daha çok doldurur
            accuracyBar.ApplyCorrect(accuracyBar.correctReward * (scoreManager ? scoreManager.BarMultiplier : 1f));
            GameJuice.Correct();
        }
        else
        {
            wrong++;
            if (scoreManager) scoreManager.Break();
            accuracyBar.ApplyWrong();
            GameJuice.Wrong();
        }

        feedback.Show(isCorrect, explanation);
        FinishNews();
    }

    void NewsTimedOut()
    {
        bool isTrue = currentNews.IsTrue(liveTruth);
        string explanation = currentNews.Explanation(liveTruth);
        AudioManager.Play(Sfx.Timeout);

        if (timeoutCountsAsShare)
        {
            if (stamp) stamp.Show("YAYILDI!", GameJuice.WarnColor);

            if (isTrue)
            {
                // Doğru haber yayıldı: ödül de ceza da yok
                feedback.Show("ZAMAN DOLDU", GameJuice.WarnColor, "Haber kendiliğinden yayıldı, neyse ki doğruydu. " + explanation);
            }
            else
            {
                wrong++;
                if (scoreManager) scoreManager.Break();
                accuracyBar.ApplyWrong();
                GameJuice.Wrong();
                feedback.Show("YANLIŞ HABER YAYILDI!", GameJuice.BadColor, explanation);
            }
        }
        else
        {
            wrong++;
            if (scoreManager) scoreManager.Break();
            accuracyBar.ApplyWrong(missPenalty);
            GameJuice.Wrong();
            feedback.Show("ZAMAN DOLDU!", GameJuice.BadColor, "Haber hakkında karar verilmedi. " + explanation);
        }

        FinishNews();
    }

    void FinishNews()
    {
        SetButtons(false);
        currentNews = null;
        if (!gameOver) StartCoroutine(NextAfterDelay());
    }

    IEnumerator NextAfterDelay()
    {
        tv.Clear();
        yield return new WaitForSeconds(delayBetweenNews);
        ShowNextNews();
    }

    void SetButtons(bool on)
    {
        shareButton.interactable = on;
        deleteButton.interactable = on;
    }

    // ---------- Dünya değişimleri ----------

    void UpdateWorldChanges()
    {
        if (!enableWorldChanges || ElapsedTime < nextChangeTime) return;

        World.RandomChange();

        float t = Mathf.Clamp01(ElapsedTime / Mathf.Max(1f, changeRampDuration));
        float interval = Mathf.Lerp(changeIntervalStart, changeIntervalEnd, t) * Random.Range(0.75f, 1.25f);
        nextChangeTime = ElapsedTime + interval;
    }

    void HandleWorldChanged(EvidenceSource source)
    {
        RefreshWindows();
        AudioManager.Play(Sfx.DeviceUpdate);

        // Neyin değiştiği söylenmez; sadece ilgili cihaz yanıp söner
        int blinked = 0;
        foreach (var item in deskItems)
            if (item && item.source == source) { item.StartBlink(); blinked++; }

        if (blinked == 0)
            Debug.LogWarning($"Dünya değişti ({source}) ama Source'u '{source}' olan bir DeskItem bulunamadı. " +
                             "Masa eşyalarındaki DeskItem > Source alanlarını kontrol edin.");
        else
            Debug.Log($"Dünya değişti: {source}");
    }

    // Test için: Play modunda GameManager'daki script başlığına sağ tıklayıp çalıştırın
    [ContextMenu("TEST: Dünyayı şimdi değiştir")]
    void TestWorldChange()
    {
        if (World != null) World.RandomChange();
    }

    void RefreshWindows()
    {
        mapWindow.Setup(World);
        reportWindow.Setup(World);
        laptopWindow.Setup(World);
    }

    // ---------- Diğer sistemler için ----------

    // Ziyaretçi gibi diğer sistemlerin sonuçlarını sayar
    public void CountResult(bool wasCorrect)
    {
        if (wasCorrect) correct++;
        else wrong++;
    }

    // ---------- Oyun sonu ----------

    void EndGame()
    {
        if (gameOver) return;
        gameOver = true;
        currentNews = null;
        SetButtons(false);
        StopAllCoroutines();

        if (visitorManager) visitorManager.Stop();
        if (scoreManager) scoreManager.Running = false;
        DeskWindow.CloseAll();
        AudioManager.SetAlarm(false);

        GameSession.SurvivedSeconds = ElapsedTime;
        GameSession.CorrectCount = correct;
        GameSession.WrongCount = wrong;
        GameSession.Score = scoreManager
            ? scoreManager.Score
            : ScoreCalculator.Calculate(ElapsedTime, correct, wrong);
        GameSession.MaxStreak = scoreManager ? scoreManager.MaxStreak : 0;
        GameSession.SaveBestScore();

        StartCoroutine(GoToEnd());
    }

    IEnumerator GoToEnd()
    {
        yield return new WaitForSeconds(delayBeforeEnd);
        GameSession.LoadEnd();
    }
}
