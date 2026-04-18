using UnityEngine;

namespace Rolice.System
{
    [CreateAssetMenu(fileName = "DeveloperSettings", menuName = "Rolice/Developer Settings")]
    public class RcDeveloperSettingsSO : ScriptableObject
    {
        [Header("Backend")]
        [Tooltip("서버 연결 없이 NullBackend로 실행")]
        public bool offlineMode;

        [Tooltip("오프라인 모드에서 인증/소비/광고 등 모두 성공으로 처리")]
        public bool backendAlwaysSucceed = true;
    }
}
