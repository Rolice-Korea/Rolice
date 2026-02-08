using Rolice.Data;

namespace Rolice.System
{
    /// <summary>
    /// 저장 시스템 인터페이스
    /// </summary>
    public interface IRcSaveSystem
    {
        /// <summary>
        /// 플레이어 데이터 저장
        /// </summary>
        void Save(RcPlayerData data);

        /// <summary>
        /// 플레이어 데이터 로드
        /// </summary>
        RcPlayerData Load();

        /// <summary>
        /// 저장 데이터 존재 여부
        /// </summary>
        bool HasSaveData();

        /// <summary>
        /// 저장 데이터 삭제
        /// </summary>
        void Delete();
    }
}
