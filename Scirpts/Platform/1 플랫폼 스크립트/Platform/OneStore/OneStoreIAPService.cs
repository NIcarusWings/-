using UnityEngine;

public class OneStoreIAPService : PlatformIAPService
{
    protected override void Initialize()
    {
        if (mIAPManager == null)
        {
            GameObject go = new GameObject();

            go.name = "OneStoreIAPManager";

            mIAPManager = go.AddComponent<OneStoreIAPManager>();
            mIAPManager.Initialize();
        }        
    }

    protected override void Purchase()
    {
        Initialize();
        mIAPManager.Purchase(mData);
    }
}
