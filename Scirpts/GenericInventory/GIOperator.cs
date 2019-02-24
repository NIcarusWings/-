using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GIOperator
{
    public const int NONE_LINE = -1;
    
    private GenericInventory mGenericInventory;

    private CustomScrollView mInvScrollView;
    private CustomGrid mCustomGrid;

    private List<GIFieldLine> mFieldLines;
    private GIItems mGIItems;
    
    private int mVisibleFieldCount = 0;
    private int mItemCountInOwnLine = 0;
    
    public void Initialize(GenericInventory genericInventory)
    {
        InitFields(genericInventory);
        //보이는 개수를 미리 모두 생성해둔다.(생성해봤자 대략 5 ~ 30개(모바일 기준))
        //즉 아이템을 미리 생성해두고 UpdateItems에서 활성화/비활성화 작업을 한다.
        InitItems();
    }

    private void InitFields(GenericInventory genericInventory)
    {
        mGenericInventory = genericInventory;
        
        mFieldLines = new List<GIFieldLine>();
        mGIItems = new GIItems();

        mInvScrollView = genericInventory.GetComponentInChildren<CustomScrollView>();
        mCustomGrid = genericInventory.GetComponentInChildren<CustomGrid>();

        mInvScrollView.Initialize(this);
        mCustomGrid.Initialize(this);
        mGIItems.Initialize(this);

        mVisibleFieldCount = mInvScrollView.GetVisibleItemFieldCount();
        mItemCountInOwnLine = mInvScrollView.GetItemCountInOwnLine();
    }

    //처음에 반드시 한 번만 호출되는 함수이다.
    //현재 보이는 영역을 미리 다 생성해두고 해당 아이템의 개수에 따라서 활성/비활성으로 처리하기 때문이다.
    private void InitItems()
    {
        if (mFieldLines.Count != 0) return;

        GameObject go = null;
        GIFieldLine giFieldLine = null;
        CNData settingData = GetGenericInventory().GetGISettingData();

        for (int i = 0; i < mVisibleFieldCount; ++i)
        {
            go = NGUITools.AddChild(mCustomGrid.gameObject, Utility.LoadResource<GameObject>(settingData.GetStringValue(GameTableSetting.eKey.ResourceLinePath)));
#if UNITY_EDITOR
            go.name = "GIFieldLine";
#endif
            giFieldLine = go.GetComponent<GIFieldLine>();
            giFieldLine.Initialize(i, GetGenericInventory());

            mFieldLines.Add(giFieldLine);
        }
    }

    public void Update(int currentTap)
    {
        mGIItems.UpdateItem(currentTap);

        Refresh();
    }

    public void Refresh()
    {
        ResettingFieldLines();

        UpdateItems();
        UpdateFields();

        mInvScrollView.Resetting();
        mInvScrollView.UpdateBounds();

        if (!mGIItems.IsEmpty())
        {
            mCustomGrid.Reposition();
        }
    }

    //Init이 아니라 Update로 명칭을 정함. 유저가 인벤토리 뷰가 보이는 상태에서 아이템이 추가되는 기능을 넣었을 경우
    //Init(한번만 초기화되는 것)이 아니라 Update를 해줘야되기 때문이다.
    //여기서 mVisibleItemList 의 아이템 개수는 항상 [mItemCountInOwnLine * (mVisibleFieldCount + 1)의 결과값]과 동일하다는 것을 상기한다.
    private void UpdateItems()
    {
        //라인 처리방식
        int row = 0;

        //현재 인벤토리에 출력할 아이템이 존재하지 않으면 
        if (mGIItems.IsEmpty())
        {
            //활성화되어 있는 라인들을 모두 비활성화시킨다.
            //이 부분은 주로 아이템들을 삭제했을 때 호출.
            while (row < mFieldLines.Count)
            {
                //FieldLine의 순서를 보장할 수 없기 때문에 Open 상태가 아니라고 해서 바로 break를 해서는 안된다.
                if (mFieldLines[row].IsOpen())
                {
                    mFieldLines[row].SetActive(false);
                }
                //else
                //{
                //    break;
                //}

                ++row;
            }
        }
        else
        {
            while (row < mVisibleFieldCount)
            {
                mFieldLines[row].UpdateLine();

                ++row;
            }
        }
    }

    private void UpdateFields()
    {
        if (mGIItems.IsEmpty())
        {
            mInvScrollView.LastFieldIndex = 0;
            mInvScrollView.RemainLastItemCount = 0;
        }
        else
        {
            //라인에서 첫 번째 아이템 인덱스 ~ 마지막 아이템 인덱스
            //01 ~ 07 = (0, 1) (StartLineIndex, LastlineIndex)
            //08 ~ 14 = (1, 2) (StartLineIndex, LastlineIndex)
            //15 ~ 21 = (2, 3) (StartLineIndex, LastlineIndex)
            //22 ~ 28 = (3, 4) (StartLineIndex, LastlineIndex)
            //29 ~ 35 = (4, 5) (StartLineIndex, LastlineIndex)
            //36 ~ 42 = (5, 6) (StartLineIndex, LastlineIndex)
            //43 ~ 49 = (6, 7) (StartLineIndex, LastlineIndex)
            //=>로 변경해야됨.
            //00 ~ 06 = (0, 1) (StartLineIndex, LastlineIndex)
            //07 ~ 13 = (1, 2) (StartLineIndex, LastlineIndex)
            //14 ~ 20 = (2, 3) (StartLineIndex, LastlineIndex)
            //21 ~ 27 = (3, 4) (StartLineIndex, LastlineIndex)
            //28 ~ 34 = (4, 5) (StartLineIndex, LastlineIndex)
            //35 ~ 41 = (5, 6) (StartLineIndex, LastlineIndex)
            //42 ~ 48 = (6, 7) (StartLineIndex, LastlineIndex)
            //(mGIItems.GetItemCount() - 1)에서 -1를 하는 이유는 위의 표를 참고.
            mInvScrollView.LastFieldIndex = ((mGIItems.GetItemCount() - 1) / GetItemCountInOwnLine()) + 1;//출력하는 라인의 다음 라인이 마지막 라인이므로 +1이 붙는다.
            mInvScrollView.RemainLastItemCount = mGIItems.GetItemCount() % GetItemCountInOwnLine();
        }

        //이거 일단 테스트 해봐야됨. 예전에 한 줄 라인이 제대로 출력이 안되는 묘한 현상이 있어서 땜빵으로 만들어둔 코딩인데 현재 Data 옵션으로 사용한 아이템에만 해당되어있다.
        //실제 아이템이 추가, 삭제될 경우 제대로 동작되는지 살펴봐야한다.
        //if (mGIItems.GetItemCount() != 0 && mInvScrollView.RemainLastItemCount == 0)
        //{
        //    mInvScrollView.RemainLastItemCount = GetItemCountInOwnLine();
        //}
    }

    //순간드래그가 아닌 일반 드래그일 경우.
    public void ChangeField(Matrix4x4 matrix)
    {
        int curLine = NONE_LINE;
        
        switch (mInvScrollView.movement)
        {
            case UIScrollView.Movement.Horizontal:
                curLine = (int)matrix.m00;
                
                if (curLine != NONE_LINE)
                {
                    MoveItem((int)matrix.m01, (int)matrix.m02, Vector3.right * matrix.m30);
                }
                break;

            case UIScrollView.Movement.Vertical:
                curLine = (int)matrix.m10;

                if (curLine != NONE_LINE)
                {
                    MoveItem((int)matrix.m11, (int)matrix.m12, Vector3.up * matrix.m31);
                }
                break;
        }
    }
    
    public void MoveItem(int startLine, int itemCount, Vector3 movement)
    {
        //라인을 이동하는 방식.
        GIFieldLine moveLine = null;
        Vector3 pos = Vector3.zero;
        int mlStartIndex = 0;//ml => moveList
        
        switch (mInvScrollView.movement)
        {
            case UIScrollView.Movement.Horizontal:
                if (movement.x > 0)
                {
                    moveLine = mFieldLines[0];
                    mlStartIndex = mFieldLines.Count - 1;
                }
                else
                {
                    moveLine = mFieldLines[mFieldLines.Count - 1];
                    mlStartIndex = 0;
                }
                break;

            case UIScrollView.Movement.Vertical:
                if (movement.y < 0)
                {
                    moveLine = mFieldLines[0];
                    mlStartIndex = mFieldLines.Count - 1;
                }
                else
                {
                    moveLine = mFieldLines[mFieldLines.Count - 1];
                    mlStartIndex = 0;
                }
                break;
        }
        
        pos = mFieldLines[mlStartIndex].GetPosition(Space.Self);
        pos.Set(pos.x + movement.x, pos.y + movement.y, pos.z + movement.z);
        
        moveLine.SetPosition(pos, Space.Self);
        moveLine.UpdateLine(startLine);

        mFieldLines.Remove(moveLine);
        mFieldLines.Insert(mlStartIndex, moveLine);
    }
    
    //순간드래그일 경우.
    public void MomentField(Matrix4x4 matrix)
    {
        switch (mInvScrollView.movement)
        {
            case UIScrollView.Movement.Horizontal:
                MomentField((int)matrix.m01);
                break;

            case UIScrollView.Movement.Vertical:
                MomentField((int)matrix.m11);
                break;
        }
    }

    private void MomentField(int curLine)
    {
        int i = 0;

        while (i < mFieldLines.Count)
        {
            mFieldLines[i].UpdateLine(curLine + i);
            
            ++i;
        }
    }
    
    public GenericInventory GetGenericInventory()
    {
        return mGenericInventory;
    }
    
    public CustomScrollView GetInvScrollView()
    {
        return mInvScrollView;
    }

    public CustomGrid GetCustomGrid()
    {
        return mCustomGrid;
    }

    public List<GIFieldLine> GetFieldLines()
    {
        return mFieldLines;
    }

    public GIItems GetItems()
    {
        return mGIItems;
    }
    
    public int GetVisibleFieldCount()
    {
        return mVisibleFieldCount;
    }

    public int GetItemCountInOwnLine()
    {
        return mItemCountInOwnLine;
    }

    public Vector3 GetItemGap(int index)
    {
        Vector3 value = Vector3.zero;

        switch (mInvScrollView.movement)
        {
            case UIScrollView.Movement.Horizontal:
                value = Vector3.down * (index * mCustomGrid.cellHeight);
                break;

            case UIScrollView.Movement.Vertical:
                value = Vector3.right * (index * mCustomGrid.cellWidth);
                break;
        }

        return value;
    }

    public void ResettingFieldLines()
    {
        for (int i = 0; i < mFieldLines.Count; ++i)
        {
            mFieldLines[i].Resetting(i);
        }
    }
    
    //인벤토리의 해당되는 아이템 객체를 가져올 때 mCurrentTap를 참고하지 않는다.
    //View에서 인벤토리가 2개이상일 경우 mCurrentTap과 매칭이 맞지 않기 때문이다.
    //단 인벤토리에 해당되는 아이템 객체의 이야기일 뿐이지 그렇다고 해서 mCurrentTap가 다른 곳에서도 필요없다는 이야기는 아니다.
    public GameObject GetItemPrefab()
    {
        return Utility.LoadResource<GameObject>(GetGenericInventory().GetGISettingData().GetStringValue(GameTableSetting.eKey.ResourceItemPath));
    }
}
