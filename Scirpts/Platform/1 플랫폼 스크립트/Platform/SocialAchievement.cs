using System;
using System.Collections;

public abstract class SocialAchievement : CNData
{
    protected ARewardData mRewardData;
    
#if UNITY_ANDROID//구글 전용.
    public virtual void SetAchievement(GooglePlayGames.BasicApi.Achievement achievement, ARewardData data) { }
#endif
    public virtual void SetAchievement(
        UnityEngine.SocialPlatforms.IAchievement achievement,
        UnityEngine.SocialPlatforms.IAchievementDescription achievementDescription,
        ARewardData data)
    { }

    public ARewardData GetRewardData()
    {
        return mRewardData;
    }

    public int GetCurrentStep()
    {
        int value = 0;

        switch (mRewardData.key)
        {
            case eAchievementKey.ChapterCLR_1 :
            case eAchievementKey.ChapterCLR_2 :
            case eAchievementKey.ChapterCLR_3 :
            case eAchievementKey.ChapterCLR_4 :
            case eAchievementKey.ChapterCLR_5 :
            case eAchievementKey.ChapterCLR_6 :
            case eAchievementKey.ChapterCLR_7 :
            case eAchievementKey.ChapterCLR_8 :
            case eAchievementKey.ChapterCLR_9 :
            case eAchievementKey.ChapterCLR_10 :
                value = (int)(GetDoubleValue(GameTableSetting.eDoubleKind.PercentCompleted) / 100);
                break;

            case eAchievementKey.Level_6 :
            case eAchievementKey.Level_9 :
            case eAchievementKey.Level_15 :
            case eAchievementKey.Level_17 :
            case eAchievementKey.Level_25 :
            case eAchievementKey.Level_28 :
            case eAchievementKey.Level_30 :
            case eAchievementKey.Level_36 :
            case eAchievementKey.Level_45 :
            case eAchievementKey.Level_55 :
            case eAchievementKey.Level_66 :
            case eAchievementKey.Level_77 :
            case eAchievementKey.Level_80 :
            case eAchievementKey.Level_85 :
            case eAchievementKey.Level_90 :
            case eAchievementKey.Level_99 :
                value = UserData.Instance.UserLevel;
                break;

            case eAchievementKey.Body_Armor :
            case eAchievementKey.Body_Speed :
            case eAchievementKey.Weapon_Plazma :
            case eAchievementKey.Weapon_Rail :
            case eAchievementKey.Weapon_Charge :
            case eAchievementKey.Weapon_Blaster :
            case eAchievementKey.Weapon_Beam :
            case eAchievementKey.Weapon_Flame :
            case eAchievementKey.Weapon_Ball :
            case eAchievementKey.Weapon_Highangle :
            case eAchievementKey.Weapon_Targeter :
            case eAchievementKey.Weapon_Melt :
            case eAchievementKey.Weapon_Mine :
            case eAchievementKey.Weapon_EMP :
            case eAchievementKey.Weapon_ALL :
                value = (int)(GetDoubleValue(GameTableSetting.eDoubleKind.PercentCompleted) / 100);
                break;

            case eAchievementKey.PilotLV_10 :
            case eAchievementKey.PilotLV_40 :
            case eAchievementKey.PilotLV_70 :
            case eAchievementKey.PilotLV_99 :
                value = UserData.Instance.CurrentMaxPilotLevel;
                break;

            case eAchievementKey.Party_5 :
            case eAchievementKey.Party_10 :
                value = UserData.Instance.GetAllPilotList().Count;
                break;

            case eAchievementKey.SkillUse_10 :
            case eAchievementKey.SkillUse_30 :
            case eAchievementKey.SkillUse_60 :
            case eAchievementKey.SkillUse_100 :
            case eAchievementKey.SkillUse_200 :
            case eAchievementKey.SkillUse_300 :
            case eAchievementKey.SkillUse_500 :
                value = UserData.Instance.SkillUseCnt;
                break;

            case eAchievementKey.SkillUse_Team :
                value = (int)(GetDoubleValue(GameTableSetting.eDoubleKind.PercentCompleted) / 100);
                break;

            case eAchievementKey.Kill_10 :
            case eAchievementKey.Kill_50 :
            case eAchievementKey.Kill_200 :
            case eAchievementKey.Kill_500 :
            case eAchievementKey.Kill_1000 :
            case eAchievementKey.Kill_2000 :
            case eAchievementKey.Kill_4000 :
            case eAchievementKey.Kill_7000 :
            case eAchievementKey.Kill_10000 :
                value = UserData.Instance.MobKillCnt;
                break;

            case eAchievementKey.PartD_2 :
            case eAchievementKey.PartC_3 :
            case eAchievementKey.PartB_4 :
            case eAchievementKey.PartA_5 :
                value = (int)(GetDoubleValue(GameTableSetting.eDoubleKind.PercentCompleted) / 100);
                break;

            case eAchievementKey.ClearS_10 :
            case eAchievementKey.ClearS_20 :
            case eAchievementKey.ClearS_30 :
            case eAchievementKey.ClearS_50 :
            case eAchievementKey.ClearS_70 :
                value = UserData.Instance.RankSClearCnt;
                break;

            case eAchievementKey.GoldPlay_1 :
            case eAchievementKey.GoldPlay_5 :
            case eAchievementKey.GoldPlay_10 :
            case eAchievementKey.GoldPlay_20 :
            case eAchievementKey.GoldPlay_50 :
            case eAchievementKey.GoldPlay_100 :
            case eAchievementKey.GoldPlay_300 :
                value = UserData.Instance.GoldRushPlayCnt;
                break;

            case eAchievementKey.BossClear_1 :
            case eAchievementKey.BossClear_3 :
            case eAchievementKey.BossClear_6 :
            case eAchievementKey.BossClear_10 :
            case eAchievementKey.BossClear_20 :
            case eAchievementKey.BossClear_30 :
            case eAchievementKey.BossClear_40 :
                value = UserData.Instance.BossRushClearCnt;
                break;
        }

        return value;
    }

