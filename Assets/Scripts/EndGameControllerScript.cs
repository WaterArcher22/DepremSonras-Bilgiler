using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        int finalScore = 0;

        if (MainMenu.Instance != null)
        {
            finalScore = MainMenu.Instance.score;
        }

        if (scoreText != null)
        {
            scoreText.text = "Toplam Puan: " + finalScore;
        }
    }

    // Butona basılınca Ana Menüye dönme fonksiyonu
    public void ReturnToMainMenu()
    {
        if (MainMenu.Instance != null)
        {
            MainMenu.Instance.score = 0; // Skoru sıfırla
        }
        SceneManager.LoadScene("StartScene");
    }
}