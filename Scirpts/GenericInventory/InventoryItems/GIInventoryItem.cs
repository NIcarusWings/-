using UnityEngine;
using System;
using System.Collections.Generic;

//인벤토리 아이템의 공통적인 부분을 묶어서 처리하는 클래스.
//인벤토리를 사용한다면 이 클래스를 사용하면 됨.
public abstract class GIInventoryItem : UIBaseItem
{
    //인벤토리의 경우 이미지는 반드시 보여야된다.(눈으로 보여져야되기 때문.) 
    //그렇지만 그 요소로 볼 수 있는 이미지(배경 제외)나 라벨의 경우에는 보여질 필요가 없는 경우가 존재한다.
    //(이미지의 경우 아이템 메뉴를 들어가기 위한 버튼 리스트만 표시할 경우 배경과 라벨만으로도 충분히 대처가 되기 때문.)
    //(라벨의 경우 영구지속형 아이템, 혹은 퀘스트 아이템 등을 예시로 들 수 있다. 하나만 소유할 수 있는 경우 굳이 1개라고 표시할 필요가 없기 때문이다.)
    //(오히려 라벨을 표시하면 영구지속형인지 퀘스트 아이템인지 소모성 아이템인지 구분이 안 갈 수가 있다. 따라서 라벨의 경우 표시가 되지 않도록 만들어야된다.)
    //따라서 Type이라는 상태가 필요하며 각 Type들은 다음과 같은 종류를 가진다.(용어 정의가 아직 안됨, 좀 더 생각해봐야겠음.)
    //[소모성, 영구성, 퀘스트용]
    //public enum eType
    //{
    //    Consume, Permanency, Quest
    //};

    [SerializeField] protected KeyImage[] m_Images;
    [SerializeField] protected KeyLabel[] m_Labels;

    protected GIFieldLine mGIFieldLine;
    protected CNData mData;

    protected int mIndex = -1;
    
    public void Initialize(int index, GIFieldLine giFieldLine)
    {
        mIndex = index;
        mGIFieldLine = giFieldLine;
    }
    
    [SerializeField]
    private UILabel m_IndexLabel;

    public void UpdateItem(CNData data)
    {
        if (data == null)
        {
            mData = null;
            //할 일
            SetActive(false);

            mIndex = -1;
            mData = null;
        }
        else
        {
            SetActive(true);

            mData = data;

            m_IndexLabel.text = GetDataStartIndex().ToString();

            for (int i = 0; i < m_Images.Length; ++i)
            {
                m_Images[i].Update(data);
            }

            for (int i = 0; i < m_Labels.Length; ++i)
            {
                m_Labels[i].Update(data);
            }

            AppendUpdateItem();
        }
    }

    protected virtual void AppendUpdateItem(){}
    
    public void UpdateItem()
    {
        UpdateItem(mData);
    }

    public GenericInventory GetGenericInventory()
    {
        return mGIFieldLine.GetGenericInventory();
    }
    
    public int GetDataStartIndex()
    {
        return mGIFieldLine.GetDataLineStartIndex() + mIndex;
    }

    public CNData GetData()
    {
        return mData;
    }
    
    [Serializable]
    protected class KeyImage
    {
        [SerializeField]
        protected GameTableSetting.eKey m_Key;
        [SerializeField]
        protected UISprite m_Image;

        public void Update(CNData data)
        {
            m_Image.spriteName = data.GetStringValue(m_Key);
        }
    }

    [Serializable]
    protected class KeyLabel
    {
        [SerializeField]
        protected GameTableSetting.eKey m_Key;
        [SerializeField]
        protected UILabel m_Label;

        public void Update(CNData data)
        {
            m_Label.text = data.GetStringValue(m_Key);
        }
    }
    
    public enum eState { Default, Remove }

    //인벤토리를 최적화하기 위하여 현재 유저가 볼 수 있는 영역에 한에서만 인벤토리 아이템 객체를 생성하는데 인벤토리 아이템 객체에서 설정한 설정과 해당 유저 아이템과의 설정을 동기화해야되는 작업을 해야한다.
    //이 문제가 나타나는 경우는 유저가 인벤토리를 스크롤 할 경우에 발생한다. 
    //스크롤을 해서 인벤토리 아이템 활성화된 객체의 출력정보가 변환될 때 해당 스크롤의 값에 따라 해당되는 유저가 보유한 아이템을 출력한다.
    //문제는 인벤토리 아이템 활성화된 객체에서 설정된 상태의 값이 전혀 상관없는 해당되는 유저가 보유한 아이템에 적용된다는 점이다.
    //이는 명백히 오류이다. 따라서 이를 맞춰주는 작업을 해야한다.
    //GIInventoryItem.eState의 상태는 다음과 같은 조건을 성립해야한다.
    //1. 유저 데이터에 저장할 필요가 없는 상태.(삭제기능처럼 현재 이 아이템은 삭제대상이다 라는 식의 상태, [장착, 강화, 각성]과 같은 상태는 유저 데이터에 저장되는 데이터)
    //2. 인벤토리가 비활성화될 때 sStateContainer가 null이 아니고 아이템 개수가 남아있다면 반드시 기본상태로 되돌려야한다.
    //1.의 경우에 대한 예시 
    //1-1. 삭제의 경우에는 유저가 삭제를 실제로 하겠다고 진행하지 않는 이상 상태로써 설정만 될 뿐이며 세이브 데이터에는 저장할 필요가 없다.
    //1-2. 장착의 경우에는 유저에 의해서 해당 아이템이 장착이 되었고 유저가 해제하겠다는 의사 기능을 진행하지 않는 이상 계속적으로 유지되어있어야되므로 세이브 데이터에 저장되야한다.
    //2.의 경우에 대한 예시
    //삭제의 경우에는 유저가 해당 아이템을 삭제하겠다고 했지만 삭제를 안하겠다고 하여 해당 인벤토리를 그냥 나가버리는 경우도 존재할 수가 있다.
    //이 경우 인벤토리에 남아있는 아이템들의 상태를 원래상태로 복구를 시켜줘야한다.
    private static Dictionary<int, Dictionary<int, eState>> sStateContainer;

    public static void SReset()
    {
        if (sStateContainer != null)
        {
            foreach (KeyValuePair<int, Dictionary<int, eState>> keyValuePairParent in sStateContainer)
            {
                if (keyValuePairParent.Value != null)
                {
                    keyValuePairParent.Value.Clear();
                }
            }

            sStateContainer.Clear();
        }
    }

    protected static bool IsValidState(int tap, int dataID)
    {
        return (sStateContainer != null) && 
                sStateContainer.ContainsKey(tap) &&
                sStateContainer[tap].ContainsKey(dataID);
    }

    protected static void SSetState(int tap, int dataID, eState state)
    {
        if (sStateContainer == null)
        {
            sStateContainer = new Dictionary<int, Dictionary<int, eState>>();
        }

        if (!sStateContainer.ContainsKey(tap))
        {
            sStateContainer.Add(tap, new Dictionary<int, eState>());
        }

        if (!sStateContainer[tap].ContainsKey(dataID))
        {
            sStateContainer[tap].Add(dataID, state);
        }
        else
        {
            sStateContainer[tap][dataID] = state;
        }
    }

    protected static eState SGetState(int tap, int dataID)
    {
        return sStateContainer[tap][dataID];
    }
}