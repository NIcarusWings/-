using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//팩토리 패턴에 의해 복합체 패턴으로 들어갔어야되는데... 이 부분은 명백히 설계적인 실수임... 아직 멀은 듯(객체합성의 실전을 조금만 더 빨리 알았다면 좋았을 듯...)
//Infomation와 Data를 통합해서 생각하는 부분 역시 꼬인 부분이 많음. Data를 통해서 밑바탕을 깔아두고
//Information을 넘겨줘서 부가적인 설정을 해서 객체화시키는 것이 맞음.(데이터 와 정보의 연결고리를 좀만 더 빨리 알았더라면...)
//현재 Information 안에 Data가 종속되어있는 바람에 아군을 생성하는 함수 따로 적을 생성하는 함수 따로 이렇게 되버림...(으악으악으아악...-_- 반성해야겠음...)
public class GameUnitObject : MonoBehaviour, IChainOfResponsibility
{
    //이 함수도 현재 잘못되어있음. 원형패턴의 용도로 사용되는 함수인데 생성과 동시에 초기화가 되어있음.
    //원형패턴의 경우 디폴트로 설정되어있는 하나의 객체를 복제해서 그 객체를 리턴하는 용도로써 사용되고 있는데
    //이 함수의 경우 복제를 하는 것까지는 좋은데 동시에 공통부분으로써 사용되는 데이터가 아니라 정보를 가지고 초기화를 하고 있기 때문에 원형패턴의 규칙을 위반하고 있음.
    //P.S 현재 수정불가.(UI까지 다 손을 봐야되는 상황 -> 수정뿐만 아니라 추가를 해줘야됨.)
    public void Summon(InformationCharacter ic, bool isSummonBody, ePlayType playType = ePlayType.None)
    {
        gameObject.SetActive(true);

        SummonChar(
            ic.Data.Rsc_Name,
            ic.GetCostumeNumber().ToString(), 
            playType);

        mSubpacks = ic.GetSubpack();

        //추가적으로 이 부분도 문제임. 
        //캐릭터와 메카닉을 객체를 소환하는데 캐릭터가 주체인지 메카닉이 주체인지 둘다 공용으로 사용되는지 알수가 없음.
        //즉 mGameUnit이 무엇을 의미하는지 명확하게 들어오지 않음.
        if (mGameUnit != null)
        {
            mGameUnit.Set(ic);
        }

        if (isSummonBody)
        {
            SummonBody(ic.GetBodyData());

            if (mMechBody != null)
            {
                mMechBody.Set(
                    ic.Data.Type, 
                    ic.GetWeaponDatas());

                mMechBody.SetPosition(
                        Space.Self,
                        GetMechBodyPositionByCharacter(playType, ic.Data.Type));
            }
        }
    }

    public void Summon(MobInfo mobInfo, ePlayType playType)
    {
        switch (mobInfo.UnitType)
        {
            case eUnitType.Character:
                SummonChar(
                    mobInfo.Data.Rsc_Name, 
                    (mobInfo.GetDataCostume().CostumeType - 1).ToString(), 
                    playType);

                SummonBody(mobInfo.NBodyData);

                if (PlayData.Instance.IsPvPMode())
                {
                    //SetCutSceneInfo(InGameManager.Instance.CutSceneParent);
                }
                else
                {
                    //컷씬 발동시 캐릭터와 컷 씬이 없는데 적 캐릭터일 경우가 적 캐릭터 혹은 그 기체가 컨씬에 보이는 현상을 막기 위함.
                    FormationEdit.ChangeLayersRecursively(transform, "Default");
                }

                if (mMechBody != null)
                {
                    mMechBody.SetPosition(
                        Space.Self,
                        GetMechBodyPositionByCharacter(playType, mobInfo.GetKindType()));
                }
                break;

            case eUnitType.NormalUnit:
                SummonNormalMob(mobInfo, playType);

                if (mMechBody != null)
                {
                    mMechBody.SetPosition(Space.Self, Vector3.right * 4f);
                    //mMechBody.SetHitPointPosition(Space.Self, Vector3.up * 1.5f);
                }
                break;
        }

        if (mGameUnit != null)
        {
            mGameUnit.Set(mobInfo);
        }

        if (mMechBody != null)
        {
            mMechBody.Set(
                mobInfo.GetKindType(),
                mobInfo.Weapons);
        }

        mSubpacks = mobInfo.Subpacks;
    }
    
