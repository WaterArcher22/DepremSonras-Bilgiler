using UnityEngine;
using UnityEngine.UI;

// Masadaki eşyaya tıklayınca açılan pencerelerin ortak temeli.
public abstract class DeskWindow : MonoBehaviour
{
    public GameObject root;
    public Button closeButton;

    protected virtual void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(Close);
        root.SetActive(false);
    }

    public abstract void Setup(WorldState world);

    public void Open()
    {
        root.SetActive(true);
        root.transform.SetAsLastSibling(); // En öne getir
    }

    public void Close() => root.SetActive(false);

    public void Toggle()
    {
        if (root.activeSelf) Close();
        else Open();
    }
}
