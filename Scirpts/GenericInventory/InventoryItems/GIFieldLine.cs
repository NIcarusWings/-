using UnityEngine;
using System.Collections;

//처음에는 아이템들을 각각 관리하는 방식을 사용하였다.
//하지만 각 라인마다 애니메이션이 들어가야한다던가 인벤토리 공간 확장하기 같은 기능으로써 라인에 아이템 개방하기 버튼 출력 등등
//그리고 현재 라인이 어떤 수를 가지고 있는 라인인지 알기위함이라던지
//라인단위마다 특정작업을 하기 위한 클래스가 필요한 경우가 존재한다.
//이 경우를 대비하기 위하여 만든 클래스이다.
//GI => GenericInventory의 약자
public class GIFieldLine : MonoBehaviour
{
    private GenericInventory mGenericInventory;
    private GIInventoryItem[] mInventoryItems;

    private int mCurLine = -1;
    
    public void Initialize(int curLine, GenericInventory genericInventory)
    {
        mCurLine = curLine;
        mGenericInventory = genericInventory;
        
        InitItems();
    }

    private void InitItems()
    {
        GameObject itemPrefab = mGenericInventory.GetOperator().GetItemPrefab();
        
        if (itemPrefab != null)
        {
            GameObject go = null;
            GIItems items = mGenericInventory.GetOperator().GetItems();
            int itemCountInOwnLine = mGenericInventory.GetOperator().GetItemCountInOwnLine();
            int dataStartIndex = GetDataLineStartIndex();
#if UNITY_EDITOR
            int itemIndex = -1;
#endif
            mInventoryItems = new GIInventoryItem[itemCountInOwnLine];
            
            for (int i = 0; i < itemCountInOwnLine; ++i)
            {
                go = NGUITools.AddChild(gameObject, itemPrefab);
#if UNITY_EDITOR
                itemIndex = dataStartIndex + i;
                go.name = itemIndex.ToString();
#endif
                mInventoryItems[i] = go.GetComponent<GIInventoryItem>();
                mInventoryItems[i].SetPosition(GetItemPositionByGap(i), Space.Self);
                mInventoryItems[i].Initialize(i, this);
            }
        }
    }

    public void Resetting(int curLine)
    {
        mCurLine = curLine;
    }

    public void UpdateLine()
    {
        UpdateLine(mCurLine);
    }
    
    public void UpdateLine(int curLine)
    {
        if (mInventoryItems != null)
        {
            SetActive(true);

            mCurLine = curLine;

            GIItems items = mGenericInventory.GetOperator().GetItems();
            int i = 0;
            int dataIndex = -1;
            int itemAllCount = items.GetItemCount();
            int startDataIndex = GetDataLineStartIndex();
            
            while (i < mInventoryItems.Length)
            {
                dataIndex = startDataIndex + i;

                if (dataIndex < itemAllCount)
                {
                    mInventoryItems[i].UpdateItem(items.GetData(dataIndex));
                }
                else
                {
                    if (mInventoryItems[i].IsOpen())
                    {
                        mInventoryItems[i].SetActive(false);
                    }
                }

                ++i;
            }
        }
    }
    
    public void SetPosition(Vector3 pos, Space space)
    {
        switch (space)
        {
            case Space.World:
                transform.position = pos;
                break;

            case Space.Self:
                transform.localPosition = pos;
                break;
        }
    }

    public bool IsOpen()
    {
        return gameObject.activeSelf;
    }

    public void SetActive(bool value)
    {
        if (value != gameObject.activeSelf)
        {
            gameObject.SetActive(value);
        }
    }
    
    public GIInventoryItem[] GetItems()
    {
        return mInventoryItems;
    }
    
    public Vector3 GetPosition(Space space)
    {
        Vector3 pos = Vector3.zero;

        switch (space)
        {
            case Space.World:
                pos = transform.position;
                break;

            case Space.Self:
                pos = transform.localPosition;
                break;
        }

        return pos;
    }

    public GenericInventory GetGenericInventory()
    {
        return mGenericInventory;
    }

    public void UpdateItem(CNData data)
    {

    }

    private Vector3 GetItemPositionByGap(int index)
    {
        return GetPosition(Space.Self) + mGenericInventory.GetOperator().GetItemGap(index);
    }

    public int GetDataLineStartIndex()
    {
        return mCurLine * mGenericInventory.GetOperator().GetItemCountInOwnLine();
    }
}