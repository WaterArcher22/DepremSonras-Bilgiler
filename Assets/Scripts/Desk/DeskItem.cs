using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Masadaki tıklanabilir eşya (harita, rapor, laptop).
// Sahnede BoxCollider2D ile kullanılır (kamerada Physics 2D Raycaster gerekir).
// Fare üzerine gelince collider'ın etrafına otomatik olarak piksel çerçeve çizer.
// Canvas içinde UI Image olarak da çalışır (o durumda çerçeve yerine renk değişir).
public class DeskItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public DeskWindow window;

    [Header("Çerçeve (BoxCollider2D varsa)")]
    public bool showFrame = true;
    public Color frameColor = new Color(0.3f, 1f, 0.4f);
    [Tooltip("Çerçeve kalınlığı (piksel)")]
    public int frameThickness = 1;
    [Tooltip("Çerçevenin collider'dan ne kadar dışarıda olacağı (piksel)")]
    public int framePadding = 1;
    [Tooltip("Sprite'larınızın Pixels Per Unit değeri")]
    public int pixelsPerUnit = 16;
    [Tooltip("Boş bırakılırsa en öndeki Sorting Layer kullanılır")]
    public string frameSortingLayer = "";
    public int frameSortingOrder = 100;

    [Header("Sprite / Image varsa")]
    [Tooltip("Kenarı parlayan versiyon (isteğe bağlı)")]
    public Sprite highlightSprite;
    public Color hoverTint = new Color(1f, 1f, 0.75f);

    Image image;
    SpriteRenderer spriteRenderer;
    Sprite normalSprite;
    GameObject frame;

    static Sprite pixelSprite;

    void Awake()
    {
        image = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (image) normalSprite = image.sprite;
        else if (spriteRenderer) normalSprite = spriteRenderer.sprite;

        // Eşyadaki tüm collider'ları kapsayan tek bir çerçeve
        var colliders = GetComponents<Collider2D>();
        if (showFrame && colliders.Length > 0)
        {
            Bounds all = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                all.Encapsulate(colliders[i].bounds);
            BuildFrame(all);
        }
    }

    // ---------- Tıklama ve fare ----------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (window) window.Toggle();
    }

    public void OnPointerEnter(PointerEventData eventData) => SetHover(true);
    public void OnPointerExit(PointerEventData eventData) => SetHover(false);

    void OnDisable() => SetHover(false);

    void SetHover(bool on)
    {
        if (frame) frame.SetActive(on);

        if (image || spriteRenderer)
        {
            if (on && highlightSprite) SetSprite(highlightSprite);
            else SetSprite(normalSprite);

            SetColor(on && !highlightSprite && !frame ? hoverTint : Color.white);
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

    // ---------- Çerçeve ----------

    void BuildFrame(Bounds b)
    {
        if (!pixelSprite)
        {
            var tex = new Texture2D(1, 1) { filterMode = FilterMode.Point };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            pixelSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        float px = 1f / pixelsPerUnit;

        // Collider'ların dünya üzerindeki sınırları, piksel ızgarasına oturtulmuş
        float left   = Snap(b.min.x, px) - framePadding * px;
        float right  = Snap(b.max.x, px) + framePadding * px;
        float bottom = Snap(b.min.y, px) - framePadding * px;
        float top    = Snap(b.max.y, px) + framePadding * px;

        float t = frameThickness * px;
        float w = right - left;
        float h = top - bottom;

        // Dünya merkezinde oluşturup eşyanın altına taşı (dünya konumu korunur)
        frame = new GameObject("HoverFrame");
        frame.transform.SetParent(transform, true);

        // Üst, alt, sol, sağ kenarlar
        CreateEdge(new Vector2(left + w / 2f, top - t / 2f),    new Vector2(w, t));
        CreateEdge(new Vector2(left + w / 2f, bottom + t / 2f), new Vector2(w, t));
        CreateEdge(new Vector2(left + t / 2f, bottom + h / 2f), new Vector2(t, h));
        CreateEdge(new Vector2(right - t / 2f, bottom + h / 2f), new Vector2(t, h));

        frame.SetActive(false);
    }

    void CreateEdge(Vector2 center, Vector2 size)
    {
        var edge = new GameObject("Edge");
        edge.transform.SetParent(frame.transform, true);
        edge.transform.position = new Vector3(center.x, center.y, transform.position.z);

        // Ebeveyn ölçeğinden etkilenmesin
        Vector3 parentScale = frame.transform.lossyScale;
        edge.transform.localScale = new Vector3(
            size.x / Mathf.Max(parentScale.x, 0.0001f),
            size.y / Mathf.Max(parentScale.y, 0.0001f),
            1f);

        var sr = edge.AddComponent<SpriteRenderer>();
        sr.sprite = pixelSprite;
        sr.color = frameColor;
        var layers = SortingLayer.layers;
        sr.sortingLayerName = string.IsNullOrEmpty(frameSortingLayer)
            ? layers[layers.Length - 1].name
            : frameSortingLayer;
        sr.sortingOrder = frameSortingOrder;
    }

    static float Snap(float v, float step) => Mathf.Round(v / step) * step;
}
