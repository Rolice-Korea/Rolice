using System.Collections.Generic;

/// 레벨 로드 작업의 결과를 담는 클래스
/// - 성공/실패 여부
/// - 에러 메시지
/// - 로드된 타일 수
/// - 경고 메시지 리스트

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
    
    public static LevelLoadResult CreateFailure(string errorMessage)
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
        if (Success)
        {
            return $"Success: {TilesLoaded} tiles loaded, {ColorTilesCount} color tiles";
        }
        else
        {
            return $"Failed: {ErrorMessage}";
        }
    }
}
