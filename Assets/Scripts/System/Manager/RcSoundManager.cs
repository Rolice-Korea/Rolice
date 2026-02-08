using System;
using System.Collections;
using System.Collections.Generic;
using Engine;
using UnityEngine;
using UnityEngine.Audio;

namespace Rolice.System.Manager
{
    // Make sure to implement ISingletonMonoInterface if necessary, or adjust base class if not.
    // Assuming RcSingletonMono handles the interface internally.
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
        [SerializeField] private float _fadeDuration = 1.0f; // Default fade duration
        [SerializeField] private int _maxSfxPlayers = 10; // Max SFX players for the pool

        private RcAudioPlayer _bgmAudioPlayer;
        private List<RcAudioPlayer> _allSfxPlayers = new List<RcAudioPlayer>();
        private Queue<RcAudioPlayer> _availableSfxPlayers = new Queue<RcAudioPlayer>();
        private RcAudioMixerController _audioMixerController;

        private void Awake()
        {
            InitializeSingleton();
        }

        // Called when the singleton is initialized
        public override void InitializeSingleton() 
        {
            base.InitializeSingleton();
            
            if (_masterMixer == null || _bgmMixerGroup == null || _sfxMixerGroup == null)
            {
                Debug.LogError("RcSoundManager: AudioMixer or AudioMixerGroups are not assigned. Please assign them in the Inspector.");
                return;
            }

            // Initialize AudioMixerController
            _audioMixerController = new RcAudioMixerController();
            _audioMixerController.Initialize(_masterMixer, _masterVolumeParameterName, _bgmVolumeParameterName, _sfxVolumeParameterName);

            // Initialize BGM player
            GameObject bgmPlayerGO = new GameObject("BGM_Player");
            bgmPlayerGO.transform.SetParent(this.transform);
            _bgmAudioPlayer = bgmPlayerGO.AddComponent<RcAudioPlayer>();
            _bgmAudioPlayer.Setup(_bgmMixerGroup);

            // Initialize SFX player pool
            for (int i = 0; i < _maxSfxPlayers; i++)
            {
                GameObject sfxPlayerGO = new GameObject($"SFX_Player_{i}");
                sfxPlayerGO.transform.SetParent(this.transform);
                RcAudioPlayer sfxPlayer = sfxPlayerGO.AddComponent<RcAudioPlayer>();
                sfxPlayer.Setup(_sfxMixerGroup);
                sfxPlayer.gameObject.SetActive(false); // Deactivate until needed
                sfxPlayer.OnFinishedPlaying += () => ReturnSfxPlayerToPool(sfxPlayer);
                _allSfxPlayers.Add(sfxPlayer);
                _availableSfxPlayers.Enqueue(sfxPlayer);
            }

            Debug.Log("RcSoundManager initialized.");

            // Play default BGM if assigned
            if (_defaultBGM != null)
            {
                PlayBGM(_defaultBGM);
            }
        }

        /// <summary>
        /// Plays an SFX clip.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
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

