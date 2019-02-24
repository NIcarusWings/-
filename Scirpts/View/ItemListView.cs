using UnityEngine;
using System.Collections;

public class ItemListView : CView
{
    [SerializeField]
    private GenericInventory mItemListGI;

    private int mCurrentTap = -1;

    private void Awake()
    {
        mItemListGI.Initialize(GISettingData.eKind.GI_KIND_EQUIPMENT, 0);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (mItemListGI != null)
        {
            mItemListGI.UpdateInventory();
        }
    }

    public void OnClick_Tap()
    {
        int tap = UIButton.current.transform.GetSiblingIndex();

        if (mCurrentTap != tap)
        {
            mItemListGI.UpdateInventory(tap);

            mCurrentTap = tap;
        }
    }
}
