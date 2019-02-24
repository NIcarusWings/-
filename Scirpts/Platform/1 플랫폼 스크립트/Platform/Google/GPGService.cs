#if UNITY_ANDROID
using UnityEngine;
using System;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using Com.Google.Android.Gms.Common.Api;

public class GPGService: PlatformGameService
{
    //private int mCurAchievementIndex;
    
    protected override void Login()
    {
        if (!Social.localUser.authenticated)
        {
            //게임 서비스 플러그인 초기화시에 EnableSavedGame()를 넣어서 저장된 게임을 사용할 수 있게합니다.
            //주의 하실점은 구글 플레이 개발자 콘솔의 게임 서비스에서 해당 게임의 세부정보에서 저장된 게임을
            //사용할 수 있도록 설정해야합니다.
            PlayGamesClientConfiguration config =
                new PlayGamesClientConfiguration.Builder()
                .EnableSavedGames()
                .Build();
            
            PlayGamesPlatform.DebugLogEnabled = false;

            PlayGamesPlatform.InitializeInstance(config);

            //using GooglePlayGames 를 선언해줘야한다.
            PlayGamesPlatform.Activate();
            //유니티 버전 5.4.3 1f의 이하버전에서는 Authenticate에는 매개변수가 하나이지만
            //최신버전에서는 매개변수가 2개를 지정할 수 있다.(결과 변수, 에러발생시 에러발생내용을 가진 string 변수)
            //이에 대해서는 다음과 같은 사이트를 참고
            //https://openlevel.postype.com/post/640518
            AuthenticateLogin(CallBackLogin);            
        }
    }

    //GPGS를 로그아웃 합니다.
    protected override void Logout()
    {
        if (IsValid())
        {
            ((PlayGamesPlatform)Social.Active).SignOut();
            base.Reset();
        }
    }//
    
    protected override void OpenLeaderBoard(bool dummy = true)
    {
        if (IsValid())
        {
            string[] value = mData.PopStringExtraData();

            ((PlayGamesPlatform)Social.Active).ShowLeaderboardUI(value[0]);
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
    protected override void LoadAchievement()
    {
        PlayGamesPlatform.Instance.LoadAchievements(
            (IAchievement[] list) =>
            {
                //등록한 순서대로 정렬하는 방법.
                //string.CompareTo는 대소문자 비교를 안함.!!!
                //Array.Sort(list, (IAchievement lhs, IAchievement rhs) =>
                //    System.String.Compare(lhs.id, rhs.id, System.StringComparison.Ordinal));
                
                if (mAchievements == null)
                {
                    mAchievements = new GoogleAchievement[list.Length];
                }
                
                if (mRewardList == null)
                {
                    mRewardList = GameDataManager.Instance.GetData<ARewardData>(DATA_TYPE.GA_REWARD);
                }

                //달성한 순서대로 정렬하는 방법.
                //Array.Sort(list, delegate(IAchievement lhs, IAchievement rhs)
                //{
                //    if (lhs.completed && !rhs.completed)
                //    {
                //        return (UserData.Instance.IsGiveAchievement(lhs.id)) ? -1 : 1;
                //    }
                //    else if (!lhs.completed && rhs.completed)
                //    {
                //        return (UserData.Instance.IsGiveAchievement(rhs.id)) ? 1 : -1;
                //    }
                //    else
                //    {
                //        if (UserData.Instance.IsGiveAchievement(lhs.id) && !UserData.Instance.IsGiveAchievement(rhs.id))
                //        {
                //            return -1;
                //        }
                //        else if (!UserData.Instance.IsGiveAchievement(lhs.id) && UserData.Instance.IsGiveAchievement(rhs.id))
                //        {
                //            return 1;
                //        }
                //        else
                //        {
                //            return System.String.Compare(lhs.id, rhs.id, System.StringComparison.Ordinal);
                //        }
                //    }
                //});
                SortComparer sortComparer = new SortComparer();

                //Array.Sort(list, mRewardList, sortComparer);
                Array.Sort(list, sortComparer);
                SortList(list, ref mRewardList);

                //Debug.Log("==============Start List Achievement=============");
                //for (int i = 0; i < list.Length; ++i)
                //{
                //    Debug.Log("List ID : " + list[i].id);
                //}

                //Debug.Log("==============End List Achievement=============");

                //Debug.Log("==============Start mRewardList Achievement=============");
                //for (int i = 0; i < mRewardList.Length; ++i)
                //{
                //    Debug.Log("mRewardList ID : " + mRewardList[i].achievementID);
                //}
                //Debug.Log("==============End mRewardList Achievement=============");

                for (int i = 0; i < list.Length; ++i)
                {
                    if (mAchievements[i] == null)
                    {
                        mAchievements[i] = new GoogleAchievement();
                    }

                    mAchievements[i].SetAchievement(
                            PlayGamesPlatform.Instance.GetAchievement(list[i].id),
                            mRewardList[i]);
                }

                if (mData.GetAction() != null)
                {
                    mData.GetAction().Invoke(true);
                }

                mData.Reset();
            });
    }

    private void SortList(IAchievement[] achievements, ref ARewardData[] rewardData)
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
            int extraValue = 1;

            if (extraData.Length >= 2)
            {
                extraValue = int.Parse(extraData[1]);
            }
            
            switch (data.updateType)
            {
                case ARewardData.eUpdateType.Single:
                    Social.ReportProgress(
                        data.GetAchievementID(),
                        100.0, CallbackUpdateAchievement);
                    break;

                case ARewardData.eUpdateType.Step:
                    PlayGamesPlatform.Instance.SetStepsAtLeast(
                        data.GetAchievementID(),
                        extraValue,
                        CallbackUpdateAchievement);
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
            Action<bool> action = mData.GetAction();

            if (action != null)
            {
                action.Invoke(result);
            }
        }
        else
        {
            PopupManager.Instance.OpenNotificationMenu("FAILED_LOGIN_IOS", null, CallbackOK);
        }
    }

    private void CallbackOK()
    {
        Application.Quit();
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

        if (IsValid())
        {
            return ((PlayGamesLocalUser)Social.localUser).AvatarURL;
        }
        else
        {
            return null;
        }
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
        if (IsValid())
        {
            return ((PlayGamesLocalUser)Social.localUser).Email;
        }
        else
        {
            return null;
        }
    }
}
#endif