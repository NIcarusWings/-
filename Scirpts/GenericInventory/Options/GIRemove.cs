using UnityEngine;
using System.Collections.Generic;
using System;

public class GIRemove : GIOption
{
    private enum eOpertation : byte { Add, Revert };

    private Dictionary<int, List<CNData>> mRemoveContainer;

    private GIItems mItems;
    
    public override void Initialize(GIOptionContainer optionContainer, string optionData, ref int dataIndex)
    {
        base.Initialize(optionContainer, optionData, ref dataIndex);        
    }
    
    public void Set()
    {
        //유저가 아이템을 삭제할 때 선택만 하고 현재 선택이 되어있는 상태에서 아무것도 안하고 다른 탭으로 이동할 경우를 대비해서 만든 함수이다.
        //유저가 아이템을 삭제할 때 선택을 한 뒤에 추가 삭제를 하고자하는 경우에는 초기화를 하지 말아야되지만 위의 상황이 발생하면 초기화를 해줘야한다.
        Reset();

        mItems = mGIOptionContainer.GetGenericInventory().GetOperator().GetItems();
    }
    
    //유저가 아이템들을 삭제하겠다는 확실을 받았을 경우 호출된다.
    //아이템을 단일삭제하든 복수단제를 하든 상관없이 반드시 이 함수를 통해서 삭제가 되어야한다.
    //이런 구조를 만든 이유는 아이템을 삭제한 뒤에 아이템의 순서를 재설정하고 출력을 해야되기 때문이다.
    //RemoveList는 GIItems의 배열인덱스를 담고 있다.
    //이 리스트를 GIItems의 Removes를 호출하면 된다.
    public void Operation()
    {
        if (mRemoveContainer != null)
        {
            mItems.Removes(mRemoveContainer);
            
            //아이템들의 삭제가 끝나면 반드시 Clear를 해야한다.
            Reset();
        }
    }

    //배열의 인덱스를 기준으로 처리하기 때문에 반드시 매개인자는 배열의 인덱스를 넘겨줘야한다.
    //즉 아이템의 ID가 아니라 배열에 연결된 아이템의 인덱스를 넘겨줘야한다.(removeListIndex == removeList의 배열인덱스를 넣어라는 의미임.)
    public void AddData(int currentTap, CNData data)
    {
        if (IsLink())
        {
            OperationLinkData(currentTap, data, eOpertation.Add);
        }
        else
        {
            OperateData(this, currentTap, data, eOpertation.Add);
        }
    }

    //이 함수는 유저가 삭제할 아이템 리스트를 확인할 때 해당 아이템들 중 삭제 해제를 할 때 호출되는 함수이다.
    //Operator()함수에서 아이템 삭제할 때 배열 인자가 다른데 Operator()함수에서는 실제 아이템을 삭제하는 것이고 이 함수는 삭제 대상을 해제할 때 호출되는 함수라는 점을 상기해야한다.    
    public void RevertData(int currentTap, CNData data)
    {
        if (IsLink())
        {
            OperationLinkData(currentTap, data, eOpertation.Revert);
        }
        else
        {
            //함수로 리턴을 한 값을 ref 키워드를 사용할 수가 없어서 이런 형식이 되었음.
            //자세한 것은 OperationLinkData 함수 내부의 주석을 참고.
            OperateData(this, currentTap, data, eOpertation.Revert);
        }
    }

    private void OperationLinkData(int currentTap, CNData data, eOpertation opertation)
    {
        if (mGIOptionContainer.GetLinkKindList() != null)
        {
            List<GISettingData.eKind> linkList = mGIOptionContainer.GetLinkKindList();
            Dictionary<GISettingData.eKind, GenericInventory> giContainer = GenericInventory.GetGIContainer();
            GIRemove linkRemove = null;

            for (int i = 0; i < linkList.Count; ++i)
            {
                if (giContainer.ContainsKey(linkList[i]))
                {
                    linkRemove = giContainer[linkList[i]].GetOptionContainer().GetRemove();

                    if (linkRemove != null)
                    {
                        OperateData(linkRemove, currentTap, data, opertation);
                        //유니티에서는 밑의 함수호출과 같은 형식이 안된다 ㅡㅡ...
                        //OperateData(ref linkRemove.GetRemoveContainer(), currentTap, data, opertation);
                        giContainer[linkList[i]].UpdateInventory();
                    }
                }
            }
        }
    }

    private void OperateData(GIRemove remove, int currentTap, CNData data, eOpertation opertation)
    {
        remove.Init(currentTap);

        Dictionary<int, List<CNData>> container = remove.GetRemoveContainer();
        
        switch (opertation)
        {
            case eOpertation.Add:
                container[currentTap].Add(data);
                break;

            case eOpertation.Revert:
                if (container[currentTap].Count != 0)
                {
                    container[currentTap].Remove(data);
                }
                break;
        }
    }

    private void Init(int currentTap)
    {
        if (mRemoveContainer == null)
        {
            mRemoveContainer = new Dictionary<int, List<CNData>>();
        }

        if (!mRemoveContainer.ContainsKey(currentTap))
        {
            mRemoveContainer.Add(currentTap, new List<CNData>());
        }
    }

    public void SetDataList(int currentTap, List<CNData> list)
    {
        if (!mRemoveContainer.ContainsKey(currentTap))
        {
            mRemoveContainer.Add(currentTap, null);
        }

        mRemoveContainer[currentTap] = list;
    }

    public GIOptionContainer GetOption()
    {
        return mGIOptionContainer;
    }
    
    public Dictionary<int, List<CNData>> GetRemoveContainer()
    {
        return mRemoveContainer;
    }

    private void Reset()
    {
        if (mRemoveContainer != null)
        {
            foreach (KeyValuePair<int, List<CNData>> keyValuePair in mRemoveContainer)
            {
                if (keyValuePair.Value != null && keyValuePair.Value.Count != 0)
                {
                    keyValuePair.Value.Clear();
                }
            }
        }
    }

    protected override void SetOptionValues(string key, List<int> values)
    {
        base.SetOptionValues(key, values);
    }
}
