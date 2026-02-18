using System;
using UnityEngine;

namespace Engine.UI
{
    public abstract class RcUIPanel : MonoBehaviour
    {
        private RcTweenAnimator animator;

        public bool IsOpen { get; private set; }

        protected virtual void Awake()
        {
            if (GetComponent<CanvasGroup>() == null)
                gameObject.AddComponent<CanvasGroup>();

            animator = GetComponent<RcTweenAnimator>();
        }

        public void Open()
        {
            if (IsOpen) return;

            IsOpen = true;
            
            gameObject.SetActive(true);
            OnOpen();

            if (animator != null)
            {
                animator.OnComplete = null;
                animator.PlayOnOpen();
            }
        }

        // 애니메이션이 있으면 완료 후 비활성화, 없으면 즉시 비활성화
        public void Close(Action onComplete = null)
        {
            if (!IsOpen) return;

            IsOpen = false;
            OnBeforeClose();

            if (animator != null)
            {
                animator.OnComplete = () =>
                {
                    FinishClose();
                    onComplete?.Invoke();
                };
                animator.PlayOnClose();
            }
            else
            {
                FinishClose();
                onComplete?.Invoke();
            }
        }

        public void CloseImmediate()
        {
            if (!IsOpen) return;

            IsOpen = false;
            OnBeforeClose();

            if (animator != null)
                animator.Stop();

            FinishClose();
        }

        // 애니메이션 재생만 하고 비활성화는 하지 않음 (씬 전환 시 사용)
        public void CloseAnimated()
        {
            if (!IsOpen) return;

            IsOpen = false;
            OnBeforeClose();

            if (animator != null)
            {
                animator.OnComplete = null;
                animator.PlayOnClose();
            }
        }

        public void Deactivate()
        {
            if (animator != null)
                animator.Stop();

            FinishClose();
        }

        private void FinishClose()
        {
            OnClosed();
            gameObject.SetActive(false);
        }

        protected virtual void OnOpen() { }
        protected virtual void OnBeforeClose() { }
        protected virtual void OnClosed() { }
    }

    public abstract class RcUIPanel<TData> : RcUIPanel
    {
        public TData Data { get; private set; }

        public void Open(TData data)
        {
            Data = data;
            base.Open();
        }
    }
}
