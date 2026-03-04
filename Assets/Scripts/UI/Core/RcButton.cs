using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Engine.UI
{
    public class RcButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private RcTweenAnimator tweenAnimator;
        [SerializeField] private bool interactable = true;

        public event Action OnClick;
        public event Action OnDown;
        public event Action OnUp;

        public bool Interactable
        {
            get => interactable;
            set => interactable = value;
        }

        private void OnDisable()
        {
            tweenAnimator?.PlayInstant("OnRelease");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;
            tweenAnimator?.Play("OnPress");
            OnDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable) return;
            tweenAnimator?.Play("OnRelease");
            OnUp?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;
            OnClick?.Invoke();
        }
    }
}
