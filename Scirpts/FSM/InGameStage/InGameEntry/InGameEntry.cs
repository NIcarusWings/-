using System.Collections.Generic;
using UnityEngine;

public abstract class InGameEntry : InGameFSM
{
    public override void Initailize(InGameView inGameView)
    {
        base.Initailize(inGameView);

        mCameraManager = inGameView.GetCameraManager();

        SummonPlayers();
        SummonMobs();
    }

    private void SummonPlayers()
    {
        GameManager gm = mInGameView.GetGameManager();
        DataManager dm = gm.GetDataManager();
        UserData ud = dm.GetUserData();
        PlayData pd = dm.GetPlayData();

        List<List<FormationField>> pacs = InGameManager.Instance.Pacs;
        List<int> selectedPlatoonIndexList = pd.GetSelectedPlatoonIndexList();
        List<int> members = null;

        FormationField field = null;
        List<InformationCharacter> ics = new List<InformationCharacter>();
        InformationCharacter ic = null;
        GameObject go = null;
        
        for (int i = 0; i < selectedPlatoonIndexList.Count; ++i)
        {
            members = ud.GetPlatoonChar(selectedPlatoonIndexList[i]);

            go = mInGameView.CreateMove(true, i);
            
            for (int j = 0; j < members.Count; ++j)
            {
                if (members[j] != BaseData.NONE_VALUE)
                {
                    ic = ud.GetInfomationByMyChars(members[j]);

                    field = SummonCharacter(
                        ic,
                        go.transform,
                        ePlayType.Player);

                    ics.Add(ic);
                    pacs[i].Add(field);
                }
            }

            SetFormationBuff(pacs[i], ics);

            go.SetActive(false);

            InGameManager.Instance.AddStartMemberCount(pacs[i].Count);
        }
    }

    protected void SetFormationBuff(List<FormationField> fields, List<InformationCharacter> ics)
    {
        GameUnitObject fromBuff = null;
        GameUnitObject toBuff = null;
        FormationBuff fb = null;

        for (int i = 0; i < fields.Count; ++i)
        {
            if (ics[i] != null)
            {
                fromBuff = fields[i].CurUnit;
                fb = GameTableContainer.Instance.GetFormatuinBuff(ics[i].DataIndex);

                for (int j = 0; j < fields.Count; ++j)
                {
                    toBuff = fields[j].CurUnit;

                    if (FormationField.IsFormationIn(
                            fromBuff.GetFormationIndex() + 1, toBuff.GetFormationIndex() + 1,
                            fb.BaseFormationValue, fb.Formations))
                    {
                        toBuff.SetBuff(ConvertFormationBuffToSkillUseData(fb.Amount1, fb.Effect1));
                    }
                }
            }
        }
    }

    private SkillContainer.SkillUseData ConvertFormationBuffToSkillUseData(int amount, FormationBuff.eBuffType buffType)
    {
        SkillContainer.SkillUseData useData = new SkillContainer.SkillUseData();
        DataSkill dataSkill = new DataSkill
        {
            id = BaseData.NONE_VALUE,
            value3 = amount
        };
        
        switch (buffType)
        {
            case FormationBuff.eBuffType.Atk:
                dataSkill.code = DataSkill.SkillCode.ATK;
                break;

            case FormationBuff.eBuffType.Def:
                dataSkill.code = DataSkill.SkillCode.DEF;
                break;

            case FormationBuff.eBuffType.AtkTime:
                dataSkill.code = DataSkill.SkillCode.SPD;
                break;

            case FormationBuff.eBuffType.Acc:
                dataSkill.code = DataSkill.SkillCode.ACC;
                break;

            case FormationBuff.eBuffType.Eva:
                dataSkill.code = DataSkill.SkillCode.EVA;
                break;

            case FormationBuff.eBuffType.Cri:
                dataSkill.code = DataSkill.SkillCode.CRI;
                break;
        }

        useData.mBuffData = new BuffData()
        {
            //fxObjs = null,
            data = dataSkill,
            level = 1,
        };

        return useData;
    }

    protected FormationField SummonCharacter(InformationCharacter ic, Transform parent, ePlayType player)
    {
        GameObject go = null;
        FormationField field = null;

        go = GameManager.Summon(DataPrefab.eIndex.FormationField);

        if (go != null)
        {
#if UNITY_EDITOR
            go.name = "Formation_" + ic.FormationIndex;
#endif
            go.transform.parent = parent;

            field = go.GetComponent<FormationField>();
            field.Summon(ic, true, player);

            if (player == ePlayType.Enemy)
            {
                go.transform.localEulerAngles = Vector3.up * 180f;
            }
        }

        return field;
    }

    protected abstract void SummonMobs();

    protected override void Start()
    {
        //mCameraManager.Add(CameraManager.WORLD_ANGLE | CameraManager.WORLD_POS);

        State = eState.Update;
    }

    protected override void Update()
    {
        State = eState.End;

        //if (mCameraManager.IsEndAnimation())
        //{
        //    State = eState.End;
        //}
        //else
        //{
        //    mCameraManager.DetectAction();
        //}
    }

    protected override void End()
    {
        mCameraManager.NReset();

        base.End();

        mInGameView.NextFSM(InGameView.eGame.Play);
    }

    private CameraManager mCameraManager;
}
