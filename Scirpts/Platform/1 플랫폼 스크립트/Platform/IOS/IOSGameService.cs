#if UNITY_IOS
using UnityEngine;
using UnityEngine.SocialPlatforms;
using System;
using System.Collections;
using UnityEngine.SocialPlatforms.GameCenter;

public class IOSGameService : PlatformGameService
{
    protected override void Login()
    {
        if (!Social.localUser.authenticated)
        {
            AuthenticateLogin(CallBackLogin);
        }
    }

    //IOS에는 로그아웃 기능을 제공해주지 않음.
    protected override void Logout(){}

    protected override void OpenLeaderBoard(bool dummy = true)
    {
        if (IsValid())
        {
            
        }
        else
        {
            base.Operation(PlatformProvider.eWork.SLogin);
        }
    }

    protected override bool IsValid()
    {
        if (!GameManager.IsServiceMode()) return false;
        if (!Social.localUser.authenticated) return false;

        return true;
    }

    #region 업적
    //IOS의 경우 IAchievement는 한 번이라도 이 업적이 변화된적이 있을 경우에만 호출된다.
    //따라서 업적의 기본 데이터를 토대로 출력할려면 IAchievement가 아닌 IAchievementDescription를 기준으로 출력해야한다.
    //만약 해당 업적에 따라서 연관되는 데이터를 따로 초기화할 경우 반드시 IAchievementDescription를 기준으로 처리해야한다.
    //위에 언급하다시피 IAchievement는 한 번이라도 이 업적이 변화된적이 있을 경우에만 호출되기 때문에 인덱스가 맞지 않아서 오류가 발생할 수가 있다.
    //(- 이 부분은 로그를 찍어보면서 확인을 해봤음.)
    //추가적으로 구글과는 달리 정렬을 마지막에 작업을 해줘야하며 배열인자도 다르다. 왜냐하면 IAchievement가 언제나 모든 업적을 가지고 오는 것이 아니므로
    //IAchievementDescription를 통해 기본 업적들을 깔아두고 IAchievement의 배열인자와 비교해서 인덱스를 찾고 그 찾은 업적을
    //SocialAchievement 웹퍼 클래스에 적용시킨다. 이후 SocialAchievement 웹퍼 클래스 배열을 가지고 정렬을 해야 모든 업적을 정렬할 수가 있다.
    //...결론 IOS 게임센터로부터 업적가져와서 작업하자는 이야기가 나오면 무조건 반대해야겠음 ㅡㅡ...
    protected override void LoadAchievement()
    {
        bool isFinishLoadAchievement = false;

        Social.LoadAchievements(
            (IAchievement[] achievementList) =>
            {
                if (mRewardList == null)
                {
                    mRewardList = GameDataManager.Instance.GetData<ARewardData>(DATA_TYPE.GA_REWARD);
                }
                
                Social.LoadAchievementDescriptions(
                    (IAchievementDescription[] achievementDescriptionList) =>
                    {
                        SortList(achievementDescriptionList, ref mRewardList);

                        if (mAchievements == null)
                        {
                            mAchievements = new IOSAchievement[achievementDescriptionList.Length];
                        }
                        
                        IAchievement achievement = null;                        
                        
                        for (int i = 0; i < achievementDescriptionList.Length; ++i)
                        {
                            achievement = FindAchievement(achievementList, achievementDescriptionList[i].id, i);

                            if (mAchievements[i] == null)
                            {
                                mAchievements[i] = new IOSAchievement();
                            }

                            mAchievements[i].SetAchievement(
                                    achievement,
                                    achievementDescriptionList[i],
                                    mRewardList[i]);
                        }

                        SortComparerSocialAchievement sortComparerSocialAchievement = new SortComparerSocialAchievement();
                        
                        Array.Sort(mAchievements, sortComparerSocialAchievement);

                        isFinishLoadAchievement = true;

                        if (isFinishLoadAchievement)
                        {
                            if (mData.GetAction() != null)
                            {
                                mData.GetAction().Invoke(true);
                            }
                        }
                });
        });
    }

    private void Test2(Array a, IComparer c)
    {

    }

    protected override void OpenAchievement(bool dummy = true)
    {
        if (IsValid())
        {
            Social.ShowAchievementsUI();
        }
        else
        {
            base.Operation(PlatformProvider.eWork.SLogin);
        }
    }