    private void SummonChar(string rscName, string costumeNumber, ePlayType playType)
    {
        GameObject go = null;

        if (mGameUnit == null)
        {
            string path = GameUtil.ReplacePath("Prefabs/3DChar/3dchr_@_c", rscName, costumeNumber);

            //Character Object를 가져옴.
            go = GameManager.Summon(path, false);

            if (go != null)
            {
                go.transform.SetParent(transform);

                GameUnitChar guc = GetUnit<GameUnitChar>(go);

                guc.PlayType = playType;
                guc.Initialize(this);

                mGameUnit = guc;
            }
        }
    }

    private void SummonBody(MechBodyData bodyData)
    {
        if (mMechBody == null)
        {
            //go = 메카닉 Body
            GameObject go = GameManager.Summon(bodyData.ResourceName, false);

            if (go != null)
            {
                go.transform.SetParent(transform);
                go.transform.position = transform.position + Vector3.left * 6f;

                mMechBody = go.GetComponent<MechBody>();
                mMechBody.Initialize(this, bodyData);
            }
        }
    }

    public void SetInGame()
    {
        if (mGameUnit != null)
        {
            mGameUnit.SetPosition(Vector3.back * InGameManager.Instance.CharBack);

            SetSkill();

            if (mAI == null)
            {
                mAI = new UnitAI(this);
            }
        }

        if (mMechBody != null)
        {
            mMechBody.SetInGame(PlayType);

            if (PlayType == ePlayType.Enemy)
            {
                if (mGameUnit.UnitType == eUnitType.Character)
                {
                    mMechBody.SetPosition(
                        Space.Self,
                        GetMechBodyPositionByCharacterInInGame(mGameUnit.PlayType, mGameUnit.GetMechType()));
                }
                else
                {
                    mMechBody.SetPosition(Space.Self, Vector3.zero);
                    //mMechBody.SetHitPointPosition(Space.Self, Vector3.up * 1.5f);
                }
            }
            else
            {
                mMechBody.SetPosition(
                        Space.Self,
                        GetMechBodyPositionByCharacterInInGame(mGameUnit.PlayType, mGameUnit.GetMechType()));
            }

            transform.localScale = Vector3.one * mMechBody.NBodyData.FitScale;
        }

        SetHUD();
    }

    private void SetSkill()
    {
        //if (BuffList == null)
        //{
        //    BuffList = new List<SkillContainer.SkillUseData>();
        //}
        
        int[] skillIDs = GetSkillIds();
        int[] skillLevels = GetSkillLevels();

        if (skillIDs != null && skillLevels != null &&
            skillIDs.Length > 0 && skillIDs[0] != BaseData.NONE_VALUE && skillIDs[0] != 0)
        {
            if (NSkillContainer == null)
            {
                NSkillContainer = new SkillContainer();
            }

            NSkillContainer.Init(this);
        }
    }

    public void SetCutSceneInfo(Transform parentCutScene)
    {
        if (NSkillContainer != null)
        {
            //스킬이 있어도 컷씬을 사용하지 않을 수도 있음.!!!!
            NSkillContainer.SetCutSceneInfo(parentCutScene);
        }
    }

    private void SummonNormalMob(MobInfo mobInfo, ePlayType playType)
    {
        //mobData를 통하여 GameUnitMob에 관한 객체를 소환하여 초기화를 하면 된다.
        //mobInfo의 ResourcePath는 BodyData의 ResourcePath와 동일하고 MechBody가 부착된 객체를 가져온다.
        //따라서 캐릭터와 달리 go.GetComponent<MechBody>() 로 바로 가져올 수가 있는데 
        //이에 대한 초기화 방식은 GameUnitMob 클래스의 초기화함수를 참고.
        GameObject go = GameManager.Summon(mobInfo.ResourcePath, true);

        if (go != null)
        {
            go.transform.SetParent(transform);

            GameUnitMob gub = GetUnit<GameUnitMob>(go);

            gub.PlayType = playType;
            gub.Initialize(this, mobInfo);
            
            mGameUnit = gub;

            mMechBody = go.GetComponent<MechBody>();
            mMechBody.Initialize(this, mobInfo.NBodyData);
        }
    }

