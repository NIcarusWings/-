using UnityEngine;
using System.Collections.Generic;

//공격 포인터는 캐릭터에 달려있지 않고 이 스크립트 안에 존재함.
//기획상 기본 공격 포인터는 캐릭터의 정보에 의존하고, 서브 공격 포인터는 장착되는 장비에 의존된다.
public class MechBody : MonoBehaviour, IChainOfResponsibility
{
    //기본 공격 포인터의 능력치가 캐릭터에 의존해서 결정될 경우 이 함수를 호출.
    public void Initialize(GameUnitObject unitObject, MechBodyData bodyData)
    {
        Transform tr = transform.GetChild(0);//tr = 메카닉 이름(애니메이터가 부착되어있음)

        mHUDPoint = tr.Find("HUDPoint");
        Joint = tr.Find("joint");

        if (mAnimator == null)
        {
            mAnimator = transform.GetComponentInChildren<Animator>();
        }

        //비행기와 같이 몸체를 회전시킬 필요가 있는 객체의 경우에는 joint를 부모로써 시작됨.
        if (Joint != null)
        {
            tr = Joint;
        }

        HitPoint = tr.Find("HitPoint");
        //HitPoint = transform.Find("HitPoint");

        if (HitPoint != null)
        {
            HitPoint.GetComponent<Collider>().enabled = false;
        }

        mUnitObject = unitObject;
        NBodyData = bodyData;
    }

    //로비, 인게임에서 공통적으로 처리되어야될 함수.
    public void Set(int bodyType, MechWeaponData[] weapons)
    {
        if (weapons != null && weapons[0] != null)
        {
            mWeapons = new List<MechWeapon>();
            #region 기본 무기 설정
            GameObject go = null;
            MechWeapon mechWeapon = null;

            for (int i = 0; i < weapons.Length; ++i)
            {
                if (i >= WeaponParents.Length) break;

                if (WeaponParents[i] != null && weapons[i] != null)
                {
                    go = GameManager.Summon(weapons[i].ResourceName, false);

                    if (go != null)
                    {
                        go.transform.SetParent(WeaponParents[i]);
                        go.transform.localPosition = Vector3.zero;
                        go.transform.localRotation = Quaternion.identity;
                        go.transform.localScale = Vector3.one;

                        mechWeapon = go.AddComponent<MechWeapon>();
                        mechWeapon.Initialize(this, weapons[i]);
                        mWeapons.Add(mechWeapon);
                    }
                }
            }

            if (mUnitObject.GetUnit().UnitType == eUnitType.Character)
            {
                if (mWeapons != null && mWeapons.Count != 0)
                {
                    mWeapons[0].SetFirstMainShooter();
                }
            }
            #endregion
        }

        mBodyType = bodyType;
    }

    public void SetInGame(ePlayType playType)
    {
        if (HitPoint != null)
        {
            HitPoint.GetComponent<Collider>().enabled = true;
        }

        if (mFlashWhite == null)
        {
            mFlashWhite = new FlashWhite();
            mFlashWhite.Initialize(gameObject);
        }

        gameObject.tag = (playType == ePlayType.Enemy) ? "Enemy" : "Player";
    }

    public void StandBy()
    {
        mState = eState.Rotation;
        mCurTime = mRotTime * 0.1f;

        if (mWeapons != null)
        {
            if (Joint != null)
            {
                Joint.localEulerAngles = Vector3.zero;   
            }

            for (int i = 0; i < mWeapons.Count; ++i)
            {
                mWeapons[i].StandBy();
            }
        }
    }

    public void NUpdate(GameUnitObject target)
    {
        if (target != null && mWeapons != null)
        {
            switch (mState)
            {
                case eState.Rotation:
                    if (Joint != null && target.HitPoint != null)
                    {
                        mCurTime -= GameManager.DeltaTime;

                        Joint.rotation = Quaternion.Slerp(
                            Joint.rotation,
                            Quaternion.LookRotation(target.HitPoint.position - Joint.position),
                            mRotTime * GameManager.DeltaTime);

                        if (mCurTime < 0)
                        {
                            mState = eState.None;
                        }
                    }
                    else
                    {
                        mState = eState.None;
                    }
                    break;
            }

            for (int i = 0; i < mWeapons.Count; ++i)
            {
                if (InGameManager.Instance.TIsForbbienMainWeapon)
                {
                    GameUnitObject guo = GetUnitObject();
                    GameUnit gu = guo.GetUnit();
                    int mainWeaponCont = gu.GetMainWeaponCount();

                    if (i < mainWeaponCont)
                    {
                        continue;
                    }
                }

                mWeapons[i].NUpdate(target);
            }
        }

        if (mFlashWhite != null)
        {
            mFlashWhite.NUpdate();
        }
    }

