using UnityEngine;
using System.Collections.Generic;
using System;
//게임에서 쓰이는 데이터가 인벤토리에 출력되는 경우 2가지 종류가 존재한다.
//1. 유저가 소유하고 있는 데이터(UserData)
//2. 게임 데이터(GameData, 도감 등등 설정된 값들로만 보여지는 데이터)
//즉 이렇게 여러개의 데이터 형식이 존재할 경우 이들을 관리해줄 클래스가 필요하다.
//P.S 아이템 리스트를 감싸기 위한 클래스인데 반드시 상기해야되는 점이 있다.
//이 클래스 안에 사용되는 해당 아이템 리스트와 이 클래스의 관계는 1:1관계이다.
//이 말은 웹퍼 클래스는 반드시 하나의 아이템 리스트들만 가지고 있어야되며 다른 아이템 리스트를 가지고 있으면 안된다.
//만약 뷰에서 아이템 리스트를 2개이상 가지고 있어야된다면 WeapperItems 클래스 안에서 2개 이상의 아이템을 관리하는 것이 아니라
//뷰 안에 WrapperItems 클래스를 2개이상 선언해서 처리하도록 해야한다.
//Wrapper Class
//인벤토리 스크립트에서 구현할려다가 인벤토리 초기화만 해도 복잡해서 뒤지겠는데 거기에 Items관련 소스코드까지 추가되고 Items에 대한 옵션들(정렬, 삭제)까지 추가되니 혼돈의 카오스를 맞보는 기분이라 나누기로 함.
//-> 음.. 이 부분을 고민하다가 얻은 결론이 MVC모델을 적용하는 것이다.
//M의 경우 인벤토리 아이템이니깐 형식은 비슷하고 사용 특성에 맞게 구현하면 되므로 추상화 및 구현으로 설계를 하면 될 것이고
//V의 경우 이미지만 뿌려주면 되므로 이미지 뿌려주는 기능 이외에는 넣지 않도록 설계를 하면 될 것이고
//C의 경우 M과 V의 중간다리 역할을 해주는 설계를 하면 될 것인데... 책 좀 더 보고 가닥을 잡아봐야겠음.
//이 부분이 잘 설계가 된다면 열거형으로 인벤토리를 구분해주는 방식을 제거할 수 있을 것 같다.(현재 재활용성이 극히 떨어짐...)
//추가적으로 아이템이 아이템들을 복합해야되는 경우가 있다면 복합체 패턴을 같이 생각해봐야한다.(근데 이런 경우가 있는지는 모르겠지만 생각은 해두는 걸로)
public class GIItems
{
    //주 기본이 되는 아이템들을 모아둔 컨테이너.
    private Dictionary<int, List<CNData>> mDataContainer = new Dictionary<int, List<CNData>>();

    private GIOperator mGIOperator;
    
    public void Initialize(GIOperator giOperator)
    {
        mGIOperator = giOperator;
    }
    
    public void UpdateItem(int currentTap)
    {
        if (!mDataContainer.ContainsKey(currentTap))
        {
            switch (mGIOperator.GetGenericInventory().GetKind())
            {
                default:
                    mDataContainer.Add(currentTap, null);
                    break;
            }
        }
        
        UpdateData(mGIOperator.GetGenericInventory().GetKind(), currentTap);
    }
    
    public bool IsEmpty()
    {
        return mDataContainer[mGIOperator.GetGenericInventory().GetCurrentTap()] == null || 
            mDataContainer[mGIOperator.GetGenericInventory().GetCurrentTap()].Count == 0;
    }

    public GIOperator GetOperator()
    {
        return mGIOperator;
    }

    public Dictionary<int, List<CNData>> GetDataContainer()
    {
        return mDataContainer;
    }

    public List<CNData> GetDatas()
    {
        return mDataContainer[mGIOperator.GetGenericInventory().GetCurrentTap()];
    }
    
    public CNData GetData(int index)
    {
        return mDataContainer[mGIOperator.GetGenericInventory().GetCurrentTap()][index];
    }

    public int GetItemCount()
    {
        return mDataContainer[mGIOperator.GetGenericInventory().GetCurrentTap()].Count;
    }
    
    private List<CNData> ConvertListToArray<T>(List<T> list) where T : CNData
    {
        List<CNData> datas = GetDatas();

        if (datas == null)
        {
            datas = new List<CNData>();
        }

        int i = 0;
        
        while (i < list.Count)
        {
            if (i < datas.Count)
            {
                datas[i] = list[i];
            }
            else
            {
                datas.Add(list[i]);
            }

            ++i;
        }

        while (i < datas.Count)
        {
            if (mGIOperator.GetGenericInventory().GetGISettingData().GetBoolValue(GameTableSetting.eKey.IsSetNullWhenRemoving))
            {
                datas[i] = null;
                ++i;
            }
            else
            {
                datas.RemoveAt(datas.Count - 1);
            }
        }

        return datas;
    }

    //실제로 아이템을 삭제하는 함수.
    private void RemoveUserItems<T>(List<T> list, List<CNData> removeList) where T : CNData
    {
        int uniqueID = CNData.NONE_VALUE;
        int removeIndex = CNData.NONE_VALUE;
        
        //2. 반복문을 사용하여 아이템을 다음과 같은 형식으로 제거한다.
        for (int i = 0; i < removeList.Count; ++i)
        {
            uniqueID = removeList[i].GetIntValue(GameTableSetting.eKey.I_UniqueID);
            removeIndex = Utility.BinarySearch(list, uniqueID);
            
            if (uniqueID != CNData.NONE_VALUE && removeIndex != CNData.NONE_VALUE)
            {
                list.RemoveAt(removeIndex);
            }
        }

        removeList.Clear();
    }
    
