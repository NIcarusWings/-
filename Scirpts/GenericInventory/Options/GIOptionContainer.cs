using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GIOptionContainer
{
    //Remove와 RemoveAndLink의 차이점은 Remove는 단일삭제 인벤토리를 의미하고 RemoveAndLink는 삭제리스트 인벤토리와 삭제대상 인벤토리가 따로 존재하면서 데이터를 주고 받는 구조를 의미한다.
    //RemoveAndLink에 대한 컨셉은 리니지M, 테라M, 소녀전선 등 삭제 인벤토리를 참고.
    //public enum eOption { Normal = 0, Sort, Remove, RemoveAndLink, Upgrade, ItemEvent };

    private GenericInventory mGenericInventory;
    
    private GISort mGISort;
    private GIRemove mGIRemove;

    private List<GISettingData.eKind> mLinkKindList = null;

    public void Initialize(GenericInventory genericInventory)
    {
        mGenericInventory = genericInventory;

        InitOptions();
    }

    private void InitOptions()
    {
        string options = GetGenericInventory().GetGISettingData().GetStringValue(GameTableSetting.eKey.Options).Trim();
        System.Text.StringBuilder builder = new System.Text.StringBuilder(20);
        
        if (options != null && options.Length != 0)
        {
            int i = 0;
            
            while (i < options.Length)
            {
                switch (options[i])
                {
                    case '\"':
                        //options[i] == '\"' 이므로 '\"' 다음부터 데이터이기 때문에 인덱스를 하나 올린다.
                        ++i;

                        builder.Length = 0;

                        while (options[i] != '\"')
                        {
                            builder.Append(options[i]);
                            ++i;
                        }
                        
                        //while문을 빠져나오면 options[i] == '\"'이므로 builder에 대한 값을 가져오고 인덱스를 하나 올린다.
                        string value = builder.ToString();

                        if ((value[0] >= 'A' && value[0] <= 'Z') ||
                            (value[0] >= 'a' && value[0] <= 'z'))
                        {
                            value = value.ToLower();
                            
                            switch (value)
                            {
                                case "sort":
                                    if (mGISort == null)
                                    {
                                        mGISort = new GISort();
                                        mGISort.Initialize(this, options, ref i);
                                    }
                                    
                                    break;

                                //단일 삭제든 링크삭제든 삭제에 관련된 옵션 객체는 Remove이다. 삭제 연출이 바뀌는 것이지 삭제의 기본 구조가 바뀌는 것은 아니기 때문이다.
                                case "remove":
                                    if (mGIRemove == null)
                                    {
                                        mGIRemove = new GIRemove();
                                        mGIRemove.Initialize(this, options, ref i);
                                    }

                                    mGIRemove.Set();
                                    break;
                            }
                        }
                        break;

                    default:
                        
                        break;
                }

                ++i;
            }
        }
    }
    
    public void Sort(GIItems items, int sortIndex)
    {
        if (mGISort != null)
        {
            mGISort.Operator(items, sortIndex);
        }
    }

    public GenericInventory GetGenericInventory()
    {
        return mGenericInventory;
    }

    public GISort GetSort()
    {
        return mGISort;
    }

    public GIRemove GetRemove()
    {
        return mGIRemove;
    }
    
    public List<GISettingData.eKind> GetLinkKindList()
    {
        return mLinkKindList;
    }

    public void SetLinkKindList(List<int> values)
    {
        if (mLinkKindList == null)
        {
            mLinkKindList = new List<GISettingData.eKind>();
        }

        mLinkKindList.Clear();

        for (int i = 0; i < values.Count; ++i)
        {
            mLinkKindList.Add((GISettingData.eKind)values[i]);
        }
    }
}