    public void NReset(eBattleState battleState)
    {
        switch (battleState)
        {
            case eBattleState.Retire:
                Vector3 effectPos = HitPoint.position + Vector3.right * 5f;

                InGameManager.SPlayFx(this.GetRetireFx(), effectPos, HitPoint.lossyScale, transform.localEulerAngles);
                InGameManager.SPlaySfx(effectPos, this.GetRetireSfx());

                if (mWeapons != null)
                {
                    for (int i = 0; i < mWeapons.Count; ++i)
                    {
                        mWeapons[i].NReset(battleState);
                    }
                }

                gameObject.SetActive(false);
                break;

            case eBattleState.ClearWave:
            case eBattleState.EndGame:
                if (mWeapons != null)
                {
                    for (int i = 0; i < mWeapons.Count; ++i)
                    {
                        mWeapons[i].NReset(battleState);
                    }
                }
                break;
        }
    }
    
    public void UpdateMotion(string motionName)
    {
        if (mAnimator != null)
        {
            //mAnimator.Play(motionName);
        }
    }

    public void Restart()
    {
        gameObject.SetActive(true);

        if (mWeapons != null)
        {
            for (int i = 0; i < mWeapons.Count; ++i)
            {
                mWeapons[i].Restart();
            }
        }
    }

    public void Pause(bool isPause)
    {
        if (mWeapons != null)
        {
            for (int i = 0; i < mWeapons.Count; ++i)
            {
                mWeapons[i].Pause(isPause);
            }
        }
    }

    public void SetPosition(Space space, Vector3 pos)
    {
        switch (space)
        {
            case Space.World:
                transform.position = pos;
                break;

            case Space.Self:
                transform.localPosition = pos;
                break;
        }
    }

    public void SetHitPointPosition(Space space, Vector3 pos)
    {
        if (HitPoint != null)
        {
            switch (space)
            {
                case Space.World:
                    HitPoint.transform.position = pos;
                    break;

                case Space.Self:
                    HitPoint.transform.localPosition = pos;
                    break;
            }
        }
    }

    public GameUnitObject GetUnitObject()
    {
        return mUnitObject;
    }

    public float GetBuff(eFaculty faculty)
    {
        return mUnitObject.GetBuff(faculty);
    }

    public string GetTag()
    {
        return gameObject.tag;
    }

    public int GetKindType()
    {
        return mBodyType;
    }

    public string GetRetireFx()
    {
        return NBodyData.DeathFX;
    }

    public string GetRetireSfx()
    {
        return NBodyData.DeathSfx;
    }

    private bool IsHideWeapon(string path)
    {
        return string.IsNullOrEmpty(path) || path.Length < 3;
    }

    public void Handle(CRMessage message)
    {
        switch (message.Context)
        {
            case CRMessage.eContext.COMMON_UNDER_ATTACK:
                if (message.BValue)//크리티컬이면(BValue가 true이면 크리티컬임)
                {
                    if (mFlashWhite != null)
                    {
                        mFlashWhite.NStart();
                    }
                }
                //{
                //    InGameManager.SPlayFx(message.SValue(0), HitPoint.position, HitPoint.lossyScale, transform.localEulerAngles);
                //    InGameManager.SPlaySfx(HitPoint.position, message.SValue(1));
                //}
                break;

            //case CRMessage.eContext.UNDER_ATTACK_Y_ZERO:
            //    {
            //        //Vector3 pos = HitPoint.position;
            //        Vector3 pos = transform.position;

            //        pos.y = 0.1f;

            //        InGameManager.SPlayFx(message.SValue(0), pos, HitPoint.lossyScale, transform.localEulerAngles);
            //        InGameManager.SPlaySfx(HitPoint.position, message.SValue(1));
            //    }
            //    break;

            default:
                break;
        }
    }

	public Shooter GetMainShooter()
	{
		return mWeapons[0].GetMainShooter();
	}

    public void SetRot()
    {
        mState = eState.Rotation;
        mCurTime = mRotTime * 0.1f;

        if (mWeapons != null)
        {
            for (int i = 0; i < mWeapons.Count; ++i)
            {
                mWeapons[i].SetRot();
            }
        }
    }

    public MechBodyData NBodyData;
    public Transform[] WeaponParents;

    [HideInInspector] public Transform HitPoint;
    [HideInInspector] public Transform Joint;

    private Animator mAnimator;

    private GameUnitObject mUnitObject;

    private List<MechWeapon> mWeapons;
    private Transform mHUDPoint;

    private FlashWhite mFlashWhite;
    
    private int mBodyType;

    private float mCurTime = 0f;
    private float mRotTime = 64f * 0.1f;

    private eState mState = eState.None;

    //상태 FSM만들 여력이 안됨.
    private enum eState : byte
    {
        StandBy = 0,
        Rotation,
        None = 127,
    }
}
