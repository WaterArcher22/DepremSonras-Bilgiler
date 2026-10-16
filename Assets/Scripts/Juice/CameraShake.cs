using UnityEngine;

// Main Camera'ya ekleyin. Kamerayı tam piksel adımlarıyla sarsar (pixel art bulanıklaşmaz).
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Tooltip("Sprite'larınızın Pixels Per Unit değeri")]
    public int pixelsPerUnit = 16;
    public float defaultDuration = 0.2f;
    [Tooltip("Sarsıntının en fazla kaç piksel olacağı")]
    public int defaultStrength = 2;

    Vector3 basePosition;
    float timer;
    int strength;

    void Awake()
    {
        Instance = this;
        basePosition = transform.localPosition;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Shake() => Shake(defaultDuration, defaultStrength);

    public void Shake(float duration, int strengthInPixels)
    {
        timer = Mathf.Max(timer, duration);
        strength = Mathf.Max(1, strengthInPixels);
    }

    void LateUpdate()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            var offset = new Vector3(Random.Range(-strength, strength + 1),
                                     Random.Range(-strength, strength + 1), 0f) / pixelsPerUnit;
            transform.localPosition = basePosition + offset;
        }
        else
        {
            transform.localPosition = basePosition;
        }
    }
}
