using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//무기는 하나인데 해당 무기에서 발사체가 여러개 일수가 있다.(대표적으로 개틀링)
public class MechWeapon : MonoBehaviour
{
    public void Initialize(MechBody body, MechWeaponData weaponData)
    {
        mWeaponData = weaponData;
        mMechBody = body;
        mShooters = new List<Shooter>();

        #region 기본 무기 설정
        mJoint = transform.Find("joint");
		if(mJoint == null)
			mJoint = transform.GetChild(0).Find("joint");

		//if (mJoint == null)
		//{
		//    mJoint = transform.Find("EmptyWeapon/joint");
		//}

		Transform gun = mJoint.Find("gun");
        Transform attackPoint = null;
        Shooter shooter = null;

        for (int i = 0; i < gun.childCount; ++i)
        {
            attackPoint = gun.GetChild(i);

            if (shooter == null)
            {
                shooter = attackPoint.gameObject.AddComponent<Shooter>();
            }

            shooter.Initialize(new Percussion(this));

            mShooters.Add(shooter);
        }
        #endregion
    }

    public void SetFirstMainShooter()
    {
        if (mShooters != null && mShooters.Count != 0)
        {
            mShooters[0].SetFirstMainShooter();
        }
    }

    public void StandBy()
    {
        mJoint.localEulerAngles = Vector3.zero;
        mCurTime = mRotTime * 0.1f;

        mState = eState.Rotation;
    }

    public void NUpdate(GameUnitObject target)
    {
        if (target != null)
        {
            switch (mState)
            {
                case eState.Rotation:
                    if (mJoint != null && target.HitPoint != null)
                    {
                        mCurTime -= GameManager.DeltaTime;

                        mJoint.rotation = Quaternion.Slerp(
                            mJoint.rotation,
                            Quaternion.LookRotation(target.HitPoint.position - mJoint.position),
                            mRotTime * GameManager.DeltaTime);
                        
                        if (mCurTime < 0)
                        {
                            mState = eState.Shot;
                        }
                    }
                    else
                    {
                        mState = eState.Shot;
                    }
                    break;

                case eState.Shot:
                    for (int i = 0; i < mShooters.Count; ++i)
                    {
                        mShooters[i].Shot(target);
                    }
                    break;
            }
        }

        Target = target;
    }

    public void Pause(bool isPause)
    {
        for (int i = 0; i < mShooters.Count; ++i)
        {
            mShooters[i].Pause(isPause);
        }
    }

    public void NReset(eBattleState battleState)
    {
        for (int i = 0; i < mShooters.Count; ++i)
        {
            mShooters[i].NReset(battleState);
        }
    }

    public void Restart()
    {
        for (int i = 0; i < mShooters.Count; ++i)
        {
            mShooters[i].Restart();
        }
    }

    public MechBody GetMechBody()
    {
        return mMechBody;
    }

    public int GetKindType()
    {
        return mMechBody.GetKindType();
    }

	public Shooter GetMainShooter()
	{
		return mShooters[0];
	}

    public MechWeaponData.eBulletType BulletType
    {
        get
        {
            return mWeaponData.BulletType;
        }
    }

    public MechWeaponData.eBulletMoveType BulletMoveType
    {
        get
        {
            return mWeaponData.BulletMoveType;
        }
    }

	public MechWeaponData.eHitType BulletHitType
	{
		get
		{
			return mWeaponData.BulletHitType;
		}
	}

	public GameUnit GetUnit()
    {
        return mMechBody.GetUnitObject().GetUnit();
    }

    public void SetRot()
    {
        mState = eState.Rotation;

        mCurTime = mRotTime * 0.1f;
    }

    public float Atk
    {
        get
        {
            return mWeaponData.Atk + 
                (mWeaponData.Atk * GetBuff(eFaculty.Atk));
        }
    }

    public float Acc
    {
        get
        {
            return mWeaponData.Acc + 
                (mWeaponData.Acc * GetBuff(eFaculty.Acc));
        }
    }

    public float AtkTime
    {
        get
        {
            return mWeaponData.AtkTime/* +
                (mWeaponData.AtkTime * GetBuff(eFaculty.AtkTime))*/;
        }
    }

    public float Cri
    {
        get
        {
            return mWeaponData.Cri + 
                (mWeaponData.Cri * GetBuff(eFaculty.Cri));
        }
    }

    public float Cri_Dmg
    {
        get
        {
            return mWeaponData.Cri_Dmg + 
                (mWeaponData.Cri_Dmg * GetBuff(eFaculty.Cri_Dmg));
        }
    }

    public float BulletSpeed
    {
        get
        {
            return mWeaponData.BulletSpeed;
        }
    }

    public float AtkFrame
    {
        get
        {
            float deltaTime = GameManager.DeltaTime;
            float customRate = GetBuff(eFaculty.AtkTime);

            deltaTime *= (1f + customRate);

            if (deltaTime < 0f)
            {
                deltaTime = 0f;
            }

            return deltaTime;
        }
    }

    //NFSM에서도 생각했지만 부모가 가지고 있는 것을 자식이 참조를 해야되는데 현재 구조적으로 설계가 되어있지 않는 관계로 링크를 타는 방식으로 가는데 여간 불편한게 아님...
    //상속과는 좀 다른 개념으로써 복합체가 부모인데 이 부분이 자꾸 발목을 잡고 있는 중.
    //유니티에서 제공하는 GetComponentInParent 라는 함수를 응용한 설계를 도입하면 개발에 좀 수월해질 것이라는 생각이 드는데 알아봐야겠음.
    private float GetBuff(eFaculty faculty)
    {
        return mMechBody.GetBuff(faculty);
    }

    public MechWeaponData Data
    {
        get
        {
            return mWeaponData;
        }
    }

    public string BulletFx
    {
        get
        {
            return mWeaponData.BulletFx;
        }
    }

    public string ShotFX
    {
        get
        {
            return mWeaponData.ShotFx;
        }
    }

    public string ShotSFX
    {
        get
        {
            return mWeaponData.ShotSfx;
        }
    }

    public string HitFX
    {
        get
        {
            return mWeaponData.HitFx;
        }
    }

    public string HitSfx
    {
        get
        {
            return mWeaponData.HitSfx;
        }
    }

    public GameUnitObject Target;

    private MechWeaponData mWeaponData;

    private Transform mJoint;

    private List<Shooter> mShooters;

    private MechBody mMechBody;

    private float mCurTime = 0f;
    private float mRotTime = 64f * 0.1f;
    private eState mState = eState.None;

    //상태 FSM만들 여력이 안됨.
    private enum eState : byte
    {
        StandBy = 0,
        Rotation,
        Shot,
        None = 127,
    }
}