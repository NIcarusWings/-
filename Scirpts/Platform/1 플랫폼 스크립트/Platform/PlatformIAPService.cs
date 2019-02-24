using UnityEngine;
using System.Collections;

public abstract class PlatformIAPService
{
    protected IAPManager mIAPManager;
    protected PlatformProvider mProvider;
    protected PlatformProvider.PlatformData mData;

    protected SocialAchievement[] mAchievements;
    protected ARewardData[] mRewardList;

    public void Operation(PlatformProvider provider, PlatformProvider.PlatformData data)
    {
        mProvider = provider;
        
        Operation(data);
    }

    protected void Operation(PlatformProvider.PlatformData data)
    {
        mData = data;

        switch (data.GetWork())
        {
            case PlatformProvider.eWork.BInitialize:
                Initialize();
                break;

            case PlatformProvider.eWork.BPurchase:
                Purchase();
                break;
        }
    }

    protected abstract void Initialize();
    protected abstract void Purchase();
}
