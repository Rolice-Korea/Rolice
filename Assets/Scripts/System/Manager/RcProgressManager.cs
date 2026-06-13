using Cysharp.Threading.Tasks;
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

    /// <summary>전 스테이지 누적 별 합. 상점 스킨 해금 게이트 판정용.</summary>
    public int GetTotalStars()
    {
        return PlayerData.GetTotalStars();
    }

    public RcStageProgress GetStageProgress(int stageNumber)
    {
        return PlayerData.GetProgress(stageNumber);
    }

    public void RecordStageClear(int stageNumber, int moveCount, float elapsedTime)
    {
        var levelData = stageDatabase.GetStage(stageNumber);
        if (levelData == null)
        {
            Debug.LogError($"[ProgressManager] 스테이지 {stageNumber} 데이터 없음");
            return;
        }

        int stars    = levelData.StageInfo.CalculateStars(moveCount, elapsedTime);
        var progress = PlayerData.GetProgress(stageNumber);
        progress.UpdateClear(moveCount, elapsedTime, stars);

        RcPlayerState.Instance.SaveLocal();
        RcPlayerState.Instance.NotifyChanged();
    }

    public async UniTask ResetProgressAsync()
    {
        await RcPlayerState.Instance.ResetAllAsync();
    }

    public RcStageDatabaseSO StageDatabase  => stageDatabase;
    public int               TotalStageCount => stageDatabase?.StageCount ?? 0;
}
