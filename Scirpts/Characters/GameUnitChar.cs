using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUnitChar : GameUnit
{
    public void Initialize(GameUnitObject unitObj)
    {
        base.Initialize();

        UnitType = eUnitType.Character;

        if (mUnitObj == null)
        {
            mUnitObj = unitObj;
            mMotionAnimators = new List<Animator>();

            //캐릭터 표정을 가져옴.
            Transform realModel = transform.GetChild(0);
            SkinnedMeshRenderer smr = null;
            for (int i = realModel.childCount - 1; i >= 0; --i)
            {
                if (realModel.GetChild(i).name.Contains("face"))
                {
                    smr = realModel.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                    break;
                }
            }

            if (smr != null)
            {
                mFaceMaterial = smr.material;
            }

            mMotionAnimators.Add(transform.GetChild(0).GetComponent<Animator>());
            //CurMotions = new eMotion[mMotionAnimators.Count];
        }
    }

    protected override string GetMotionName(eMotion motion)
    {
        string value = null;
        string motionName = GetMotionNameBy(motion);
        string rscName = null;
        int motionCount = GetMotionCount(motion);
        int costumeType = BaseData.NONE_VALUE;

        if (mIC != null && mMobInfo == null)
        {
            rscName = mIC.Data.Rsc_Name;
            costumeType = mIC.GetDataCostume().CostumeType;

            //if (IsExtra(rscName))
            //{
            //    value = (motionName + "_" + //idle_
            //        mIC.GetDataCostume().GetAnimationKey()) + "_" + //idle_short_skirts_
            //        costumeType.ToString("00") + "@";//idle_short_skirts_XX@
            //}
            //else
            {
                if (mIC.CostumeIndex == 0)
                {
                    value = (motionName + "_ch_" + //idle_ch
                        mIC.Data.Rsc_Name) + "_" + //idle_ch_sooji_
                        motionCount.ToString("00") + "@";//idle_ch_sooji_01@
                }
                else
                {
                    value = (motionName + "_ch_" + //idle_ch
                        mIC.Data.Rsc_Name) + "_" + //idle_ch_sooji_
                        motionCount.ToString("00") + "_" +
                        costumeType.ToString("00") + "@";//idle_ch_sooji_01_0X@
                }
            }
        }
        //mIC == null && mDI != null
        else
        {
            if (mMobInfo.UnitType == eUnitType.Character)
            {
                rscName = mMobInfo.Data.Rsc_Name;
                costumeType = mMobInfo.GetDataCostume().CostumeType;

                //if (IsExtra(rscName))
                //{
                //    value = (motionName + "_" + //idle_
                //        mMobInfo.GetDataCostume().GetAnimationKey()) + "_" + //idle_short_skirts_
                //        costumeType.ToString("00") + "@";//idle_short_skirts_XX@   
                //}
                //else
                {
                    if (costumeType == 1)
                    {
                        value = (motionName + "_ch_" + //idle_ch
                            mMobInfo.Data.Rsc_Name) + "_" + //idle_ch_sooji_
                            motionCount.ToString("00") + "@";//idle_ch_sooji_01@
                    }
                    else
                    {
                        value = (motionName + "_ch_" + //idle_ch
                            mMobInfo.Data.Rsc_Name) + "_" + //idle_ch_sooji_
                            motionCount.ToString("00") + "_" +
                            costumeType.ToString("00") + "@";//idle_ch_sooji_01_0X@
                    }
                }
            }
        }

        value = value.ToLower();
        
        return value;
    }

    protected override string GetMotionNameBy(eMotion motion)
    {
        string result = null;

        switch (motion)
        {
            case eMotion.ingame_idle:
            case eMotion.lobby_idle:
                result = "idle";
                break;

            default:
                result = base.GetMotionNameBy(motion);
                break;
        }

        return result;
    }

    protected override int GetMotionCount(eMotion motion)
    {
        int result = BaseData.NONE_VALUE;

        switch (motion)
        {
            case eMotion.lobby_idle:
                result = 2;
                break;

            default:
                result = base.GetMotionCount(motion);
                break;
        }

        return result;
    }

    //private bool IsExtra(string rscName)
    //{
    //    string newRscName = null;
    //    bool isExtra = true;
    //    int startIndex = 0;
    //    int lenght = 0;
    //    for (int i = 0; i < mNewMotionNames.Length; ++i)
    //    {
    //        startIndex = mNewMotionNames[i].IndexOf('_') + 1;
    //        lenght = mNewMotionNames[i].LastIndexOf('_');
    //        newRscName = mNewMotionNames[i].Substring(startIndex, lenght - startIndex);

    //        if (newRscName.CompareTo(rscName) == 0)
    //        {
    //            isExtra = false;
    //            break;
    //        }
    //    }

    //    return isExtra;
    //}

    //private string[] mNewMotionNames = new string[]
    //{
    //    "ch_sooji_01", "ch_jiwoo_01", "ch_nana_01", "ch_eun_01", "ch_hara_01",
    //    "ch_saena_01", "ch_claire_01", "ch_fletcher_01", "ch_saeqeh_01", "ch_sora_01",
    //    "ch_yuri_01", "ch_iru_01", "ch_hani_01", "ch_titi_01", "ch_choa_01", "ch_haruna_01"
    //};

    public override void UpdateFace(eFace newFace)
    {
        if (Face != newFace && mFaceMaterial != null)
        {
            string rscName = null;

            if (mIC != null && mMobInfo == null)
            {
                rscName = mIC.Data.Rsc_Name;
            }
            else
            {
                if (mMobInfo.UnitType == eUnitType.Character)
                {
                    rscName = mMobInfo.Data.Rsc_Name;
                }
            }

            if (rscName != null)
            {
                //if (IsExtra(rscName))
                //{
                //    switch (newFace)
                //    {
                //        case eFace.Base:
                //            mFaceMaterial.SetTextureOffset("_MainTex", Vector2.up * 0.5f);
                //            break;

                //        case eFace.Smail:
                //            mFaceMaterial.SetTextureOffset("_MainTex", Vector2.one * 0.5f);
                //            break;

                //        case eFace.Angry:
                //            mFaceMaterial.SetTextureOffset("_MainTex", Vector2.zero);
                //            break;

                //        case eFace.Sad:
                //            mFaceMaterial.SetTextureOffset("_MainTex", Vector2.left * 0.5f);
                //            break;
                //    }
                //}
                //else
                {
                    switch (newFace)
                    {
                        case eFace.Base:
                            mFaceMaterial.SetTextureOffset("_MainTex", Vector2.zero);
                            break;

                        case eFace.Smail:
                            mFaceMaterial.SetTextureOffset("_MainTex", Vector2.right * 0.5f);
                            break;

                        case eFace.Angry:
                            mFaceMaterial.SetTextureOffset("_MainTex", Vector2.up * 0.5f);
                            break;

                        case eFace.Sad:
                            mFaceMaterial.SetTextureOffset("_MainTex", Vector2.one * 0.5f);
                            SoundManager.PlayCharFx(GetRscName(), SoundManager.CharState.lose, 1, false);
                            break;
                    }
                }

                Face = newFace;
            }
        }
    }

    public override void NReset(eBattleState battleState)
    {
        base.NReset(battleState);

        switch (battleState)
        {
            case eBattleState.Retire:
                StartCoroutine(Animation_Retire());
                break;

            case eBattleState.ClearWave:
                //할 일
                break;
        }
    }

    private IEnumerator Animation_Retire()
    {
        CameraManager cm = InGameManager.Instance.GetCameraManager();

        if (cm != null)
        {
            cm.StartShake(1.0f);
        }

        UpdateFace(eFace.Sad);
        UpdateMotion(eMotion.run);

        //거리 = 속도 * 시간
        float v = 40f;
        float limitTime = 2f;
        float curTime = 0f;

        transform.localEulerAngles = Vector3.up * 180f;

        while (true)
        {
            if (curTime >= limitTime)
            {
                break;
            }

            yield return null;

            if (PlayType == ePlayType.Player)
            {
                transform.position -= Vector3.forward * v * GameManager.DeltaTime;
            }
            else
            {
                transform.position += Vector3.forward * v * GameManager.DeltaTime;
            }

            curTime += GameManager.DeltaTime;
        }

        mUnitObj.Refresh();
    }

    //몹 캐릭터와 관련이 없음.
    public override void SetLevel(int value)
    {
        mIC.Level = value;
    }
    
    //몹 캐릭터와 관련이 없음.
    public override int GetExp()
    {
        return mIC.Exp;
    }

    //몹 캐릭터와 관련이 없음.
    public override string GetResultCharName()
    {
        return mIC.CharName;
    }

    public override int GetUnitIndex()
    {
        if (mIC != null && mMobInfo == null)
        {
            return mIC.DataIndex;
        }
        //mIC == null && mDI != null
        else
        {
            return mMobInfo.Data.Index;
        }
    }

    public override int GetMainWeaponCount()
    {
        int[] weaponIndexs = null;
        int count = 0;

        if (mIC != null && mMobInfo == null)
        {
            weaponIndexs = mIC.Data.GetWeapons();
        }
        //mIC == null && mDI != null
        else
        {
            weaponIndexs = mMobInfo.Data.GetWeapons();
        }

        for (int i = 0; i < weaponIndexs.Length; ++i)
        {
            if (weaponIndexs[i] == BaseData.NONE_VALUE ||
                weaponIndexs[i] == 0)
            {
                break;
            }
            else
            {
                ++count;
            }
        }

        return count;
    }

    public override int GetMechType()
    {
        if (mIC != null && mMobInfo == null)
        {
            return mIC.Data.Type;
        }
        //mIC == null && mDI != null
        else
        {
            return mMobInfo.Data.Type;
        }
    }

    public override int GetFormationIndex()
    {
        if (mIC != null && mMobInfo == null)
        {
            return mIC.FormationIndex - 1;
        }
        //mIC == null && mDI != null
        else
        {
            return mMobInfo.FormationValue;
        }
    }

    public override float GetRealFaculty(eFaculty faculty)
    {
        float value = 0f;

        if (mIC != null && mMobInfo == null)
        {
            value = mIC.GetRealFaculty(faculty);
        }
        //mIC == null && mDI != null
        else
        {
            value = mMobInfo.GetRealFaculty(faculty);
        }

        return value;
    }

    public override int[] GetSkillIds()
    {
        if (mIC != null && mMobInfo == null)
        {
            return mIC.Data.GetSkills();
        }
        //mIC == null && mDI != null
        else
        {
            return mMobInfo.Data.GetSkills();
        }
    }

    public override int[] GetSkillLevels()
    {
        if (mIC != null && mMobInfo == null)
        {
            return mIC.GetSkillLevels();
        }
        //mIC == null && mDI != null
        else
        {
            return mMobInfo.Data.GetSkills();
        }
    }

    public override string GetRscName()
    {
        if (mIC != null && mMobInfo == null)
        {
            return mIC.Data.Rsc_Name;
        }
        //mIC == null && mDI != null
        else
        {
            return mMobInfo.Data.Rsc_Name;
        }
    }

    public override void ReadyBattle()
    {
        switch (PlayType)
        {
            case ePlayType.Player:
                transform.localEulerAngles = Vector3.up * 30f;
                break;

            case ePlayType.Enemy:
                if (PlayData.Instance.IsPvPMode())
                {
                    transform.localEulerAngles = Vector3.up * 330f;
                }
                break;
        }
    }

    public override void Restart()
    {
        if (mIC != null && mMobInfo == null)
        {
            Set(mIC);
        }
        else
        {
            Set(mMobInfo);
        }
    }

    //몹 캐릭터와 관련이 없음.
    public override string GetName()
	{
		return LangManager.Instance.GetText(mIC.Data.Rsc_Name);
	}

	//몹 캐릭터와 관련이 없음.
	public override int GetStar()
    {
        return mIC.Star;
    }
    
    //몹 캐릭터와 관련이 없음.
    public override int GetLevel()
    {
        return mIC.Level;
    }

    public int GetBaseWeaponCount()
    {
        return 1;
    }

    public override void SetCharInfo(int level, int exp)
	{
		mIC.Hp = (int)CurHP;

        if (level > 0)
        {
            mIC.Level = level;
            mIC.Exp = exp;
        }
	}
}
