using System;
using Cysharp.Threading.Tasks;
using Engine.UI;
using Rolice.Define;
using Rolice.System;
using Rolice.System.Backend;
using Rolice.System.Economy;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 하트 충전 팝업 프레젠터.
    /// 잼을 소모해 하트를 최대치까지 충전한다 (잼 차감 선행 → 성공 시 하트 증가).
    /// </summary>
    public class RcHeartShopPresenter : RcUIPresenter<RcHeartShopPanel>
    {
        private bool processing;

        protected override void OnInitialize()
        {
            Panel.OnCloseClicked  += HandleClose;
            Panel.OnRefillClicked += HandleRefill;

            RcPlayerState.Instance.OnProgressChanged += Refresh;
            RcHeartManager.Instance.OnChanged        += Refresh;

            Refresh();
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked  -= HandleClose;
            Panel.OnRefillClicked -= HandleRefill;

            RcPlayerState.Instance.OnProgressChanged -= Refresh;
            RcHeartManager.Instance.OnChanged        -= Refresh;
        }

        private void Refresh()
        {
            if (Panel == null) return;

            int hearts = RcHeartManager.Instance.Count;
            int gems   = RcBackendServices.Economy.GetBalance(RcCurrencyId.Gem);
            int cost   = Panel.GemCostPerHeart;
            bool full  = RcHeartManager.Instance.IsFull;

            Panel.SetHeartCount(hearts, RcHeartManager.MaxHearts);
            Panel.SetGemBalance(gems);
            Panel.SetCostText(full ? "가득 참" : $"{cost} 💎");
            Panel.SetRefillInteractable(!processing && !full && gems >= cost);
        }

        private void HandleClose() => Panel.RequestClose();

        private void HandleRefill() => HandleRefillAsync().Forget();

        private async UniTaskVoid HandleRefillAsync()
        {
            if (processing) return;
            if (RcHeartManager.Instance.IsFull) return;

            int cost = Panel.GemCostPerHeart;
            if (RcBackendServices.Economy.GetBalance(RcCurrencyId.Gem) < cost) return;

            processing = true;
            Panel.SetRefillInteractable(false);
            try
            {
                bool spent = await RcBackendServices.Economy.SpendAsync(RcCurrencyId.Gem, cost);
                if (!spent)
                {
                    Debug.Log("[RcHeartShop] 잼 부족");
                    return;
                }

                int added = await RcHeartManager.Instance.RefillAsync(1);

                // 가득 차서 하트가 들어가지 못한 경우 잼 환불 (경합 방어)
                if (added <= 0)
                {
                    await RcBackendServices.Economy.AddAsync(RcCurrencyId.Gem, cost);
                    Debug.LogWarning("[RcHeartShop] 충전 실패(가득 참) → 잼 환불");
                }
                else
                {
                    Debug.Log($"[RcHeartShop] 충전 완료: +{added}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[RcHeartShop] 충전 실패: {e.Message}");
            }
            finally
            {
                processing = false;
                Refresh();
            }
        }
    }
}
