using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Oyunu başlatma fonksiyonu
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    // Oyundan çıkış fonksiyonu
    public void QuitGame()
    {
        Debug.Log("Oyundan Çıkıldı!");
        Application.Quit();
    }
}