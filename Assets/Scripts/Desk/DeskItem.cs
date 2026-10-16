using UnityEngine;
using UnityEngine.EventSystems;

// Masadaki tıklanabilir eşya (harita, rapor, laptop).
// Sahne objesi (SpriteRenderer + BoxCollider2D) olarak kullanılır.
// Kamerada "Physics 2D Raycaster" bileşeni olmalı; böylece açık pencerelerin arkasındaki eşyalara tıklanmaz.
[RequireComponent(typeof(Collider2D))]
public class DeskItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public DeskWindow window;

    [Header("Üzerine gelince")]
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite;
    [Tooltip("Kenarı parlayan versiyon (çizer hazırlayacak). Boşsa renk değişir.")]
    public Sprite highlightSprite;
    public Color hoverTint = new Color(1f, 1f, 0.75f);

    void Reset() => spriteRenderer = GetComponent<SpriteRenderer>();

    public void OnPointerClick(PointerEventData eventData) => window.Toggle();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!spriteRenderer) return;
        if (highlightSprite) spriteRenderer.sprite = highlightSprite;
        else spriteRenderer.color = hoverTint;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!spriteRenderer) return;
        if (highlightSprite) spriteRenderer.sprite = normalSprite;
        spriteRenderer.color = Color.white;
    }
}
