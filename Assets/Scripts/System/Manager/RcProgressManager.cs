using Engine;
using Rolice.Data;
using Rolice.System;
using UnityEngine;

public class RcProgressManager : RcSingleton<RcProgressManager>
{
    private IRcSaveSystem saveSystem;
    private RcPlayerData playerData;
    private RcStageDatabaseSO stageDatabase;

    public bool IsInitialized { get; private set; }

    public void Initialize(RcStageDatabaseSO stageDatabase)
    {
        this.stageDatabase = stageDatabase;
        saveSystem = new RcJsonSaveSystem();
        playerData = saveSystem.Load();
        IsInitialized = true;
    }

    public bool IsStageUnlocked(int stageNumber)
    {
        if (stageNumber <= 1)
            return true;

        return playerData.IsStageCleared(stageNumber - 1);
    }

    public bool IsStageCleared(int stageNumber)
    {
        return playerData.IsStageCleared(stageNumber);
    }

    public int GetStageStars(int stageNumber)
    {
        return playerData.GetStageStars(stageNumber);
    }

    public RcStageProgress GetStageProgress(int stageNumber)
    {
        return playerData.GetProgress(stageNumber);
    }

    public void RecordStageClear(int stageNumber, int turnCount)
    {
        var levelData = stageDatabase.GetStage(stageNumber);
        if (levelData == null)
        {
            Debug.LogError($"[ProgressManager] 스테이지 {stageNumber} 데이터 없음");
            return;
        }

        int stars = levelData.StageInfo.CalculateStars(turnCount);
        var progress = playerData.GetProgress(stageNumber);
        progress.UpdateClear(turnCount, stars);
        Save();
    }

    public void Save()
    {
        saveSystem.Save(playerData);
    }

    public void ResetProgress()
    {
        saveSystem.Delete();
        playerData = RcPlayerData.CreateNew();
    }

    public RcStageDatabaseSO StageDatabase => stageDatabase;

    public int TotalStageCount => stageDatabase?.StageCount ?? 0;
}