    protected override void UpdateAchievement()
    {
        if (CommDataManager.IsServerWork && Social.localUser.authenticated)
        {
            string[] extraData = mData.PopStringExtraData();
            ARewardData data = FindAchievementData((SocialAchievement.eAchievementKey)Enum.Parse(typeof(SocialAchievement.eAchievementKey), extraData[0]));
            //Achievement achievement = PlayGamesPlatform.Instance.GetAchievement(data.achievementID);
            //mCurAchievementIndex = FindAchievementData((ARewardData.eKey)Enum.Parse(typeof(ARewardData.eKey), value[0]));
            
            switch (data.updateType)
            {
                case ARewardData.eUpdateType.Single:
                    Social.ReportProgress(
                        data.achievementIOSID,
                        100.0, CallbackUpdateAchievement);
                    break;

                case ARewardData.eUpdateType.Step:
                    {
                        int currentValue = -1;
                        int maxValue = data.limitValue;

                        if (extraData.Length >= 2)
                        {
                            currentValue = int.Parse(extraData[1]);                            
                        }

                        Social.ReportProgress(
                            data.achievementIOSID,
                            ((float)currentValue / maxValue) * 100.0, CallbackUpdateAchievement);

                        //PlayGamesPlatform.Instance.SetStepsAtLeast(
                        //    data.achievementID,
                        //    extraValue,
                        //    CallbackUpdateAchievement);
                    }
                    break;
            }

            //if (sAchievements[mCurAchievementIndex].GetBoolData(SocialAchievement.eBoolKind.Completed)) return;

            //if (sAchievements[mCurAchievementIndex].GetBoolData(SocialAchievement.eBoolKind.IsIncremental))
            //{
            //    PlayGamesPlatform.Instance.IncrementAchievement(
            //        sAchievements[mCurAchievementIndex].GetStringData(SocialAchievement.eStringKind.ID),
            //        int.Parse(value[1]),
            //        CallbackUpdateAchievement);
            //}
            //else
            //{
            //    Social.ReportProgress(
            //        sAchievements[mCurAchievementIndex].GetStringData(SocialAchievement.eStringKind.ID), 
            //        100.0, CallbackUpdateAchievement);
            //}
        }
    }

    private void CallbackUpdateAchievement(bool result)
    {
        if (result)
        {
            //sGoogleAchievements[mCurAchievementIndex].SetAchievement(
            //    PlayGamesPlatform.Instance.GetAchievement(
            //        sGoogleAchievements[mCurAchievementIndex].GetStringData(
            //            SocialAchievement.eStringKind.ID)));
        }
        else
        {
            Debug.Log("CallbackUnlockAchievement - 실패.");
        }

        if (mData.GetAction() != null)
        {
            mData.GetAction().Invoke(result);
        }
    }

    private ARewardData FindAchievementData(SocialAchievement.eAchievementKey key)
    {
        ARewardData data = null;

        if (mRewardList == null)
        {
            mRewardList = GameDataManager.Instance.GetData<ARewardData>(DATA_TYPE.GA_REWARD);
        }

        int i = 0;

        while (i < mRewardList.Length)
        {
            data = mRewardList[i];

            if (data.key.CompareTo(key) == 0) break;

            ++i;
        }

        return data;
    }
#endregion

#region 리더 보드
    protected override void UpdateLeaderBoard()
    {
        if (IsValid())
        {
            string[] extraData = mData.PopStringExtraData();

            Social.ReportScore(int.Parse(extraData[1]), extraData[0], CallBackUpdateLeaderBoard);
        }
    }
#endregion

    //GPGS Login CallBack
    private void CallBackLogin(bool result)
    {
        if (result)
        {
            CallbackAction(true);
        }
        else
        {
            PopupManager.Instance.OpenNotificationMenu("FAILED_LOGIN_IOS", null, CallbackOK);
        }
    }

    private void CallbackOK()
    {
        CallbackAction(false);
    }

    private void CallbackAction(bool result)
    {
        Action<bool> action = mData.GetAction();

        if (action != null)
        {
            action.Invoke(result);
        }
    }

    private void CallBackUpdateLeaderBoard(bool result)
    {
        if (!result)
        {

        }
    }

    //GPGS 에서 사용자 이름을 가져옵니다.
    protected override string GetUserName()
    {
        if (Social.localUser.authenticated)
        {
            return Social.localUser.userName;
        }
        else
        {
            return null;
        }
    }

    protected override string GetUserIconURL()
    {
        return null;
        //if (IsValid())
        //{
        //    return ((PlayGamesLocalUser)Social.localUser).AvatarURL;
        //}
        //else
        //{
        //    return null;
        //}
    }

    // GPGS에서 자신의 프로필 이미지를 가져옵니다.
    public Texture2D GetImageTexture()
    {
        if (IsValid())
        {
            return Social.localUser.image;
        }
        else
        {
            return null;
        }
    }

    protected override string GetEmail()
    {
        return null;
        //if (IsValid())
        //{
        //    return ((PlayGamesLocalUser)Social.localUser).Email;
        //}
        //else
        //{
        //    return null;
        //}
    }

    private IAchievement FindAchievement(IAchievement[] achievementList, string key, int achievementDescriptionIndex)
    {
        IAchievement achievement = null;

        if (achievementList != null)
        {
            for (int i = 0; i < achievementList.Length; ++i)
            {
                if (System.String.Compare(
                    achievementList[i].id,
                    key,
                    System.StringComparison.Ordinal) == 0)
                {
                    achievement = achievementList[i];
                    break;
                }
            }
        }

        return achievement;
    }
    
    private void SortList(IAchievementDescription[] achievements, ref ARewardData[] rewardData)
    {
        ARewardData[] temp = new ARewardData[rewardData.Length];

        for (int i = 0; i < achievements.Length; ++i)
        {
            for (int j = 0; j < rewardData.Length; ++j)
            {
                if (achievements[i].id.CompareTo(rewardData[j].GetAchievementID()) == 0)
                {
                    temp[i] = rewardData[j];
                    break;
                }
            }
        }

        rewardData = temp;
    }
}
#endif