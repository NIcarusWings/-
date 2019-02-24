using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InGamePlayMove : InGamePlayFSM
{
    public override void Initailize(InGamePlay inGamePlay)
    {
        base.Initailize(inGamePlay);
        
        InGameView view = mInGamePlay.GetView();
        DataStage stage = view.GetStage();

        mCameraManager = view.GetCameraManager();

        InGameManager.Instance.StartInGameTimer();
    }

    protected override void Start()
    {
        base.Start();

        mInGamePlay.MoveForce[0] = mInGamePlay.GetView().MovePac;
        mInGamePlay.MoveForce[1] = mInGamePlay.GetView().MoveMob;

        mInGamePlay.MoveForce[0].gameObject.SetActive(true);
        mInGamePlay.MoveForce[1].gameObject.SetActive(true);

        InGameTopUI.Instance.Refresh(InGameTopUI.eRefresh.TopHPGauge, 0);
        InGameTopUI.Instance.Refresh(InGameTopUI.eRefresh.TopHPGauge, 1);
    }

    protected void Run(InGameView view, Vector3 center, int moveMode, int moveCount)
    {
        int modeModeIndex = moveMode - 1;

        mInGamePlay.MoveForce[1].gameObject.SetActive(true);

        switch (moveMode)
        {
            case DataStage.RCharMode:
                mInGamePlay.TargetPosition[modeModeIndex] = center * moveCount + sCharPos;
                mInGamePlay.MoveForce[modeModeIndex + 1].position = center * moveCount + sMobsPos;

                for (int i = 0; i < Pacs.Count; ++i)
                {
                    Pacs[i].StandBy();

                    view.UpdateMotion(Pacs[i].GetUnits(), GameUnit.eMotion.run);
                }

                for (int i = 0; i < WaveMobs.Count; ++i)
                {
                    WaveMobs[i].StandBy();
                }

                mCameraManager.SetMoveValue(center * moveCount);
                break;

            case DataStage.REnemyMode:
                mInGamePlay.TargetPosition[modeModeIndex] = center * moveCount + sMobsPos;
                mInGamePlay.MoveForce[modeModeIndex].position = center * moveCount + mStandByMobsPos;

                for (int i = 0; i < Pacs.Count; ++i)
                {
                    Pacs[i].StandBy();
                }

                for (int i = 0; i < WaveMobs.Count; ++i)
                {
                    WaveMobs[i].StandBy();
                    view.UpdateMotion(WaveMobs[i].GetUnits(), GameUnit.eMotion.run);
                }
                break;
        }
    }

    public bool IsArrive(int index)
    {
        float sqrMagnitude = (mInGamePlay.MoveForce[index].position - mInGamePlay.TargetPosition[index]).sqrMagnitude;

        return (sqrMagnitude < 8.0f);
    }

    public override void OnNoticeByServer()
    {
        mIsEntryAction = true;
        mInGamePlay.NextFSM(InGamePlay.ePlay.Battle);

        InGameTopUI.Instance.Refresh(InGameTopUI.eRefresh.Time);
    }

    protected CameraManager mCameraManager;

    protected Vector3 mStartPos = new Vector3(0f, 0f, -200f);
    public static Vector3 sCharPos = new Vector3(0f, 0f, -35f);

    //캐릭터 진형과 적 진형의 차이의 거리를 의미함.
    //캐릭터 진형이 이동할 경우 이 벡터를 사용하여 적 진형들의 위치를 결정함.
    //추가적으로 적 진형이 이동할 경우 이 벡터는 적 진형의 도착지점의 위치를 결정할 때 사용됨.
    public static Vector3 sMobsPos = new Vector3(0f, 0f, 35f);
    protected Vector3 mStandByMobsPos = new Vector3(0f, 0f, 100f);

    protected Vector3 mCurMovePos = Vector3.zero;

    protected byte mCharMoveCount = 0;

    protected bool mIsEntryAction = false;
}