    //GIRemove에서 호출되는 함수이다.
    //웹퍼 클래스인 GIItems에서 사용되는 DataContainer와 연결된 유저 데이터의 컨테이너랑 담긴 데이터들은 서로 링크되어있지만 다른 점들이 존재한다.
    //1. DataContainer에서 관리하고 있는 CNData 자료구조는 배열이라는 점이고 유저 데이터의 컨테이너는 리스트라는 점이다.
    //2. 자료구조가 다르므로 정렬 기능을 수행했을 경우 DataContainer에 담긴 CNData 배열과 유저 데이터의 컨테이너의 순서가 다르다.
    //removeList에 담긴 정수형들의 값들은 CNData 배열의 인덱스를 담고 있다.
    //따라서 이 인덱스를 통하여 CNData의 데이터를 가지고 온 다음 이 데이터를 토대로 유저 데이터의 컨테이너에서 해당 데이터를 삭제해야한다.
    public void Removes(Dictionary<int, List<CNData>> removeContainer)
    {
        foreach (KeyValuePair<int, List<CNData>> keyValuePair in removeContainer)
        {
            switch (mGIOperator.GetGenericInventory().GetKind())
            {
                case GISettingData.eKind.GI_KIND_EQUIPMENT:
                case GISettingData.eKind.GI_WHOLE_AMOUNT_RECYCLE:
                    switch (keyValuePair.Key)//keyValuePair.Key == 탭 인덱스
                    {
                        case 0:
                            RemoveUserItems(GameManager.Instance.GetUserData().BodyInfos, keyValuePair.Value);
                            break;

                        case 1:
                            RemoveUserItems(GameManager.Instance.GetUserData().WeaponInfos, keyValuePair.Value);
                            break;
                    }
                    break;
            }
        }
    }
    
    private void UpdateData(GISettingData.eKind GI_KIND, int currentTap)
    {
        switch (GI_KIND)
        {
            case GISettingData.eKind.GI_KIND_EQUIPMENT:
            case GISettingData.eKind.GI_SELECT_RECYCLE:
                switch (currentTap)
                {
                    case 0:
                        //ToArray 함수가 클래스에 대해서 얕은 복사를 할 줄 알았지만 알고보니 깊은 복사를 한다...
                        //따라서 얕은 복사 방식을 새롭게 만들어야한다. 
                        mDataContainer[currentTap] = ConvertListToArray(GameManager.Instance.GetUserData().BodyInfos);
                        break;

                    case 1:
                        mDataContainer[currentTap] = ConvertListToArray(GameManager.Instance.GetUserData().WeaponInfos);
                        break;
                }
                break;

            case GISettingData.eKind.GI_WHOLE_AMOUNT_RECYCLE:
                {
                    try
                    {
                        if (mDataContainer[0] == null)
                        {
                            mDataContainer[0] = new List<CNData>();
                        }

                        GenericInventory genericInventory = GetOperator().GetGenericInventory();
                        GIRemove remove = genericInventory.GetOptionContainer().GetRemove();

                        if (remove != null)
                        {
                            Dictionary<int, List<CNData>> removeContainer = remove.GetRemoveContainer();

                            if (removeContainer != null)
                            {
                                int wholeRecycleCount = 0;
                                int i = 0;

                                foreach (KeyValuePair<int, List<CNData>> keyValuePair in removeContainer)
                                {
                                    for (i = 0; i < keyValuePair.Value.Count; ++i)
                                    {
                                        if ((wholeRecycleCount + i) < mDataContainer[0].Count)
                                        {
                                            mDataContainer[0][wholeRecycleCount + i] = keyValuePair.Value[i];
                                        }
                                        else
                                        {
                                            mDataContainer[0].Add(keyValuePair.Value[i]);
                                        }
                                    }

                                    wholeRecycleCount += keyValuePair.Value.Count;
                                }

                                i = wholeRecycleCount;

                                while (i < mDataContainer[0].Count)
                                {
                                    if (mGIOperator.GetGenericInventory().GetGISettingData().GetBoolValue(GameTableSetting.eKey.IsSetNullWhenRemoving))
                                    {
                                        mDataContainer[0][i] = null;
                                        ++i;
                                    }
                                    else
                                    {
                                        mDataContainer[0].RemoveAt(mDataContainer[0].Count - 1);
                                    }
                                }
                                
                                //정렬을 하는데 1개 이하라면 할 필요가 없다.
                                if (mDataContainer[0].Count > 1)
                                {
                                    mDataContainer[0].Sort(delegate (CNData lhs, CNData rhs)
                                    {
                                        return lhs.GetIntValue(GameTableSetting.eKey.I_UniqueID).CompareTo(rhs.GetIntValue(GameTableSetting.eKey.I_UniqueID));
                                    });
                                }
                            }
                        }
                    }
                    catch
                    {
                        Debug.LogError("Error : GIItems -> SetOperatorDatas -> GI_WHOLE_AMOUNT_RECYCLE");
                    }
                }    
                break;
        }
    }
};