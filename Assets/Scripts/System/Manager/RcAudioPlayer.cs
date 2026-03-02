using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Rolice.System.Manager
{
    public class RcAudioPlayer : MonoBehaviour
    {
        private AudioSource _audioSource;
        private float _originalVolume = 1.0f;
        private Coroutine _fadeCoroutine;
        public Action OnFinishedPlaying;

        public void Setup(AudioMixerGroup mixerGroup)
        {
            _audioSource = this.gameObject.AddComponent<AudioSource>();
            _audioSource.outputAudioMixerGroup = mixerGroup;
            _audioSource.playOnAwake = false;
        }

        public void Play(AudioClip clip, bool loop = false)
        {
            StopFade();
            _audioSource.clip = clip;
            _audioSource.loop = loop;
            _audioSource.volume = _originalVolume;
            _audioSource.Play();
            if (!loop)
            {
                StartCoroutine(Co_WaitForSoundToFinish());
            }
        }

        public void PlayWithFade(AudioClip clip, float duration, bool loop = false)
        {
            StopFade();
            _audioSource.clip = clip;
            _audioSource.loop = loop;
            _audioSource.volume = 0f;
            _audioSource.Play();
            _fadeCoroutine = StartCoroutine(Co_Fade(_audioSource, _originalVolume, duration));
            if (!loop)
            {
                StartCoroutine(Co_WaitForSoundToFinish());
            }
        }

        public void Stop()
        {
            StopFade();
            _audioSource.Stop();
            OnFinishedPlaying?.Invoke();
        }

        public IEnumerator StopWithFade(float duration)
        {
            if (_audioSource.isPlaying)
            {
                StopFade();
                yield return _fadeCoroutine = StartCoroutine(Co_Fade(_audioSource, 0f, duration, stopAfterFade: true));
            }
            else
            {
                OnFinishedPlaying?.Invoke();
            }
        }

        public void SetVolume(float volume)
        {
            _originalVolume = volume;
            if (_fadeCoroutine == null)
            {
                _audioSource.volume = volume;
            }
        }

        public bool IsPlaying()
        {
            return _audioSource.isPlaying;
        }

        public AudioSource GetAudioSource()
        {
            return _audioSource;
        }

        private void StopFade()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

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
            audioSource.volume = targetVolume;

            if (stopAfterFade && targetVolume == 0f)
            {
                audioSource.Stop();
                audioSource.clip = null;
                OnFinishedPlaying?.Invoke();
            }
            _fadeCoroutine = null;
        }

        private IEnumerator Co_WaitForSoundToFinish()
        {
            yield return new WaitWhile(() => _audioSource.isPlaying);
            if (_fadeCoroutine == null)
            {
                OnFinishedPlaying?.Invoke();
            }
            _audioSource.clip = null;
        }
    }
}