    private void SetHUD()
    {
        GameObject go = GameManager.Summon("Prefabs/InGame/HUDItem", true);

        if (go != null)
        {
            GameObject parent = InGameManager.Instance.NHUDParent.gameObject;
            int layer = parent.gameObject.layer;

            if (parent != null)
            {
                Transform t = go.transform;
                t.parent = parent.transform;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                if (layer == -1) go.layer = parent.layer;
                else if (layer > -1 && layer < 32) go.layer = layer;
            }

            go.SetActive(true);

            if (mHUDPoint == null)
            {
                mHUDPoint = new GameObject();
                mHUDPoint.transform.SetParent(mGameUnit.transform);
                mHUDPoint.transform.localScale = Vector3.one;

                switch (mGameUnit.PlayType)
                {
                    case ePlayType.Player:
                    case ePlayType.NPC:
                        mHUDPoint.transform.localPosition = Vector3.up * 2.5f;
                        break;

                    case ePlayType.Enemy:
                        mHUDPoint.transform.localPosition = Vector3.forward * 1.5f + Vector3.up * 0.5f;
                        break;
                }

#if UNITY_EDITOR
                mHUDPoint.name = "HUD_POINT";
#endif
            }

            NHUDItem = go.GetComponent<InGameHUDItem>();

            NHUDItem.Initialize(mHUDPoint.transform, GetCurHP(), GetUnitIndex());
        }
    }

    private Vector3 GetMechBodyPositionByCharacter(ePlayType playType, int kindType)
    {
        Vector3 result = Vector3.zero;
        bool isEnemy = (playType == ePlayType.Enemy);

        switch (kindType)
        {
            case 1://탱크
            case 2://배
                if (isEnemy)
                {
                    result = Vector3.right;
                }
                else
                {
                    result = Vector3.left;
                }
                break;

            case 3://전투기
            case 4://헬기
                if (isEnemy)
                {
                    result = Vector3.right;
                }
                else
                {
                    result = Vector3.left;
                }
                break;
        }

        return result;
    }

    private Vector3 GetMechBodyPositionByCharacterInInGame(ePlayType playType, int kindType)
    {
        Vector3 result = Vector3.zero;
        bool isEnemy = (playType == ePlayType.Enemy);

        switch (kindType)
        {
            case 1://탱크
            case 2://배
                if (isEnemy)
                {
                    result = Vector3.right * InGameManager.Instance.CharLeft;
                }
                else
                {
                    result = Vector3.left * InGameManager.Instance.CharLeft;
                }
                break;

            case 3://전투기
            case 4://헬기
                if (isEnemy)
                {
                    result = Vector3.right * InGameManager.Instance.CharLeft + Vector3.up * InGameManager.Instance.CharUp;
                }
                else
                {
                    result = Vector3.left * InGameManager.Instance.CharLeft + Vector3.up * InGameManager.Instance.CharUp;
                }
                break;
        }

        return result;
    }

    public MechBody GetMechBody()
    {
        return mMechBody;
    }
    
    public void NUpdate()
    {
        if (!IsRetire())
        {
            if (!IsStop())
            {
                if (mMechBody != null)
                {
                    mMechBody.NUpdate(NormalTarget);
                }

                if (NSkillContainer != null && NSkillContainer.NCutSceneInfo != null)
                {
                    NSkillContainer.NCutSceneInfo.NUpdate();
                }
            }

            Buff();
        }
        else
        {
            if (Buffs != null && Buffs.Count != 0)
            {
                Buffs.Clear();
            }
        }
    }

	private void Buff()
	{
		//if (BuffList != null)
		//{
		//    for (int i = 0; i < BuffList.Count; ++i)
		//    {
		//        if (BuffList[i].IsSkillUseState(SkillContainer.SkillUseData.SkillUseState.OnUse))
		//        {
		//            BuffData buffData = BuffList[i].mBuffData;
		//            DataSkill dataSkill = buffData.data;

		//            buffData.dotduration += GameManager.DeltaTime;

		//            if (buffData.dotduration >= dataSkill.getVal2(buffData.level))
		//            {
		//                switch (BuffList[i].mBuffData.data.code)
		//                {
		//                    case DataSkill.SkillCode.Heal_EK:
		//                        Heal_EK(BuffList[i].mBuffData);
		//                        break;
		//                }

		//                buffData.dotduration = 0;
		//            }
		//        }
		//    }
		//}

		int cnt = Buffs.Count;
		BuffData buffData;
		for (int i = 0; i < cnt; i++)
		{
            try
            {
                buffData = Buffs[i];
                if (Buffs[i] != null)
                    buffData.Update();

                if (buffData == null || buffData.IsDisabled())
                {
                    RemoveBuff(Buffs[i]);
                    i--;
                    cnt--;
                }
            }
            catch
            {
#if UNITY_EDITOR
                Debug.Log("Buffs가 초기화가 되서 인덱스 범위가 벋어나는 에러가 발생함.");
#endif
            }
		}
	}

