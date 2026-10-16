using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Odaya rastgele aralıklarla biri gelir, soru sorar; Evet/Hayır cevabına göre bar değişir.
// Kişi gelince sprite'ı ve diyalog kutusu belirir, gidince ikisi de kaybolur.
public class VisitorManager : MonoBehaviour
{
    [Header("Kişi (odadaki sprite)")]
    [Tooltip("Odada belirecek kişi objesi (SpriteRenderer)")]
    public GameObject visitor;
    public float fadeTime = 0.25f;

    [Header("Diyalog (Canvas içinde)")]
    [Tooltip("Soru kutusu + Evet/Hayır butonlarının hepsini içeren obje")]
    public GameObject dialogRoot;
    public TMP_Text questionText;
    public Button yesButton;
    public Button noButton;
    [Tooltip("Kişinin sabrını gösteren çubuk (isteğe bağlı)")]
    public TimerBarView patienceBar;

    [Header("Zamanlama")]
    public float firstVisitDelay = 30f;
    public float minInterval = 25f;
    public float maxInterval = 45f;
    [Tooltip("Kişinin cevap beklediği süre")]
    public float patience = 12f;

    [Header("Sonuçlar")]
    public float correctReward = 12f;
    public float wrongPenalty = 15f;
    [Tooltip("Cevap verilmeden giderse")]
    public float ignorePenalty = 8f;
    [Tooltip("Doğru 'Evet'e rağmen aksilik çıkma ihtimali (sadece bazı sorularda)")]
    [Range(0f, 1f)] public float badLuckChance = 0.1f;
    public float badLuckPenalty = 8f;
    [Tooltip("Doğru cevabın 'Evet' olma ihtimali")]
    [Range(0f, 1f)] public float yesChance = 0.5f;

    GameManager game;
    VisitorRequest current;
    float nextVisitTime;
    float patienceLeft;
    bool running;
    SpriteRenderer[] renderers;
    Coroutine fadeRoutine;

    void Awake()
    {
        if (visitor)
        {
            renderers = visitor.GetComponentsInChildren<SpriteRenderer>(true);
            visitor.SetActive(false);
        }
        if (dialogRoot) dialogRoot.SetActive(false);

        yesButton.onClick.AddListener(() => Answer(true));
        noButton.onClick.AddListener(() => Answer(false));
    }

    public void Begin(GameManager gameManager)
    {
        game = gameManager;
        nextVisitTime = firstVisitDelay;
        running = true;
    }

    public void Stop()
    {
        running = false;
        current = null;
        if (dialogRoot) dialogRoot.SetActive(false);
        if (visitor) visitor.SetActive(false);
    }

    void Update()
    {
        if (!running || game == null || game.IsGameOver) return;

        if (current == null)
        {
            if (game.ElapsedTime >= nextVisitTime) Arrive();
            return;
        }

        patienceLeft -= Time.deltaTime;
        if (patienceBar) patienceBar.SetNormalized(patienceLeft / patience);
        if (patienceLeft <= 0f) Ignored();
    }

    void Arrive()
    {
        current = VisitorRequestGenerator.Create(game.World, Random.value < yesChance);
        if (current == null)
        {
            ScheduleNext();
            return;
        }

        questionText.text = current.question;
        patienceLeft = patience;
        if (patienceBar) patienceBar.SetNormalized(1f);

        if (visitor)
        {
            visitor.SetActive(true);
            FadeVisitor(0f, 1f, false);
        }
        dialogRoot.SetActive(true);
        AudioManager.Play(Sfx.VisitorEnter);
    }

    void Answer(bool yes)
    {
        if (current == null || game.IsGameOver) return;

        var req = current;
        bool correct = yes == req.yesIsCorrect();
        string explanation = req.explain(yes);   // Dünya değişmeden önce hesapla

        if (correct)
        {
            bool badLuck = yes && req.badLuckMessage != null && Random.value < badLuckChance;
            if (badLuck)
            {
                game.Bar.ApplyWrong(badLuckPenalty);
                game.Feedback.Show("ŞANSSIZLIK!", GameJuice.WarnColor, req.badLuckMessage);
                GameJuice.Wrong();
            }
            else
            {
                game.CountResult(true);
                game.Bar.ApplyCorrect(correctReward);
                game.Feedback.Show(true, explanation);
                GameJuice.Correct();
                if (yes) req.onYesCorrect?.Invoke();
            }
        }
        else
        {
            game.CountResult(false);
            game.Bar.ApplyWrong(wrongPenalty);
            game.Feedback.Show(false, explanation);
            GameJuice.Wrong();
        }

        Leave();
    }

    void Ignored()
    {
        game.Bar.ApplyWrong(ignorePenalty);
        game.Feedback.Show("CEVAPSIZ KALDI!", GameJuice.BadColor, "Gelen kişi cevap alamadan gitti.");
        GameJuice.Wrong();
        Leave();
    }

    void Leave()
    {
        current = null;
        dialogRoot.SetActive(false);
        AudioManager.Play(Sfx.VisitorLeave);
        if (visitor && visitor.activeSelf) FadeVisitor(1f, 0f, true);
        ScheduleNext();
    }

    void ScheduleNext()
    {
        nextVisitTime = game.ElapsedTime + Random.Range(minInterval, maxInterval);
    }

    // ---------- Belirme / kaybolma ----------

    void FadeVisitor(float from, float to, bool deactivateAtEnd)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        SetAlpha(from);
        fadeRoutine = StartCoroutine(Fade(from, to, deactivateAtEnd));
    }

    IEnumerator Fade(float from, float to, bool deactivateAtEnd)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / fadeTime));
            yield return null;
        }
        SetAlpha(to);
        if (deactivateAtEnd) visitor.SetActive(false);
        fadeRoutine = null;
    }

    void SetAlpha(float a)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
        {
            var c = r.color;
            c.a = a;
            r.color = c;
        }
    }
}
