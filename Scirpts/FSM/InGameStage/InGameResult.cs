
using UnityEngine;

public class InGameResult : InGameFSM
{
    protected override void Start()
    {
        State = eState.Update;
    }

    protected override void Update()
    {
        State = eState.End;
    }

    protected override void End()
    {
        base.End();
    }
}
