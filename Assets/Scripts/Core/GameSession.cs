using UnityEngine.SceneManagement;

// Sahneler arası taşınan sonuçlar ve sahne geçişleri.
public static class GameSession
{
    public const string StartScene = "StartScene";
    public const string GameScene = "GameScene";
    public const string EndScene = "EndScene";

    public static float SurvivedSeconds;
    public static int CorrectCount;
    public static int WrongCount;
    public static int Score;

    public static void LoadStart() => SceneManager.LoadScene(StartScene);
    public static void LoadGame() => SceneManager.LoadScene(GameScene);
    public static void LoadEnd() => SceneManager.LoadScene(EndScene);
}

// Puan formülü burada. Formülü verdiğinizde sadece bu fonksiyon değişecek.
public static class ScoreCalculator
{
    public static int Calculate(float survivedSeconds, int correct, int wrong)
    {
        return UnityEngine.Mathf.FloorToInt(survivedSeconds);
    }
}
