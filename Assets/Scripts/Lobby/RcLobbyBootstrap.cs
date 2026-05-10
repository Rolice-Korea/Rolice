using Engine.UI;
using Rolice.UI;
using UnityEngine;

public class RcLobbyBootstrap : MonoBehaviour
{
    private void Start()
    {
        RcGameContext.Clear();
        RcUIManager.Instance.Open<RcCurrencyHudPanel>();
        RcUIManager.Instance.Open<RcStageSelectPanel>();
    }
}
