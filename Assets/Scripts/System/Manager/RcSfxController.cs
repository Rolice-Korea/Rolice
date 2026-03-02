using UnityEngine;

namespace Rolice.System.Manager
{
    public class RcSfxController : MonoBehaviour
    {
        [Header("Game SFX")]
        [SerializeField] private AudioClip sfxMove;
        [SerializeField] private AudioClip sfxTileMatch;
        [SerializeField] private AudioClip sfxLevelClear;
        [SerializeField] private AudioClip sfxGameLose;

        [Header("UI SFX")]
        [SerializeField] private AudioClip sfxUiClick;

        public static RcSfxController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            RcGameEvents.Instance.Subscribe(RcGameEvent.MoveCompleted,    HandleMoveCompleted);
            RcGameEvents.Instance.Subscribe(RcGameEvent.ColorTileCleared, HandleTileCleared);
            RcGameEvents.Instance.Subscribe(RcGameEvent.LevelCompleted,   HandleLevelCompleted);
            RcGameEvents.Instance.Subscribe(RcGameEvent.GameLose,         HandleGameLose);
        }

        private void OnDisable()
        {
            RcGameEvents.Instance.Unsubscribe(RcGameEvent.MoveCompleted,    HandleMoveCompleted);
            RcGameEvents.Instance.Unsubscribe(RcGameEvent.ColorTileCleared, HandleTileCleared);
            RcGameEvents.Instance.Unsubscribe(RcGameEvent.LevelCompleted,   HandleLevelCompleted);
            RcGameEvents.Instance.Unsubscribe(RcGameEvent.GameLose,         HandleGameLose);
        }

        private void HandleMoveCompleted(Vector2Int _)  => Play(sfxMove);
        private void HandleTileCleared(Vector2Int _)     => Play(sfxTileMatch);
        private void HandleLevelCompleted()              => Play(sfxLevelClear);
        private void HandleGameLose()                    => Play(sfxGameLose);

        public void PlayUiClick() => Play(sfxUiClick);

        private void Play(AudioClip clip)
        {
            if (clip == null) return;
            RcSoundManager.Instance.PlaySFX(clip);
        }
    }
}
