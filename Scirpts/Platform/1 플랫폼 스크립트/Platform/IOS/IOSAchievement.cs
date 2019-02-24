#if UNITY_IOS
using UnityEngine.SocialPlatforms;
using System;
using System.Collections;

public class IOSAchievement : SocialAchievement
{
    private IAchievement mAchievement;
    private IAchievementDescription mAchievementDescription;
    
    //private WWW mImageFetcher;

    //유니티의 공식문서를 참고하면 Social.LoadAchievements는 업적을 달성하거나 Social.ReportProgress 함수를 통해 한번이라도 호출된 경우의 업적을 불러온다. 라고 적혀있다.
    //구글이랑 다르게 Social.LoadAchievements를 한다고 해서 다 불러오는게 아니다.(애시당초 구글은 자체 플러그인을 통해서 불러오기 때문에 기능이 다르다.)
    //구성된 시스템을 살펴보니 Social.LoadAchievementDescriptions는 현재 사용되는 업적 리스트를 불러오는 걸 확인할 수 있었다.(해당 업적이 달성이 되든 안 되든 호출이 되든 안되든 상관없이 불러온다.)
    //따라서 IOS 업적은 Social.LoadAchievementDescriptions를 중심으로 하되 초기화는 IAchievement를 같이 받는 걸로 한다.
    //이유는 처음에는 당연히 IAchievement가 null이지만 게임을 진행하다보면 업적을 달성했을 수도 있고 
    //한 번이라도 Social.ReportProgress를 호출했을 수도 있기 때문이다.
    //(즉 초기화하는 방식은 구글이랑 비슷하니 고칠 필요가 없다. 단 최초 초기화되는 방식이 다르다는 것은 인지해야 나중에 헷갈리지 않는다.)
    //(이에 대해 연관되는 방식은 IOSAchievement생성자를 클릭한 다음 참조(Shift + F12)를 클릭해서 알아볼 것(어짜피 하나밖에 없다.)
    //[구글과는 달리 Step이라는 증가방식이 없기 떄문에 해당 기능은 직접 구현해줘야한다.!!!!!!]
    public override void SetAchievement(IAchievement achievement, IAchievementDescription achievementDescription, ARewardData data)
    {
        mAchievement = achievement;
        mAchievementDescription = achievementDescription;

        mRewardData = data;
    }
    
    public override bool GetBoolValue(GameTableSetting.eBoolKind kind)
    {
        bool value = false;

        switch (kind)
        {
            case GameTableSetting.eBoolKind.Completed:
                value = (mAchievement == null) ? false : mAchievement.completed;
                break;

            case GameTableSetting.eBoolKind.IsIncremental:
                value = (mRewardData.updateType == ARewardData.eUpdateType.Step) ? true : false;
                break;
        }

        return value;
    }

    public override int GetIntValue(GameTableSetting.eIntKind kind)
    {
        int value = -1;

        switch (kind)
        {
            case GameTableSetting.eIntKind.Points:
                value = mAchievementDescription.points;
                break;

            case GameTableSetting.eIntKind.CurrentSteps:
                value = GetCurrentStep();
                break;

            case GameTableSetting.eIntKind.TotalSteps:
                value = mRewardData.limitValue;
                break;

            case GameTableSetting.eIntKind.RewardValue:
                value = mRewardData.rewardValue;
                break;
        }

        return value;
    }

    public override float GetFloatValue(GameTableSetting.eFloatKind kind)
    {
        float value = -1;

        switch (kind)
        {
            case GameTableSetting.eFloatKind.PercentCompleted:
                value = (float)GetDoubleValue(GameTableSetting.eDoubleKind.PercentCompleted);
                break;

            case GameTableSetting.eFloatKind.CompletedBetweenZeroAndOne:
                value = (float)this.GetIntValue(GameTableSetting.eIntKind.CurrentSteps) / 
                        ((this.GetIntValue(GameTableSetting.eIntKind.TotalSteps) == 0) ? 1 : this.GetIntValue(GameTableSetting.eIntKind.TotalSteps));
                break;
        }

        return value;
    }

    public override double GetDoubleValue(GameTableSetting.eDoubleKind kind)
    {
        double value = -1;

        switch (kind)
        {
            case GameTableSetting.eDoubleKind.PercentCompleted:
                value = (mAchievement == null) ? 0 : mAchievement.percentCompleted;
                break;
        }

        return value;
    }

    public override string GetStringValue(GameTableSetting.eStringKind kind)
    {
        string value = null;

        switch (kind)
        {
#region ORIGINAL
            case GameTableSetting.eStringKind.ID:
                value = mAchievementDescription.id;
                break;

            case GameTableSetting.eStringKind.Title:
                value = mAchievementDescription.title;
                break;

            case GameTableSetting.eStringKind.Description:
                value = mAchievementDescription.achievedDescription;
                break;

            //case eStringKind.RevealedImageUrl:
            //    value = mAchievement.RevealedImageUrl;
            //    break;

            //case eStringKind.UnlockedImageUrl:
            //    value = mAchievement.UnlockedImageUrl;
            //    break;
#endregion

#region CustomStringData
            case GameTableSetting.eStringKind.RewardImage:
                value = GameDataManager.Instance.GetResourceImage(mRewardData.itemSub, false);
                break;

            case GameTableSetting.eStringKind.RewardValue:
                value = mRewardData.rewardValue.ToString();
                break;

            case GameTableSetting.eStringKind.ImageName:
                value = mRewardData.imageName;
                break;
#endregion
#region INT_TO_STRING
            case GameTableSetting.eStringKind.Points:
                value = GetIntValue(GameTableSetting.eIntKind.Points).ToString();
                break;

            case GameTableSetting.eStringKind.CurrentSteps:
                value = GetIntValue(GameTableSetting.eIntKind.CurrentSteps).ToString();
                break;

            case GameTableSetting.eStringKind.TotalSteps:
                value = GetIntValue(GameTableSetting.eIntKind.TotalSteps).ToString();
                break;
#endregion
        }

        return value;
    }

    public override DateTime GetDateTime(GameTableSetting.eDateTimeKind kind)
    {
        DateTime value = default(DateTime);

        switch (kind)
        {
            //case eDateTimeKind.LastReportedDate:
            //    value = mAchievement.LastModifiedTime;
            //    break;
        }

        return value;
    }

    public override IEnumerator SetTexture(GameTableSetting.eTextureKind kind, UITexture image, Action onCallback)
    {
        image.mainTexture = mAchievementDescription.image;

        yield return null;
        //활성화되어있을 때만
        //if (!mAchievement.hidden)
        //{
        //    //string url = (mAchievement.) ? mAchievement.UnlockedImageUrl : mAchievement.RevealedImageUrl;

        //    //// the url can be null if the image is not configured.
        //    //if (!string.IsNullOrEmpty(url))
        //    //{
        //    //    if (mImageFetcher == null || mImageFetcher.url.CompareTo(url) != 0)
        //    //    {
        //    //        mImageFetcher = new WWW(url);
        //    //        yield return mImageFetcher;
        //    //    }

        //    //    if (mImageFetcher.isDone)
        //    //    {
        //    //        image.mainTexture = mImageFetcher.texture;
        //    //        image.Update();

        //    //        if (onCallback != null)
        //    //        {
        //    //            onCallback();
        //    //        }
        //    //    }
        //    //}

        //    // if there is no url, always return null.
        //    yield return null;
        //}
    }
}
#endif