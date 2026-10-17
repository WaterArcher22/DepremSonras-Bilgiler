using System;
using UnityEngine;

// Puan ve kombo (seri) sistemi.
// GameManager objesine ekleyin. Doğru kararlar seriyi artırır, seri yükseldikçe çarpan artar.
// Çarpan hem puanı hem de doğru kararda barın ne kadar dolacağını büyütür.
public class ScoreManager : MonoBehaviour
{
    [Header("Puanlar")]
    public int correctNewsPoints = 100;
    [Tooltip("Haberin süresinden kalan her saniye için bonus")]
    public int speedBonusPerSecond = 10;
    public int visitorPoints = 150;
    [Tooltip("Hayatta kalınan her saniye için puan")]
    public float survivalPointsPerSecond = 1f;

    [Header("Kombo")]
    [Tooltip("Seri bu değerlere ulaşınca çarpan bir artar (3 → x2, 6 → x3, 10 → x4)")]
    public int[] tierThresholds = { 3, 6, 10 };
    [Tooltip("Her çarpan seviyesinde, doğru kararda barın dolma miktarına eklenen oran (0.25 = %25)")]
    public float barBonusPerTier = 0.25f;

    public int Score { get; private set; }
    public int Streak { get; private set; }
    public int MaxStreak { get; private set; }
    public int Tier { get; private set; }               // 0 = x1
    public int Multiplier => Tier + 1;
    public float BarMultiplier => 1f + barBonusPerTier * Tier;
    public bool Running { get; set; }

    // (toplam puan, eklenen puan; eklenen 0 ise sadece sayaç güncellenir, yazı çıkmaz)
    public event Action<int, int> OnScoreAdded;
    // (seri, çarpan, çarpan bu adımda yükseldi mi)
    public event Action<int, int, bool> OnComboChanged;
    // (kaybedilen seri uzunluğu)
    public event Action<int> OnComboBroken;

    float survivalAccumulator;

    void Update()
    {
        if (!Running) return;

        survivalAccumulator += survivalPointsPerSecond * Time.deltaTime;
        if (survivalAccumulator >= 1f)
        {
            int add = Mathf.FloorToInt(survivalAccumulator);
            survivalAccumulator -= add;
            AddPoints(add, false);
        }
    }

    // ---------- Oyundan çağrılır ----------

    // Doğru haber kararı. timeLeft: haberin süresinden kalan saniye.
    public int NewsCorrect(float timeLeft)
    {
        IncreaseStreak();
        int points = (correctNewsPoints + Mathf.Max(0, Mathf.FloorToInt(timeLeft)) * speedBonusPerSecond) * Multiplier;
        AddPoints(points, true);
        return points;
    }

    // Doğru ziyaretçi cevabı
    public int VisitorCorrect()
    {
        IncreaseStreak();
        int points = visitorPoints * Multiplier;
        AddPoints(points, true);
        return points;
    }

    // Yanlış karar, yanlış haberin yayılması ya da cevapsız ziyaretçi
    public void Break()
    {
        if (Streak == 0) return;

        int lost = Streak;
        Streak = 0;
        Tier = 0;

        // Kısa seriler bozulunca ses çalıp oyuncuyu sıkmayalım
        if (tierThresholds.Length > 0 && lost >= tierThresholds[0])
            AudioManager.Play(Sfx.ComboBreak);

        OnComboBroken?.Invoke(lost);
    }

    // ---------- İç işleyiş ----------

    void IncreaseStreak()
    {
        Streak++;
        MaxStreak = Mathf.Max(MaxStreak, Streak);

        int newTier = 0;
        foreach (int threshold in tierThresholds)
            if (Streak >= threshold) newTier++;

        bool tierUp = newTier > Tier;
        Tier = newTier;
        if (tierUp) AudioManager.Play(Sfx.ComboUp);

        OnComboChanged?.Invoke(Streak, Multiplier, tierUp);
    }

    void AddPoints(int points, bool showPopup)
    {
        if (points <= 0) return;
        Score += points;
        OnScoreAdded?.Invoke(Score, showPopup ? points : 0);
    }
}
