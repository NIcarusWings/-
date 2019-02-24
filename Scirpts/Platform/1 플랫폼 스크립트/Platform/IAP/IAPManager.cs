using UnityEngine;
using System.Collections;

public abstract class IAPManager : MonoBehaviour
{
    protected enum ePurchaseState { None = -1, RequestPaymenetItemInfo, RequestPaymenet, Exit };
    protected ePurchaseState mPurchaseState = ePurchaseState.None;

    protected AndroidJavaClass unityPlayerClass = null;
    protected AndroidJavaObject currentActivity = null;

    protected PlatformProvider.PlatformData mData;
    
    public virtual void Purchase(PlatformProvider.PlatformData data)
    {
        mPurchaseState = ePurchaseState.RequestPaymenetItemInfo;
        mData = data;
    }
    
    //Initialize
    public virtual void Initialize()
    {
        if (unityPlayerClass == null)
        {
            unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    protected virtual void OnDestroy()
    {
        if (unityPlayerClass != null)
            unityPlayerClass.Dispose();
        if (currentActivity != null)
            currentActivity.Dispose();
    }
}
