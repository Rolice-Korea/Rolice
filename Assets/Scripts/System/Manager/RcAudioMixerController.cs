using UnityEngine;
using UnityEngine.Audio;

namespace Rolice.System.Manager
{
    public class RcAudioMixerController
    {
        private AudioMixer _masterMixer;
        private string _masterVolumeParameterName;
        private string _bgmVolumeParameterName;
        private string _sfxVolumeParameterName;

        public void Initialize(AudioMixer masterMixer, string masterParam, string bgmParam, string sfxParam)
        {
            _masterMixer = masterMixer;
            _masterVolumeParameterName = masterParam;
            _bgmVolumeParameterName = bgmParam;
            _sfxVolumeParameterName = sfxParam;
        }

        /// <summary>
        /// Sets the volume of a specific mixer parameter.
        /// Converts a linear volume (0-1) to a logarithmic decibel scale.
        /// </summary>
        /// <param name="parameterName">The name of the exposed parameter in the AudioMixer.</param>
        /// <param name="linearVolume">The linear volume value (0 to 1).</param>
        public void SetVolume(string parameterName, float linearVolume)
        {
            float volumeInDb = -80f; // Minimum volume (near mute)
            if (linearVolume > 0f)
            {
                volumeInDb = Mathf.Log10(linearVolume) * 20f;
            }
            _masterMixer.SetFloat(parameterName, volumeInDb);
        }

        /// <summary>
        /// Gets the volume of a specific mixer parameter.
        /// Converts a logarithmic decibel scale volume to a linear volume (0-1).
        /// </summary>
        /// <param name="parameterName">The name of the exposed parameter in the AudioMixer.</param>
        /// <returns>The linear volume value (0 to 1).</returns>
        public float GetVolume(string parameterName)
        {
            float volumeInDb;
            if (_masterMixer.GetFloat(parameterName, out volumeInDb))
            {
                if (volumeInDb <= -80f)
                {
                    return 0f;
                }
                return Mathf.Pow(10f, volumeInDb / 20f);
            }
            return 0f; // Error or parameter not found
        }
    }
}