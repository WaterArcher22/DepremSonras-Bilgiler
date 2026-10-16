using UnityEngine;

// Doğru/yanlış anlarının ses + flaş + sarsıntı paketini tek yerden çağırmak için.
// Sahnede ScreenFlash veya CameraShake yoksa o kısım atlanır.
public static class GameJuice
{
    public static readonly Color GoodColor = new Color(0.3f, 0.85f, 0.4f);
    public static readonly Color BadColor  = new Color(0.95f, 0.3f, 0.3f);
    public static readonly Color WarnColor = new Color(1f, 0.65f, 0.15f);

    public static void Correct()
    {
        AudioManager.Play(Sfx.Correct);
        if (ScreenFlash.Instance) ScreenFlash.Instance.Flash(GoodColor);
    }

    public static void Wrong()
    {
        AudioManager.Play(Sfx.Wrong);
        if (ScreenFlash.Instance) ScreenFlash.Instance.Flash(BadColor);
        if (CameraShake.Instance) CameraShake.Instance.Shake();
    }
}
