namespace Engine.UI
{
    public enum RcUILayer
    {
        HUD             = 0,
        Main            = 100,
        Popup           = 200,
        Overlay         = 300,
        AbsoluteOverlay = 400,  // 항상 ScreenSpaceOverlay — 블러/PP 영향 없음
    }
}
