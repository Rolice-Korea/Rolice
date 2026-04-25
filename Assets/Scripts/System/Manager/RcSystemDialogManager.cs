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

        public async UniTask ShowUntilSuccessAsync(
            Func<UniTask> operation,
            string message = "서버 연결에 실패했습니다.\n재시도해 주세요.")
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
