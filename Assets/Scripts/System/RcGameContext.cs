
public static class RcGameContext
{
    public static int SelectedStageNumber { get; private set; }

    public static void SetStage(int stageNumber)
    {
        SelectedStageNumber = stageNumber;
    }

    public static void Clear()
    {
        SelectedStageNumber = 0;
    }
}