    public void NReset(eBattleState battleState)
    {
        if (mGameUnit != null)
        {
            mGameUnit.NReset(battleState);
        }
        
        if (mMechBody != null)
        {
            mMechBody.NReset(battleState);
        }

        SetTarget(null);

        switch (battleState)
        {
            case eBattleState.Retire:
                InGameManager.Instance.OnRetired(this);

                if (NHUDItem != null)
                {
                    NHUDItem.HideHUD();
                }

                if (IsShield)
                {
                    if (Link != null)
                    {
                        //List<SkillContainer.SkillUseData> buffList = Link.BuffList;
                        SkillContainer.SkillUseData useData = null;

                        //for (int i = 0; i < buffList.Count; ++i)
                        //{
                        //    if (buffList[i].mBuffData.data.code == DataSkill.SkillCode.Shield)
                        //    {
                        //        useData = buffList[i];
                        //        break;
                        //    }
                        //}

                        if (useData != null)
                        {
                            Link.RemoveBuff(useData);
                        }
                        
                        Link = null;
                    }

                    IsShield = false;
                }

                if (NSkillContainer != null)
                {
                    NSkillContainer.Reset();
                }

                NHUDItem = null;
                mTargeteds.Clear();
                break;

            case eBattleState.EndGame:
                if (NSkillContainer != null)
                {
                    NSkillContainer.Reset();
                }
                break;
        }
    }

    public void Refresh()
    {
        gameObject.SetActive(false);

        if (NFormationField != null)
        {
            NFormationField.Refresh();
        }
    }

    public void Restart()
    {
        gameObject.SetActive(true);
        SetInGame();

        if (mGameUnit != null)
        {
            mGameUnit.Restart();
        }

        if (mMechBody != null)
        {
            mMechBody.Restart();
        }

        if (NFormationField != null)
        {
            NFormationField.Restart();
        }
    }

    public bool IsRetire()
    {
        return mGameUnit.IsRetire();
    }

    public void UpdateMotion(params GameUnit.eMotion[] motions)
    {
        if (!IsRetire())
        {
            mGameUnit.UpdateMotion(motions);
        }
    }

    public void Pause(bool isPause)
    {
        if (mGameUnit != null && !mGameUnit.IsRetire())
        {
            mGameUnit.Pause(isPause);
        }

        if (mMechBody != null)
        {
            mMechBody.Pause(isPause);
        }
    }

    public void StandBy()
    {
        gameObject.SetActive(true);

        if (mGameUnit != null)
        {
            mGameUnit.StandBy();
        }

        if (mMechBody != null)
        {
            mMechBody.StandBy();
        }
    }

