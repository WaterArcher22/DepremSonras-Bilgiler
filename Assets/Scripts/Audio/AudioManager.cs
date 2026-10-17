using System;
using System.Collections;
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
    VisitorLeave,
    ComboUp,        // Kombo çarpanı yükseldi
    ComboBreak      // Seri bozuldu
}

// Tüm sesler. Kök objeye konur, sahneler arası yok olmaz (müzik kesilmez).
// Kod her yerden AudioManager.Play(Sfx.Correct) diye çağırır; ses atanmamışsa hiçbir şey olmaz.
// Her ses türünün kendine ait kanalları vardır: farklı sesler aynı anda çalar, birbirini kesmez.
public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class SfxEntry
    {
        public Sfx type;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Tooltip("Ses dosyasının kaçıncı saniyesinden başlansın")]
        public float startTime = 0f;
        [Tooltip("0 = ses dosyası sonuna kadar çalar. Değer girilirse (başlangıç noktasından itibaren) o kadar saniye çalıp kısılarak kesilir.")]
        public float maxDuration = 0f;
    }

    // Bir ses türüne ait kanallar
    class Channel
    {
        public SfxEntry entry;
        public AudioSource[] sources;
        public Coroutine[] cuts;
        public int next;
    }

    public static AudioManager Instance { get; private set; }

    [Header("Müzik")]
    [Tooltip("Varsayılan müzik. Sahnelerde SceneMusic varsa o sahnenin müziği çalar.")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.4f;
    [Tooltip("Sahne değişince müziğin kısılıp yenisinin açılma süresi")]
    public float musicFadeTime = 0.6f;

    [Header("Alarm (doğruluk barı kritikken döngüde çalar)")]
    public AudioClip alarmLoop;
    [Range(0f, 1f)] public float alarmVolume = 0.4f;

    [Header("Efektler")]
    public List<SfxEntry> effects = new List<SfxEntry>();
    [Tooltip("Aynı ses tekrar tekrar çalınca sıkmasın diye küçük perde farkı")]
    [Range(0f, 0.2f)] public float pitchVariation = 0.05f;
    [Tooltip("Aynı sesin üst üste kaç kez aynı anda çalabileceği")]
    [Range(1, 4)] public int voicesPerSound = 2;

    AudioSource musicSource, alarmSource;
    Coroutine musicRoutine;
    readonly Dictionary<Sfx, Channel> channels = new Dictionary<Sfx, Channel>();

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

        // Her ses türü için ayrı kanallar
        foreach (var e in effects)
        {
            if (e == null || channels.ContainsKey(e.type))
            {
                if (e != null) Debug.LogWarning($"AudioManager: '{e.type}' listede birden fazla kez var, ilki kullanılıyor.");
                continue;
            }

            var ch = new Channel
            {
                entry = e,
                sources = new AudioSource[voicesPerSound],
                cuts = new Coroutine[voicesPerSound]
            };
            for (int i = 0; i < voicesPerSound; i++) ch.sources[i] = CreateSource(false);
            channels[e.type] = ch;
        }

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

    // Müziği değiştirir. Aynı müzik zaten çalıyorsa baştan başlatmaz.
    public static void PlayMusic(AudioClip clip)
    {
        if (!Instance) return;
        var src = Instance.musicSource;
        if (src.clip == clip && src.isPlaying) return;

        if (Instance.musicRoutine != null) Instance.StopCoroutine(Instance.musicRoutine);
        Instance.musicRoutine = Instance.StartCoroutine(Instance.SwitchMusic(clip));
    }

    public static void SetAlarm(bool on)
    {
        if (!Instance || !Instance.alarmSource || !Instance.alarmSource.clip) return;
        var src = Instance.alarmSource;
        if (on && !src.isPlaying) src.Play();
        else if (!on && src.isPlaying) src.Stop();
    }

    // ---------- Efektler ----------

    void PlayInternal(Sfx type)
    {
        if (!channels.TryGetValue(type, out var ch) || !ch.entry.clip) return;
        var entry = ch.entry;

        // Bu sesin kanallarından sıradakini kullan (aynı ses üst üste gelirse de üst üste çalar)
        int i = ch.next;
        ch.next = (ch.next + 1) % ch.sources.Length;
        var src = ch.sources[i];

        if (ch.cuts[i] != null) StopCoroutine(ch.cuts[i]);
        ch.cuts[i] = null;

        src.Stop();
        src.clip = entry.clip;
        src.volume = entry.volume;
        src.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        src.time = Mathf.Clamp(entry.startTime, 0f, Mathf.Max(0f, entry.clip.length - 0.01f));
        src.Play();

        if (entry.maxDuration > 0f)
            ch.cuts[i] = StartCoroutine(CutAfter(src, entry.maxDuration, entry.volume));
    }

    // Sesi verilen sürede kısa bir kısılmayla keser (ani kesilme "çıt" sesi yapmasın)
    IEnumerator CutAfter(AudioSource src, float duration, float volume)
    {
        const float fade = 0.05f;
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, duration - fade));
        for (float t = 0f; t < fade; t += Time.unscaledDeltaTime)
        {
            src.volume = Mathf.Lerp(volume, 0f, t / fade);
            yield return null;
        }
        src.Stop();
    }

    // ---------- Müzik ----------

    IEnumerator SwitchMusic(AudioClip clip)
    {
        float half = musicFadeTime / 2f;
        float start = musicSource.volume;

        // Eski müziği kıs
        if (musicSource.isPlaying)
        {
            for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(start, 0f, t / half);
                yield return null;
            }
        }

        musicSource.Stop();
        musicSource.clip = clip;
        if (!clip) yield break;

        // Yenisini aç
        musicSource.volume = 0f;
        musicSource.Play();
        for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t / half);
            yield return null;
        }
        musicSource.volume = musicVolume;
    }
}
