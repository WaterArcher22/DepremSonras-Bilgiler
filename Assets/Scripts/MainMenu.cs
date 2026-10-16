using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // Sahneler arası kalıcı olması için Instance
    public static MainMenu Instance;

    [Header("Skor Sistemi")]
    public int score = 0;

    [Header("Paneller ve Ayarlar")]
    public GameObject mainButtonsPanel;
    public GameObject optionsPanel;
    public Slider volumeSlider;

    void Awake()
    {
        // Bu objenin sahneler arası yok olmamasını sağlar
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OpenOptions()
    {
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    // Puan ekleme fonksiyonu
    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("Güncel Puan: " + score);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan Çıkıldı!");
        Application.Quit();
    }
}