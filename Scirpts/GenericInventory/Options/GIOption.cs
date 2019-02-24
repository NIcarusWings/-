using System.Collections.Generic;
using System;

public abstract class GIOption
{
    protected GIOptionContainer mGIOptionContainer;

    private bool mIsLink = false;
    
    public virtual void Initialize(GIOptionContainer optionContainer, string optionData, ref int dataIndex)
    {
        mGIOptionContainer = optionContainer;

        ConvertOptionDataToValues(optionData, ref dataIndex);
    }
    
    private void ConvertOptionDataToValues(string optionData, ref int dataIndex)
    {
        Stack<char> characterStack = new Stack<char>();
        string key = null;
        List<int> values = new List<int>();
        int valueIndex = 0;
        bool isLoop = true;

        //+1을 하여 데이터의 시작인덱스로 지정한다.('\"')
        ++dataIndex;
        
        //optionData :  : [0], "ItemEvent" : [0] }
        //optionData :  : {"Link" : ["1"], "Key" : ["0, 0, 5, 9", "0, 1, 5, 9"]}, "Remove" : {"Link" : ["1"]}, "ItemEvent" : [0] }
        while (isLoop)
        {
            switch (optionData[dataIndex])
            {
                case '{':
                    characterStack.Push(optionData[dataIndex]);

                    ++dataIndex;

                    SetNextIndex(optionData, ref dataIndex);

                    if (optionData[dataIndex] == '\"')
                    {
                        key = GetKey(optionData, ref dataIndex);
                    }
                    break;

                case '[':
                    characterStack.Push(optionData[dataIndex]);

                    ++dataIndex;

                    while (optionData[dataIndex] == '\"')
                    {
                        //첫 번째가 '\"'이므로 하나를 더한다.
                        ++dataIndex;

                        //각 옵션의 끝지점은 '\"'이므로 '\"' ~ '\"'안에 존재하는 데이터들을 뽑아서 설정한다.
                        //values의 최종 결과는 다음과 같다. "0,0,0,0" => values[0] = 0, values[1] = 0, values[2] = 0, values[3] = 0
                        while (optionData[dataIndex] != '\"')
                        {
                            if (optionData[dataIndex] >= '0' && optionData[dataIndex] <= '9')
                            {
                                values.Add(0);

                                valueIndex = values.Count - 1;

                                do
                                {
                                    values[valueIndex] = (values[valueIndex] * 10) + optionData[dataIndex] - '0';

                                    ++dataIndex;
                                } while (optionData[dataIndex] >= '0' && optionData[dataIndex] <= '9');
                            }
                            else
                            {
                                ++dataIndex;
                            }
                        }

                        SetOptionValues(key, values);

                        values.Clear();

                        ++dataIndex;

                        SetNextIndex(optionData, ref dataIndex, ' ', ',');
                    }

                    --dataIndex;
                    break;

                case '}':
                    characterStack.Pop();

                    key = null;
                    values.Clear();

                    if (characterStack.Count == 0)
                    {
                        isLoop = false;
                    }
                    break;

                case ']':
                    characterStack.Pop();

                    key = null;
                    values.Clear();

                    if (characterStack.Count == 0)
                    {
                        isLoop = false;
                    }
                    else
                    {
                        ++dataIndex;

                        SetNextIndex(optionData, ref dataIndex);

                        if (optionData[dataIndex] == ',')
                        {
                            while (optionData[dataIndex] != '\"')
                            {
                                ++dataIndex;
                            }

                            key = GetKey(optionData, ref dataIndex);
                        }
                        else
                        {
                            --dataIndex;
                        }
                    }
                    break;
            }

            //while (optionData[dataIndex] != '\"')의 반복문에서 optionData[dataIndex] == '\"' 일 때 빠져나오는데
            //optionData[dataIndex] == '\"'일 때 별도로 처리없이 바로 ++dataIndex;를 해도 상관없다.
            //왜냐하면 while (optionData[dataIndex] != '\"')에서 optionData[dataIndex] == '\"'일 때는 일반적인 optionData[dataIndex] == '\"'과 다른데
            //일반적인 optionData[dataIndex] == '\"'의 경우에는 하나의 옵션의 시작을 의미하지만 while (optionData[dataIndex] != '\"')에서 조건이 맞지 않는 optionData[dataIndex] == '\"'의 경우는
            //하나의 옵션의 끝을 의미하기 때문이다. 이 경우 따로 처리할 필요없이 바로 연산하여 다음 데이터가 무엇인지 알아보면 된다.
            ++dataIndex;
        }
    }

    private void SetNextIndex(string optionData, ref int dataIndex, params char[] filter)
    {
        bool isLoop = true;

        if (filter == null || filter.Length == 0)
        {
            do
            {
                isLoop = false;

                if (optionData[dataIndex] == ' ')
                {
                    isLoop = true;

                    ++dataIndex;
                }
            } while (isLoop);
        }
        else
        {
            do
            {
                isLoop = false;

                for (int i = 0; i < filter.Length; ++i)
                {
                    if (optionData[dataIndex] == filter[i])
                    {
                        isLoop = true;

                        ++dataIndex;
                    }
                }
            } while (isLoop);
        }
    }

    private string GetKey(string optionData, ref int dataIndex)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder(20);
        string value = null;

        ++dataIndex;

        while (optionData[dataIndex] != '\"')
        {
            builder.Append(optionData[dataIndex]);
            ++dataIndex;
        }

        value = builder.ToString();

        if ((value[0] >= 'A' && value[0] <= 'Z') ||
            (value[0] >= 'a' && value[0] <= 'z'))
        {
            value = value.ToLower();
        }

        return value;
    }

    protected virtual void SetOptionValues(string key, List<int> values)
    {
        switch (key)
        {
            case "link":
            case "sendto":
                mIsLink = true;

                mGIOptionContainer.SetLinkKindList(values);
                break;

            case "receiveto":
                mGIOptionContainer.SetLinkKindList(values);
                break;

        }
    }

    protected void UpdateLinkGenericInventory()
    {
        if (mGIOptionContainer.GetLinkKindList() != null)
        {
            List<GISettingData.eKind> linkList = mGIOptionContainer.GetLinkKindList();
            Dictionary<GISettingData.eKind, GenericInventory> giContainer = GenericInventory.GetGIContainer();

            for (int i = 0; i < linkList.Count; ++i)
            {
                if (giContainer.ContainsKey(linkList[i]))
                {
                    giContainer[linkList[i]].UpdateInventory();
                }
            }
        }
    }

    protected bool IsLink()
    {
        return mIsLink;
    }
}
