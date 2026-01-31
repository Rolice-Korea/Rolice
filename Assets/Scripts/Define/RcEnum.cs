using UnityEngine;


public enum ColorType
{
    Gray,
    Red,
    Green,
    Blue,
    Yellow,
    Orange,
    Purple
}

public interface ITileBehavior
{
    bool CanEnter(RcDicePawn pawn);
    void OnEnter(RcDicePawn pawn);
    void OnExit(RcDicePawn pawn);
}