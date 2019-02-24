using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePlayMovePvP : InGamePlayMove
{
    public override void Initailize(InGamePlay inGamePlay)
    {
        base.Initailize(inGamePlay);

        mInGamePlay.PvPMoveMode = DataStage.RBothMove;

        mStandByMobsPos.z += 100f;//mStartPos 와 z축의 값을 일치시키기 위하여 100을 더함

        mCameraManager.SetShakePos();
    }

    protected override void Start()
    {
        base.Start();

        InGameView view = mInGamePlay.GetView();
        //InGameTopUI.Instance.Refresh

        switch (mInGamePlay.PvPMoveMode)
        {
            case DataStage.RBothMove:
                MovePlayers(mInGamePlay.GetView());
                MoveEnemys(mInGamePlay.GetView());
                break;

            case DataStage.RCharMode:
                MovePlayers(mInGamePlay.GetView());
                break;

            case DataStage.REnemyMode:
                MoveEnemys(mInGamePlay.GetView());
                break;
        }        
    }

    protected override void Update()
    {
        int moveModeIndex = 0;

        if (mInGamePlay.PvPMoveMode != DataStage.RBothMove)
        {
            moveModeIndex = mInGamePlay.PvPMoveMode - 1;
        }

        if (IsArrive(moveModeIndex))
        {
            State = eState.End;
        }
        else
        {
            float deltaTime = GameManager.DeltaTime;
            
            switch (mInGamePlay.PvPMoveMode)
            {
                case DataStage.RBothMove:
                    for (int i = 0; i < mInGamePlay.MoveForce.Length; ++i)
                    {
                        mInGamePlay.MoveForce[i].position = Vector3.Lerp(
                            mInGamePlay.MoveForce[i].position,
                            mInGamePlay.TargetPosition[i],
                            deltaTime);
                    }
                    break;

                default:
                    mInGamePlay.MoveForce[moveModeIndex].position = Vector3.Lerp(
                        mInGamePlay.MoveForce[moveModeIndex].position,
                        mInGamePlay.TargetPosition[moveModeIndex],
                        deltaTime);
                    break;
            }   
        }
    }

    protected override void End()
    {
        base.End();

        InGameView view = mInGamePlay.GetView();

        switch (mInGamePlay.PvPMoveMode)
        {
            case DataStage.RBothMove:
                IdlePlayers(mInGamePlay.GetView());
                IdleEnemys(mInGamePlay.GetView());
                break;

            case DataStage.RCharMode:
                IdlePlayers(mInGamePlay.GetView());
                break;

            case DataStage.REnemyMode:
                IdleEnemys(mInGamePlay.GetView());
                break;
        }

        if (!mIsEntryAction)
        {
            if (InGameManager.Instance.TIsTestMode)
            {
                InGameManager.Instance.OnCallbackStartBattle();
            }
            else
            {
				if (PVPManager.Instance != null)
					PVPManager.Instance.OpenPVPInfo();
				else
					InGameManager.Instance.OnReadyToBattle();
			}
        }
        else
        {
            if (InGameManager.Instance.TIsTestMode)
            {
                InGameManager.Instance.OnCallbackStartNextTag();
            }
            else
            {
                InGameManager.Instance.OnReadyToNextTag();
            }
        }
        
        if (InGameManager.Instance.mEndPvPTimeState == InGameManager.EndPvPTimeState.End)
        {
            InGameManager.Instance.mEndPvPTimeState = InGameManager.EndPvPTimeState.None;
            InGameTopUI.Instance.RestartTimer();
        }
    }

    private void MovePlayers(InGameView view)
    {
        mInGamePlay.MoveForce[0].position = mStartPos;
        mInGamePlay.TargetPosition[0] = sCharPos;

        for (int i = 0; i < Pacs.Count; ++i)
        {
            Pacs[i].StandBy();
            view.UpdateMotion(Pacs[i].GetUnits(), GameUnit.eMotion.run);
        }

        InGameDownUI.Instance.RefreshSkillBtns(true);
    }

    private void MoveEnemys(InGameView view)
    {
        mInGamePlay.MoveForce[1].position = mStandByMobsPos;
        mInGamePlay.TargetPosition[1] = sMobsPos;

        for (int i = 0; i < WaveMobs.Count; ++i)
        {
            WaveMobs[i].StandBy();
            view.UpdateMotion(WaveMobs[i].GetUnits(), GameUnit.eMotion.run);
        }

        InGameDownUI.Instance.RefreshSkillBtns(false);
    }

    private void IdlePlayers(InGameView view)
    {
        mInGamePlay.MoveForce[0].position = sCharPos;

        for (int i = 0; i < Pacs.Count; ++i)
        {
            Pacs[i].StandBy();
            view.UpdateMotion(Pacs[i].GetUnits(), GameUnit.eMotion.ingame_idle);
        }
    }

    private void IdleEnemys(InGameView view)
    {
        mInGamePlay.MoveForce[1].position = sMobsPos;

        for (int i = 0; i < WaveMobs.Count; ++i)
        {
            WaveMobs[i].StandBy();
            view.UpdateMotion(WaveMobs[i].GetUnits(), GameUnit.eMotion.ingame_idle);
        }
    }
	
	public override void OnNoticePVPStartNextTag()
	{
		mInGamePlay.NextFSM(InGamePlay.ePlay.Battle);

		InGameTopUI.Instance.Refresh(InGameTopUI.eRefresh.Time);

		++mNextPatternIndex;
	}

	private int mNextPatternIndex = 0;
}
