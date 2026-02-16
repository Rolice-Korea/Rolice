using System;
using System.Collections;
using DG.Tweening;
using Engine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RcSceneLoader : RcSingletonMono<RcSceneLoader>
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float minimumLoadTime = 0.8f;

    private bool isLoading;

    public bool IsLoading => isLoading;

    private void Awake()
    {
        InitializeSingleton();
    }

    public void LoadScene(string sceneName, Action onBeforeActivate = null)
    {
        if (isLoading)
        {
            Debug.LogWarning($"[SceneLoader] 이미 로딩 중입니다. {sceneName} 요청 무시.");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName, onBeforeActivate));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, Action onBeforeActivate)
    {
        isLoading = true;
        float startTime = Time.unscaledTime;

        var fadeOutTween = RcScreenFader.Instance.FadeOut(fadeOutDuration);
        yield return fadeOutTween.WaitForCompletion(true);

        var asyncOp = SceneManager.LoadSceneAsync(sceneName);
        asyncOp.allowSceneActivation = false;

        while (asyncOp.progress < 0.9f)
            yield return null;

        float elapsed = Time.unscaledTime - startTime;
        if (elapsed < minimumLoadTime)
            yield return new WaitForSecondsRealtime(minimumLoadTime - elapsed);

        onBeforeActivate?.Invoke();
        asyncOp.allowSceneActivation = true;

        while (!asyncOp.isDone)
            yield return null;

        // 씬 Awake/Start 완료 보장
        yield return null;

        var fadeInTween = RcScreenFader.Instance.FadeIn(fadeInDuration);
        yield return fadeInTween.WaitForCompletion(true);

        isLoading = false;
    }
}