    public enum eAchievementKey
    {
        None = 0,
        ChapterCLR_1,
        ChapterCLR_2,
        ChapterCLR_3,
        ChapterCLR_4,
        ChapterCLR_5,
        ChapterCLR_6,
        ChapterCLR_7,
        ChapterCLR_8,
        ChapterCLR_9,
        ChapterCLR_10,
        Level_6,
        Level_9,
        Level_15,
        Level_17,
        Level_25,
        Level_28,
        Level_30,
        Level_36,
        Level_45,
        Level_55,
        Level_66,
        Level_77,
        Level_80,
        Level_85,
        Level_90,
        Level_99,
        Body_Armor,
        Body_Speed,
        Weapon_Plazma,
        Weapon_Rail,
        Weapon_Charge,
        Weapon_Blaster,
        Weapon_Beam,
        Weapon_Flame,
        Weapon_Ball,
        Weapon_Highangle,
        Weapon_Targeter,
        Weapon_Melt,
        Weapon_Mine,
        Weapon_EMP,
        Weapon_ALL,
        PilotLV_10,
        PilotLV_40,
        PilotLV_70,
        PilotLV_99,
        Party_5,
        Party_10,
        SkillUse_10,
        SkillUse_30,
        SkillUse_60,
        SkillUse_100,
        SkillUse_200,
        SkillUse_300,
        SkillUse_500,
        SkillUse_Team,
        Kill_10,
        Kill_50,
        Kill_200,
        Kill_500,
        Kill_1000,
        Kill_2000,
        Kill_4000,
        Kill_7000,
        Kill_10000,
        PartD_2,
        PartC_3,
        PartB_4,
        PartA_5,
        ClearS_10,
        ClearS_20,
        ClearS_30,
        ClearS_50,
        ClearS_70,
        GoldPlay_1,
        GoldPlay_5,
        GoldPlay_10,
        GoldPlay_20,
        GoldPlay_50,
        GoldPlay_100,
        GoldPlay_300,
        BossClear_1,
        BossClear_3,
        BossClear_6,
        BossClear_10,
        BossClear_20,
        BossClear_30,
        BossClear_40,
    }
}