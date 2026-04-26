using System;
using Cysharp.Threading.Tasks;
using Engine;
using Engine.UI;
using Rolice.UI;
using UnityEngine;

namespace Rolice.System
{
    public class RcSystemDialogManager : RcSingletonMono<RcSystemDialogManager>
    {
        private void Awake() => InitializeSingleton();

#if UNITY_EDITOR
        [ContextMenu("Test: Show Retry Dialog")]
        private void TestShowRetryDialog()
        {
            RcUIManager.Instance.Open<RcRetryDialogPanel, string>("Connection failed.\nPlease retry.");
        }
#endif

        public async UniTask ShowUntilSuccessAsync(
            Func<UniTask> operation,
            string message = "Connection failed.\nPlease retry.")
        {
            while (true)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SystemDialog] 재시도 대기: {e.Message}");
                }

                var panel = RcUIManager.Instance.Open<RcRetryDialogPanel, string>(message);
                await panel.WaitForRetryAsync();
                RcUIManager.Instance.Close<RcRetryDialogPanel>();
            }
        }
    }
}
