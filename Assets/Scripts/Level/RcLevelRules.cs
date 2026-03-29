using System;

[Serializable]
public class RcLevelRules
{
    public bool HasTurnLimit = false;
    public int  MaxTurns     = 30;

    public bool Validate() => true;
}
