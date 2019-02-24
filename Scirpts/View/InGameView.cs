using System.Collections.Generic;
using UnityEngine;

//FSM의 객체화는 나중에 해야겠음.
//InGame이라도 스테이지, 레이드 등등 여러가지 방식으로 존재할 수가 있다.
//InGameView는 위 방식들의 공통된 리소스를 관리하는 클래스이고 실제 처리는 InGameXFSM에서 처리한다.
//InGameXFSM의 X는 방식을 의미한다. InGame인데 스테이지면 InGameStageFSM, 레이드 이면 InGameRaidFSM 이다.
public class InGameView : CView
{
    public void Initialize(InGameManager inGameManager)
    {
        mInGameManager = inGameManager;
        
        InitializeField();
        InitializeFSM();
    }

    private void InitializeField()
    {
        Transform units = transform.Find("Stage/Units");

        MovePacs = units.GetChild(0);
        MoveMobs = units.GetChild(1);

        DataStage ds = GetStage();

		GameObject field = GameManager.Summon(ds.ResourceName, false);

		if (field != null)
        {
            SummonFieldEffect(ds.AppendObject1, ds.MoveStageEffect);
            SummonFieldEffect(ds.AppendObject2, ds.MoveStageEffect);
            SummonFieldEffect(ds.AppendObject3, ds.MoveStageEffect);

            //field.transform.localPosition = new Vector3(-6f, 0f, 0f);
        }

        Transform light = transform.Find("Light");
        Transform time = null;

        switch (ds.Time)
        {
            case 0:
                time = light.Find("Day");
                time.gameObject.SetActive(true);
                break;

            case 1:
                time = light.Find("Night");
                time.gameObject.SetActive(true);
                break;
        }

        Light lightComponent = time.GetComponent<Light>();

        if (lightComponent != null)
        {
            lightComponent.color = GameUtil.GetColorByHexString(ds.LightingRGB);
        }
    }

    private void SummonFieldEffect(string effectName, int moveStageEffect)
    {
        if (effectName != null && effectName.Length > 2)
        {
            if (moveStageEffect != 0)
            {
                GameObject go = GameManager.Summon(effectName, false);

                if (moveStageEffect == 2)
                {
                    CameraManager cm = mInGameManager.GetCameraManager();

                    go.transform.SetParent(cm.GetMainCameraParent());
                }
            }
        }
    }

    private void InitializeFSM()
    {
        mFSMList = GetFSMList();

        for (int i = 0; i < mFSMList.Count; ++i)
        {
            mFSMList[i].Initailize(this);
        }
    }

    public void Pause(bool isPause)
    {
        if (mCurrentFSM != null)
        {
            mCurrentFSM.Pause(isPause);
        }
    }

    protected override void Opening()
    {
        if (mInGameManager != null)
        {
            base.Opening();
            InGameManager.Instance.ActiveRelationDialogUI(false);
            InGameManager.Instance.OpenDialogView(DataStageEvent.eSituation.StartMissionInInGame, StartFSM);
        }
        else
        {
            Debug.LogError("InGameManager is null");
        }
    }

    public void StartFSM()
    {
        InGameManager.Instance.ActiveRelationDialogUI(true);
        StartFSM(eGame.Entry);
    }

    #region FSM
    public void StartFSM(eGame game)
    {
        if (mFSMList != null)
        {
            NGame = game;
            mCurrentFSM = mFSMList[(int)game];
            mCurrentFSM.NStart();
        }
    }

    public void NextFSM(eGame next)
    {
        GameManager.CollectGC();

        if (next == eGame.Result)
        {
            InGameManager.SReset();
            mCurrentFSM = null;
            NGame = eGame.None;

            if (GameManager.sIsUseServer && !InGameManager.Instance.IsTestMode())
            {
                //dispatchTeam;
                //mvpIndex;
                //UnitHP[] unitHps;
                GirlsWarsClient.StageEndPacket sep = new GirlsWarsClient.StageEndPacket()
                {
                    dispatchTeam = -1,
                    mvpIndex = (PlayData.Instance.GameResult == PlayData.eGameResult.Win) ? InGameManager.Instance.GetMVP() : -1,
                    unitHps = InGameManager.Instance.GetUnitHPs(),
                };
                
                GirlsWarsClient.Instance.StageEnd(
                    sep,
                    (sepr) =>
                    {
                        Result();
                    },
                    (err)=>
                    {
                        Debug.Log(err.error);
                        Debug.Log(err.exception);
                        Debug.Log(err.message);
                        Debug.Log(err.raw);
                    });
            }
            else
            {
                Result();
            }
#if UNITY_EDITOR
            Debug.Log("InGameView FSM 끝");
#endif
        }
        else
        {
            StartFSM(next);
        }
    }

    private void Result()
    {
        InGameUIManager uiManager = mInGameManager.GetUIManager();

        uiManager.OpenResultPop(PlayData.Instance.GameResult,
            delegate ()
            {
#if UNITY_EDITOR
                    Debug.Log("결과 팝업이 종료되고 연출을 설정해야됨.");
#endif
            });
    }

