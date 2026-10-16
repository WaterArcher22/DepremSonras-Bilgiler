using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// GameScene'in beyni: dünyayı kurar, haberleri sırayla getirir, kararları değerlendirir, oyunu bitirir.
public class GameManager : MonoBehaviour
{
    [Header("Dünya ayarları (her run rastgele)")]
    public WorldState.Settings worldSettings = new WorldState.Settings();
    [Range(0f, 1f)] public float trueNewsChance = 0.5f;

    [Header("Bağlantılar")]
    public AccuracyBar accuracyBar;
    public TVDisplay tv;
    public Button shareButton;
    public Button deleteButton;
    public FeedbackView feedback;

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

    NewsGenerator generator;
    NewsItem currentNews;
    int correct, wrong;
    bool gameOver;

    void Start()
    {
        World = WorldState.Generate(worldSettings);
        generator = new NewsGenerator(World, trueNewsChance);

        mapWindow.Setup(World);
        reportWindow.Setup(World);
        laptopWindow.Setup(World);

        shareButton.onClick.AddListener(() => Decide(true));
        deleteButton.onClick.AddListener(() => Decide(false));
        SetButtons(false);

        accuracyBar.OnDepleted += EndGame;
        accuracyBar.ResetBar();
        accuracyBar.IsRunning = true;

        ShowNextNews();
    }

    void Update()
    {
        if (!gameOver) ElapsedTime += Time.deltaTime;
    }

    void ShowNextNews()
    {
        if (gameOver) return;
        currentNews = generator.Next();
        tv.Show(currentNews.headline);
        SetButtons(true);
    }

    // share = true -> PAYLAŞ, false -> SİL
    void Decide(bool share)
    {
        if (gameOver || currentNews == null) return;
        SetButtons(false);

        bool isCorrect = share == currentNews.isTrue;

        if (isCorrect) { correct++; accuracyBar.ApplyCorrect(); }
        else           { wrong++;   accuracyBar.ApplyWrong(); }

        feedback.Show(isCorrect, currentNews.explanation);
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

    void EndGame()
    {
        if (gameOver) return;
        gameOver = true;
        SetButtons(false);
        StopAllCoroutines();

        GameSession.SurvivedSeconds = ElapsedTime;
        GameSession.CorrectCount = correct;
        GameSession.WrongCount = wrong;
        GameSession.Score = ScoreCalculator.Calculate(ElapsedTime, correct, wrong);

        StartCoroutine(GoToEnd());
    }

    IEnumerator GoToEnd()
    {
        yield return new WaitForSeconds(delayBeforeEnd);
        GameSession.LoadEnd();
    }
}
