using UnityEngine;
using UnityEngine.UI;

// StartScene: Başla ve Çıkış butonları.
public class StartMenu : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;
    [Tooltip("Açıksa Başla'dan sonra önce giriş (SON DAKİKA) sahnesi oynar")]
    public bool playIntro = true;

    void Start()
    {
        playButton.onClick.AddListener(() => { if (playIntro) GameSession.LoadIntro(); else GameSession.LoadGame(); });

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
