using TMPro;
using UnityEngine;
using UnityEngine.UI;

// EndScene: sonuçları gösterir.
public class EndMenu : MonoBehaviour
{
    public TMP_Text timeText;
    public TMP_Text correctText;
    public TMP_Text wrongText;
    public TMP_Text scoreText;

    public Button retryButton;
    public Button menuButton;

    void Start()
    {
        int seconds = Mathf.FloorToInt(GameSession.SurvivedSeconds);
        timeText.text = $"DAYANILAN SÜRE: {seconds / 60:00}:{seconds % 60:00}";
        correctText.text = $"DOĞRU KARAR: {GameSession.CorrectCount}";
        wrongText.text = $"YANLIŞ KARAR: {GameSession.WrongCount}";
        scoreText.text = $"PUAN: {GameSession.Score}";

        retryButton.onClick.AddListener(GameSession.LoadGame);
        menuButton.onClick.AddListener(GameSession.LoadStart);
    }
}
