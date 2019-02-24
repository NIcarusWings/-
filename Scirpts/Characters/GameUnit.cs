using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ePlayType : byte
{
    Player, NPC, Enemy, None = 127
}

public enum eUnitType : byte
{
    Character, NormalUnit,
}

public abstract class GameUnit : MonoBehaviour
{
    protected void Initialize(/*int[] skillIndexs, int[] skillLevels*/)
    {
        State = eState.StandBy;
    }
    #region 현재 수정 불가
    //InformationCharacter 과 MobInfo를 현재 수정할 수가 없음.
    //GameUnitObject에서 언급했듯이 정보와 데이터를 하나로 합쳐서 관리해야되는 방식을 사용했어야됨.
    //현재 이 부분을 수정하자니 인게임뿐만 아니라 UI부분도 많은 영향을 주기 때문.
    public void Set(InformationCharacter ic)
    {
        if (mIC == null)
        {
            mIC = ic;
        }

        SetCommon();

#if UNITY_EDITOR
        CurHP = ic.Hp;// GetRealFaculty(eFaculty.HP, BaseData.NONE_VALUE);
                      //mCurHP = InGameManager.Instance.TPlayerHP;
#else
        CurHP = ic.Hp;
#endif
    }

    public void Set(MobInfo mi)
    {
        if (mMobInfo == null)
        {
            mMobInfo = mi;
        }

        SetCommon();

        CurHP = GetRealFaculty(eFaculty.HP);
    }

    private void SetCommon()
    {
        UpdateFace(eFace.Base);

        //if (CurMotions != null)
        //{
        //    for (int i = 0; i < CurMotions.Length; ++i)
        //    {
        //        CurMotions[i] = eMotion.None;
        //    }
        //}
    }
    #endregion

    public void UpdateMotion(params eMotion[] motion)
    {
        if (mMotionAnimators != null && motion.Length != 0)
        {
            string motionName = null;

            for (int i = 0; i < motion.Length; ++i)
            {
                motionName = GetMotionName(motion[i]);

                if (motionName != null)
                {
                    mMotionAnimators[i].Play(motionName);
                    //CurMotions[i] = motion[i];
                }
            }
        }
    }

    public void StandBy()
    {
        transform.localEulerAngles = Vector3.zero;
    }
    
    public void UpdateScale(float timeScale)
    {
        if (mMotionAnimators != null)
        {
            for (int i = 0; i < mMotionAnimators.Count; ++i)
            {
                mMotionAnimators[i].speed = timeScale;
            }
        }
    }

    public virtual void UpdateFace(eFace newFace)
    {
        if (Face != newFace && mFaceMaterial != null)
        {
            switch (newFace)
            {
                case eFace.Base:
                    mFaceMaterial.SetTextureOffset("_MainTex", Vector2.up * 0.5f);
                    break;

                case eFace.Smail:
                    mFaceMaterial.SetTextureOffset("_MainTex", Vector2.one * 0.5f);
                    break;

                case eFace.Angry:
                    mFaceMaterial.SetTextureOffset("_MainTex", Vector2.zero);
                    break;

                case eFace.Sad:
                    mFaceMaterial.SetTextureOffset("_MainTex", Vector2.left * 0.5f);
                    break;
            }

            Face = newFace;
        }
    }

    public bool IsRetire()
    {
        return State == eState.Retire;
    }

    public NPoint GetFormationPoint()
    {
        NPoint point;

        point.x = 0;
        point.y = GetFormationIndex();

        while (true)
        {
            if (point.y - GameManager.FSquare < 0)
            {
                break;
            }

            point.y -= GameManager.FSquare;

            ++point.x;
        }

        return point;
    }

    public GameUnitObject GetUnitObj()
    {
        return mUnitObj;
    }

    public void GiveBy(float damage)
    {
        CurHP += damage;

        if (CurHP <= 0f)
        {
            CurHP = 0f;
            State = eState.Retire;
        }
    }

    public void OnRetire()
    {
        CurHP = 0f;
        State = eState.Retire;
    }

    //방식 설명
    //Index이므로 0부터 시작함. 즉 기본 무기가 1개이면 인덱스는 0.
    //Count는 Value로써 1부터 시작
    //따라서 인덱스 - Count(Value)는 0보다 작으면 기본 무기, 0 혹은 0보다 클 경우에는 서브무기가 됨.
    public bool IsBaseWeapon(int shooterIsWhoIndex)
    {
        return shooterIsWhoIndex == 0;
        //return (shooterIsWhoIndex - GetBaseWeaponCount()) < 0;
    }

    public float CurHP
    {
        get
        {
            return mCurHP;
        }
        set
        {
            mCurHP = value;
        }   
    }

    public ePlayType PlayType
    {
        get
        {
            return mPlayType;
        }
        set
        {
            mPlayType = value;
        }
    }
    
    protected eState State
    {
        get
        {
            return mState;
        }
        set
        {
            mState = value;
        }
    }

