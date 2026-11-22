public static class RcGameContext
{
    public const int DefaultGridSize = 4;

    public static int SelectedGridSize { get; private set; } = DefaultGridSize;

    public static void SetGridSize(int gridSize)
    {
        SelectedGridSize = gridSize;
    }

    public static void Clear()
    {
        SelectedGridSize = DefaultGridSize;
    }
}
