#if UNITY_ANDROID
using UnityEngine;
using GooglePlayGames.BasicApi;
using System;
using System.Collections;

#region 주석
//구글에서 제공하는 업적의 내용은 IAchievement, IAchievementDescription를 기본적으로 제공한다.
//PlayGamesPlatform.Instance.을 한 다음 Achievement를 입력해보면 여러가지는 나오지만 
//구글에 등록된 업적들을 가져올 목적이라면 대표적으로 다음과 같은 함수를 중심으로 보게 된다.
//---------------------------------------------------------------------------------------------------------------------
//1. PlayGamesPlatform.Instance.GetAchievement - 리턴 값 Achievement 클래스.
//2. PlayGamesPlatform.Instance.LoadAchievements - 매개 인자를 IAchievement[] 함수를 받음.(콜백함수)
//3. PlayGamesPlatform.Instance.LoadAchievementDescriptions; - 매개 인자를 IAchievementDescription 함수를 받음.(콜백함수)
//---------------------------------------------------------------------------------------------------------------------
//여기서 복수를 가져올 목적이라면 1번은 제외하고 2, 3번째의 함수에 중심을 두게 된다.
//근데 이 망할 구글놈들이 인터페이스에 업적의 모든 정보를 다 제공해주지 않는 것이 문제가 된다.
//그러므로 어쩔수 없이 첫 번째 함수를 써야되는데 이 더블 망할 놈들이 정보제공을 애매하게 만들어두었다.(자세한 것은 Achievement, PlayGamesAchievement 클래스를 비교)
//그래서 생각해낸 것이 IAchievement 와 IAchievementDescription를 상속받는 놈이 어떤 놈인지 검색해보니
//나오는 클래스가 PlayGamesAchievement 라는 놈이 있다.(아싸 인제 이놈 리턴 받으면 끝나겠구나.~~ 라고 생각했더니...)
//근데 이 트리플 망할 놈들이 PlayGamesAchievement 클래스를 리턴으로 제공하는 함수가 없다.(-.-...)
//그래서 PlayGamesAchievement 클래스의 internal PlayGamesAchievement(Achievement ach) : this() 생성자를 분석해보니
//Achievement를 넘겨서 초기화를 하는 것을 알게 되었다.(트리플 이상 망할 놈들이 되지 않아서 감사하게 생각 중이다...)
//P.S Achievement를 그대로 사용할 수도 있으나 단어가 매칭되지 않으므로 PlayGamesAchievement 생성한 뒤 Achievement로 초기화를 하는 것으로 함.
//...PlayGamesAchievement를 사용했더니 이번에는 업적 이미지가 받아와지지 않는다.(씨밤...!!!!)
//내부 소스코드를 보니 이 밎힌 놈들이 코루틴을 써야되는데 일반 함수로 호출하는 바람에 되지 않았던 것이다.(개쓰레기 ㅡㅡ)
//자세한 것은 PlayGamesAchievement의 Getter의 image 안에 LoadImage 함수를 참고
//그래서 결국 PlayGamesAchievement에서 Achievement로 돌아왔음(ㅡㅡX), 이미지 호출하는 것도 자체함수 제작해서 불러옴.
#endregion
public class GoogleAchievement : SocialAchievement
{
    private Achievement mAchievement;

    private WWW mImageFetcher;
    
    public override void SetAchievement(Achievement achievement, ARewardData data)
    {
        mAchievement = achievement;
        mRewardData = data;
    }
    
    public override bool GetBoolValue(GameTableSetting.eBoolKind kind)
    {
        bool value = false;

        switch (kind)
        {
            case GameTableSetting.eBoolKind.Completed:
                value = mAchievement.IsUnlocked;
                break;

            case GameTableSetting.eBoolKind.Revealed:
                value = mAchievement.IsRevealed;
                break;

            case GameTableSetting.eBoolKind.IsIncremental:
                value = mAchievement.IsIncremental;
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
                value = (int)mAchievement.Points;
                break;

            case GameTableSetting.eIntKind.CurrentSteps:
                if (mAchievement.IsUnlocked && !mAchievement.IsIncremental)
                {
                    value = 1;
                }
                else
                {
                    value = mAchievement.CurrentSteps;
                }
                break;

            case GameTableSetting.eIntKind.TotalSteps:
                value = mAchievement.TotalSteps;
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
                value = (float)mAchievement.CurrentSteps / ((mAchievement.TotalSteps == 0) ? 1 : mAchievement.TotalSteps);
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
                value = GetPercentCompleted();
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
                value = mAchievement.Id;
                break;

            case GameTableSetting.eStringKind.Title:
                value = mAchievement.Name;
                break;

            case GameTableSetting.eStringKind.Description:
                value = mAchievement.Description;
                break;

            case GameTableSetting.eStringKind.RevealedImageUrl:
                value = mAchievement.RevealedImageUrl;
                break;

            case GameTableSetting.eStringKind.UnlockedImageUrl:
                value = mAchievement.UnlockedImageUrl;
                break;
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
            case GameTableSetting.eDateTimeKind.LastReportedDate:
                value = mAchievement.LastModifiedTime;
                break;
        }

        return value;
    }

    public override IEnumerator SetTexture(GameTableSetting.eTextureKind kind, UITexture image, Action onCallback)
    {
        //활성화되어있을 때만
        if (mAchievement.IsRevealed)
        {
            string url = (mAchievement.IsUnlocked) ? mAchievement.UnlockedImageUrl : mAchievement.RevealedImageUrl;
            
            // the url can be null if the image is not configured.
            if (!string.IsNullOrEmpty(url))
            {
                if (mImageFetcher == null || mImageFetcher.url.CompareTo(url) != 0)
                {
                    mImageFetcher = new WWW(url);
                    yield return mImageFetcher;
                }

                if (mImageFetcher.isDone)
                {
                    image.mainTexture = mImageFetcher.texture;
                    image.Update();

                    if (onCallback != null)
                    {
                        onCallback();
                    }
                }
            }

            // if there is no url, always return null.
            yield return null;
        }
    }
    
    private double GetPercentCompleted()
    {
        double value = 0;

        if (mAchievement.IsIncremental)
        {
            if (mAchievement.TotalSteps > 0)
            {
                value =
                    ((double)mAchievement.CurrentSteps / (double)mAchievement.TotalSteps) * 100.0;
            }
            else
            {
                value = 0.0;
            }
        }
        else
        {
            value = mAchievement.IsUnlocked ? 100.0 : 0.0;
        }

        return value;
    }
}
#endif