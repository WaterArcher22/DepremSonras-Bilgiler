using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Sfx
{
    NewsStatic,     // Haber geldiğinde parazit
    Correct,        // Doğru karar
    Wrong,          // Yanlış karar
    Timeout,        // Haber süresi doldu
    DeviceUpdate,   // Bir cihazın bilgisi güncellendi
    WindowOpen,
    WindowClose,
    Click,          // Buton
    Stamp,          // PAYLAŞILDI / SİLİNDİ damgası
    VisitorEnter,   // Odaya biri geldi
    VisitorLeave
}

// Tüm sesler. StartScene'de kök objeye konur, sahneler arası yok olmaz (müzik kesilmez).
// Kod her yerden AudioManager.Play(Sfx.Correct) diye çağırır; ses atanmamışsa hiçbir şey olmaz.
public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class SfxEntry
    {
        public Sfx type;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public static AudioManager Instance { get; private set; }

    [Header("Müzik")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.4f;

    [Header("Alarm (doğruluk barı kritikken döngüde çalar)")]
    public AudioClip alarmLoop;
    [Range(0f, 1f)] public float alarmVolume = 0.4f;

    [Header("Efektler")]
    public List<SfxEntry> effects = new List<SfxEntry>();
    [Tooltip("Aynı ses tekrar tekrar çalınca sıkmasın diye küçük perde farkı")]
    [Range(0f, 0.2f)] public float pitchVariation = 0.05f;

    AudioSource musicSource, alarmSource;
    AudioSource[] pool;
    int nextSource;
    readonly Dictionary<Sfx, SfxEntry> lookup = new Dictionary<Sfx, SfxEntry>();

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);   // İkinci kopya (sahneye geri dönünce) silinir
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = CreateSource(true);
        alarmSource = CreateSource(true);
        pool = new AudioSource[6];
        for (int i = 0; i < pool.Length; i++) pool[i] = CreateSource(false);

        foreach (var e in effects) lookup[e.type] = e;

        if (music)
        {
            musicSource.clip = music;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        alarmSource.clip = alarmLoop;
        alarmSource.volume = alarmVolume;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    AudioSource CreateSource(bool loop)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.loop = loop;
        s.playOnAwake = false;
        return s;
    }

    // ---------- Her yerden çağrılır ----------

    public static void Play(Sfx type)
    {
        if (Instance) Instance.PlayInternal(type);
    }

    public static void SetAlarm(bool on)
    {
        if (!Instance || !Instance.alarmSource || !Instance.alarmSource.clip) return;
        var src = Instance.alarmSource;
        if (on && !src.isPlaying) src.Play();
        else if (!on && src.isPlaying) src.Stop();
    }

    void PlayInternal(Sfx type)
    {
        if (!lookup.TryGetValue(type, out var entry) || !entry.clip) return;

        var src = pool[nextSource];
        nextSource = (nextSource + 1) % pool.Length;

        src.Stop();
        src.clip = entry.clip;
        src.volume = entry.volume;
        src.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        src.Play();
    }
}
