using UnityEngine;

public class RecycleView : CView
{
    [SerializeField] private GenericInventory mRecycleGI;
    [SerializeField] private GenericInventory mTargetGI;

    private int mCurrentTap = 0;
    
    //활성화하면 렉이 걸리는데 최초일 경우에만 발생한다.
    //이유는 최초 초기화할 때 아이템이 없어서 그런 것이다.
    //이에 대한 방안책으로 타이틀이든 로비이든 최초 실행되는 스크립트에서 객체 풀을 생성하여 미리 만들어두는 방식이 있다.
    //음... 이 방식에 대해서 한 번 생각을 해봐야겠다.(생성하는 것이랑 가비지 컬렉터 비워두는 것이랑 등등.)
    void Awake()
    {
        if (mRecycleGI != null)
        {
            mRecycleGI.Initialize(GISettingData.eKind.GI_WHOLE_AMOUNT_RECYCLE, mCurrentTap);
        }

        if (mTargetGI != null)
        {
            mTargetGI.Initialize(GISettingData.eKind.GI_SELECT_RECYCLE, mCurrentTap);
        }
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();

        if (mRecycleGI != null)
        {
            mRecycleGI.UpdateInventory();
        }

        if (mTargetGI != null)
        {
            mTargetGI.UpdateInventory(mCurrentTap);
        }
    }

    public void OnClick_RemoveItems()
    {
        mRecycleGI.OperatorRemoveItems();
    }

    public void OnClick_Tap()
    {
        int tap = UIButton.current.transform.GetSiblingIndex();

        if (mCurrentTap != tap)
        {
            mTargetGI.UpdateInventory(tap);

            mCurrentTap = tap;
        }
    }
}
