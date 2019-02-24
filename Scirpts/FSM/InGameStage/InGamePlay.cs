using System;
using System.Collections.Generic;
using UnityEngine;

//프롤로그
//-> 대사가 존재할 경우 -> 대사 진행 -> 대사 진행 이후 -> Play 시작
//-> 대사가 존재하지 않을 경우 -> Play 시작
//전투 진입
//-> 대사가 존재할 경우 -> 대사 진행 -> 대사 진행 이후 -> Play 시작
//-> 대사가 존재하지 않을 경우 -> Play 시작
//배틀 시작전
//-> 대사가 존재할 경우 -> 대사 진행 -> 대사 진행 이후 -> 전투 시작
//-> 대사가 존재하지 않을 경우 -> 전투 시작
//배틀 끝
//-> 대사가 존재할 경우 -> 대사 진행 -> 대사 진행 이후 -> 이동
//-> 대사가 존재하지 않을 경우 -> 이동
//위를 정리해본 결과 배틀 안에서 이루어지고 있음.
public class InGamePlay : InGameFSM
{
    public override void Initailize(InGameView inGameView)
    {
        base.Initailize(inGameView);

        mFSMList = GetFSMList();

        for (int i = 0; i < mFSMList.Count; ++i)
        {
            mFSMList[i].Initailize(this);
        }

        MoveForce = new Transform[2];
        TargetPosition = new Vector3[2];
    }

    protected override void Start()
    {
        State = eState.Update;

        StartFSM(ePlay.Move);
    }
    
    protected override void Update()
    {
        if (!mInGameView.IsPause())
        {
            mCurrentFSM.NUpdate();
        }
    }

    protected override void End()
    {
        NPlay = ePlay.None;
        mCurrentFSM = null;

        base.End();

        mInGameView.NextFSM(
            InGameView.eGame.Result);
    }

    public DataStage GetStage()
    {
        return mInGameView.GetStage();
    }

    public CameraManager GetCameraManager()
    {
        return mInGameView.GetCameraManager();
    }

    public override void Pause(bool isPause)
    {
        mCurrentFSM.Pause(isPause);

        base.Pause(isPause);
    }

    #region FSM
    public void StartFSM(ePlay play)
    {
        if (mFSMList != null)
        {
            NPlay = play;

            mCurrentFSM = mFSMList[(int)play];
            mCurrentFSM.NStart();
        }
    }

    public void NextFSM(ePlay next)
    {
        GameManager.CollectGC();

        if (next == ePlay.Result)
        {
            End();
        }
        else
        {
            StartFSM(next);
        }
    }

    private ePlay NPlay
    {
        get
        {
            return mPlay;
        }
        set
        {
            mPlay = value;
        }
    }
    #endregion

    //당장 가라형식으로 만듬
    //정규화를 시킬려면 윈도우 메시지 통신 방식인 WPARAM, LPARAM 과 같은 형식을 사용해야되는데 현재 시간상 보류
    public override int GetGameMode()
    {
        return (int)NPlay;
    }

    public override void OnNoticeByServer()
    {
        mCurrentFSM.OnNoticeByServer();
	}
	public override void OnNoticePVPStartNextTag()
	{
		mCurrentFSM.OnNoticePVPStartNextTag();
	}

	private List<InGamePlayFSM> GetFSMList()
    {
        List<InGamePlayFSM> fsmList = null;

        switch (PlayData.Instance.PlayMode)
        {
            case PlayData.ePlayMode.Single:
                fsmList = new List<InGamePlayFSM>
                {
                    new InGamePlayMoveSingle(),
                    new InGamePlayBattle()
                };
                break;

            case PlayData.ePlayMode.PvP:
            case PlayData.ePlayMode.PTvPT:
                fsmList = new List<InGamePlayFSM>
                {
                    new InGamePlayMovePvP(),
                    new InGamePlayBattlePvP()
                };
                break;
        }

        return fsmList;
    }
    
    public Transform[] MoveForce;
    public Vector3[] TargetPosition;
    public int PvPMoveMode = BaseData.NONE_VALUE;

    private List<InGamePlayFSM> mFSMList;
    private InGamePlayFSM mCurrentFSM;
    
    private ePlay mPlay = ePlay.None;

    public enum ePlay : byte
    {
        Move,        
        Battle,
        Result,
        None = 127,
    }
}