namespace Engine.UI
{
    public abstract class RcUIPresenter<TPanel> where TPanel : RcUIPanel
    {
        protected TPanel Panel { get; private set; }

        public void Bind(TPanel panel)
        {
            Panel = panel;
            OnInitialize();
        }

        public void Unbind()
        {
            OnDispose();
            Panel = null;
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnDispose() { }
    }
}
