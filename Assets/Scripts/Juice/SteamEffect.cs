using System.Collections.Generic;
using UnityEngine;

// Kahve bardağından tüten pikselli duman. Sprite gerekmez, pikseller kodla oluşturulur.
// Boş bir obje oluşturup bardağın ağzına koyun ve bu scripti ekleyin (Canvas'ın içine DEĞİL).
// Sahnede seçiliyken duman çıkış noktası sarı bir çizgiyle gösterilir.
public class SteamEffect : MonoBehaviour
{
    [Header("Piksel")]
    [Tooltip("Sprite'larınızın Pixels Per Unit değeri")]
    public int pixelsPerUnit = 16;
    [Tooltip("Pozisyonlar piksel ızgarasına otursun (titrek / bulanık görünmez)")]
    public bool snapToPixels = true;

    [Header("Çıkış")]
    [Tooltip("Kaç saniyede bir yeni duman pikseli çıksın")]
    public float spawnInterval = 0.18f;
    [Tooltip("Dumanın çıktığı alanın genişliği (piksel)")]
    public float spawnWidth = 3f;
    [Tooltip("Aynı anda en fazla kaç piksel")]
    public int maxPuffs = 30;

    [Header("Hareket")]
    [Tooltip("Yükselme hızı (piksel / saniye)")]
    public float riseSpeed = 5f;
    [Tooltip("Bir pikselin ömrü (saniye)")]
    public float lifetime = 2.2f;
    [Tooltip("Sağa sola kıvrılma miktarı (piksel). Yükseldikçe artar.")]
    public float wobbleAmount = 2f;
    [Tooltip("Kıvrılma hızı")]
    public float wobbleSpeed = 2.5f;

    [Header("Görünüm")]
    public Color color = new Color(1f, 1f, 1f, 0.55f);
    [Tooltip("Başlangıç ve bitiş boyutu (piksel). Duman yükseldikçe genişler.")]
    public int startSize = 1;
    public int endSize = 2;
    [Tooltip("Saydamlık kaç basamakta azalsın (pikselli his için). 0 = yumuşak.")]
    public int alphaSteps = 4;

    [Header("Çizim sırası")]
    [Tooltip("Boş bırakılırsa 'Default' kullanılır. Arka planın önünde görünmesi gerekir.")]
    public string sortingLayer = "";
    public int sortingOrder = 50;
    [Tooltip("Işıklandırmadan etkilenmesin diye Sprite-Unlit-Default önerilir. Boşsa varsayılan materyal.")]
    public Material material;

    class Puff
    {
        public Transform t;
        public SpriteRenderer r;
        public float age, x0, phase, speed;
        public bool alive;
    }

    readonly List<Puff> puffs = new List<Puff>();
    Sprite pixel;
    float timer;

    void Awake()
    {
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        // 1 piksel = 1 / pixelsPerUnit birim
        pixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    void Update()
    {
        timer += Time.deltaTime;
        while (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            Spawn();
        }

        float px = 1f / pixelsPerUnit;
        Vector3 origin = transform.position;

        foreach (var p in puffs)
        {
            if (!p.alive) continue;

            p.age += Time.deltaTime;
            float k = p.age / lifetime;
            if (k >= 1f)
            {
                p.alive = false;
                p.t.gameObject.SetActive(false);
                continue;
            }

            // Yukarı çıkarken sağa sola kıvrılır; yükseldikçe kıvrım genişler
            float y = p.age * p.speed;
            float x = p.x0 + Mathf.Sin(p.age * wobbleSpeed + p.phase) * wobbleAmount * k;

            Vector3 pos = origin + new Vector3(x * px, y * px, 0f);
            if (snapToPixels)
            {
                pos.x = Mathf.Round(pos.x * pixelsPerUnit) / pixelsPerUnit;
                pos.y = Mathf.Round(pos.y * pixelsPerUnit) / pixelsPerUnit;
            }
            p.t.position = pos;

            int size = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(startSize, endSize, k)));
            p.t.localScale = new Vector3(size, size, 1f);

            // Kısa sürede belirir, sonra yavaşça kaybolur
            float a = k < 0.1f ? k / 0.1f : 1f - (k - 0.1f) / 0.9f;
            if (alphaSteps > 0) a = Mathf.Ceil(a * alphaSteps) / alphaSteps;
            var c = color;
            c.a = color.a * a;
            p.r.color = c;
        }
    }

    void Spawn()
    {
        var p = GetFree();
        if (p == null) return;

        p.alive = true;
        p.age = 0f;
        p.x0 = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        p.phase = Random.Range(0f, Mathf.PI * 2f);
        p.speed = riseSpeed * Random.Range(0.8f, 1.2f);
        p.t.gameObject.SetActive(true);
    }

    Puff GetFree()
    {
        foreach (var p in puffs)
            if (!p.alive) return p;
        if (puffs.Count >= maxPuffs) return null;

        var go = new GameObject("SteamPixel");
        go.transform.SetParent(transform, false);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = pixel;
        if (material) r.sharedMaterial = material;
        if (!string.IsNullOrEmpty(sortingLayer)) r.sortingLayerName = sortingLayer;
        r.sortingOrder = sortingOrder;

        var puff = new Puff { t = go.transform, r = r };
        puffs.Add(puff);
        return puff;
    }

    void OnDrawGizmosSelected()
    {
        float px = 1f / Mathf.Max(1, pixelsPerUnit);
        Vector3 o = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(o - Vector3.right * spawnWidth * px / 2f, o + Vector3.right * spawnWidth * px / 2f);
        Gizmos.DrawLine(o, o + Vector3.up * riseSpeed * lifetime * px);
    }
}
