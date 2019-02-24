using UnityEngine;
using System.Collections.Generic;
using System;

public class GISort : GIOption
{
    //tapIndex, sortIndex, sortKeyList
    private Dictionary<int, Dictionary<int, GameTableSetting.eKey[]>> mKeys = new Dictionary<int, Dictionary<int, GameTableSetting.eKey[]>>();

    private int mSortIndex = -1;

    public override void Initialize(GIOptionContainer optionContainer, string optionData, ref int dataIndex)
    {
        base.Initialize(optionContainer, optionData, ref dataIndex);
    }
    
    public void Operator(GIItems items, int sortIndex)
    {
        Operator(items.GetDatas(), mGIOptionContainer.GetGenericInventory().GetCurrentTap(), sortIndex);
    }
    
    //이 함수는 정보뿐만 아니라 데이터도 정렬대상으로 들어가기 때문에 매개변수가 GenericInventory가 아니다. 
    //GenericInventory를 매개변수로 받아버리면 GenericInventory에 해당되는 데이터만 정렬하게 된다는 의미가 된다.
    //하지만 도감의 경우엔 [현재 유저가 보유하고 있는 도감 아이템 정보들]도 정렬을 해야하고 [게임에서 표시하고자 하는 도감 데이터들]도 정렬을 해야한다.
    //도감을 구현할 때 보유한 아이템과 보유하지 않은 아이템들을 모두 표시해야되므로 주 베이스는 [게임에서 표시하고자 하는 도감 데이터들]가 되지만 
    //보유한 아이템인지 아닌지를 알아볼려면 [현재 유저가 보유하고 있는 도감 아이템 정보들] 역시 같이 가지고 와야하고 역시 정렬대상이다.
    //따라서 GenericInventory가 소유한 아이템만을 정렬하는 것이 아니라 정보나 데이터와 상관없이 정렬할 수 있는 매개변수를 써야한다.
    public void Operator(List<CNData> datas, int currentTap, int sortIndex)
    {
        if (IsEmpty(datas)) return;

        mSortIndex = sortIndex;

        Sorting(datas, GetKeys(currentTap));               
    }

    private bool IsEmpty(List<CNData> datas)
    {
        return (datas == null || datas.Count == 0);
    }

    public GameTableSetting.eKey[] GetKeys(int currentTap)
    {
        return (mSortIndex != -1) ? mKeys[currentTap][mSortIndex] : null;
    }
    
    private void Sorting(List<CNData> datas, GameTableSetting.eKey[] keys)
    {
        datas.Sort(
            delegate (CNData lhs, CNData rhs)
            {
                return ResultCompared(keys, lhs, rhs);
            });
    }
    
    private int ResultCompared(GameTableSetting.eKey[] keys, CNData lhs, CNData rhs)
    {
        int result = 0;
        int i = 0;

        while (i < keys.Length)
        {
            result = CompareData(keys[i], lhs, rhs);

            if (result == 0)
            {
                ++i;
            }
            else
            {
                break;
            }
        }

        if (result == 0)
        {
            result = CompareData(GameTableSetting.eKey.ID, lhs, rhs);

            if (result == CNData.NONE_VALUE)
            {
                Debug.LogError("Sort Not Failed : " + result);
                result = GetDescendingOrder();
            }
        }

        return result;
    }

