using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WhiskerHaven.Utils;

namespace WhiskerHaven
{
    [System.Serializable]
    public class SFXClip
    {
        public string key;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.8f;
        [Range(0.8f, 1.2f)] public float pitchVariance = 0.05f;
    }

    [System.Serializable]
    public class MusicTrack
    {
        public string key;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.5f;
    }

    public class AudioManager : Singleton<AudioManager>
    {
        [Header("SFX")]
        public SFXClip[] sfxClips;
        [SerializeField] private int sfxSourceCount = 6;

        [Header("Music")]
        public MusicTrack[] musicTracks;
        [SerializeField] private AudioSource musicSourceA;
        [SerializeField] private AudioSource musicSourceB;
        [SerializeField] private float crossfadeDuration = 1.5f;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume    = 0.8f;
        [Range(0f, 1f)] public float musicVolume  = 0.5f;

        private Queue<AudioSource> _sfxPool = new();
        private Dictionary<string, SFXClip>   _sfxLookup   = new();
        private Dictionary<string, MusicTrack> _musicLookup = new();
        private AudioSource _activeMusic;
        private Coroutine _crossfadeRoutine;

        protected override void OnAwake()
        {
            // Build SFX pool
            for (int i = 0; i < sfxSourceCount; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _sfxPool.Enqueue(src);
            }

            // Lookups
            if (sfxClips != null)
                foreach (var s in sfxClips) _sfxLookup[s.key] = s;
            if (musicTracks != null)
                foreach (var m in musicTracks) _musicLookup[m.key] = m;

            // Setup music sources
            if (musicSourceA == null)
            {
                musicSourceA = gameObject.AddComponent<AudioSource>();
                musicSourceA.loop = true; musicSourceA.playOnAwake = false;
            }
            if (musicSourceB == null)
            {
                musicSourceB = gameObject.AddComponent<AudioSource>();
                musicSourceB.loop = true; musicSourceB.playOnAwake = false;
            }
        }

        // ── SFX ──────────────────────────────────────────────────────────────
        public void PlaySFX(string key)
        {
            if (!_sfxLookup.TryGetValue(key, out var sfx) || sfx.clip == null) return;

            AudioSource src = _sfxPool.Count > 0 ? _sfxPool.Dequeue() : GetOrCreateSource();
            src.clip   = sfx.clip;
            src.volume = sfx.volume * sfxVolume * masterVolume;
            src.pitch  = 1f + Random.Range(-sfx.pitchVariance, sfx.pitchVariance);
            src.Play();
            StartCoroutine(ReturnSourceAfterPlay(src, sfx.clip.length + 0.1f));
        }

        private IEnumerator ReturnSourceAfterPlay(AudioSource src, float delay)
        {
            yield return new WaitForSeconds(delay);
            _sfxPool.Enqueue(src);
        }

        private AudioSource GetOrCreateSource()
        {
            return gameObject.AddComponent<AudioSource>();
        }

        // ── Music ─────────────────────────────────────────────────────────────
        public void PlayMusic(string key, bool immediate = false)
        {
            if (!_musicLookup.TryGetValue(key, out var track) || track.clip == null) return;

            var inactive = (_activeMusic == musicSourceA) ? musicSourceB : musicSourceA;
            inactive.clip   = track.clip;
            inactive.volume = 0f;
            inactive.Play();

            if (_crossfadeRoutine != null) StopCoroutine(_crossfadeRoutine);
            _crossfadeRoutine = StartCoroutine(Crossfade(_activeMusic, inactive, track.volume * musicVolume * masterVolume));
            _activeMusic = inactive;
        }

        private IEnumerator Crossfade(AudioSource fadeOut, AudioSource fadeIn, float targetVol)
        {
            float elapsed = 0f;
            float startVol = fadeOut != null ? fadeOut.volume : 0f;

            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / crossfadeDuration;
                if (fadeOut != null) fadeOut.volume = Mathf.Lerp(startVol, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, targetVol, t);
                yield return null;
            }

            if (fadeOut != null) { fadeOut.Stop(); fadeOut.volume = 0f; }
            fadeIn.volume = targetVol;
        }

        public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); RefreshMusicVolume(); }
        public void SetSFXVolume(float v)    { sfxVolume    = Mathf.Clamp01(v); }
        public void SetMusicVolume(float v)  { musicVolume  = Mathf.Clamp01(v); RefreshMusicVolume(); }

        private void RefreshMusicVolume()
        {
            if (_activeMusic != null)
                _activeMusic.volume = musicVolume * masterVolume;
        }

        public void StopMusic()
        {
            if (_crossfadeRoutine != null) StopCoroutine(_crossfadeRoutine);
            musicSourceA.Stop();
            musicSourceB.Stop();
        }
    }
}
