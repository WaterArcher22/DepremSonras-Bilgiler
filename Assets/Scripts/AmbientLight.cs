using UnityEngine;
using UnityEngine.UI;

// Ortam ışığı: cihaz LED'leri, TV parıltısı, titreyen tavan lambası gibi küçük canlılık efektleri.
// SpriteRenderer'lı bir objeye (sahnede) ya da Image'lı bir objeye (Canvas'ta) ekleyin.
// Sprite atanmamışsa şekli kendisi üretir: küçük kare LED ya da pixel art tarzı yumuşak parıltı.
public class AmbientLight : MonoBehaviour
{
    public enum Mode
    {
        Blink,   // Açık / kapalı yanıp söner (LED)
        Pulse,   // Yavaşça parlayıp söner (bekleme ışığı, TV parıltısı)
        Flicker  // Çoğunlukla sabit, arada bir titrer (bozuk floresan, elektrik dalgalanması)
    }

    public enum Shape { None, Square, Glow, RectGlow }

    [Header("Davranış")]
    public Mode mode = Mode.Blink;
    public Color color = new Color(1f, 0.25f, 0.2f);
    [Tooltip("Birden fazla ışık aynı anda yanıp sönmesin diye başlangıcı rastgele kaydır")]
    public bool randomizeStart = true;

    [Header("Blink")]
    public float onDuration = 0.15f;
    public float offDuration = 1.2f;
    [Tooltip("Sürelere eklenen rastgelelik (0 = tam düzenli)")]
    [Range(0f, 1f)] public float blinkRandomness = 0.3f;

    [Header("Pulse")]
    [Tooltip("Bir nefes süresi (saniye)")]
    public float pulsePeriod = 2.5f;
    [Range(0f, 1f)] public float pulseMin = 0.2f;
    [Range(0f, 1f)] public float pulseMax = 1f;
    [Tooltip("Parlaklığı kaç kademeye bölelim (pixel art hissi için 4-6 iyi, 0 = yumuşak)")]
    public int pulseSteps = 5;

    [Header("Flicker")]
    [Tooltip("Normal haldeki görünürlük")]
    [Range(0f, 1f)] public float restAlpha = 1f;
    [Tooltip("Titreme anında inip çıktığı görünürlük")]
    [Range(0f, 1f)] public float flickerAlpha = 0.3f;
    [Tooltip("Saniyede titreme başlama ihtimali")]
    [Range(0f, 1f)] public float flickerChancePerSecond = 0.15f;
    public float flickerDuration = 0.35f;

    [Header("Şekil (sprite atanmamışsa)")]
    public Shape generateShape = Shape.Square;
    [Tooltip("Square: kenar uzunluğu, Glow: yarıçap (piksel)")]
    public int sizeInPixels = 1;
    [Tooltip("Glow'un kaç halkadan oluşacağı")]
    public int glowBands = 4;
    public int pixelsPerUnit = 16;

    [Header("RectGlow (monitör, TV ekranı gibi dikdörtgenler)")]
    [Tooltip("Parlayan dikdörtgenin genişliği (piksel). Monitör ekranının genişliği kadar verin.")]
    public int rectWidth = 40;
    [Tooltip("Parlayan dikdörtgenin yüksekliği (piksel)")]
    public int rectHeight = 24;
    [Tooltip("Işığın kenarlardan dışarı ne kadar taşacağı (piksel)")]
    public int rectSpread = 8;
    [Tooltip("Dikdörtgenin içinin parlaklığı (0 = sadece kenarlardan taşan ışık)")]
    [Range(0f, 1f)] public float rectInnerAlpha = 0.35f;

    SpriteRenderer spriteRenderer;
    Image image;
    float timer;
    bool isOn = true;
    float flickerTimer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();

        bool hasSprite = (spriteRenderer && spriteRenderer.sprite) || (image && image.sprite);
        if (!hasSprite && generateShape != Shape.None)
        {
            Sprite sprite;
            switch (generateShape)
            {
                case Shape.Glow:     sprite = MakeGlow(sizeInPixels, glowBands); break;
                case Shape.RectGlow: sprite = MakeRectGlow(rectWidth, rectHeight, rectSpread, glowBands, rectInnerAlpha); break;
                default:             sprite = MakeSquare(sizeInPixels); break;
            }
            if (spriteRenderer) spriteRenderer.sprite = sprite;
            else if (image) { image.sprite = sprite; image.SetNativeSize(); }
        }

