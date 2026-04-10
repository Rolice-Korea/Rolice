using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 클라우드 작업 실패 시 재시도 UI를 표시하고, 성공할 때까지 반복.
/// 어떤 씬에서도 동적으로 생성해 사용 가능.
/// </summary>
public sealed class RcCloudRetryOverlay : MonoBehaviour
{
    private UniTaskCompletionSource _tcs;
    private string                  _message;

    /// <summary>
    /// operation이 true를 반환할 때까지 재시도 UI를 반복 표시.
    /// </summary>
    public static async UniTask ShowUntilSuccessAsync(
        Func<UniTask<bool>> operation,
        string message = "서버 연결에 실패했습니다.\n재시도해 주세요.")
    {
        while (true)
        {
            bool success = await operation();
            if (success) return;

            var go      = new GameObject("CloudRetryOverlay");
            var overlay = go.AddComponent<RcCloudRetryOverlay>();
            overlay._message = message;
            overlay._tcs     = new UniTaskCompletionSource();

            await overlay._tcs.Task;
            Destroy(go);
        }
    }

    private void OnGUI()
    {
        var boxRect    = new Rect(Screen.width / 2f - 200, Screen.height / 2f - 70, 400, 140);
        var labelRect  = new Rect(boxRect.x + 20, boxRect.y + 20,  360, 60);
        var buttonRect = new Rect(boxRect.x + 80, boxRect.y + 90,  240, 40);

        GUI.Box(boxRect, "");
        GUI.Label(labelRect, _message);
        if (GUI.Button(buttonRect, "재시도"))
            _tcs?.TrySetResult();
    }
}
