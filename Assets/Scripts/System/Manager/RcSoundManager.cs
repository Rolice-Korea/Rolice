using System;
using System.Collections;
using System.Collections.Generic;
using Engine;
using UnityEngine;
using UnityEngine.Audio;

namespace Rolice.System.Manager
{
    public class RcSoundManager : RcSingletonMono<RcSoundManager>
    {
        [Header("Audio Mixer Settings")]
        [SerializeField] private AudioMixer _masterMixer;
        [SerializeField] private AudioMixerGroup _bgmMixerGroup;
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;
        [SerializeField] private string _masterVolumeParameterName = "MasterVolume";
        [SerializeField] private string _bgmVolumeParameterName = "BGMVolume";
        [SerializeField] private string _sfxVolumeParameterName = "SFXVolume";

        [Header("Default BGM")]
        [SerializeField] private AudioClip _defaultBGM;

        [Header("Sound Settings")]
        [SerializeField] private float _fadeDuration = 1.0f;
        [SerializeField] private int _maxSfxPlayers = 10;

        private RcAudioPlayer _bgmAudioPlayer;
        private List<RcAudioPlayer> _allSfxPlayers = new List<RcAudioPlayer>();
        private Queue<RcAudioPlayer> _availableSfxPlayers = new Queue<RcAudioPlayer>();
        private RcAudioMixerController _audioMixerController;

        private void Awake()
        {
            InitializeSingleton();
        }

        public override void InitializeSingleton() 
        {
            base.InitializeSingleton();
            
            if (_masterMixer == null || _bgmMixerGroup == null || _sfxMixerGroup == null)
            {
                Debug.LogError("RcSoundManager: AudioMixer or AudioMixerGroups are not assigned. Please assign them in the Inspector.");
                return;
            }

            _audioMixerController = new RcAudioMixerController();
            _audioMixerController.Initialize(_masterMixer, _masterVolumeParameterName, _bgmVolumeParameterName, _sfxVolumeParameterName);

            GameObject bgmPlayerGO = new GameObject("BGM_Player");
            bgmPlayerGO.transform.SetParent(this.transform);
            _bgmAudioPlayer = bgmPlayerGO.AddComponent<RcAudioPlayer>();
            _bgmAudioPlayer.Setup(_bgmMixerGroup);

            for (int i = 0; i < _maxSfxPlayers; i++)
            {
                GameObject sfxPlayerGO = new GameObject($"SFX_Player_{i}");
                sfxPlayerGO.transform.SetParent(this.transform);
                RcAudioPlayer sfxPlayer = sfxPlayerGO.AddComponent<RcAudioPlayer>();
                sfxPlayer.Setup(_sfxMixerGroup);
                sfxPlayer.gameObject.SetActive(false);
                sfxPlayer.OnFinishedPlaying += () => ReturnSfxPlayerToPool(sfxPlayer);
                _allSfxPlayers.Add(sfxPlayer);
                _availableSfxPlayers.Enqueue(sfxPlayer);
            }

            if (_defaultBGM != null)
            {
                PlayBGM(_defaultBGM);
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("Attempted to play null SFX clip.");
                return;
            }

            RcAudioPlayer player = GetAvailableSfxPlayer();

            if (player == null)
            {
                Debug.LogWarning("No available SFX players in the pool. Consider increasing _maxSfxPlayers.");
                return;
            }

            player.gameObject.SetActive(true);
            player.Play(clip);
        }

        public void PlayBGM(AudioClip clip, float? fadeDuration = null)
        {
            if (clip == null)
            {
                Debug.LogWarning("Attempted to play null BGM clip.");
                return;
            }

            float currentFadeDuration = fadeDuration ?? _fadeDuration;

            if (_bgmAudioPlayer.IsPlaying())
            {
                if (_bgmAudioPlayer.GetAudioSource().clip == clip)
                {
                    return;
                }

                _bgmAudioPlayer.StopWithFade(currentFadeDuration);
            }
            _bgmAudioPlayer.PlayWithFade(clip, currentFadeDuration, loop: true);
        }

        public void StopBGM(float? fadeDuration = null)
        {
            if (!_bgmAudioPlayer.IsPlaying())
            {
                return;
            }

            float currentFadeDuration = fadeDuration ?? _fadeDuration;
            _bgmAudioPlayer.StopWithFade(currentFadeDuration);
        }

        public void TransitionBGM(AudioClip newClip, float? fadeOutDuration = null, float? fadeInDuration = null)
        {
            if (newClip == null)
            {
                Debug.LogWarning("Attempted to transition to null BGM clip.");
                return;
            }

            float currentFadeOutDuration = fadeOutDuration ?? _fadeDuration;
            float currentFadeInDuration = fadeInDuration ?? _fadeDuration;

            StartCoroutine(Co_TransitionBGM(newClip, currentFadeOutDuration, currentFadeInDuration));
        }

        private IEnumerator Co_TransitionBGM(AudioClip newClip, float fadeOutDuration, float fadeInDuration)
        {
            if (_bgmAudioPlayer.IsPlaying())
            {
                yield return _bgmAudioPlayer.StopWithFade(fadeOutDuration);
            }

            _bgmAudioPlayer.PlayWithFade(newClip, fadeInDuration, loop: true);
        }

        public void SetMasterVolume(float volume)
        {
            _audioMixerController.SetVolume(_masterVolumeParameterName, volume);
        }

        public void SetBGMVolume(float volume)
        {
            _audioMixerController.SetVolume(_bgmVolumeParameterName, volume);
        }

        public void SetSFXVolume(float volume)
        {
            _audioMixerController.SetVolume(_sfxVolumeParameterName, volume);
        }

        private RcAudioPlayer GetAvailableSfxPlayer()
        {
            RcAudioPlayer player = null;

            if (_availableSfxPlayers.Count > 0)
            {
                player = _availableSfxPlayers.Dequeue();
            }

            if (player == null)
            {
                RcAudioPlayer oldestActivePlayer = null;

                foreach (var p in _allSfxPlayers)
                {
                    if (p.gameObject.activeSelf && p.IsPlaying())
                    {
                        oldestActivePlayer = p;
                        break;
                    }
                }

                if (oldestActivePlayer != null)
                {
                    oldestActivePlayer.Stop();
                    player = _availableSfxPlayers.Dequeue();
                }
                else
                {
                    Debug.LogWarning("SFX Pool logic issue: _availableSfxPlayers is empty but no active players found to interrupt.");
                    return null;
                }
            }

            return player;
        }

        private void ReturnSfxPlayerToPool(RcAudioPlayer player)
        {
            player.gameObject.SetActive(false);
            player.GetAudioSource().clip = null;
            _availableSfxPlayers.Enqueue(player);
        }
    }
}
