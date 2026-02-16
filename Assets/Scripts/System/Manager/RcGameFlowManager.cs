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

    public void GoToStage(int stageNumber)
    {
        if (RcSceneLoader.Instance.IsLoading) return;

        RcUIManager.Instance.CloseAll();
        RcGameContext.SetStage(stageNumber);
        RcSceneLoader.Instance.LoadScene(MainSceneName);
    }

    public void RetryStage()
    {
        if (RcSceneLoader.Instance.IsLoading) return;

        RcUIManager.Instance.CloseAll();
        RcSceneLoader.Instance.LoadScene(MainSceneName);
    }

    public void GoToNextStage()
    {
        if (RcSceneLoader.Instance.IsLoading) return;

        int next = RcGameContext.SelectedStageNumber + 1;

        RcUIManager.Instance.CloseAll();
        RcGameContext.SetStage(next);
        RcSceneLoader.Instance.LoadScene(MainSceneName);
    }

    public void GoToLobby()
    {
        if (RcSceneLoader.Instance.IsLoading) return;

        RcUIManager.Instance.CloseAll();
        RcSceneLoader.Instance.LoadScene(LobbySceneName);
    }
}