    public void ReadyBattle()
    {
        if (mGameUnit != null)
        {
            mGameUnit.ReadyBattle();
        }
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    //...-_-시간이 없었다곤 하지만 HUD를 종속시켜서 처리를 하다니...
    //정의된 복합체와 HUD 객체는 종속관계가 아니므로 HUD 처리는 HUD의 정적함수를 구현해서 처리했어야됨...
    public void ShowHUD(eHUDType hudType, int value)
    {
        if (NHUDItem != null)
        {
            NHUDItem.ShowHUD(hudType, value, mGameUnit.CurHP, GetMaxHP());
        }
    }

    public void ShowHUD(eHUDType hudType, string value)
    {
        if (NHUDItem != null)
        {
            NHUDItem.ShowHUD(hudType, value, mGameUnit.CurHP, GetMaxHP());
        }
    }

    public bool IsCutScene()
    {
        return mGameUnit.UnitType == eUnitType.Character;
    }

    private void SetTarget(GameUnitObject unitObject)
    {
        for (int i = 0; i < mTargeteds.Count; ++i)
        {
            mTargeteds[i].NormalTarget = unitObject;
        }
    }

    public void SetBuff(SkillContainer.SkillUseData buffData)
    {
		//BuffList.Add(buffData);
		SetBuff(buffData.mBuffData);
    }

    public void RemoveBuff(SkillContainer.SkillUseData buffData)
    {
		//BuffList.Remove(buffData);
		RemoveBuff(buffData.mBuffData);
    }

	public void SetBuff(BuffData buffData)
	{
		//buffData.ActionDisabled = RemoveBuff;
		Buffs.Add(buffData);

        if (NHUDItem != null && buffData != null && buffData.data.buffColor != null)
        {
            NHUDItem.Add(buffData);
        }
    }

    public void RemoveBuff(BuffData buffData)
	{
        Buffs.Remove(buffData);

        if (NHUDItem != null && buffData != null && buffData.data.buffColor != null)
        {
            NHUDItem.Remove(buffData);
        }

        buffData.data = null;
    }

	public float GetBuff(eFaculty faculty)
    {
        float buffValue = GetBuffValue(faculty);

        return (buffValue * BuffData.PERCENT_RATE);
    }

    public float GetSubpackBuff(eFaculty faculty)
    {
        float value = 0f;

        if (mSubpacks != null)
        {
            for (int i = 0; i < mSubpacks.Count; ++i)
            {
                if (mSubpacks[i] != null)
                {
                    switch (faculty)
                    {
                        case eFaculty.HP:
                            value += mSubpacks[i].Hp;
                            break;

                        case eFaculty.Atk:
                            value += mSubpacks[i].Atk;
                            break;

                        case eFaculty.Def:
                            value += mSubpacks[i].Def;
                            break;

                        case eFaculty.AtkTime:
                            value += mSubpacks[i].Atktime;
                            break;

                        case eFaculty.Acc:
                            value += mSubpacks[i].Acc;
                            break;

                        case eFaculty.Eva:
                            value += mSubpacks[i].Eva;
                            break;

                        case eFaculty.Cri:
                            value += mSubpacks[i].Cri;
                            break;

                        case eFaculty.Cri_Dmg:
                            value += mSubpacks[i].Cri_Dmg;
                            break;
                    }
                }
            }
        }
        
        return value;
    }

    public float GetBuff(DataSkill.SkillCode code, eFaculty faculty = eFaculty.Count)
	{
		float value = 0f;
		float baseValue = (faculty == eFaculty.Count ? 0f : GetRealFacultyInUnit(faculty));

		int cnt = Buffs.Count;
		BuffData buffData;
		for (int i = 0; i < cnt; i++)
		{
			buffData = Buffs[i];
			if (buffData != null && buffData.CheckBuffData(code))
			{
				value += buffData.GetValueBuff(baseValue);
			}
		}

		return value;
	}

	//능력치 관련 부분을 하나로 통합하여 사용할 수 있는 방법을 고민하다가 비트연산을 생각했지만
	//능력 종류(eFaculty)에 대한 부분이 비트 연산부분으로 구현이 되어있지 않고 순차나열 값으로 되어있음.
	//어쩔 수 없이 하드코딩으로 땜방을 해야될듯...
	private float GetBuffValue(eFaculty faculty)
    {
		float value = 0f;
		BuffData buffData = null;
		//for (int i = 0; i < BuffList.Count; ++i)
		//{
		//    buffData = BuffList[i].mBuffData;

		//    if (buffData != null && IsApplyBuff(faculty, buffData))
		//    {
		//        value += buffData.data.getVal3(buffData.level);
		//    }
		//}

		int cnt = Buffs.Count;
		for (int i = 0; i < cnt; i++)
		{
			buffData = Buffs[i];
			if (buffData != null && !buffData.IsDisabled() && IsApplyBuff(faculty, buffData))
			{                
				value += buffData.data.getVal1(buffData.level);
			}
		}

		return value;
    }

    public bool IsApplyBuff(eFaculty faculty, BuffData buffData)
    {
        DataSkill.SkillCode code = buffData.data.code;
        eFaculty convertFaculty = eFaculty.Count;
        bool isBuff = false;

        switch (buffData.data.code)
        {
            case DataSkillBase.SkillCode.ATK:
                convertFaculty = eFaculty.Atk;
                break;

            case DataSkillBase.SkillCode.DEF:
                convertFaculty = eFaculty.Def;
                break;

            case DataSkillBase.SkillCode.SPD:
                convertFaculty = eFaculty.AtkTime;
                break;

            case DataSkillBase.SkillCode.ACC:
                convertFaculty = eFaculty.Acc;
                break;

            case DataSkillBase.SkillCode.EVA:
                convertFaculty = eFaculty.Eva;
                break;

            case DataSkillBase.SkillCode.CRI:
                convertFaculty = eFaculty.Cri;
                break;

            case DataSkillBase.SkillCode.CRI_DMG:
                convertFaculty = eFaculty.Cri_Dmg;
                break;
        }

        if (faculty == convertFaculty)
        {
            isBuff = true;
        }

        return isBuff;
    }

	public bool IsApplySkill(params DataSkill.SkillCode[] codes)
	{
		bool isApply = false;
		
		if (Buffs != null && Buffs.Count != 0)
		{
			DataSkillBase dataSkill = null;

			int cnt = Buffs.Count;
			BuffData buffData;			
			for (int i = 0; i < cnt; i++)
			{
				buffData = Buffs[i];
				if (buffData == null || buffData.IsDisabled())
					continue;

				dataSkill = buffData.data;

				for (int j = 0; j < codes.Length; ++j)
				{
					if (buffData.CheckBuffData(codes[j]))
					{
						isApply = true;
						break;
					}
				}

				if (isApply)
				{
					break;
				}
			}
		}

		return isApply;
	}

	public GameUnitObject NormalTarget
    {
        get
        {
            return mAI.NormalTarget;
        }
        set
        {
            mAI.NormalTarget = value;
        }
    }

    private T GetUnit<T>(GameObject go) where T : GameUnit
    {
        T guc = go.GetComponent<T>();

        if (guc == null)
        {
            guc = go.AddComponent<T>();
        }

        return guc;
    }

    public int GetFormationIndex()
    {
        return mGameUnit.GetFormationIndex();
    }

    public NPoint GetFormationPoint()
    {
        return mGameUnit.GetFormationPoint();
    }

    public GameUnit GetUnit()
    {
        return mGameUnit;
    }

    //public DataSkill GetDataSkill(int skill)
    //{

    //}

    public Transform HitPoint
    {
        get
        {
            return GetMechBody().HitPoint;
        }
    }

    public void AddTargetOn(GameUnitObject unit)
    {
        mTargeteds.Add(unit);
    }

    public int GetUnitIndex()
    {
        return GetUnit().GetUnitIndex();
    }
    
    public int GetMechType()
    {
        return GetUnit().GetMechType();
    }

    public string GetRscName()
    {
        return mGameUnit.GetRscName();
    }

    public float GetCurHP()
    {
        return mGameUnit.CurHP;
    }

	public float GetMaxHP()
	{
		return GetRealFacultyInUnit(eFaculty.HP);
	}

	public float GetRealFacultyInUnit(eFaculty faculty)
    {
        float value = mGameUnit.GetRealFaculty(faculty);

        return value + (value * GetBuff(faculty)) + GetSubpackBuff(faculty);
    }

    public float GetPrecedenceTimeRatio()
    {
        float ratio = 0f;

        if (mGameUnit != null)
        {

        }
        if (NSkillContainer != null)
        {
            DataSkill dataSkill = NSkillContainer.skills[0];

            ratio = dataSkill.precedenceTime / dataSkill.coolTime;
        }

        return ratio;
    }

    public int GetKindType()
    {
        if (mMechBody == null)
        {
            return BaseData.NONE_VALUE;
        }
        else
        {
            return mMechBody.GetKindType();
        }
    }

    public int[] GetSkillIds()
    {
        return mGameUnit.GetSkillIds();
    }

    public int[] GetSkillLevels()
    {
        return mGameUnit.GetSkillLevels();
    }

    public ePlayType PlayType
    {
        get
        {
            return mGameUnit.PlayType;
        }
        set
        {
            mGameUnit.PlayType = value;
        }
    }

    private void Heal_EK(BuffData buffData)
    {
		DataSkillBase dataSkill = buffData.data;
		Heal_EK(dataSkill.getVal3(buffData.level));
	}

	public void Heal_EK(float value, bool isPercent = true)
	{
		float maxHP = GetMaxHP();
		if(isPercent)
			value = maxHP * value * BuffData.PERCENT_RATE;
		
		Heal(value, maxHP);
	}

	public void Heal(float value, float maxHP = 0f)
	{
		if(maxHP == 0f)
			maxHP = GetMaxHP();

        BuffData b = GetCondition(DataSkillBase.SkillCode.Malfunction);
        eHUDType hudType = eHUDType.NormalDamage;

        if (b != null)
        {
            float buffValue = GetTotalValueBy(4, DataSkillBase.SkillCode.Malfunction);//오작동 버프의 경우 반드시 value4에 수치가 들어가 있어야됨.
            
            value = (value * buffValue);
            value *= -1;

            hudType = eHUDType.NormalDamage;
        }
        else
        {
            hudType = eHUDType.Heal;
        }
        
		mGameUnit.CurHP += value;

		if (mGameUnit.CurHP > maxHP)
		{
			mGameUnit.CurHP = maxHP;
		}

		ShowHUD(hudType, (int)value);
	}

	public void Handle(CRMessage message)
    {
        switch (message.Context)
        {
            case CRMessage.eContext.HIT_ATTACK:
                mGameUnit.GiveTotalDamge += message.IValue;
                
                if (message.BValue)
                {
                    ++mGameUnit.EnemyCount;
                }
                break;

            case CRMessage.eContext.COMMON_UNDER_ATTACK:
                //case CRMessage.eContext.UNDER_ATTACK_Y_ZERO:
                {
                    int value = (int)(message.IValue +
                            message.IValue *
                            GetTotalValueBy(1, DataSkillBase.SkillCode.ElectricShock));

                    mGameUnit.GiveBy(value);

                    ShowHUD((message.BValue) ? eHUDType.CriDamage : eHUDType.NormalDamage,
                            (value));

                    if (mMechBody != null)
                    {
                        mMechBody.Handle(message);
                    }

                    if (IsRetire())
                    {
                        CRMessage localMessage = new CRMessage();

                        localMessage.Reset(CRMessage.eContext.RETIRE_UNIT);

                        Handle(localMessage);
                    }
                    else
                    {
                        if (IsApplyCondition(DataSkillBase.SkillCode.Temptation))
                        {
                            RemoveBuff(GetCondition(DataSkillBase.SkillCode.Temptation));
                        }
                    }
                    InGameTopUI.Instance.Refresh(InGameTopUI.eRefresh.TopHPGauge, (mGameUnit.PlayType == ePlayType.Enemy) ? 1 : 0);
                }
                break;

            case CRMessage.eContext.RETIRE_UNIT:
                NReset(eBattleState.Retire);
                break;
        }
    }

	public void GotAttkDamage(GameUnitObject attacker, float dmg, bool isHit = true, bool isCritial = false, bool isSkill = false, string hitFXPath = null, string hitSFXPath = null, XVector3 hitPos = null)
	{
        //if (IsApplySkill(DataSkill.SkillCode.Shield))
        //{
        //	//SkillShield(from, ref to);
        //	return;
        //}
        
        if (isHit)
		{
			// 타입별 데미지 비율
			dmg *= (GetElementRate(attacker.GetKindType()));


            // 방어 적용
            //방어력 수치가 낮을 경우 버프를 받아도 그다지 티가 안남.
            //ex=> 방어력이 4이고 증가버프량이 10%이면 4.4인데 이 경우 의미가 없음.
			float def = GetRealFacultyInUnit(eFaculty.Def);
			dmg *= (1000f / (1000f + def));

			// 최종 데미지
			int iTotalDmg = (int)dmg;
			if (iTotalDmg < 1)
				iTotalDmg = 1;
            
            CRMessage message = new CRMessage();
			if (IsApplySkill(DataSkill.SkillCode.DamageReflection))
			{
				if (!attacker.IsRetire())
				{
					message.Reset(CRMessage.eContext.COMMON_UNDER_ATTACK)
							.Set(-iTotalDmg)
                            .Set(isCritial)
                            .Handle(attacker);

                    message.Reset(CRMessage.eContext.HIT_ATTACK)
                            .Set(iTotalDmg)
                            .Set(this.IsRetire())
                            .Handle(this);
                }
			}
            else
            {
                message.Reset(CRMessage.eContext.COMMON_UNDER_ATTACK)
                    .Set(-iTotalDmg)
                    .Set(isCritial)
                    .Handle(this);

                message.Reset(CRMessage.eContext.HIT_ATTACK)
                    .Set(iTotalDmg)
                    .Set(this.IsRetire())
                    .Handle(attacker);
            }
			

            InGameManager.SPlayFx(hitFXPath, (hitPos == null ? mMechBody.HitPoint.position : hitPos.vec3));
			InGameManager.SPlaySfx(transform.position, hitSFXPath);
		}
		else
		{
			ShowHUD(eHUDType.Miss, "Miss");
		}
	}

	public bool IsHit(GameUnitObject shooter)
	{		
		return IsHit(shooter.GetRealFacultyInUnit(eFaculty.Acc));
	}
	public bool IsHit(float fromAcc)
	{
		//bool isShield = IsApplySkill(DataSkill.SkillCode.Shield);

		float toEva = GetRealFacultyInUnit(eFaculty.Eva);
		float cal = ((fromAcc / (fromAcc + toEva)) * 100f);
        
		//공격 성공율 = 명중률 / (명중률 + 타켓 회피율)
		bool result = RandomRange(0f, 100f) < cal;

		return result;
	}

	private float RandomRange(float min, float max)
	{
		if (PlayData.Instance.IsPvPMode())
		{
			if (mRandomManager == null)
				mRandomManager = new RandomManager(56789);

			return mRandomManager.RandomRange(min, max);
		}

		return GameUtil.RandomRange(min, max);
	}

	public float GetElementRate(int attackerType)
	{
		int type = GetKindType();
		if (attackerType == BaseData.NONE_VALUE || type == BaseData.NONE_VALUE)
		{
			return 1f;
		}

		DataElement goElData = GameTableContainer.Instance.GetDataElements(attackerType);

		return goElData.GetValue(type) * BuffData.PERCENT_RATE;
	}

    public bool IsStop()
    {
        return IsApplyCondition(DataSkillBase.SkillCode.Stun) ||
            IsApplyCondition(DataSkillBase.SkillCode.Temptation);
    }

    public bool IsStopSkill()
    {
        return IsApplyCondition(DataSkillBase.SkillCode.Silence) ||
            IsApplyCondition(DataSkillBase.SkillCode.Stun) ||
            IsApplyCondition(DataSkillBase.SkillCode.Temptation);
    }

    public bool IsApplyCondition(DataSkill.SkillCode skillCode)
    {
        bool isApply = false;

        for (int i = 0; i < Buffs.Count; ++i)
        {
            isApply = Buffs[i].CheckBuffData(skillCode);

            if (isApply)
            {
                break;
            }
        }

        return isApply;
    }

    private BuffData GetCondition(DataSkill.SkillCode skillCode)
    {
        return Buffs.Find(b => b.data.code == skillCode);
    }

    private float GetTotalValueBy(int index, DataSkill.SkillCode skillCode)
    {
        float value = 0;

        for (int i = 0; i < Buffs.Count; ++i)
        {
            if (Buffs[i].CheckBuffData(skillCode))
            {
                value += Buffs[i].data.GetValue(index, Buffs[i].level);
            }
        }

        return value * BuffData.PERCENT_RATE;
    }

    public GameUnitObject Link;
    public bool IsShield = false;

    public FormationField NFormationField;
    public InGameHUDItem NHUDItem;

    private GameUnit mGameUnit;
    private MechBody mMechBody;

    private GameObject mHUDPoint;

    public SkillContainer NSkillContainer;
    //public List<SkillContainer.SkillUseData> BuffList = new List<SkillContainer.SkillUseData>();
	public List<BuffData> Buffs = new List<BuffData>();
    
	//이 변수의 의미는 [내가 타켓으로 잡고 있는 리스트]가 아니라 [나를 타켓으로 잡고 있는 리스트]임.
	private List<GameUnitObject> mTargeteds = new List<GameUnitObject>();
    private UnitAI mAI;

    private List<EquipItemData> mSubpacks = null;

    private RandomManager mRandomManager;
}