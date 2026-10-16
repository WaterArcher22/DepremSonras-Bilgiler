using UnityEngine;
using UnityEngine.UI;

// Herhangi bir butona ekleyin: tıklanınca "Click" sesi çalar.
[RequireComponent(typeof(Button))]
public class UIClickSound : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => AudioManager.Play(Sfx.Click));
    }
}
