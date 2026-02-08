public enum RcGameEvent
{
    LevelCompleted,
    ColorTileCleared,  // Value: Vector2Int
    GameWin,
    GameLose,
    TurnChanged,       // Value: int
    MoveCompleted,     // Value: Vector2Int
}
