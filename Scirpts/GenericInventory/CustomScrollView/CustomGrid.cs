using UnityEngine;
using System.Collections.Generic;

public class CustomGrid : UIGrid
{
    private CustomScrollView mScrollView;

    private GIOperator mGIOperator;
    
    protected override void Start()
    {
        base.enabled = false;
    }

    public void Initialize(GIOperator giOperator)
    {
        mGIOperator = giOperator;
        mScrollView = giOperator.GetInvScrollView();
    }

    public override void Reposition()
    {
        if (Application.isPlaying && !mInitDone && NGUITools.GetActive(gameObject)) Init();
        if (mScrollView == null) return;

        List<GIFieldLine> fieldLines = mGIOperator.GetFieldLines();

        Vector3 baseAxisValue = GetBaseFieldPosition();

        for (int i = 0; i < fieldLines.Count; ++i)
        {
            switch (mScrollView.movement)
            {
                case UIScrollView.Movement.Horizontal:
                    fieldLines[i].SetPosition(
                        (Vector3.right * baseAxisValue.x) +
                        (Vector3.up * baseAxisValue.y),
                        Space.Self);

                    baseAxisValue.x = baseAxisValue.x + cellWidth;
                    break;

                case UIScrollView.Movement.Vertical:
                    fieldLines[i].SetPosition(
                        (Vector3.right * baseAxisValue.x) +//x
                        (Vector3.up * baseAxisValue.y),//y 
                        Space.Self);

                    baseAxisValue.y = baseAxisValue.y - cellHeight;
                    break;
            }            
        }

        //List<UIInventoryItem[]> visibleItemList = mGenericInventory.GetVisibleItemList();

        //Vector3 baseAxisValue = GetBaseFieldPosition();
        //Vector3 baseGapValue = GetAxisValue(0.5f);

        //for (int i = 0; i < visibleItemList.Count; ++i)
        //{
        //    for (int j = 0; j < visibleItemList[i].Length; ++j)
        //    {
        //        switch (mScrollView.movement)
        //        {
        //            case UIScrollView.Movement.Horizontal:
        //                visibleItemList[i][j].SetPosition(
        //                    (Vector3.right * (((j + 1) * cellWidth) - baseGapValue.x)) +
        //                    (Vector3.up * baseAxisValue.y),
        //                    Space.Self);
        //                break;

        //            case UIScrollView.Movement.Vertical:
        //                visibleItemList[i][j].SetPosition(
        //                    (Vector3.right * (((j + 1) * cellHeight) - baseGapValue.y)) +
        //                    (Vector3.up * baseAxisValue.y),
        //                    Space.Self);
        //                break;
        //        }
        //    }

        //    baseAxisValue.Set(
        //        baseAxisValue.x - cellWidth,
        //        baseAxisValue.y - cellHeight,
        //        0);
        //}
    }

    private Vector3 GetBaseFieldPosition()
    {
        CPoint curField = mScrollView.GetCurrentField();
        Vector3 cellAxisValue = GetAxisValue();
        Vector3 result = Vector3.zero;

        //Vertical로 설정되어있을 경우.
        //TopLeft(무조건 이 옵션으로 고정)설정이 되어있으므로 0을 기준점에서 밑으로 배치해야되므로 최종 결과에 음수를 붙여줘야한다.
        //cell 간격의 절반을 더해야하는 건 아이템들 역시 TopLeft를 기준으로 설정되어있기 때문이다.
        //참고로 스크롤 뷰의 Movement에 따라서 result 값이 전부다 사용될 수도 있고 그렇지 않을 수도 있다는 점이다.
        //스크롤 뷰의 movement가 Horizontal이면 result.x의 값만 사용되고 result.y값은 사용되지 않는다.
        //스크롤 뷰의 movement가 Vertical이면 result.x의 값은 사용되지 않고 result.y값만 사용된다.
        //스크롤 뷰의 movement가 Custom이면 result.x 와 result.y값 둘다 사용된다.
        result.Set(
            ((curField.x * cellAxisValue.x) + (cellAxisValue.x * 0.5f)),
            -((curField.y * cellAxisValue.y) + (cellAxisValue.y * 0.5f)),
            0);

        return result;
    }

    public Vector3 GetAxisValue(float ratio = 1f)
    {
        Vector3 value = Vector3.zero;

        value.Set(cellWidth * ratio, cellHeight * ratio, 0);

        return value;
    }
}
