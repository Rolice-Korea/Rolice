using UnityEngine;

namespace Rolice.System.Manager
{
    public class RcBgmController : MonoBehaviour
    {
        [SerializeField] private AudioClip _startBGM;

        private void Start()
        {
            if (_startBGM != null)
                RcSoundManager.Instance.PlayBGM(_startBGM);
        }
    }
}
