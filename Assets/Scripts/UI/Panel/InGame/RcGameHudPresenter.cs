using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    public class RcGameHudPresenter : RcUIPresenter<RcGameHudPanel>
    {
        private int lastRemainingTurns = -1;

        protected override void OnInitialize()
        {
            RcGameEvents.Instance.Subscribe(RcGameEvent.TurnChanged, OnTurnChanged);

            InitializeDisplay();
        }

        protected override void OnDispose()
        {
            RcGameEvents.Instance.Unsubscribe(RcGameEvent.TurnChanged, OnTurnChanged);
        }

        private void InitializeDisplay()
        {
            var ruleManager = RcGameRuleManager.Instance;
            if (!ruleManager.IsInitialized) return;

            int remainingTurns = ruleManager.GetRemainingTurns();
            Panel.SetRemainingTurns(remainingTurns >= 0 ? remainingTurns : 0);
            lastRemainingTurns = remainingTurns;
        }

        private void OnTurnChanged(int currentTurn)
        {
            var ruleManager = RcGameRuleManager.Instance;

            int remainingTurns = ruleManager.GetRemainingTurns();
            Panel.SetRemainingTurns(remainingTurns >= 0 ? remainingTurns : 0);

            if (lastRemainingTurns > 0 && remainingTurns <= 0)
            {
                Panel.PlayGameOverAnimation();
            }

            lastRemainingTurns = remainingTurns;
        }
    }
}
