using System.Collections.Generic;

public class RcLevelLoadResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int TilesLoaded { get; set; }
    public int ColorTilesCount { get; set; }
    public List<string> Warnings { get; set; } = new List<string>();

    public static RcLevelLoadResult CreateSuccess(int tilesLoaded, int colorTiles)
    {
        return new RcLevelLoadResult
        {
            Success = true,
            TilesLoaded = tilesLoaded,
            ColorTilesCount = colorTiles
        };
    }

    public static RcLevelLoadResult CreateFailure(string errorMessage)
    {
        return new RcLevelLoadResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    public override string ToString()
    {
        return Success
            ? $"Success: {TilesLoaded} tiles loaded, {ColorTilesCount} color tiles"
            : $"Failed: {ErrorMessage}";
    }
}