        if (image) image.raycastTarget = false;

        if (randomizeStart)
        {
            timer = Random.Range(0f, offDuration);
            isOn = false;
        }
    }

    void Update()
    {
        float a;
        switch (mode)
        {
            case Mode.Blink:   a = UpdateBlink(); break;
            case Mode.Pulse:   a = UpdatePulse(); break;
            default:           a = UpdateFlicker(); break;
        }
        Apply(a);
    }

    float UpdateBlink()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isOn = !isOn;
            float baseTime = isOn ? onDuration : offDuration;
            timer = baseTime * (1f + Random.Range(-blinkRandomness, blinkRandomness));
        }
        return isOn ? 1f : 0f;
    }

    float UpdatePulse()
    {
        timer += Time.deltaTime;
        float t = (Mathf.Sin(timer / Mathf.Max(0.01f, pulsePeriod) * Mathf.PI * 2f) + 1f) * 0.5f;
        if (pulseSteps > 1) t = Mathf.Round(t * (pulseSteps - 1)) / (pulseSteps - 1);
        return Mathf.Lerp(pulseMin, pulseMax, t);
    }

    float UpdateFlicker()
    {
        if (flickerTimer > 0f)
        {
            flickerTimer -= Time.deltaTime;
            // Titreme sırasında kare kare rastgele aç-kapa
            return Random.value < 0.5f ? flickerAlpha : restAlpha;
        }

        if (Random.value < flickerChancePerSecond * Time.deltaTime)
            flickerTimer = flickerDuration * Random.Range(0.5f, 1.5f);

        return restAlpha;
    }

    void Apply(float alpha)
    {
        var c = color;
        c.a *= alpha;
        if (spriteRenderer) spriteRenderer.color = c;
        else if (image) image.color = c;
    }

    // ---------- Şekil üretimi ----------

    Sprite MakeSquare(int size)
    {
        size = Mathf.Max(1, size);
        var tex = NewTexture(size, size);
        var px = new Color32[size * size];
        for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    // Kademeli halkalardan oluşan, pixel art tarzı yumuşak parıltı
    Sprite MakeGlow(int radius, int bands)
    {
        radius = Mathf.Max(2, radius);
        bands = Mathf.Max(1, bands);
        int size = radius * 2;
        var tex = NewTexture(size, size);
        var px = new Color32[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - radius, dy = y + 0.5f - radius;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / radius;          // 0 merkez, 1 kenar
                float v = Mathf.Clamp01(1f - d);
                v = Mathf.Ceil(v * bands) / bands;                          // Kademelere böl
                byte alpha = (byte)(v * v * 255f);                          // Merkeze doğru yoğunlaşsın
                px[y * size + x] = new Color32(255, 255, 255, alpha);
            }

        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    // Dikdörtgen parıltı: içi hafif aydınlık, kenarlardan dışarı kademeli taşan ışık
    Sprite MakeRectGlow(int w, int h, int spread, int bands, float innerAlpha)
    {
        w = Mathf.Max(1, w);
        h = Mathf.Max(1, h);
        spread = Mathf.Max(1, spread);
        bands = Mathf.Max(1, bands);

        int texW = w + spread * 2;
        int texH = h + spread * 2;
        var tex = NewTexture(texW, texH);
        var px = new Color32[texW * texH];

        for (int y = 0; y < texH; y++)
            for (int x = 0; x < texW; x++)
            {
                // İç dikdörtgene olan uzaklık (içerideyse 0)
                int dx = Mathf.Max(spread - x, 0, x - (spread + w - 1));
                int dy = Mathf.Max(spread - y, 0, y - (spread + h - 1));
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                float v;
                if (d <= 0f) v = innerAlpha;
                else
                {
                    v = Mathf.Clamp01(1f - d / spread);
                    v = Mathf.Ceil(v * bands) / bands;   // Kademelere böl
                    v = v * v * Mathf.Max(innerAlpha, 0.5f);
                }

                px[y * texW + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(v) * 255f));
            }

        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    static Texture2D NewTexture(int w, int h) =>
        new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
}
