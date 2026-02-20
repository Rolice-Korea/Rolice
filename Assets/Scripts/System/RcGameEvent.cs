public enum RcGameEvent
{
    LevelCompleted,
    ColorTileCleared,  // Value: Vector2Int
    GameWin,
    GameLose,
    TurnChanged,       // Value: int
    MoveStarted,       // Value: Vector2Int
    MoveCompleted,     // Value: Vector2Int
}
