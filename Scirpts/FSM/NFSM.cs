using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//N->Neet
//현재 책임연쇄패턴을 흉내낸 것뿐이지 정형화해서 사용하는 수준은 아님
public abstract class NFSM : IChainOfResponsibility
{
    public void NStart()
    {
        State = eState.Start;
    }

    public void NUpdate()
    {
        if (!IsPause())
        {
            //mMoveForce.position = Vector3.Lerp(mMoveForce.position, mTargetPosition, GameManager.DeltaTime);
            switch (State)
            {
                case eState.Start:
                    Start();
                    break;

                case eState.Update:
                    Update();
                    break;

                case eState.End:
                    End();
                    break;
            }
        }
    }

    public virtual void NEnd()
    {
        State = eState.End;
    }

    public bool IsPause()
    {
        return
            State == eState.None ||
            State == eState.Pause;
    }

    public virtual void Pause(bool isPause)
    {
        if (isPause)
        {
            if (State != eState.Pause)
            {
                PrevState = State;
                State = eState.Pause;
            }
        }
        else
        {
            if (PrevState != eState.None)
            {
                State = PrevState;
                PrevState = eState.None;
            }
        }
    }

    protected virtual void Start()
    {
        State = eState.Update;
    }

    protected abstract void Update();
    
    protected virtual void End()
    {
        State = eState.None;
    }

    public virtual void Handle(CRMessage message) { }

    protected eState State = eState.None;
    protected eState PrevState = eState.None;
    
    public enum eState : byte
    {
        None,
        Start,
        Pause,
        Update,
        End
    }
}