    //기본적으로 내침차순을 기준으로 하고 있다.
    private int CompareData(GameTableSetting.eKey key, CNData lhs, CNData rhs)
    {
        int value = CNData.NONE_VALUE;

        //CNData 가 null인 경우는 가변 개수를 데이터 구조를 사용할 때이다.
        //가변 개수의 데이터 구조란 주로 유저데이터에서 아이템을 추가, 삭제하는 데이터 구조를 의미한다.
        //이 가변 개수의 데이터 구조를 여기로 가져올 때 CNData 타입의 배열로 변환을 하는데 변환할 때 가변개수이므로 추가될 경우를 대비하여
        //보유하고 있는 아이템 개수에 따라서 추가할당을 한다.
        //ex) 유저데이터의 가변개수 데이터구조인 List<데이터타입> mItemList가 존재한다.
        //    mItemList를 이 클래스에서 사용할 경우 CNData[] mCNData = new CNData[GetAllowCount(mItemList.Length)];
        //추가할당을 하지만 아이템이 추가되는 것에 대비하여 할당만 할 뿐 데이터를 초기화하지 않는다. 따라서 null체크를 해야한다.
        //둘다 null일 경우에도 고려해야한다.!!
        if ((lhs == null || rhs == null))
        {
            if ((lhs != null && rhs == null) ||
                (lhs == null && rhs == null))
            {
                value = GetDescendingOrder();
            }
            else//if (lhs == null && rhs != null)
            {
                value = GetAscendingOrder();
            }
        }
        else
        {
            //[활성화 & 비활성화], [신규획득, & 신규획득아님], [소유 & 무소유]등 우선순위적으로 검사해야될 변수타입이 bool이므로
            //최우선적으로 검사한다.
            if (lhs.GetBoolValue(key) != rhs.GetBoolValue(key))
            {
                if (lhs.GetBoolValue(key) == true && rhs.GetBoolValue(key) == false)
                {
                    value = GetDescendingOrder();
                }
                else//if (lhsValue == false && rhsValue == true)
                {
                    value = GetAscendingOrder();
                }
            }
            else if (lhs.GetIntValue(key) != CNData.NONE_VALUE)
            {
                value = lhs.GetIntValue(key).CompareTo(rhs.GetIntValue(key)) * GetDescendingOrder();
            }
            else if (lhs.GetStringValue(key) != null)
            {
                value = String.Compare(lhs.GetStringValue(key), rhs.GetStringValue(key), StringComparison.Ordinal) * GetDescendingOrder();
            }
            else
            {
                //bool, int, string 순서로 비교해보는데 그 이후에 비교타입이 UI에서 표현되는 일이 있을까 라는 생각에 들어서 일단 실수형에 대해서 작업을 해두진 않음.
                value = CNData.NONE_VALUE;
            }
        }

        //아직 float과 double은 구현을 하지 않았는데 UI에서 실수형을 표시하는 게임을 아직 본적이 없기 때문이다.

        //기본적으로 내림차순을 기준으로 하고 있지만 오름차순이 기준인 키도 존재한다.
        //오름차순일 경우 설정된 값(value)에서 -1을 곱하면 오름차순으로 변경된다.
        return GetKeyDefaultOrder(key, value);
    }

    private int GetDescendingOrder()
    {
        return -1;
    }

    private int GetAscendingOrder()
    {
        return 1;
    }
    
    private int GetKeyDefaultOrder(GameTableSetting.eKey key, int value)
    {
        //기본적으로 내림차순 정렬을 기준으로 하고 있다.
        //오름차순의 키일 경우 여기에 추가시켜주면 된다.
        switch (key)
        {
            //case GameTableSetting.eKey.WindQuality://현재는 단순한 예시로써 임시키를 사용하고 있다.
            //    value *= -1;//오름차순의 기준을 경우 반전시켜주면 된다.
            //    break;
        }

        return value;
    }

    protected override void SetOptionValues(string key, List<int> values)
    {
        base.SetOptionValues(key, values);
        
        switch (key)
        {
            case "key":
                int valueIndex = values.Count - 2;

                //하나의 옵션에 대한 변환이 끝났으므로 인제 컨테이너로 옮겨담는 작업을 한다.
                //단 현재 이 기능에 문제점이 존재한다. sort에 대한 옵션이 현재 하나밖에 없다는 점만 고려한 것이다.
                //만약 특정 옵션이 추가되었을 경우 현재 코드로는 대응할 수가 없다. 현재 values[0]가 tapIndex로 시작한다는 가정하에 만들어 진 것인데 특정 옵션이 추가될 경우
                //values[0], values[1], values[2 ~ (n - 1)]의 순서관계가 무너진다. 또한 특정옵션이 values[2 ~ (n - 1)]과 같은 형태를 가진다고 했을 경우에도 문제가 된다.
                //이 경우 해결책은 Sort라는 객체를 대상으로 두고 각 옵션에 명칭을 부여하여 옵션에 내포되는 요소들끼리 묶어서 처리하는 방법으로 변경해야된다. 
                //이 작업이 시간이 걸리는 작업이므로 보류하지만 정렬뿐만 아니라 다른 곳에서도 발생할 수 있는 문제이므로 반드시 기억해두고 처리해야할 문제이다.
                //만약에 옵션이 추가될 경우를 고려하여 현재 옵션들에 대해 명칭을 부여하자면 keyOption.음...-_- 좀 더 생각해봐야겠다.
                //values[0] == tapIndex
                if (!mKeys.ContainsKey(values[0]))
                {
                    mKeys.Add(values[0], new Dictionary<int, GameTableSetting.eKey[]>());
                }

                //values[0] == tapIndex, values[1] = sortIndex
                //values[1] 이후부터는 정렬 키에 관한 옵션들이다. 따라서 전체키의 2를 빼면 정렬 키의 개수가 된다.
                //이론상 sortIndex는 중복키를 가지면 안되지만 예방차원에서 작성하기로 함.
                if (!mKeys[values[0]].ContainsKey(values[1]))
                {
                    mKeys[values[0]].Add(values[1], new GameTableSetting.eKey[valueIndex]);
                }

                //values[2 ~ (n - 1)] = sort Keys
                for (int i = valueIndex; i < values.Count; ++i)
                {
                    mKeys[values[0]][values[1]][i - valueIndex] = (GameTableSetting.eKey)values[i];
                }
                break;
        }
    }
}
