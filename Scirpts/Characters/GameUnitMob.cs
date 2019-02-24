using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUnitMob : GameUnit
{
    //자세한 것은 GameUnitChar클래스의 Initialize 부분을 참고하여 유사한 부분을 찾아서 초기화할 것.
    public void Initialize(GameUnitObject unitObj, MobInfo mi)
    {
        base.Initialize();

        UnitType = eUnitType.NormalUnit;

        if (mUnitObj == null)
        {
            mUnitObj = unitObj;
            mMobInfo = mi;
        }
    }

    public override int GetUnitIndex()
    {
        return mMobInfo.Data.Index;
    }

    public override int GetMechType()
    {
        return mMobInfo.Data.Type;
    }

    public override int GetFormationIndex()
    {
        return mMobInfo.FormationValue;
    }

    public override int GetStandByIndex()
    {
        return mMobInfo.StandByIndex;
    }

    public override void NReset(eBattleState battleState)
    {
        base.NReset(battleState);

        mUnitObj.Refresh();
    }

    public override void Restart()
    {
        if (mMobInfo != null)
        {
            Initialize(mUnitObj, mMobInfo);
            Set(mMobInfo);
        }
    }

    protected override string GetMotionName(eMotion motion)
    {
        return null;
    }

    public override float GetRealFaculty(eFaculty faculty)
    {
        float value = mMobInfo.GetRealFaculty(faculty);

        return value;
    }

    public override string GetRscName()
    {
        return mMobInfo.Data.Rsc_Name;
    }

    public override int[] GetSkillIds()
    {
        return mMobInfo.Data.GetSkills();
    }

    public override int[] GetSkillLevels()
    {
        return mMobInfo.Data.GetSkillLevels();
    }

    public override int GetStar()
    {
        return BaseData.NONE_VALUE;
    }

    public override int GetLevel()
    {
        return BaseData.NONE_VALUE;
    }

    public override int GetMainWeaponCount()
    {
        return mMobInfo.Data.GetWeapons().Length;
    }

    public override void ReadyBattle()
    {
        
    }
}
