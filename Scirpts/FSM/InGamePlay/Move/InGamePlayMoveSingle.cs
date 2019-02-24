using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePlayMoveSingle : InGamePlayMove
{
    protected override void Start()
    {
        base.Start();

        InGameView view = mInGamePlay.GetView();
        DataStage stage = view.GetStage();

        if (!mIsEntryAction)
        {
            DataWavePattern dwp = GameTableContainer.Instance.GetDataWavePattern(stage.Index);
            int[] patterns = dwp.GetWaves();

            for (int i = 0; i < patterns.Length; ++i)
            {
                mWavePatterns.Add(patterns[i]);
            }

            for (int i = 0; i < WaveMobs.Count; ++i)
            {
                WaveMobs[i].StandBy();
            }

            switch (stage.EntryAction)
            {
                case DataStage.eEntryAction.Move:
                    EntryRun(view, stage.EntryAction);
                    break;
            }
            
        }
        else
        {
            Run(
                view,
                Vector3.forward * (stage.FieldWidth * 0.5f),
                mWavePatterns[CurWaveIndex],
                (mWavePatterns[CurWaveIndex] == DataStage.RCharMode) ? ++mCharMoveCount : mCharMoveCount);

            InGameTopUI.Instance.Refresh(InGameTopUI.eRefresh.Wave);
        }
    }

    protected void EntryRun(InGameView view, DataStage.eEntryAction entryAction)
    {
        switch (entryAction)
        {
            case DataStage.eEntryAction.Move:
                mInGamePlay.MoveForce[0].position = mStartPos;
                mInGamePlay.TargetPosition[0] = sCharPos;

                for (int i = 0; i < Pacs.Count; ++i)
                {
                    view.UpdateMotion(Pacs[i].GetUnits(), GameUnit.eMotion.run);
                }
                break;

            case DataStage.eEntryAction.StandBy:
                mInGamePlay.MoveForce[0].position = sCharPos;
                mInGamePlay.TargetPosition[0] = sCharPos;
                break;
        }

        mInGamePlay.MoveForce[1].gameObject.SetActive(false);
    }

    protected override void Update()
    {
        int moveModeIndex = mWavePatterns[CurWaveIndex] - 1;

        if (IsArrive(moveModeIndex))
        {
            State = eState.End;
        }
        else
        {
            float deltaTime = GameManager.DeltaTime;

            
            mInGamePlay.MoveForce[moveModeIndex].position = Vector3.Lerp(
                mInGamePlay.MoveForce[moveModeIndex].position,
                mInGamePlay.TargetPosition[moveModeIndex],
                deltaTime);

            switch (mWavePatterns[CurWaveIndex])
            {
                case DataStage.RCharMode:
                    mCameraManager.Move(deltaTime);
                    break;

                case DataStage.REnemyMode:
                    break;
            }
        }
    }

    protected override void End()
    {
        base.End();

        int moveModeIndex = mWavePatterns[CurWaveIndex] - 1;

        mInGamePlay.MoveForce[moveModeIndex].position = mInGamePlay.TargetPosition[moveModeIndex];

        if (!mIsEntryAction)
        {
            InGameView view = mInGamePlay.GetView();
            DataStage stage = view.GetStage();

            switch (stage.EntryAction)
            {
                case DataStage.eEntryAction.Move:
                    if (mWavePatterns[0] != DataStage.RCharMode)
                    {
                        for (int i = 0; i < Pacs.Count; ++i)
                        {
                            view.UpdateMotion(Pacs[i].GetUnits(), GameUnit.eMotion.ingame_idle);
                        }
                    }
                    break;
            }

            mIsEntryAction = true;
            mInGamePlay.NextFSM(InGamePlay.ePlay.Move);
        }
        else
        {
            InGameView view = mInGamePlay.GetView();

            switch (mWavePatterns[CurWaveIndex])
            {
                case DataStage.RCharMode:
                    mCameraManager.EndMove();

                    for (int i = 0; i < Pacs.Count; ++i)
                    {
                        view.UpdateMotion(Pacs[i].GetUnits(), GameUnit.eMotion.ingame_idle);
                    }
                    break;

                case DataStage.REnemyMode:
                    GameUnitObject unitObj = null;

                    for (int i = 0; i < WaveMobs.Count; ++i)
                    {
                        if (WaveMobs[i].CurUnit.GetUnitIndex() < DataMob.MOB_BODY_START_NUMBER)
                        {
                            unitObj = WaveMobs[i].CurUnit;
                        }

                        view.UpdateMotion(WaveMobs[i].GetUnits(), GameUnit.eMotion.ingame_idle);
                    }
                    break;
            }

            #region 전투 없이 웨이브 배치가 제대로 된 것을 확인만 하고 싶을 경우
            //++CurWaveIndex;
            //mInGamePlay.NextFSM(InGamePlay.ePlay.Move);
            #endregion

            mInGamePlay.NextFSM(InGamePlay.ePlay.Battle);
        }

        mCameraManager.SetShakePos();
    }

    protected List<int> mWavePatterns = new List<int>();
}
