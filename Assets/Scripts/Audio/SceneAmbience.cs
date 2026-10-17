using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Sahnenin ortam sesleri. Müzikle birlikte çalar; AudioManager'dan bağımsızdır.
// Sahneye bir tane koyun ve listelere istediğiniz kadar ses ekleyin.
// Sahne değişince kendiliğinden susar.
public class SceneAmbience : MonoBehaviour
{
    [Serializable]
    public class LoopSound
    {
        [Tooltip("Sadece not için (örn. 'Yağmur')")]
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.3f;
    }

    [Serializable]
    public class RandomSound
    {
        [Tooltip("Sadece not için (örn. 'Uzak siren')")]
        public string name;
        [Tooltip("Birden fazla dosya koyarsanız her seferinde aralarından biri seçilir")]
        public AudioClip[] clips;
        [Range(0f, 1f)] public float minVolume = 0.2f;
        [Range(0f, 1f)] public float maxVolume = 0.4f;
        [Tooltip("İki çalma arasındaki en kısa süre (saniye)")]
        public float minInterval = 15f;
        [Tooltip("İki çalma arasındaki en uzun süre (saniye)")]
        public float maxInterval = 40f;
        [Tooltip("Oyun başladıktan sonra ilk kez çalmadan önce beklenen süre")]
        public float firstDelay = 5f;
        [Tooltip("Her çalışta perde farkı (sesin tekrar ettiği anlaşılmasın)")]
        [Range(0f, 0.3f)] public float pitchVariation = 0.08f;
        [Tooltip("Sesin sağ/sol hoparlörde rastgele konumlanması (0 = tam ortada)")]
        [Range(0f, 1f)] public float stereoSpread = 0.6f;
        [Tooltip("Ses en fazla bu kadar saniye çalar, sonra yavaşça kısılır (uzun kayıtlar için). 0 = sonuna kadar çalar.")]
        public float maxDuration = 6f;
    }

    [Header("Genel")]
    [Tooltip("Tüm ortam seslerinin ana ses seviyesi")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Tooltip("Sahne açılınca seslerin yavaşça yükselme süresi")]
    public float fadeInTime = 2f;

    [Header("Sürekli çalan sesler (döngü)")]
    public List<LoopSound> loops = new List<LoopSound>();

    [Header("Ara sıra çalan sesler")]
    public List<RandomSound> randomSounds = new List<RandomSound>();

    readonly List<AudioSource> loopSources = new List<AudioSource>();
    readonly List<float> loopVolumes = new List<float>();
    float fade;

    void Start()
    {
        foreach (var l in loops)
        {
            if (l == null || !l.clip) continue;
            var src = CreateSource();
            src.clip = l.clip;
            src.loop = true;
            src.volume = 0f;
            // Aynı anda başlayan döngüler tek ses gibi duyulmasın diye rastgele bir yerden başlat
            src.time = Random.Range(0f, l.clip.length);
            src.Play();
            loopSources.Add(src);
            loopVolumes.Add(l.volume);
        }

        foreach (var r in randomSounds)
            if (r != null && r.clips != null && r.clips.Length > 0)
                StartCoroutine(RandomLoop(r));
    }

    void Update()
    {
        // Döngülerin yavaşça açılması ve master ses ayarının anında uygulanması
        fade = fadeInTime > 0f ? Mathf.Min(1f, fade + Time.unscaledDeltaTime / fadeInTime) : 1f;
        for (int i = 0; i < loopSources.Count; i++)
            loopSources[i].volume = loopVolumes[i] * masterVolume * fade;
    }

    IEnumerator RandomLoop(RandomSound r)
    {
        yield return new WaitForSeconds(r.firstDelay + Random.Range(0f, r.minInterval * 0.5f));

        var src = CreateSource();

        while (true)
        {
            var clip = r.clips[Random.Range(0, r.clips.Length)];
            if (clip)
            {
                float volume = Random.Range(r.minVolume, r.maxVolume) * masterVolume * fade;
                src.clip = clip;
                src.loop = false;
                src.pitch = 1f + Random.Range(-r.pitchVariation, r.pitchVariation);
                src.panStereo = Random.Range(-r.stereoSpread, r.stereoSpread);
                src.volume = volume;
                src.Play();

                // Ses bitene (ya da süre sınırına gelene) kadar bekle
                float length = clip.length / Mathf.Max(0.1f, Mathf.Abs(src.pitch));
                float playTime = r.maxDuration > 0f ? Mathf.Min(length, r.maxDuration) : length;
                yield return new WaitForSeconds(playTime);

                // Sınıra gelindiyse kısarak kes
                if (src.isPlaying)
                {
                    const float fadeOut = 1f;
                    for (float t = 0f; t < fadeOut; t += Time.deltaTime)
                    {
                        src.volume = Mathf.Lerp(volume, 0f, t / fadeOut);
                        yield return null;
                    }
                    src.Stop();
                }
            }

            // Bekleme, ses bittikten sonra başlar
            float wait = Random.Range(r.minInterval, Mathf.Max(r.minInterval, r.maxInterval));
            yield return new WaitForSeconds(wait);
        }
    }

    AudioSource CreateSource()
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.spatialBlend = 0f;
        return s;
    }
}
