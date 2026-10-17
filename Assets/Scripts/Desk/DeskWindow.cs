using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Masadaki eşyaya tıklayınca açılan pencerelerin ortak temeli.
// Bu script her zaman açık duran bir üst objede olmalı; root ise açılıp kapanan panel.
public abstract class DeskWindow : MonoBehaviour
{
    public GameObject root;
    public Button closeButton;

    [Header("Açılış animasyonu")]
    public float popTime = 0.08f;
    [Range(0.5f, 1f)] public float popStartScale = 0.85f;

    // ---- Açık pencere takibi (karartma ve butonların gizlenmesi için) ----
    static readonly HashSet<DeskWindow> openWindows = new HashSet<DeskWindow>();
    public static bool AnyOpen => openWindows.Count > 0;
    public static event Action<bool> OnAnyOpenChanged;
    // Belirli bir pencere açıldığında (öğretici bunu dinler)
    public static event Action<DeskWindow> OnWindowOpened;

    public bool IsOpen => root && root.activeSelf;

    protected virtual void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(Close);
        root.SetActive(false);
    }

    protected virtual void OnDestroy()
    {
        openWindows.Remove(this);
    }

    public abstract void Setup(WorldState world);

    public void Open()
    {
        if (IsOpen) return;

        root.SetActive(true);
        transform.SetAsLastSibling(); // Diğer pencerelerin önüne getir
        StartCoroutine(Pop());
        AudioManager.Play(Sfx.WindowOpen);
        SetOpenState(true);
        OnWindowOpened?.Invoke(this);
    }

    public void Close()
    {
        if (!IsOpen) return;

        root.SetActive(false);
        AudioManager.Play(Sfx.WindowClose);
        SetOpenState(false);
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public static void CloseAll()
    {
        foreach (var w in new List<DeskWindow>(openWindows))
            if (w) w.Close();
    }

    void SetOpenState(bool open)
    {
        bool before = AnyOpen;
        if (open) openWindows.Add(this);
        else openWindows.Remove(this);
        if (before != AnyOpen) OnAnyOpenChanged?.Invoke(AnyOpen);
    }

    IEnumerator Pop()
    {
        var rt = root.transform;
        float t = 0f;
        while (t < popTime)
        {
            t += Time.unscaledDeltaTime;
            rt.localScale = Vector3.one * Mathf.Lerp(popStartScale, 1f, t / popTime);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }
}
