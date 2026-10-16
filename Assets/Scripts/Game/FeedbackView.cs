using System.Collections;
using TMPro;
using UnityEngine;

// Karardan sonra kısa bir süre "DOĞRU / YANLIŞ" ve nedenini gösterir.
public class FeedbackView : MonoBehaviour
{
    public GameObject root;
    public TMP_Text titleText;
    public TMP_Text explanationText;
    public float duration = 2f;

    public Color correctColor = new Color(0.3f, 0.85f, 0.4f);
    public Color wrongColor = new Color(0.95f, 0.3f, 0.3f);

    Coroutine hideRoutine;

    void Awake() => root.SetActive(false);

    public void Show(bool correct, string explanation)
    {
        titleText.text = correct ? "DOĞRU KARAR!" : "YANLIŞ KARAR!";
        titleText.color = correct ? correctColor : wrongColor;
        explanationText.text = explanation;
        root.SetActive(true);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideLater());
    }

    IEnumerator HideLater()
    {
        yield return new WaitForSeconds(duration);
        root.SetActive(false);
    }
}
