using System;
using Engine;
using UnityEngine;

public class RcGameEvents : RcSingleton<RcGameEvents>
{
    private readonly RcEventHub<RcGameEvent> _events = new();
    private readonly RcEventHub<RcGameEvent, int> _intEvents = new();
    private readonly RcEventHub<RcGameEvent, Vector2Int> _posEvents = new();

    // 값 없는 이벤트 (LevelCompleted, GameWin, GameLose)
    public void Subscribe(RcGameEvent e, Action action) => _events.Subscribe(e, action);
    public void Unsubscribe(RcGameEvent e, Action action) => _events.Unsubscribe(e, action);
    public void Publish(RcGameEvent e) => _events.Publish(e);

    // int 이벤트 (TurnChanged)
    public void Subscribe(RcGameEvent e, Action<int> action) => _intEvents.Subscribe(e, action);
    public void Unsubscribe(RcGameEvent e, Action<int> action) => _intEvents.Unsubscribe(e, action);
    public void Publish(RcGameEvent e, int value) => _intEvents.Publish(e, value);

    // Vector2Int 이벤트 (ColorTileCleared, MoveCompleted)
    public void Subscribe(RcGameEvent e, Action<Vector2Int> action) => _posEvents.Subscribe(e, action);
    public void Unsubscribe(RcGameEvent e, Action<Vector2Int> action) => _posEvents.Unsubscribe(e, action);
    public void Publish(RcGameEvent e, Vector2Int value) => _posEvents.Publish(e, value);
}
