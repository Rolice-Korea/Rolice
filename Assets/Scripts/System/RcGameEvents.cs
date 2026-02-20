using System;
using Engine;
using UnityEngine;

public class RcGameEvents : RcSingleton<RcGameEvents>
{
    private readonly RcEventHub<RcGameEvent> events = new();
    private readonly RcEventHub<RcGameEvent, int> intEvents = new();
    private readonly RcEventHub<RcGameEvent, Vector2Int> posEvents = new();

    // 값 없는 이벤트 (LevelCompleted, GameWin, GameLose)
    public void Subscribe(RcGameEvent e, Action action) => events.Subscribe(e, action);
    public void Unsubscribe(RcGameEvent e, Action action) => events.Unsubscribe(e, action);
    public void Publish(RcGameEvent e) => events.Publish(e);

    // int 이벤트 (TurnChanged)
    public void Subscribe(RcGameEvent e, Action<int> action) => intEvents.Subscribe(e, action);
    public void Unsubscribe(RcGameEvent e, Action<int> action) => intEvents.Unsubscribe(e, action);
    public void Publish(RcGameEvent e, int value) => intEvents.Publish(e, value);

    // Vector2Int 이벤트 (ColorTileCleared, MoveCompleted)
    public void Subscribe(RcGameEvent e, Action<Vector2Int> action) => posEvents.Subscribe(e, action);
    public void Unsubscribe(RcGameEvent e, Action<Vector2Int> action) => posEvents.Unsubscribe(e, action);
    public void Publish(RcGameEvent e, Vector2Int value) => posEvents.Publish(e, value);
}
