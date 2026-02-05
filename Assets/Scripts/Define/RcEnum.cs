using UnityEngine;

public interface ITileBehavior
{
    bool CanEnter(RcDicePawn pawn);
    void OnEnter(RcDicePawn pawn);
    void OnExit(RcDicePawn pawn);
}