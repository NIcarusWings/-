using System;
using System.Collections.Generic;

public class PlatformProvider
{
    protected eStore mStore = eStore.Google;
    protected eSocial mSocialByStore = eSocial.None;
    protected eSocial mCurSocial = eSocial.None;

    protected Dictionary<eSocial, PlatformGameService> mPlatformGameService;
    protected PlatformIAPService mPlatformIAPService;
    
    protected PlatformData mData;

    public PlatformProvider(eStore store)
    {
        mStore = store;

        mPlatformGameService = new Dictionary<eSocial, PlatformGameService>();
        mSocialByStore = ConvertStoreToSocial();
    }
    
    public void Operation(eService service, eWork work, Action<bool> action, params string[] extraData)
    {
        switch (service)
        {
            case eService.SocialService:
                Operation(mSocialByStore, service, work, action, extraData);
                break;

            case eService.Billing:
                CreateService(service);
                CreateData(work, action, extraData);

                mPlatformIAPService.Operation(this, mData);
                break;
        }
    }
    
    public void Operation(eSocial social, eService service, eWork work, Action<bool> action, params string[] extraData)
    {
        CreateService(social, service);
        CreateData(work, action, extraData);
        
        switch (service)
        {
            case eService.SocialService:
                mPlatformGameService[social].Operation(this, mData);
                break;
        }
    }

    private void CreateService(eService service)
    {
        switch (service)
        {
            case eService.Billing:
                CreateIAPService();
                break;
        }
    }

    private void CreateService(eSocial social, eService service)
    {
        mCurSocial = social;
        
        if (!mPlatformGameService.ContainsKey(social))
        {
            switch (service)
            {
                case eService.SocialService:
                    CreateGameService();
                    break;
            }
        }
    }
    
    private void CreateGameService()
    {
        PlatformGameService platformGameService = null;

        switch (mCurSocial)
        {
            case eSocial.Google:
#if UNITY_ANDROID
                platformGameService = new GPGService();
#endif
                break;

            case eSocial.IOS:
#if UNITY_IOS
                platformGameService = new IOSGameService();
#endif
                break;

        }

        mPlatformGameService.Add(mCurSocial, platformGameService);
    }

    private void CreateIAPService()
    {
        if (mPlatformIAPService == null)
        {
            switch (mStore)
            {
                case eStore.OneStore:
                    mPlatformIAPService = new OneStoreIAPService();
                    break;
            }
        }
    }

    ////업적 대비용으로 만든것인데 아직 미완성임.
    //public void Operation(eService service, eWork work, Action<bool> action, params int[] extraData)
    //{
    //    CreateService(service);
    //    CreateData(work, action, extraData);

    //    mPlatformGameService.Operation(this, mData);
    //}

    public string GetStringData(eSocial social, eWork work, params string[] extraData)
    {
        CreateData(work, null, extraData);

        return mPlatformGameService[social].GetStringData(mData);
    }

    //시간이 없어서 일단 이렇게 만들긴 했는데... 좀 더 효율적인 관리 방식을 고민해봐야될듯...
    public void OutputAchievements(eSocial social, out SocialAchievement[] achievements)
    {
        achievements = null;

        if (mPlatformGameService != null)
        {
            mPlatformGameService[social].OutputAchievements(out achievements);
        }
    }

    //시간이 없어서 일단 이렇게 만들긴 했는데... 좀 더 효율적인 관리 방식을 고민해봐야될듯...
    public void OutputAchievement(eSocial social, SocialAchievement.eAchievementKey key, out SocialAchievement achievement)
    {
        achievement = null;

        if (mPlatformGameService != null)
        {
            mPlatformGameService[social].OutputAchievement(key, out achievement);
        }
    }

    protected void CreateData(eWork work, Action<bool> action, params int[] extraData)
    {
        if (mData == null)
        {
            mData = new PlatformData();
        }
        
        mData.Set(work, action, extraData);
    }

    protected void CreateData(eWork work, Action<bool> action, params string[] extraData)
    {
        if (mData == null)
        {
            mData = new PlatformData();
        }

        mData.Set(work, action, extraData);
    }

    public void Reset()
    {
        if (mPlatformGameService != null)
        {
            if (mPlatformGameService.ContainsKey(mCurSocial))
            {
                mPlatformGameService[mCurSocial] = null;
                mPlatformGameService.Remove(mCurSocial);
            }
        }

        if (mPlatformIAPService != null)
        {
            mPlatformIAPService = null;
        }

        if (mData != null)
        {
            mData = null;
        }
        
        PlatformStoreManager.Instance.Release();
        mSocialByStore = eSocial.None;
        mCurSocial = eSocial.None;
    }

    public eSocial GetSocialByStore()
    {
        return mSocialByStore;
    }

    public eSocial GetCurSocial()
    {
        return mCurSocial;
    }

    private eSocial ConvertStoreToSocial()
    {
        switch (mStore)
        {
            case eStore.Google:
                mSocialByStore = eSocial.Google;
                break;

            case eStore.OneStore:
                mSocialByStore = eSocial.Google;
                break;

            case eStore.IOS:
                mSocialByStore = eSocial.IOS;
                break;
        }

        return mSocialByStore;
    }

    public class PlatformData
    {
        private eWork mWork = eWork.None;
        private Action<bool> mAction = null;
        private Stack<int[]> mIntExtreaData = null;
        private Stack<string[]> mStackStringExtra = null;

        public void Set(eWork work, Action<bool> action, params int[] extraData)
        {
            mWork = work;
            mAction = action;

            if (mIntExtreaData == null)
            {
                mIntExtreaData = new Stack<int[]>();
            }

            mIntExtreaData.Push(extraData);
        }

        public void Set(eWork work, Action<bool> action, params string[] extraData)
        {
            mWork = work;
            mAction = action;

            if (mStackStringExtra == null)
            {
                mStackStringExtra = new Stack<string[]>();
            }

            mStackStringExtra.Push(extraData);
        }
        
        public void Reset()
        {
            mWork = eWork.None;
            mAction = null;
            mStackStringExtra = null;
        }

        public void SetWork(eWork work)
        {
            mWork = work;
        }

        public eWork GetWork()
        {
            return mWork;
        }

        public Action<bool> GetAction()
        {
            return mAction;
        }

        public string[] PopStringExtraData()
        {
            return mStackStringExtra.Pop();
        }
    }

    public enum eStore : byte
    {
        None = 0,
        Google, IOS, OneStore,
        Count,
    };

    public enum eSocial : byte
    {
        None = 0,
        
        Google, IOS,
        Kakao, Facebook,

        Guest,
    }
    public enum eService : byte
    {
        aStart = 0,
        Billing, SocialService,
        aEnd,
    }

    public enum eWork : byte
    {
        None = 0,
#region Billing
        BInitialize,
        BPurchase,
#endregion

#region PlayService
        SLogin, SLogout, SLoadAchivement, SOpenAchievement, SUpdateAchievement, SOpenLeaderBoard, SUpdateLeaderBoard,
        SUserName, SUserIconURL, SEmail
#endregion
    };
    
}
