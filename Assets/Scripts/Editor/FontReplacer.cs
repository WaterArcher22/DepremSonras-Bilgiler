#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menü: Tools > Font Değiştirici
// Açık sahnedeki (ya da seçili objelerdeki / prefab'lardaki) tüm TextMeshPro yazılarının fontunu tek seferde değiştirir.
// İsterseniz yazı boyutlarını da pixel fontun katlarına yuvarlar (bulanıklık olmasın diye).
// Bu dosya mutlaka "Editor" adlı bir klasörde olmalı.
public class FontReplacer : EditorWindow
{
    TMP_FontAsset font;
    bool snapSize = true;
    int sizeStep = 8;
    int minSize = 8;
    bool turnOffAutoSize = true;

    [MenuItem("Tools/Font Değiştirici")]
    static void Open() => GetWindow<FontReplacer>("Font Değiştirici");

    void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "1) Yeni font asset'ini seçin.\n" +
            "2) 'Açık sahne' ya da 'Seçili objeler' butonuna basın.\n" +
            "Ctrl+Z ile geri alınabilir (prefab dosyaları hariç).", MessageType.Info);

        font = (TMP_FontAsset)EditorGUILayout.ObjectField("Yeni font", font, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();
        snapSize = EditorGUILayout.Toggle("Boyutu yuvarla", snapSize);
        using (new EditorGUI.DisabledScope(!snapSize))
        {
            sizeStep = Mathf.Max(1, EditorGUILayout.IntField("  Katları (Press Start 2P = 8)", sizeStep));
            minSize = Mathf.Max(1, EditorGUILayout.IntField("  En küçük boyut", minSize));
        }
        turnOffAutoSize = EditorGUILayout.Toggle("Auto Size'ı kapat", turnOffAutoSize);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!font))
        {
            if (GUILayout.Button("Açık sahnedeki TÜM yazılar", GUILayout.Height(30)))
                ApplyToScene();
            if (GUILayout.Button("Seçili objeler (ve altındakiler)", GUILayout.Height(30)))
                ApplyToSelection();
        }
    }

    void ApplyToScene()
    {
        var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int n = 0;
        foreach (var t in texts) if (Apply(t)) n++;
        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"Font Değiştirici: {n} yazı güncellendi. Sahneyi kaydetmeyi unutmayın (Ctrl+S).");
    }

    void ApplyToSelection()
    {
        int n = 0;
        foreach (var go in Selection.gameObjects)
        {
            bool isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(go);
            foreach (var t in go.GetComponentsInChildren<TMP_Text>(true))
                if (Apply(t)) n++;

            if (isPrefabAsset) PrefabUtility.SavePrefabAsset(go.transform.root.gameObject);
        }
        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"Font Değiştirici: {n} yazı güncellendi.");
    }

    bool Apply(TMP_Text t)
    {
        if (!t) return false;
        Undo.RecordObject(t, "Font Değiştir");

        t.font = font;
        t.fontSharedMaterial = font.material;

        if (turnOffAutoSize && t.enableAutoSizing)
        {
            t.fontSize = t.fontSizeMax;
            t.enableAutoSizing = false;
        }

        if (snapSize)
        {
            int size = Mathf.Max(minSize, Mathf.RoundToInt(t.fontSize / sizeStep) * sizeStep);
            t.fontSize = size;
        }

        EditorUtility.SetDirty(t);
        return true;
    }
}
#endif