    protected eCondition Condition
    {
        get
        {
            return mCondition;
        }
        set
        {
            mCondition = value;
        }
    }

    protected eFace Face
    {
        get
        {
            return mFace;
        }
        set
        {
            mFace = value;
        }
    }

    public virtual void Pause(bool isPause)
    {
        if (mMotionAnimators != null)
        {
            for (int i = 0;  i < mMotionAnimators.Count; ++i)
            {
                UpdateScale((isPause) ? 0 : InGameManager.Instance.GetTimeScale());
            }
        }
    }

    public virtual int GetStandByIndex()
    {
        return BaseData.NONE_VALUE;
    }

    public virtual int GetExp()
    {
        return BaseData.NONE_VALUE;
    }

    public virtual string GetResultCharName()
    {
        return null;
    }

    #region 빌더 패턴을 쓸까말까 고민 중
    //이 원리는 3D팀에서 관리하는 테이블의 [애니메이션 관리(뉴 캐릭터)]를 참고.
    protected virtual string GetMotionNameBy(eMotion motion)
    {
        return motion.ToString();
    }

    protected virtual int GetMotionCount(eMotion motion)
    {
        return 1;
    }
    #endregion
    public virtual void SetLevel(int value)
    {

    }

	public virtual string GetName()
	{
		return null;
	}

	public virtual void SetCharInfo(int level, int exp)
	{
	}

    public void SetPosition(Vector3 pos)
    {
        transform.localPosition = pos;
    }

	public GameObject GetChar2DImage(bool isSummon = false)
	{
		if(mIC == null)
			return null;

		return mIC.GetFullPrefab(isSummon);
	}

	[SerializeField]
	protected Transform mShotPoint = null;
	public virtual Vector3 ShotPoint
	{
		get
		{
			// TODO : 각 캐릭 마다 ShotPoint 세팅
			if (mShotPoint == null)
			{
				Vector3 shotPos = transform.position;
				shotPos.y = 8f;
				shotPos.z += 4f * (transform.forward.z >= 0f ? 1f : -1f);

				return shotPos;
			}

			return mShotPoint.position;
		}
	}

    public abstract int GetUnitIndex();
    public abstract int GetMechType();
    //쿨타임 역시 해당 열거형에 포함되어있음. 이에 대한 설명은 열거형 선언부분을 참고.
    public abstract float GetRealFaculty(eFaculty faculty);
    public abstract int GetFormationIndex();

    public abstract string GetRscName();
    public abstract int GetStar();
    public abstract int GetLevel();
    public abstract int GetMainWeaponCount();

    public abstract int[] GetSkillIds();
    public abstract int[] GetSkillLevels();//데이터의 레벨일 수도 있고 스킬을 강화한 레벨일 수도 있음.

    public abstract void ReadyBattle();
    public abstract void Restart();

    public virtual void NReset(eBattleState battleState)
    {
        switch (battleState)
        {
            case eBattleState.Retire:
                State = eState.Retire;
                CurHP = 0;
                break;

            case eBattleState.ClearWave:
                UpdateMotion(eMotion.ingame_idle);
                break;
        }
    }

    protected abstract string GetMotionName(eMotion motion);

    protected InGameHUDItem mHudItem;

    protected GameUnitObject mUnitObj;

    protected List<Animator> mMotionAnimators;
    protected Material mFaceMaterial;//캐릭터 표정 재질
    //protected List<InGameSkill> mSkillList = null; 

    protected float mCorrectBulletSpeed = 1f;

    private float mCurHP;

    [HideInInspector] public int EnemyCount = 0;
    [HideInInspector] public float GiveTotalDamge = 0;
    [HideInInspector] public float Recovery = 0f;

    protected InformationCharacter mIC;
    protected MobInfo mMobInfo;

    #region PlayType
    private ePlayType mPlayType = ePlayType.None;
    #endregion

    public eUnitType UnitType = eUnitType.Character;

    #region State
    private eState mState = eState.StandBy;
    private eCondition mCondition = eCondition.None;

    public enum eState : byte { StandBy, Deploy, Dispatch, Retire }
    public enum eCondition : byte { None = 0, Provocation/*도발*/ };
    #endregion

    #region Face
    private eFace mFace = eFace.Angry;

    public enum eFace : byte { Base, Smail, Angry, Sad }
    #endregion

    #region Motion
    //protected eMotion[] CurMotions;

    //캐릭터이든 메카닉이든 구분없이 모두 하나를 통해서 사용함.
    public enum eMotion : byte
    {
        normal_attack = 0, skill_attack, damage,
        lobby_idle, ingame_idle,
        walk, run,
        victory, sit,
        //캐릭터는 동작하지 않고 로봇에는 동작할 경우 다음과 같이 매개인자를 넘겨주면 됨
        //->UpdateMotion(None, Idle)
        None = 127
    }
    #endregion
}