            player.gameObject.SetActive(true); // Activate the player
            player.Play(clip);
        }

        /// <summary>
        /// Plays a BGM clip with optional fade-in.
        /// </summary>
        /// <param name="clip">The AudioClip to play as BGM.</param>
        /// <param name="fadeDuration">The duration of the fade effect. Uses default if not specified.</param>
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
                    Debug.Log("Attempted to play the same BGM that is already playing.");
                    return; // Already playing this BGM
                }

                // Stop current BGM with fade-out
                _bgmAudioPlayer.StopWithFade(currentFadeDuration);
                // Wait for fade-out to complete before starting new BGM fade-in,
                // or just start new BGM fade-in immediately if overlapping is acceptable.
                // For now, simple transition without waiting.
            }
            _bgmAudioPlayer.PlayWithFade(clip, currentFadeDuration, loop: true);
        }

        /// <summary>
        /// Stops the current BGM with optional fade-out.
        /// </summary>
        /// <param name="fadeDuration">The duration of the fade effect. Uses default if not specified.</param>
        public void StopBGM(float? fadeDuration = null)
        {
            if (!_bgmAudioPlayer.IsPlaying())
            {
                Debug.Log("No BGM is currently playing to stop.");
                return;
            }

            float currentFadeDuration = fadeDuration ?? _fadeDuration;
            _bgmAudioPlayer.StopWithFade(currentFadeDuration);
        }

        /// <summary>
        /// Transitions to a new BGM with fade-out and fade-in effects.
        /// </summary>
        /// <param name="newClip">The new AudioClip for BGM.</param>
        /// <param name="fadeOutDuration">Duration of the fade-out for the current BGM.</param>
        /// <param name="fadeInDuration">Duration of the fade-in for the new BGM.</param>
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
                yield return _bgmAudioPlayer.StopWithFade(fadeOutDuration); // Wait for fade-out
            }

            _bgmAudioPlayer.PlayWithFade(newClip, fadeInDuration, loop: true);
        }


        /// <summary>
        /// Sets the master volume using the AudioMixer.
        /// </summary>
        /// <param name="volume">Linear volume (0 to 1).</param>
        public void SetMasterVolume(float volume)
        {
            _audioMixerController.SetVolume(_masterVolumeParameterName, volume);
        }

        /// <summary>
        /// Sets the BGM volume using the AudioMixer.
        /// </summary>
        /// <param name="volume">Linear volume (0 to 1).</param>
        public void SetBGMVolume(float volume)
        {
            _audioMixerController.SetVolume(_bgmVolumeParameterName, volume);
        }

        /// <summary>
        /// Sets the SFX volume using the AudioMixer.
        /// </summary>
        /// <param name="volume">Linear volume (0 to 1).</param>
        public void SetSFXVolume(float volume)
        {
            _audioMixerController.SetVolume(_sfxVolumeParameterName, volume);
        }

        /// <summary>
        /// Retrieves an available SFX player from the pool.
        /// Prioritizes truly inactive players. If none are available, it reuses the oldest active player.
        /// </summary>
        /// <returns>An available RcAudioPlayer, or null if no player can be retrieved/reused.</returns>
        private RcAudioPlayer GetAvailableSfxPlayer()
        {
            RcAudioPlayer player = null;

            // 1. Try to get a truly available (inactive) player from the queue
            if (_availableSfxPlayers.Count > 0)
            {
                player = _availableSfxPlayers.Dequeue();
            }

            // 2. If no inactive player is found, all players must be currently active.
            // We need to implement the "stop oldest" logic from the pseudocode here.
            if (player == null)
            {
                RcAudioPlayer oldestActivePlayer = null;

                // Iterate through all players to find the oldest currently playing one
                // For simplicity, we can just grab the first one that is active and playing.
                foreach (var p in _allSfxPlayers)
                {
                    if (p.gameObject.activeSelf && p.IsPlaying())
                    {
                        oldestActivePlayer = p;
                        break; // Found a player to interrupt
                    }
                }

                if (oldestActivePlayer != null)
                {
                    oldestActivePlayer.Stop(); // Interrupt the oldest playing SFX (this will trigger OnFinishedPlaying)
                    // The player will be returned to _availableSfxPlayers via the OnFinishedPlaying subscription.
                    // So we can dequeue it immediately.
                    player = _availableSfxPlayers.Dequeue(); // Get the just-returned player
                }
                else
                {
                    // This scenario should ideally not happen if pooling is correctly managed
                    // by OnFinishedPlaying events and _availableSfxPlayers tracks truly available ones.
                    Debug.LogWarning("SFX Pool logic issue: _availableSfxPlayers is empty but no active players found to interrupt.");
                    return null;
                }
            }

            return player;
        }

        /// <summary>
        /// Called by RcAudioPlayer when it finishes playing an SFX.
        /// Returns the player to the available pool and deactivates its GameObject.
        /// </summary>
        /// <param name="player">The RcAudioPlayer that finished playing.</param>
        private void ReturnSfxPlayerToPool(RcAudioPlayer player)
        {
            player.gameObject.SetActive(false);
            player.GetAudioSource().clip = null; // Ensure clip is nullified
            _availableSfxPlayers.Enqueue(player);
        }
    }
}
