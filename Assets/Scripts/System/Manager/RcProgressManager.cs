using Engine;
using Rolice.Data;
using Rolice.System;
using UnityEngine;

public class RcProgressManager : RcSingleton<RcProgressManager>
{
    private RcStageDatabaseSO stageDatabase;

    public bool IsInitialized { get; private set; }

    // 데이터는 RcPlayerState 가 소유 — 직접 참조만 유지
    private RcPlayerData PlayerData => RcPlayerState.Instance.Data;

    public void Initialize(RcStageDatabaseSO stageDatabase)
    {
        this.stageDatabase = stageDatabase;
        IsInitialized = true;
    }

    public bool IsStageUnlocked(int stageNumber)
    {
        if (stageNumber <= 1) return true;
        return PlayerData.IsStageCleared(stageNumber - 1);
    }

    public bool IsStageCleared(int stageNumber)
    {
        return PlayerData.IsStageCleared(stageNumber);
    }

    public int GetStageStars(int stageNumber)
    {
        return PlayerData.GetStageStars(stageNumber);
    }

    public RcStageProgress GetStageProgress(int stageNumber)
    {
        return PlayerData.GetProgress(stageNumber);
    }

    public void RecordStageClear(int stageNumber, int turnCount)
    {
        var levelData = stageDatabase.GetStage(stageNumber);
        if (levelData == null)
        {
            Debug.LogError($"[ProgressManager] 스테이지 {stageNumber} 데이터 없음");
            return;
        }

        int stars    = levelData.StageInfo.CalculateStars(turnCount);
        var progress = PlayerData.GetProgress(stageNumber);
        progress.UpdateClear(turnCount, stars);

        RcPlayerState.Instance.Save();
        RcPlayerState.Instance.NotifyChanged();
    }

    public void ResetProgress()
    {
        RcPlayerState.Instance.ResetAll();
    }

    public RcStageDatabaseSO StageDatabase  => stageDatabase;
    public int               TotalStageCount => stageDatabase?.StageCount ?? 0;
}