    private void FixedUpdate()
    {
        if (!IsPause())
        {
            if (mCurrentFSM == null)
            {
                Application.Quit();
            }
            else
            {
                mCurrentFSM.NUpdate();
            }
        }
    }

    public eGame NGame
    {
        get
        {
            return mGame;
        }
        set
        {
            mGame = value;
        }
    }
#endregion

    //애니메이션 관리 풀 매니저를 만들어서 처리하는 방식을 이용하면 이런 형태의 함수가 나오지 않아도 되나
    //시간 상 너무 오래 걸리는 관계로 가라로 만듬
    public void UpdateMotion(GameUnitObject unit, GameUnit.eMotion motion)
    {
        unit.UpdateMotion(motion);
    }

    public void UpdateMotion(List<GameUnitObject> units, GameUnit.eMotion motion)
    {
        for (int i = 0; i < units.Count; ++i)
        {
            units[i].UpdateMotion(motion);
        }
    }

    protected override void Closing()
    {
        if (mInGameManager != null)
        {
            base.Closing();
        }
        else
        {
            Debug.LogError("InGameManager is null");
        }
    }

    public GameManager GetGameManager()
    {
        return mInGameManager.GetGameManager();
    }

    public CameraManager GetCameraManager()
    {
        return mInGameManager.GetCameraManager();
    }

    public DataStage GetStage()
    {
        return mInGameManager.GetStage();
    }

    public int GetMaxWave()
    {
        return mInGameManager.GetMaxWave();
    }

    public List<FormationField> Pacs
    {
        get
        {
            return mInGameManager.GetPlatoonPlayers();
        }
    }

    public List<FormationField> WaveMobs
    {
        get
        {
            return mInGameManager.GetWaveMobs();
        }
    }

    public int CurPlatoonIndex
    {
        get
        {
            return mInGameManager.CurPlatoonIndex;
        }
        set
        {
            mInGameManager.CurPlatoonIndex = value;
        }
    }

    public int CurWaveIndex
    {
        get
        {
            return mInGameManager.CurWaveIndex;
        }
        set
        {
            mInGameManager.CurWaveIndex = value;
        }
    }

    //인 게임 전체가 중지상태로 됨.
    //FSM 중지는 FSM에 해당되는 범위에서 중지상태가 되고 그렇지 않는 객체는 계속해서 움직인다.
    public bool IsPause()
    {
        return
            NGame == eGame.None ||
            NGame == eGame.Pause;
    }

    public bool IsBattle()
    {
        return NGame == eGame.Play &&
            mCurrentFSM.GetGameMode() == 1;
    }

    public void OnNoticeByServer()
    {
        mCurrentFSM.OnNoticeByServer();
	}

	public void OnNoticePVPStartNextTag()
	{
		mCurrentFSM.OnNoticePVPStartNextTag();
	}

	public void SetFinishBattle(PlayData.eGameResult rslt)
	{
		InGamePlay gPlay = mCurrentFSM as InGamePlay;
		if (gPlay != null)
		{
			PlayData.Instance.GameResult = rslt;
			gPlay.NextFSM(InGamePlay.ePlay.Result);
		}
	}

    public GameObject CreateMove(bool isPlayer, int i)
    {
        GameObject obj = new GameObject();

#if UNITY_EDITOR
        obj.name = (isPlayer) ? ("Player" + (i + 1)) : ("Enemy" + (i + 1));
#endif
        obj.transform.SetParent((isPlayer) ? MovePacs : MoveMobs);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;

        return obj;
    }

    private List<InGameFSM> GetFSMList()
    {
        List<InGameFSM> fsmList = null;

        switch (PlayData.Instance.PlayMode)
        {
            case PlayData.ePlayMode.Single:
                fsmList = new List<InGameFSM>
                {
                    //인덱스 순서는 eGame 참고 
                    new InGameEntrySingle(),//0 => eGame.Entry
                    new InGamePlay(),//1 => eGame.Play
                    new InGameResult()//2 => eGame.Result
                };
                break;

            case PlayData.ePlayMode.PvP:
            case PlayData.ePlayMode.PTvPT:
                fsmList = new List<InGameFSM>
                {
                    //인덱스 순서는 eGame 참고 
                    new InGameEntryPvP(),//0 => eGame.Entry
                    new InGamePlay(),//1 => eGame.Play
                    new InGameResult()//2 => eGame.Result
                };
                break;
        }

        return fsmList;
    }

    public Transform MovePac
    {
        get
        {
            return MovePacs.GetChild(CurPlatoonIndex);
        }
    }

    public Transform MoveMob
    {
        get
        {
            return MoveMobs.GetChild(CurWaveIndex);
        }
    }


	private Transform MovePacs;
    private Transform MoveMobs;
    
    private InGameManager mInGameManager;

    private List<InGameFSM> mFSMList;
    private InGameFSM mCurrentFSM;

    private eGame mGame = eGame.None;
    
    public enum eGame : byte
    {
        Entry = 0,
        Play,
        Result,
        Replay,
        Pause,
        Resume,
        None = 127
    }
}
