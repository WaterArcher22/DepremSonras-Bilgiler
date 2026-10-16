using System;
using UnityEngine;

// Doğruluk barı: zamanla azalır, yanlış kararda ani düşer, doğru kararda artar.
// Tüm değerler Inspector'dan ayarlanır; denge ayarını oyunu durdurmadan yapabilirsiniz.
public class AccuracyBar : MonoBehaviour
{
    [Header("Değerler")]
    public float maxValue = 100f;
    public float startValue = 100f;

    [Tooltip("Saniyede azalma miktarı (başlangıç)")]
    public float drainPerSecond = 2f;
    [Tooltip("Oyun ilerledikçe azalma hızına her saniye eklenen miktar (zorlaşma)")]
    public float drainIncreasePerSecond = 0.02f;
    [Tooltip("Azalma hızının üst sınırı")]
    public float maxDrainPerSecond = 8f;

    [Tooltip("Yanlış kararda ani düşüş")]
    public float wrongPenalty = 20f;
    [Tooltip("Doğru kararda artış")]
    public float correctReward = 10f;

    public float Value { get; private set; }
    public float Normalized => Value / maxValue;
    public bool IsRunning { get; set; }

    public event Action<float> OnChanged;   // normalized değer (0-1)
    public event Action OnBurst;            // yanlış karar anı (sarsıntı vb. için)
    public event Action OnDepleted;

    float currentDrain;

    public void ResetBar()
    {
        Value = startValue;
        currentDrain = drainPerSecond;
        OnChanged?.Invoke(Normalized);
    }

    void Update()
    {
        if (!IsRunning) return;

        currentDrain = Mathf.Min(currentDrain + drainIncreasePerSecond * Time.deltaTime, maxDrainPerSecond);
        Change(-currentDrain * Time.deltaTime);
    }

    public void ApplyCorrect() => ApplyCorrect(correctReward);
    public void ApplyCorrect(float amount) => Change(amount);

    public void ApplyWrong() => ApplyWrong(wrongPenalty);
    public void ApplyWrong(float amount)
    {
        if (!IsRunning) return;
        OnBurst?.Invoke();
        Change(-amount);
    }

    void Change(float amount)
    {
        if (!IsRunning) return;

        Value = Mathf.Clamp(Value + amount, 0f, maxValue);
        OnChanged?.Invoke(Normalized);

        if (Value <= 0f)
        {
            IsRunning = false;
            OnDepleted?.Invoke();
        }
    }
}
