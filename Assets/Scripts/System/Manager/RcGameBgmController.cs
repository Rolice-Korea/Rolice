using UnityEngine;

namespace Rolice.System.Manager
{
    public class RcGameBgmController : MonoBehaviour
    {
        [Header("Level Clear")]
        [SerializeField] private AudioClip _levelClearBGM;
        [SerializeField] private float _levelClearFadeOut = 0.5f;
        [SerializeField] private float _levelClearFadeIn  = 3.0f;

        [Header("Lose")]
        [SerializeField] private float _loseFadeOut = 2.0f;

        private void OnEnable()
        {
            if (_levelClearBGM != null)
                RcGameEvents.Instance.Subscribe(RcGameEvent.LevelCompleted, HandleLevelCompleted);

            RcGameEvents.Instance.Subscribe(RcGameEvent.GameLose, HandleGameLose);
        }

        private void OnDisable()
        {
            if (_levelClearBGM != null)
                RcGameEvents.Instance.Unsubscribe(RcGameEvent.LevelCompleted, HandleLevelCompleted);

            RcGameEvents.Instance.Unsubscribe(RcGameEvent.GameLose, HandleGameLose);
        }

        private void HandleLevelCompleted()
        {
            RcSoundManager.Instance.TransitionBGM(_levelClearBGM, _levelClearFadeOut, _levelClearFadeIn);
        }

        private void HandleGameLose()
        {
            RcSoundManager.Instance.StopBGM(_loseFadeOut);
        }
    }
}
