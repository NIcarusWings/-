using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePlayBattlePvP : InGamePlayBattle
{
    public override void Initailize(InGamePlay inGamePlay)
    {
        base.Initailize(inGamePlay);
    }

    protected override void Start()
    {
        base.Start();

        mCurTime = 0f;
    }

    protected override void Update()
    {
        if (InGameManager.Instance.mEndPvPTimeState == InGameManager.EndPvPTimeState.None)
        {
            base.Update();
        }
        else
        {
            End();
        }
    }

    protected override void End()
    {
        if (mCurTime < mDelay)
        {
            mCurTime += GameManager.DeltaTime;
        }
        else
        {
            mCurTime = 0f;

            if (InGameManager.Instance.mEndPvPTimeState == InGameManager.EndPvPTimeState.Start)
            {
                EndPvPTime();
            }
            else
            {
                NextPvP();
            }
        }
    }

    private void EndPvPTime()
    {
        if (PlayData.Instance.GameResult == PlayData.eGameResult.None)
        {
            UISprite[] sliders = InGameTopUI.Instance.m_Sliders;
            float fillAmount1 = sliders[0].fillAmount;
            float fillAmount2 = sliders[1].fillAmount;

            if (fillAmount1 > fillAmount2)
            {
                PlayData.Instance.GameResult = PlayData.eGameResult.None;
                mInGamePlay.PvPMoveMode = DataStage.RCharMode;
            }
            else if (fillAmount1 < fillAmount2)
            {
                PlayData.Instance.GameResult = PlayData.eGameResult.Lose;
                mInGamePlay.PvPMoveMode = DataStage.REnemyMode;
            }
            else
            {
                PlayData.Instance.GameResult = PlayData.eGameResult.RetireBoth;
                mInGamePlay.PvPMoveMode = DataStage.RBothMove;
            }

            switch (PlayData.Instance.GameResult)
            {
                case PlayData.eGameResult.None:
                    RetireUnits(InGameManager.Instance.GetWaveMobs());
                    break;

                case PlayData.eGameResult.Lose:
                    RetireUnits(InGameManager.Instance.GetPlatoonPlayers());
                    break;

                case PlayData.eGameResult.RetireBoth:
                    RetireUnits(InGameManager.Instance.GetWaveMobs());
                    RetireUnits(InGameManager.Instance.GetPlatoonPlayers());
                    break;
            }

            InGameManager.Instance.mEndPvPTimeState = InGameManager.EndPvPTimeState.Update;
        }
    }

    private void NextPvP()
    {
        if (CheckEndPvP())
        {
            SetEndPvP();
            InGameManager.Instance.mEndPvPTimeState = InGameManager.EndPvPTimeState.None;
        }
        else
        {
            SetNextPvP();
            InGameManager.Instance.mEndPvPTimeState = InGameManager.EndPvPTimeState.End;
        }
    }

    private bool CheckEndPvP()
    {
        List<List<FormationField>> list = null;
        int curIndex = 0;

        switch (PlayData.Instance.GameResult)
        {
            case PlayData.eGameResult.RetireBoth:
                list = InGameManager.Instance.StageMobs;
                curIndex = CurWaveIndex;
                ++InGameManager.Instance.VictoryCounts[0];

                //list = InGameManager.Instance.Pacs;
                //curIndex = InGameManager.Instance.CurPlatoonIndex;
                ++InGameManager.Instance.VictoryCounts[1];
                break;

            case PlayData.eGameResult.Lose:
                list = InGameManager.Instance.Pacs;
                curIndex = InGameManager.Instance.CurPlatoonIndex;
                ++InGameManager.Instance.VictoryCounts[1];
                break;

            default:
                list = InGameManager.Instance.StageMobs;
                curIndex = CurWaveIndex;
                ++InGameManager.Instance.VictoryCounts[0];
                break;
        }

        return curIndex + 1 >= list.Count;
    }

    private void SetEndPvP()
    {
        if (InGameManager.Instance.VictoryCounts[0] >= InGameManager.Instance.VictoryCounts[1])
        {
            PlayData.Instance.GameResult = PlayData.eGameResult.Win;
        }
        //else if (InGameManager.Instance.VictoryCounts[0] < InGameManager.Instance.VictoryCounts[1])
        //{

        //}
        else
        {
            PlayData.Instance.GameResult = PlayData.eGameResult.Lose;
        }

        State = eState.Update;
        EndState = eEndState.EndGame;
    }

    private void SetNextPvP()
    {
        switch (PlayData.Instance.GameResult)
        {
            case PlayData.eGameResult.RetireBoth:
                ++InGameManager.Instance.CurPlatoonIndex;
                ++CurWaveIndex;
                mInGamePlay.PvPMoveMode = DataStage.RBothMove;
                break;

            case PlayData.eGameResult.Lose:
                ++InGameManager.Instance.CurPlatoonIndex;
                mInGamePlay.PvPMoveMode = DataStage.RCharMode;
                break;

            default:
                ++CurWaveIndex;
                mInGamePlay.PvPMoveMode = DataStage.REnemyMode;
                break;
        }

        PlayData.Instance.GameResult = PlayData.eGameResult.None;
        
        mInGamePlay.NextFSM(InGamePlay.ePlay.Move);
    }

    private void RetireUnits(List<FormationField> list)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            if (list[i].CurUnit != null && !list[i].CurUnit.IsRetire())
            {
                CRMessage localMessage = new CRMessage();

                localMessage.Reset(CRMessage.eContext.RETIRE_UNIT);

                list[i].CurUnit.Handle(localMessage);
            }
        }
    }

    private float mCurTime = 0f;
    private float mDelay = 1f;
}
