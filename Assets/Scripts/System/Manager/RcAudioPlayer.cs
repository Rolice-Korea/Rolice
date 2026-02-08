using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Rolice.System.Manager
{
    public class RcAudioPlayer : MonoBehaviour
    {
        private AudioSource _audioSource;
        private float _originalVolume = 1.0f; // Stores the initial volume
        private Coroutine _fadeCoroutine; // Reference to the fade coroutine
        public Action OnFinishedPlaying;

        /// <summary>
        /// Initializes the AudioSource with the given mixer group.
        /// </summary>
        /// <param name="mixerGroup">The AudioMixerGroup to assign to this AudioSource.</param>
        public void Setup(AudioMixerGroup mixerGroup)
        {
            _audioSource = this.gameObject.AddComponent<AudioSource>();
            _audioSource.outputAudioMixerGroup = mixerGroup;
            _audioSource.playOnAwake = false;
        }

        /// <summary>
        /// Plays an AudioClip.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        /// <param name="loop">Whether the audio should loop.</param>
                    public void Play(AudioClip clip, bool loop = false)
                    {
                        StopFade(); // Stop any existing fade
                        _audioSource.clip = clip;
                        _audioSource.loop = loop;
                        _audioSource.volume = _originalVolume; // Set to original volume
                        _audioSource.Play();
                        if (!loop)
                        {
                            StartCoroutine(Co_WaitForSoundToFinish());
                        }
                    }
        /// <summary>
        /// Plays an AudioClip with a fade-in effect.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        /// <param name="duration">The duration of the fade-in effect.</param>
        /// <param name="loop">Whether the audio should loop.</param>
                    public void PlayWithFade(AudioClip clip, float duration, bool loop = false)
                    {
                        StopFade(); // Stop any existing fade
                        _audioSource.clip = clip;
                        _audioSource.loop = loop;
                        _audioSource.volume = 0f; // Start from 0 volume
                        _audioSource.Play();
                        _fadeCoroutine = StartCoroutine(Co_Fade(_audioSource, _originalVolume, duration));
                        if (!loop)
                        {
                            StartCoroutine(Co_WaitForSoundToFinish());
                        }
                    }
        /// <summary>
        /// Stops the current AudioClip.
        /// </summary>
                    public void Stop()
                    {
                        StopFade(); // Stop any existing fade
                        _audioSource.Stop();
                        OnFinishedPlaying?.Invoke();
                    }
        /// <summary>
        /// Stops the current AudioClip with a fade-out effect.
        /// </summary>
        /// <param name="duration">The duration of the fade-out effect.</param>
                    public IEnumerator StopWithFade(float duration)
                    {
                        if (_audioSource.isPlaying)
                        {
                            StopFade(); // Stop any existing fade
                            yield return _fadeCoroutine = StartCoroutine(Co_Fade(_audioSource, 0f, duration, stopAfterFade: true));
                        }
                        else
                        {
                            OnFinishedPlaying?.Invoke(); // Already stopped, just invoke the action
                        }
                    }
        /// <summary>
        /// Sets the volume of the AudioSource.
        /// </summary>
        /// <param name="volume">The volume level (0 to 1).</param>
        public void SetVolume(float volume)
        {
            _originalVolume = volume; // Update original volume
            if (_fadeCoroutine == null) // If not currently fading, apply immediately
            {
                _audioSource.volume = volume;
            }
        }

        /// <summary>
        /// Checks if the AudioSource is currently playing.
        /// </summary>
        /// <returns>True if playing, false otherwise.</returns>
        public bool IsPlaying()
        {
            return _audioSource.isPlaying;
        }

        /// <summary>
        /// Returns the internal AudioSource component.
        /// </summary>
        public AudioSource GetAudioSource()
        {
            return _audioSource;
        }

        /// <summary>
        /// Internal method to stop any active fade coroutine.
        /// </summary>
        private void StopFade()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        /// <summary>
        /// Coroutine for fading the audio volume.
        /// </summary>
        private IEnumerator Co_Fade(AudioSource audioSource, float targetVolume, float duration, bool stopAfterFade = false)
        {
            float startVolume = audioSource.volume;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
                yield return null;
            }
            audioSource.volume = targetVolume; // Ensure target volume is set precisely

            if (stopAfterFade && targetVolume == 0f)
            {
                audioSource.Stop();
                audioSource.clip = null; // Release the clip
                OnFinishedPlaying?.Invoke();
            }
            _fadeCoroutine = null; // Mark coroutine as finished
        }
            /// <summary>
        /// Coroutine to wait for a non-looping sound to finish playing naturally.
        /// </summary>
        private IEnumerator Co_WaitForSoundToFinish()
        {
            // Wait until the audio source is no longer playing
            yield return new WaitWhile(() => _audioSource.isPlaying);
            // Ensure OnFinishedPlaying is not invoked twice if fade-out also invokes it
            if (_fadeCoroutine == null) // Only if not currently fading out
            {
                OnFinishedPlaying?.Invoke();
            }
            _audioSource.clip = null; // Release the clip
        }
    }
}