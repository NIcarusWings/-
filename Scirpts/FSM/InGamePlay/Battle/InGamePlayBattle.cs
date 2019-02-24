using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eBattleState : byte
{
    ClearWave = 0, Retire, EndGame,
}

public class InGamePlayBattle : InGamePlayFSM
{
    public override void Initailize(InGamePlay inGamePlay)
    {
        base.Initailize(inGamePlay);

        EndState = eEndState.None;
    }

    protected override void Start()
    {
        //대사가 있는지 없는지 체크
        //if ()
        //{
        //}
        //else
        {
            for (int i = 0; i < Pacs.Count; ++i)
            {
                Pacs[i].ReadyBattle();
            }

            for (int i = 0; i < WaveMobs.Count; ++i)
            {
                WaveMobs[i].ReadyBattle();
            }
        }

        PlayData.Instance.GameResult = PlayData.eGameResult.None;

        State = eState.Update;
        EndState = eEndState.None;
    }
    
    //전투 도중 대사창을 활성화해야하는데 대사창에 애니메이션은 유지하고 인 게임의 애니메이션은 멈추야될 때 밑의 사이트 참고
    //http://baba-s.hatenablog.com/entry/2016/11/18/100000
    protected override void Update()
    {
        if (EndState == eEndState.None)
        {
            for (int i = 0; i < Pacs.Count; ++i)
            {
                if (InGameManager.Instance.TIsAttackPlay)
                {
                    Pacs[i].NUpdate();

                    InGameDownUI.Instance.UpdateGuage(ePlayType.Player, i);
                }
            }

            for (int i = 0; i < WaveMobs.Count; ++i)
            {
                if (InGameManager.Instance.TIsAttackEnemy)
                {
                    WaveMobs[i].NUpdate();

                    InGameDownUI.Instance.UpdateGuage(ePlayType.Enemy, i);
                }
            }

            if (IsGameOver())
            {
                AllReset();

                PlayData.Instance.GameResult = PlayData.eGameResult.Lose;

                State = eState.End;
            }

            if (IsClearWave())
            {
                if (PlayData.Instance.GameResult == PlayData.eGameResult.Lose)
                {
                    PlayData.Instance.GameResult = PlayData.eGameResult.RetireBoth;
                }

                State = eState.End;
            }
        }
        else
        {
            switch (EndState)
            {
                case eEndState.NextWave:
                    NextWave();
                    break;

                case eEndState.EndGame:
                    EndGame();
                    break;
            }
        }

        CameraManager.Instance.DetectAction();
    }

    protected override void End()
    {
        for (int i = 0; i < Pacs.Count; ++i)
        {
            Pacs[i].NReset(eBattleState.ClearWave);
        }

        /*if (PlayData.Instance.IsPvPMode())
        {
            base.End();

            ServerState = eServerState.Ready;

            if (PlayData.Instance.GameResult == PlayData.eGameResult.Lose)
            {
                
            }
            else
            {
                
            }

            //mInGamePlay.NextFSM(InGamePlay.ePlay.Result);
        }
        else*/
        {
            if (PlayData.Instance.GameResult == PlayData.eGameResult.Lose ||
                PlayData.Instance.GameResult == PlayData.eGameResult.RetireBoth)
            {
                PlayData.Instance.GameResult = PlayData.eGameResult.Lose;

                AllReset();

                base.End();

                State = eState.Update;
                EndState = eEndState.EndGame;
            }
            else
            {
                //클리어 조건을 판별한 다음에 클리어 조건이 끝나면
                //NGame = eGame.Result;
                //이고 그렇지 않을 경우에는
                //NGame = eGame.Play;
                //로 진행하면 됨.
                //웨이브가 최대치이면 게임을 클리어한 것임.
                if (CurWaveIndex + 1 >= mInGamePlay.GetMaxWave())
                {
                    AllReset();
                    base.End();

                    ++CurWaveIndex;

                    PlayData.Instance.GameResult = PlayData.eGameResult.Win;
                    InGameManager.Instance.CurWaveIndex = CurWaveIndex;

                    for (int i = 0; i < Pacs.Count; ++i)
                    {
                        if (!Pacs[i].IsRetire())
                        {
                            Pacs[i].CurUnit.UpdateMotion(GameUnit.eMotion.victory);
                        }
                    }

                    State = eState.Update;
                    EndState = eEndState.EndGame;
				}
                else
                {
                    //증가하기전에 먼저 보스가 존재하는지 검사하고
                    //보스가 존재하면 보스 모드를 진행한뒤에 증가하고
                    //보스가 존재하지 않으면 바로 증가한다.
                    ++CurWaveIndex;

                    //InGameManager.SReset();
                    State = eState.Update;

                    PlayData.Instance.GameResult = PlayData.eGameResult.EndWave;
                    TCurrentTime = TDelayTime;
                    EndState = eEndState.NextWave;                    
                }
            }
        }
    }

    private void NextWave()
    {
        TCurrentTime -= GameManager.DeltaTime;

        if (TCurrentTime < 0)
        {
            State = eState.None;

            //대사가 존재하는지 없는지 체크
            mInGamePlay.NextFSM(InGamePlay.ePlay.Move);
        }
    }

    private void EndGame()
    {
        State = eState.None;
        InGameManager.Instance.StopInGameTimer();

        NextGamePlayResult();
    }

    private void NextGamePlayResult()
	{
		if (InGameManager.Instance.IsTestMode() || !PlayData.Instance.IsPvPMode())
        {
            mInGamePlay.NextFSM(InGamePlay.ePlay.Result);
        }
		else
        {
            InGameManager.Instance.OnPVPBattleFinish();
        }
	}

    private bool IsClearWave()
    {
        bool result = true;

        for (int i = 0; i < WaveMobs.Count; ++i)
        {
            //한명이라도 존재하면 조건이 성립이 되지 않으므로 빠져나옴.
            if (!WaveMobs[i].IsRetire())
            {
                result = false;
                break;
            }
        }

        return result;
    }

    private bool IsGameOver()
    {
        bool result = true;

        for (int i = 0; i < Pacs.Count; ++i)
        {
            //한명이라도 존재하면 조건이 성립이 되지 않으므로 빠져나옴.
            if (!Pacs[i].IsRetire())
            {
                result = false;
                break;
            }
        }

        return result;
    }
    
    private void AllReset()
    {
        for (int i = 0; i < Pacs.Count; ++i)
        {
            Pacs[i].NReset(eBattleState.EndGame);
        }

        for (int i = 0; i < WaveMobs.Count; ++i)
        {
            WaveMobs[i].NReset(eBattleState.EndGame);
        }
    }

    private bool mIsCommunication = false;

    public eServerState ServerState = eServerState.Ready;
    public eEndState EndState = eEndState.None;

    private float TCurrentTime = 0f;
    private float TDelayTime = 2f;

    public enum eEndState : byte
    {
        None, NextWave, EndGame,
    }

    public enum eServerState : byte
    {
        Ready, Run, Done
    }

    
}
