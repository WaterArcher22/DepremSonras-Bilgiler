using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// EndScene: sonuçları, rekoru ve unvanı gösterir.
// Yeni alanların hepsi isteğe bağlı; boş bırakılan yazı gösterilmez.
public class EndMenu : MonoBehaviour
{
    [Serializable]
    public class Rank
    {
        public int minScore;
        public string title;
        [TextArea] public string description;
    }

    [Header("Sonuçlar")]
    public TMP_Text timeText;
    public TMP_Text correctText;
    public TMP_Text wrongText;
    public TMP_Text scoreText;

    [Header("Rekor ve seri (isteğe bağlı)")]
    public TMP_Text bestScoreText;
    public TMP_Text maxStreakText;
    [Tooltip("Rekor kırılınca görünecek obje (örn. 'YENİ REKOR!' yazısı)")]
    public GameObject newRecordObject;

    [Header("İnsanlara etkisi (isteğe bağlı)")]
    public TMP_Text helpedText;
    public TMP_Text harmedText;
    [Tooltip("Yanlış kararların sonuçlarının listelendiği yazı")]
    public TMP_Text harmListText;
    [Tooltip("Listede en fazla kaç sonuç gösterilsin")]
    public int harmListMax = 3;
    public string harmListTitle = "YANLIŞ BİLGİNİN BEDELİ:";
    public string noHarmText = "Hiçbir yanlış bilgi senin masandan geçmedi.";

    [Header("Unvan (isteğe bağlı)")]
    public TMP_Text titleText;
    public TMP_Text titleDescriptionText;
    [Tooltip("Küçükten büyüğe sıralı olmalı")]
    public Rank[] ranks =
    {
        new Rank { minScore = 0,     title = "SÖYLENTİ YAYICI",  description = "Haberleri doğrulamadan paylaşmak paniği büyütür." },
        new Rank { minScore = 800,   title = "STAJYER",          description = "İlk adımları attın; kaynakları kontrol etmeyi unutma." },
        new Rank { minScore = 2000,  title = "MUHABİR",          description = "Doğru bilgiyi yanlıştan ayırmaya başladın." },
        new Rank { minScore = 4000,  title = "EDİTÖR",           description = "Halk senin onayladığın haberlere güveniyor." },
        new Rank { minScore = 7000,  title = "GÜVENİLİR KAYNAK", description = "Kriz anında insanların aradığı ses sensin." },
        new Rank { minScore = 11000, title = "DOĞRULUK USTASI",  description = "Hiçbir yalan haber senin masandan geçemez." },
    };

    [Header("Animasyon")]
    [Tooltip("Puanın sıfırdan sayılarak yükselme süresi")]
    public float scoreCountTime = 1.2f;

    [Header("Butonlar")]
    public Button retryButton;
    public Button menuButton;

    void Start()
    {
        int seconds = Mathf.FloorToInt(GameSession.SurvivedSeconds);
        SetText(timeText, $"DAYANILAN SÜRE: {seconds / 60:00}:{seconds % 60:00}");
        SetText(correctText, $"DOĞRU KARAR: {GameSession.CorrectCount}");
        SetText(wrongText, $"YANLIŞ KARAR: {GameSession.WrongCount}");
        SetText(maxStreakText, $"EN UZUN SERİ: {GameSession.MaxStreak}");

        int best = Mathf.Max(GameSession.BestScore, GameSession.LoadBestScore());
        SetText(bestScoreText, $"EN YÜKSEK: {best}");

        if (newRecordObject) newRecordObject.SetActive(false);

        SetText(helpedText, $"DOĞRU BİLGİYLE YARDIMA ULAŞAN: {Impact.FormatNumber(GameSession.PeopleHelped)} KİŞİ");
        SetText(harmedText, $"YANLIŞ BİLGİDEN ETKİLENEN: {Impact.FormatNumber(GameSession.PeopleHarmed)} KİŞİ");
        SetText(harmListText, BuildHarmList());

        var rank = GetRank(GameSession.Score);
        SetText(titleText, rank != null ? rank.title : "");
        SetText(titleDescriptionText, rank != null ? rank.description : "");

        StartCoroutine(CountScore());

        retryButton.onClick.AddListener(GameSession.LoadGame);
        menuButton.onClick.AddListener(GameSession.LoadStart);
    }

    IEnumerator CountScore()
    {
        int target = GameSession.Score;
        for (float t = 0f; t < scoreCountTime; t += Time.deltaTime)
        {
            // Sona doğru yavaşlayan sayma
            float p = 1f - Mathf.Pow(1f - t / scoreCountTime, 3f);
            SetText(scoreText, $"PUAN: {Mathf.RoundToInt(target * p)}");
            yield return null;
        }
        SetText(scoreText, $"PUAN: {target}");

        // Rekor yazısı sayma bitince belirsin
        if (GameSession.IsNewRecord && newRecordObject)
        {
            newRecordObject.SetActive(true);
            AudioManager.Play(Sfx.ComboUp);
        }
    }

    // Son yanlış kararların sonuçları (en yeniler)
    string BuildHarmList()
    {
        var log = GameSession.HarmLog;
        if (log.Count == 0) return noHarmText;

        var sb = new System.Text.StringBuilder(harmListTitle);
        int start = Mathf.Max(0, log.Count - harmListMax);
        for (int i = start; i < log.Count; i++)
            sb.Append("\n• ").Append(log[i]);
        return sb.ToString();
    }

    Rank GetRank(int score)
    {
        Rank result = null;
        foreach (var r in ranks)
            if (score >= r.minScore) result = r;
        return result;
    }

    static void SetText(TMP_Text t, string value)
    {
        if (t) t.text = value;
    }
}
