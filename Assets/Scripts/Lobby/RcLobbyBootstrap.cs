using UnityEngine;

public class RcLobbyBootstrap : MonoBehaviour
{
    private void Start()
    {
        Engine.UI.RcUIManager.Instance.Open<Rolice.UI.RcCurrencyHudPanel>();
        Engine.UI.RcUIManager.Instance.Open<Rolice.UI.RcStageSelectPanel>();
    }
}
