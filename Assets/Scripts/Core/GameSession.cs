using UnityEngine;
using UnityEngine.SceneManagement;

// Sahneler arası taşınan sonuçlar ve sahne geçişleri.
public static class GameSession
{
    public const string StartScene = "StartScene";
    public const string IntroScene = "IntroScene";
    public const string GameScene = "GameScene";
    public const string EndScene = "EndScene";

    const string BestScoreKey = "BestScore";

    public static float SurvivedSeconds;
    public static int CorrectCount;
    public static int WrongCount;
    public static int Score;
    public static int MaxStreak;
    public static int BestScore;
    public static bool IsNewRecord;

    // Kararların insanlar üzerindeki etkisi (bkz. Impact)
    public static int PeopleHelped;
    public static int PeopleHarmed;
    public static System.Collections.Generic.List<string> HarmLog = new System.Collections.Generic.List<string>();

    public static void ResetImpact()
    {
        PeopleHelped = 0;
        PeopleHarmed = 0;
        HarmLog.Clear();
    }

    public static void LoadStart() => SceneManager.LoadScene(StartScene);
    public static void LoadIntro() => SceneManager.LoadScene(IntroScene);
    public static void LoadGame() => SceneManager.LoadScene(GameScene);
    public static void LoadEnd() => SceneManager.LoadScene(EndScene);

    // En yüksek skoru bilgisayara kaydeder (WebGL'de tarayıcıya)
    public static void SaveBestScore()
    {
        int previous = PlayerPrefs.GetInt(BestScoreKey, 0);
        IsNewRecord = Score > previous && Score > 0;

        if (IsNewRecord)
        {
            PlayerPrefs.SetInt(BestScoreKey, Score);
            PlayerPrefs.Save();
        }
        BestScore = Mathf.Max(previous, Score);
    }

    public static int LoadBestScore() => PlayerPrefs.GetInt(BestScoreKey, 0);
}

// ScoreManager yoksa kullanılan yedek formül
public static class ScoreCalculator
{
    public static int Calculate(float survivedSeconds, int correct, int wrong)
    {
        return Mathf.FloorToInt(survivedSeconds);
    }
}
