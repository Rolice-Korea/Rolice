using Engine;
using Engine.UI;

public class RcGameFlowManager : RcSingletonMono<RcGameFlowManager>
{
    private const string LobbySceneName = "LobbyScene";
    private const string MainSceneName = "MainScene";

    private void Awake()
    {
        InitializeSingleton();
    }

    public void StartRandomGame(int gridSize)
    {
        if (RcSceneLoader.Instance.IsLoading) return;

        RcUIManager.Instance.CloseAll();
        RcGameContext.SetGridSize(gridSize);
        RcSceneLoader.Instance.LoadScene(MainSceneName, OnBeforeSceneActivate);
    }

    public void RetryStage()
    {
        if (RcSceneLoader.Instance.IsLoading) return;

        RcUIManager.Instance.CloseAll();
        RcSceneLoader.Instance.LoadScene(MainSceneName, OnBeforeSceneActivate);
    }

    public void GoToLobby()
    {
        if (RcSceneLoader.Instance.IsLoading) return;

        RcUIManager.Instance.CloseAll();
        RcSceneLoader.Instance.LoadScene(LobbySceneName, OnBeforeSceneActivate);
    }

    private void OnBeforeSceneActivate()
    {
        RcUIManager.Instance.DeactivateAll();
    }
}
