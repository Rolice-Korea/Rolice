using Rolice;

namespace Rolice.System
{
    /// <summary>
    /// 플레이어의 스킨 상태를 관리하고 현재 활성화된 스킨 정보를 제공하는 시스템.
    /// </summary>
    public static class RcSkinSystem
    {
        /// <summary>
        /// 현재 선택된 주사위 면(Face) & 타일 스킨 타입 반환.
        /// </summary>
        public static RcFaceSkinType ActiveFaceSkinType
        {
            get
            {
                var state = RcPlayerState.Instance;
                return state != null ? state.Data.SelectedFaceSkin : RcFaceSkinType.Default;
            }
        }

        /// <summary>
        /// 현재 선택된 주사위 테두리(Edge) 스킨 타입 반환.
        /// </summary>
        public static RcEdgeSkinType ActiveEdgeSkinType
        {
            get
            {
                var state = RcPlayerState.Instance;
                return state != null ? state.Data.SelectedEdgeSkin : RcEdgeSkinType.Default;
            }
        }
    }
}
