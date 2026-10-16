using UnityEngine;
using UnityEngine.UI;

// StartScene: Başla ve Çıkış butonları.
public class StartMenu : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;

    void Start()
    {
        playButton.onClick.AddListener(GameSession.LoadGame);

        if (quitButton)
        {
#if UNITY_WEBGL
            quitButton.gameObject.SetActive(false); // Tarayıcıda çıkış butonu işe yaramaz
#else
            quitButton.onClick.AddListener(Application.Quit);
#endif
        }
    }
}
