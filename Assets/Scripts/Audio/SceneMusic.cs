using UnityEngine;

// Her sahneye bir tane koyun: sahne açılınca bu müziğe geçilir.
// Boş bırakırsanız o sahnede müzik susar.
public class SceneMusic : MonoBehaviour
{
    public AudioClip music;

    void Start()
    {
        AudioManager.PlayMusic(music);
    }
}
