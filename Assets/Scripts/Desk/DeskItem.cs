using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Masadaki tıklanabilir eşya (harita, rapor, laptop).
// Sahnede Collider2D ile kullanılır (kamerada Physics 2D Raycaster gerekir).
// - Fare üzerine gelince collider'ın ŞEKLİNİ takip eden yeşil piksel çerçeve çizer
//   (PolygonCollider2D, BoxCollider2D, CircleCollider2D; birden fazla collider da olur).
// - Bilgisi güncellenince, oyuncu tıklayana kadar turuncu çerçeveyle yanıp söner.
public class DeskItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public DeskWindow window;
    [Tooltip("Bu eşya hangi bilgiyi gösteriyor? Değişince bu eşya yanıp söner.")]
    public EvidenceSource source;

    [Header("Çerçeve")]
    public bool showFrame = true;
    public Color frameColor = new Color(0.3f, 1f, 0.4f);
    [Tooltip("Çizgi kalınlığı (piksel)")]
    [Range(1, 4)] public int frameThickness = 1;
    [Tooltip("Sprite'larınızın Pixels Per Unit değeri")]
    public int pixelsPerUnit = 16;
    [Tooltip("Boş bırakılırsa en öndeki Sorting Layer kullanılır")]
    public string frameSortingLayer = "";
    public int frameSortingOrder = 100;

    [Header("Güncelleme uyarısı")]
    public Color blinkColor = new Color(1f, 0.65f, 0.15f);
    [Tooltip("Yanıp sönme hızı (saniye)")]
    public float blinkInterval = 0.25f;

    [Header("Sprite / Image varsa")]
    [Tooltip("Kenarı parlayan versiyon (isteğe bağlı)")]
    public Sprite highlightSprite;
    public Color hoverTint = new Color(1f, 1f, 0.75f);

    public bool IsBlinking => blinking;

    // Öğretici gibi sistemler hangi eşyaların tıklanabileceğini sınırlayabilir. null = hepsi serbest.
    public static System.Func<DeskItem, bool> CanInteract;
    bool Interactable => CanInteract == null || CanInteract(this);

    Image image;
    SpriteRenderer spriteRenderer;
    Sprite normalSprite;
    GameObject frame;
    SpriteRenderer frameRenderer;
    bool hovering, blinking;

    void Awake()
    {
        image = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (image) normalSprite = image.sprite;
        else if (spriteRenderer) normalSprite = spriteRenderer.sprite;

        if (showFrame) BuildFrame();
    }

    // ---------- Dışarıdan ----------

    public void StartBlink() => blinking = true;
    public void StopBlink() => blinking = false;

    // ---------- Tıklama ve fare ----------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable) return;
        StopBlink();
        if (window) window.Toggle();
    }

    public void OnPointerEnter(PointerEventData eventData) => hovering = Interactable;
    public void OnPointerExit(PointerEventData eventData) => hovering = false;

    void OnDisable()
    {
        hovering = false;
        if (frame) frame.SetActive(false);
    }

    void Update()
    {
        bool blinkOn = blinking && Mathf.FloorToInt(Time.time / blinkInterval) % 2 == 0;

        if (frame)
        {
            if (hovering)       { frame.SetActive(true); frameRenderer.color = frameColor; }
            else if (blinkOn)   { frame.SetActive(true); frameRenderer.color = blinkColor; }
            else                { frame.SetActive(false); }
        }

        // Sprite / Image ile çalışan eşyalar için
        if (image || spriteRenderer)
        {
            SetSprite(hovering && highlightSprite ? highlightSprite : normalSprite);

            Color c = Color.white;
            if (!frame)
            {
                if (hovering && !highlightSprite) c = hoverTint;
                else if (blinkOn) c = blinkColor;
            }
            SetColor(c);
        }
    }

    void SetSprite(Sprite s)
    {
        if (image) image.sprite = s;
        else if (spriteRenderer) spriteRenderer.sprite = s;
    }

    void SetColor(Color c)
    {
        if (image) image.color = c;
        else if (spriteRenderer) spriteRenderer.color = c;
    }

    // ---------- Çerçeve: collider şekli piksel piksel bir dokuya çizilir ----------

    void BuildFrame()
    {
        // 1) Tüm collider'ların kenarlarını dünya koordinatında topla
        var outlines = new List<Vector2[]>();
        foreach (var col in GetComponents<Collider2D>())
            CollectOutlines(col, outlines);
        if (outlines.Count == 0) return;

        // 2) Hepsini kapsayan alanı piksel ızgarasına oturt
        float px = 1f / pixelsPerUnit;
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        foreach (var path in outlines)
            foreach (var p in path)
            {
                minX = Mathf.Min(minX, p.x); minY = Mathf.Min(minY, p.y);
                maxX = Mathf.Max(maxX, p.x); maxY = Mathf.Max(maxY, p.y);
            }

        int margin = frameThickness + 1;
        float originX = Mathf.Floor(minX / px) * px - margin * px;
        float originY = Mathf.Floor(minY / px) * px - margin * px;
        int width  = Mathf.CeilToInt((maxX - originX) / px) + margin + 1;
        int height = Mathf.CeilToInt((maxY - originY) / px) + margin + 1;

        // 3) Kenarları dokuya çiz (Bresenham)
        var pixels = new Color32[width * height];
        var white = new Color32(255, 255, 255, 255);

        foreach (var path in outlines)
        {
            for (int i = 0; i < path.Length; i++)
            {
                Vector2 a = path[i];
                Vector2 b = path[(i + 1) % path.Length];
                int x0 = Mathf.RoundToInt((a.x - originX) / px), y0 = Mathf.RoundToInt((a.y - originY) / px);
                int x1 = Mathf.RoundToInt((b.x - originX) / px), y1 = Mathf.RoundToInt((b.y - originY) / px);
                DrawLine(pixels, width, height, x0, y0, x1, y1, white);
            }
        }

        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.SetPixels32(pixels);
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, width, height), Vector2.zero, pixelsPerUnit);

        // 4) Dokuyu gösteren obje (ebeveynin ölçeğinden etkilenmez)
        frame = new GameObject("HoverFrame");
        frame.transform.SetParent(transform, false);
        frame.transform.position = new Vector3(originX, originY, transform.position.z);
        frame.transform.rotation = Quaternion.identity;
        Vector3 s = transform.lossyScale;
        frame.transform.localScale = new Vector3(
            1f / Mathf.Max(Mathf.Abs(s.x), 0.0001f),
            1f / Mathf.Max(Mathf.Abs(s.y), 0.0001f),
            1f);

        frameRenderer = frame.AddComponent<SpriteRenderer>();
        frameRenderer.sprite = sprite;
        frameRenderer.color = frameColor;
        var layers = SortingLayer.layers;
        frameRenderer.sortingLayerName = string.IsNullOrEmpty(frameSortingLayer)
            ? layers[layers.Length - 1].name
            : frameSortingLayer;
        frameRenderer.sortingOrder = frameSortingOrder;

        frame.SetActive(false);
    }

    void CollectOutlines(Collider2D col, List<Vector2[]> outlines)
    {
        var t = col.transform;

        if (col is PolygonCollider2D poly)
        {
            for (int i = 0; i < poly.pathCount; i++)
            {
                var local = poly.GetPath(i);
                var world = new Vector2[local.Length];
                for (int j = 0; j < local.Length; j++)
                    world[j] = t.TransformPoint(local[j] + poly.offset);
                outlines.Add(world);
            }
        }
        else if (col is BoxCollider2D box)
        {
            Vector2 h = box.size / 2f, o = box.offset;
            outlines.Add(new Vector2[]
            {
                t.TransformPoint(o + new Vector2(-h.x, -h.y)),
                t.TransformPoint(o + new Vector2( h.x, -h.y)),
                t.TransformPoint(o + new Vector2( h.x,  h.y)),
                t.TransformPoint(o + new Vector2(-h.x,  h.y)),
            });
        }
        else if (col is CircleCollider2D circle)
        {
            const int segments = 32;
            var world = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                world[i] = t.TransformPoint(circle.offset + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * circle.radius);
            }
            outlines.Add(world);
        }
        else
        {
            // Diğer türler için sınır kutusu
            Bounds b = col.bounds;
            outlines.Add(new Vector2[]
            {
                new Vector2(b.min.x, b.min.y), new Vector2(b.max.x, b.min.y),
                new Vector2(b.max.x, b.max.y), new Vector2(b.min.x, b.max.y),
            });
        }
    }

    void DrawLine(Color32[] pixels, int w, int h, int x0, int y0, int x1, int y1, Color32 c)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            Plot(pixels, w, h, x0, y0, c);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    void Plot(Color32[] pixels, int w, int h, int x, int y, Color32 c)
    {
        // Kalınlık: noktanın etrafını da boya
        int r = frameThickness - 1;
        for (int ox = -r; ox <= r; ox++)
            for (int oy = -r; oy <= r; oy++)
            {
                int px = x + ox, py = y + oy;
                if (px >= 0 && px < w && py >= 0 && py < h)
                    pixels[py * w + px] = c;
            }
    }
}
