using Engine.UI;

namespace Rolice.UI
{
    public class RcGameHudPresenter : RcUIPresenter<RcGameHudPanel>
    {
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
        }

        private void OnTurnChanged(int currentTurn)
        {
            var ruleManager = RcGameRuleManager.Instance;

            int remainingTurns = ruleManager.GetRemainingTurns();
            Panel.SetRemainingTurns(remainingTurns >= 0 ? remainingTurns : 0);
        }
    }
}
