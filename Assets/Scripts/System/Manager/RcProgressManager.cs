using Engine;
using Rolice.Data;
using Rolice.System;
using UnityEngine;

/// <summary>
/// 플레이어 진행 상황 관리
/// </summary>
public class RcProgressManager : RcSingleton<RcProgressManager>
{
    private IRcSaveSystem _saveSystem;
    private RcPlayerData _playerData;
    private RcStageDatabaseSO _stageDatabase;

    public bool IsInitialized { get; private set; }

    /// <summary>
    /// 초기화 (게임 시작 시 호출)
    /// </summary>
    public void Initialize(RcStageDatabaseSO stageDatabase)
    {
        _stageDatabase = stageDatabase;
        _saveSystem = new RcJsonSaveSystem();
        _playerData = _saveSystem.Load();
        IsInitialized = true;

        Debug.Log($"[ProgressManager] 초기화 완료 - 스테이지 {_stageDatabase.StageCount}개");
    }

    /// <summary>
    /// 스테이지 잠금 해제 여부
    /// </summary>
    public bool IsStageUnlocked(int stageNumber)
    {
        // 첫 번째 스테이지는 항상 해제
        if (stageNumber <= 1)
            return true;

        // 이전 스테이지 클리어 시 해제
        return _playerData.IsStageCleared(stageNumber - 1);
    }

    /// <summary>
    /// 스테이지 클리어 여부
    /// </summary>
    public bool IsStageCleared(int stageNumber)
    {
        return _playerData.IsStageCleared(stageNumber);
    }

    /// <summary>
    /// 스테이지 별 개수
    /// </summary>
    public int GetStageStars(int stageNumber)
    {
        return _playerData.GetStageStars(stageNumber);
    }

    /// <summary>
    /// 스테이지 진행 상황 조회
    /// </summary>
    public RcStageProgress GetStageProgress(int stageNumber)
    {
        return _playerData.GetProgress(stageNumber);
    }

    /// <summary>
    /// 스테이지 클리어 처리
    /// </summary>
    public void RecordStageClear(int stageNumber, int turnCount)
    {
        var levelData = _stageDatabase.GetStage(stageNumber);
        if (levelData == null)
        {
            Debug.LogError($"[ProgressManager] 스테이지 {stageNumber} 데이터 없음");
            return;
        }

        // 별 계산
        int stars = levelData.StageInfo.CalculateStars(turnCount);

        // 진행 상황 업데이트
        var progress = _playerData.GetProgress(stageNumber);
        progress.UpdateClear(turnCount, stars);

        // 저장
        Save();

        Debug.Log($"[ProgressManager] 스테이지 {stageNumber} 클리어 - 턴: {turnCount}, 별: {stars}");
    }

    /// <summary>
    /// 저장
    /// </summary>
    public void Save()
    {
        _saveSystem.Save(_playerData);
    }

    /// <summary>
    /// 진행 상황 초기화 (디버그용)
    /// </summary>
    public void ResetProgress()
    {
        _saveSystem.Delete();
        _playerData = RcPlayerData.CreateNew();
        Debug.Log("[ProgressManager] 진행 상황 초기화됨");
    }

    /// <summary>
    /// 스테이지 데이터베이스
    /// </summary>
    public RcStageDatabaseSO StageDatabase => _stageDatabase;

    /// <summary>
    /// 전체 스테이지 수
    /// </summary>
    public int TotalStageCount => _stageDatabase?.StageCount ?? 0;
}
