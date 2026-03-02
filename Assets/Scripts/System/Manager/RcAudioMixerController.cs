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

        public void SetVolume(string parameterName, float linearVolume)
        {
            float volumeInDb = -80f;
            if (linearVolume > 0f)
            {
                volumeInDb = Mathf.Log10(linearVolume) * 20f;
            }
            _masterMixer.SetFloat(parameterName, volumeInDb);
        }

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
            return 0f;
        }
    }
}
