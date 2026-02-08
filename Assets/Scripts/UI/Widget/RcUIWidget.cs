using UnityEngine;

namespace Engine.UI
{
    public abstract class RcUIWidget : MonoBehaviour
    {
        public virtual void Initialize() { }
        public virtual void Cleanup() { }
    }
}
