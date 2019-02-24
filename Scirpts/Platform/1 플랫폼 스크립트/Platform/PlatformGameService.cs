using UnityEngine;
using UnityEngine.SocialPlatforms;
using System;
using System.Collections.Generic;

public abstract class PlatformGameService
{
    protected PlatformProvider mProvider;
    protected PlatformProvider.PlatformData mData;
    
    protected SocialAchievement[] mAchievements;
    protected ARewardData[] mRewardList;
    
    public void Operation(PlatformProvider provider, PlatformProvider.PlatformData data)
    {
        mProvider = provider;

        Operation(data);
    }

    public string GetStringData(PlatformProvider.PlatformData data)
    {
        string value = null;

        switch (data.GetWork())
        {
            case PlatformProvider.eWork.SUserName:
                value = GetUserName();
                break;

            case PlatformProvider.eWork.SUserIconURL:
                value = GetUserIconURL();
                break;

            case PlatformProvider.eWork.SEmail:
                value = GetEmail();
                break;
        }

        return value;
    }
    
    public void OutputAchievements(out SocialAchievement[] achievements)
    {
        achievements = mAchievements;
    }

    public void OutputAchievement(SocialAchievement.eAchievementKey key, out SocialAchievement achievement)
    {
        ARewardData data = null;

        achievement = null;

        for (int i = 0; i < mAchievements.Length; ++i)
        {
            data = mAchievements[i].GetRewardData();

            if (data.key.CompareTo(key) == 0)
            {
                achievement = mAchievements[i];
                break;
            }
        }
    }

    protected void Operation(PlatformProvider.PlatformData data)
    {
        mData = data;

        switch (data.GetWork())
        {
            case PlatformProvider.eWork.SLogin:
                Login();
                break;

            case PlatformProvider.eWork.SLogout:
                Logout();
                break;

            case PlatformProvider.eWork.SLoadAchivement:
                LoadAchievement();
                break;

            case PlatformProvider.eWork.SOpenAchievement:
                OpenAchievement();
                break;

            case PlatformProvider.eWork.SUpdateAchievement:
                UpdateAchievement();
                break;
                
            case PlatformProvider.eWork.SOpenLeaderBoard:
                OpenLeaderBoard();
                break;

            case PlatformProvider.eWork.SUpdateLeaderBoard:
                UpdateLeaderBoard();
                break;
        }
    }

    protected void AuthenticateLogin(Action<bool> callback)
    {
        Social.localUser.Authenticate(callback);
    }

    protected void Operation(PlatformProvider.eWork work, Action<bool> action = null, params string[] extraData)
    {
        mData.Set(work, action, extraData);

        Operation(mData);
    }
    
    protected virtual void Reset()
    {
        mProvider.Reset();
        mData.Reset();
        mProvider = null;
        mData = null;
    }

    protected abstract string GetUserName();
    protected abstract string GetUserIconURL();
    protected abstract string GetEmail();

    protected abstract void Login();
    protected abstract void Logout();
    protected abstract void LoadAchievement();
    protected abstract void OpenAchievement(bool dummy = true);//PlatformData의 action의 매개인자가 bool이 존재해서 발생하는 문제. 나중에 정리해야겠음.
    protected abstract void OpenLeaderBoard(bool dummy = true);//PlatformData의 action의 매개인자가 bool이 존재해서 발생하는 문제. 나중에 정리해야겠음.
    protected abstract void UpdateAchievement();
    protected abstract void UpdateLeaderBoard();
    
    protected abstract bool IsValid();

    protected class SortComparer : IComparer<IAchievement>
    {
        public int Compare(IAchievement lhs, IAchievement rhs)
        {
            if (lhs.completed && !rhs.completed)
            {
                return (UserData.Instance.IsGiveAchievement(lhs.id)) ? -1 : 1;
            }
            else if (!lhs.completed && rhs.completed)
            {
                return (UserData.Instance.IsGiveAchievement(rhs.id)) ? 1 : -1;
            }
            else
            {
                if (UserData.Instance.IsGiveAchievement(lhs.id) && !UserData.Instance.IsGiveAchievement(rhs.id))
                {
                    return -1;
                }
                else if (!UserData.Instance.IsGiveAchievement(lhs.id) && UserData.Instance.IsGiveAchievement(rhs.id))
                {
                    return 1;
                }
                else
                {
                    return System.String.Compare(lhs.id, rhs.id, System.StringComparison.Ordinal);
                }
            }
        }
    }

    protected class SortComparerSocialAchievement : IComparer<SocialAchievement>
    {
        public int Compare(SocialAchievement lhs, SocialAchievement rhs)
        {
            if (lhs.GetBoolValue(GameTableSetting.eBoolKind.Completed) && !rhs.GetBoolValue(GameTableSetting.eBoolKind.Completed))
            {
                return (UserData.Instance.IsGiveAchievement(lhs.GetStringValue(GameTableSetting.eStringKind.ID))) ? -1 : 1;
            }
            else if (!lhs.GetBoolValue(GameTableSetting.eBoolKind.Completed) && rhs.GetBoolValue(GameTableSetting.eBoolKind.Completed))
            {
                return (UserData.Instance.IsGiveAchievement(rhs.GetStringValue(GameTableSetting.eStringKind.ID))) ? 1 : -1;
            }
            else
            {
                if (UserData.Instance.IsGiveAchievement(lhs.GetStringValue(GameTableSetting.eStringKind.ID)) && !UserData.Instance.IsGiveAchievement(rhs.GetStringValue(GameTableSetting.eStringKind.ID)))
                {
                    return -1;
                }
                else if (!UserData.Instance.IsGiveAchievement(lhs.GetStringValue(GameTableSetting.eStringKind.ID)) && UserData.Instance.IsGiveAchievement(rhs.GetStringValue(GameTableSetting.eStringKind.ID)))
                {
                    return 1;
                }
                else
                {
                    return System.String.Compare(
                        lhs.GetStringValue(GameTableSetting.eStringKind.ID),
                        rhs.GetStringValue(GameTableSetting.eStringKind.ID),
                        System.StringComparison.Ordinal);
                }
            }
        }
    }
}
