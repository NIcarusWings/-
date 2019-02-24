using UnityEngine;
using System.Collections.Generic;
using System;

//주의사항!!!
//절대로 개발도중에 진행되는 프로젝트에서는 쓰지 말것.
//고려해야될 대상이 너무 많기 때문에 예외의 모든 것을 대응할 수가 없다.
//따라서 프로젝트 초기에 투입 혹은 여유가 있을 때에만 사용할 것.

//일단 생각나는 것을 적어둠.
//이 인벤토리에 들어올 때는 가공처리된 정보를 보여준다 라는 기준을 새운다.
//따라서 가공처리되지 않은 순수 데이터의 경우에는 해당되지 않는다.
//여기서 문제점은 가공처리되지 않은 순수 데이터의 경우에도 인벤토리에 출력이 되어져야한다라는 것이다.(도감, 업적 등등)
//하지만 잘 생각해보면 도감의 경우에는 [현재 내가 보유한 아이템]을 출력한다 라는 조건이 붙는다.
//또한 소녀전선의 경우에는 서버를 통하여 현재 케릭터에 대해 몇 명이 추천을 했는가에 따른 정보도 받아온다.
//따라서 도감은 얼핏보면 데이터로써만 출력된다 라고 생각할 수도 있지만 그렇지는 않다.(업적 또한 진행정도, 달성횟수, 달성상태 등등 정보를 포함한다.)
//이 경우 도감의 아이템은 도감 뷰가 활성화되었을 때 관련 아이템들을 초기화한 다음에 인벤토리에 넣어야한다.
//인벤토리의 경우에는 인벤토리의 아이템을 클릭 -> 변화되는 작업이 이루어진다 -> 그러면 갱신시킨다 라는 구조로 가야지
//인벤토리 안에서 관련 아이템이 초기화가 이루어져서는 안된다. 인벤토리 안에서 관련 아이템의 초기화를 한다면 
//관련 아이템창 GenericInventory를 별도로 가져야되므로 실상 GenericInventory를 써야할 이유가 없어진다.
//한 인벤토리에는 기본 형태가 같은 아이템들은 하나로 통합한 객체로써 관리된다.
//예를 들면 무기, 방어구, 방패등등 출력이미지와 표시값에 의해서만 달라질 뿐 기본 형태는 완전히 동일하다. 이 경우 해당 객체는 출력하는 속성만 변경할 뿐 기본 형태를 완전히 탈 바꿈하진 않는다.
//만약 한 메뉴에서 탭으로 아이템들을 출력하는데 기본 형태가 완전히 다르게 출력될 경우에는 하나의 인벤토리를 사용하면 안되고 별도의 인벤토리를 만들어서 사용해야한다.(솔직히 이런 현상이 생기면 기획이 이상한거라고 생각할 수 밖에 없다.)
public class GenericInventory : MonoBehaviour
{
    public const int NONE_LINE = -1;

    //GenericInventory간에 통신을 하기 위한 수단으로 만든 컨테이너이다.
    //어느 GenericInventory의 특정 이벤트에 의해 어느 GenericInventory가 변경되야할 경우가 존재할 경우 이 컨테이너를 통해서 갱신하면 된다.
    //자세한 것은 리니지M, 테라M, 소녀전선 등의 아이템 판매할 때의 인벤토리 구조를 가진 순위권 게임을 참고.(2018년 01월 30일 기준)
    private static Dictionary<GISettingData.eKind, GenericInventory> sGIContainer;
    
    private GISettingData.eKind mGI_KIND = GISettingData.eKind.None;
    private int mCurrentTap = -1;

    private CNData mGISettingDatas;

    //GenericInventory의 핵심기능들을 처리하는 클래스.
    private GIOperator mGIOperator;
    //GenericInventory의 부가기능들을 처리하는 클래스.
    private GIOptionContainer mGIOptionContainer;
    
    public void Initialize(GISettingData.eKind GI_KIND, int currentTap)
    {
        mGI_KIND = GI_KIND;
        mCurrentTap = currentTap;
        
        mGIOperator = new GIOperator();
        mGIOperator.Initialize(this);
        
        if (GetGISettingData().GetStringValue(GameTableSetting.eKey.Options) != null)
        {
            mGIOptionContainer = new GIOptionContainer();
            mGIOptionContainer.Initialize(this);

            AddGenericInventory(this);
        }
    }
    
    public void UpdateInventory()
    {
        if (mCurrentTap != -1)
        {
            UpdateInventory(mCurrentTap);
        }
    }

    public void UpdateLinkInventory()
    {
        List<GISettingData.eKind> mLinkKindList = mGIOptionContainer.GetLinkKindList();

        for (int i = 0; i < mLinkKindList.Count; ++i)
        {
            GetTargetInventory(mLinkKindList[i]).UpdateInventory();
        }
    }

    public void UpdateInventory(int currentTap)
    {
        //일반적인 상황에서는 일어날 일은 없음.
        if (mGIOperator != null)
        {
            mCurrentTap = currentTap;
            mGIOperator.Update(currentTap);

            if (mGIOptionContainer != null)
            {
                mGIOptionContainer.Sort(mGIOperator.GetItems(), 0);

                RefreshInventory();
            }
        }
        else
        {
            Debug.LogError("mGIOperator is not Init");
        }
    }

    public GISettingData.eKind GetKind()
    {
        return mGI_KIND;
    }
    
    public GIOperator GetOperator()
    {
        return mGIOperator;
    }

    public GIOptionContainer GetOptionContainer()
    {
        return mGIOptionContainer;
    }

    public int GetCurrentTap()
    {
        return mCurrentTap;
    }
    
    //UpdateInventory와의 차이점은 mCustomGrid.Reposition()이 없다는 것이다.(문서화해야되는데 이 놈의 귀차니즘 때문에 일단 코드에 때려박아두고 있다 -_-... 내일은 내일의 태양이 뜨겠지 -,.-)
    //왜 이 둘을 따로 구현했는가하면 인벤토리 뷰에서 뷰 아이템들이 즉시 변환이 일어나야될 경우 mCustomGrid.Reposition()를 하면 안되기 때문이다.
    //mCustomGrid.Reposition()을 했을 경우 상황을 고려하면 다음과 같다.(현 상황은 아이템이 추가에 대해서만 고려한 것이지 변경(아이템을 업그레이드했다던지, 각성을 했다던지 등), 삭제에 대한 상황까지 고려한 것은 아니다.)
    //1. 스크롤을 해서 마지막 라인까지 드래그를 했다.(이는 아이템이 추가되는 것을 확인하기 위함이다.)
    //2. 아이템을 다음 라인이 생길 때까지 계속해서 추가한다.
    //3. 다음 라인이 생길 때 mCustomGrid.Reposition()이 호출된다.
    //여기서 mCustomGrid.Reposition()를 하면 현재 첫 번째 라인을 기준으로 하여 화면에 보이는 최초 좌표로 설정한다.(Top, Left 속성의 0, 0을 기준)
    //그런데 현재 마지막 라인까지 스크롤을 했다면 최초 라인이 최초 시작하는 라인의 위치에 있는 것이 아니라 최초 라인의 바로 위 좌표에 있다.
    //위의 말을 다음과 같이 그림으로 나타내면 다음과 같다.
    //최초 시작할 때 라인들의 구조             마지막까지 스크롤 했을 때 라인들의 구조            마지막까지 스크롤 했을 때 Grid.Reposition을 실행했을 때
    //                                       Line 1|------------------|                  ---|???????????-_-??????????| <------- 문제 발생 요인1
    //--------------------------             --------------------------                  |  --------------------------
    //Line 1|------------------|             Line 2|------------------|                  |  Line 1|------------------|
    //Line 2|------------------|    Drag     Line 3|------------------|  Grid.Reposition |  Line 2|------------------|
    //Line 3|------------------|     =>      Line 4|------------------|      =>          |  Line 3|------------------|
    //Line 4|------------------|             Line 5|------------------|                  |  Line 4|------------------|
    //--------------------------             --------------------------                  |  --------------------------                    
    //Line 5|------------------|                                                            -▶Line 5|------------------| <------- 문제 발생 요인2
    //마지막까지 스크롤 했을 때 라인들의 구조에서 라인이 위에서 추가되기 떄문에 mCustomGrid.Reposition()을 하면 최초 시작할 때 라인들의 구조로 변경되고
    //이 상황에서 위로 드래그를 할 경우 라인 1이 밑으로 가버리기 때문에 라인 하나가 사라지는 현상이 발생한다.
    //음... 결론은 아이템의 추가, 삭제가 일어날 경우 이에 관한 정보를 교정해야되는 작업을 해야하는데 고민을 좀 해봐야겠다.
    //추가의 경우 교정을 하지 않아도 상관없는데 리니지M처럼 아이템을 제거했을 때 아이템이 없을 경우 이에 대한 정보를 출력하지 않을 경우 문제가 발생할 수도 있다.
    //하지만 리니지M의 경우 다른 View에서 실행하기 때문에 재차 갱신을 하기 때문에 크게 문제가 되지 않는다.(현재 문제가 되는 것은 아이템을 삭제했을 때 구조상 즉시 갱신해야될 때 문제가 발생하는 것이다.)
    //(미래구원자라던지... 미래구원자라던지... ㅡㅡ...)
    public void RefreshInventory()
    {
        mGIOperator.Refresh();
    }
    
    //정렬할 때 반드시 상기해야하는 부분이 있다.
    //GIItem에서 사용되는 유저 데이터는 실제 유저 데이터를 관리하는 컨테이너(List)가 아니라 이를 토대로 만든 컨테이너이라는 점이다.
    //따라서 유저 데이터가 정렬되는 것이 아니라 GIItem에 존재하는 배열이 정렬되는 것이다.
    //단 정렬만 안될 뿐 요소가 변경될 경우에는 유저 데이터도 같이 바뀐다는 것은 상기해야한다.
    public void OnClick_Sort()
    {
        mGIOptionContainer.Sort(mGIOperator.GetItems(), UIButton.current.transform.GetSiblingIndex());
        mGIOperator.Refresh();
    }

    public void OnClick_Refresh()
    {
        UpdateInventory(mCurrentTap);
        //for (int i = 0; i < mFieldLines.Count; ++i)
        //{
        //    mFieldLines[i].Resetting(i);
        //}

        //mGIItems.UpdateItem(mGI_KIND, mCurrentTap);

        //UpdateItems();

        //mInvScrollView.Resetting();
        //mCustomGrid.Reposition();
    }

    public void OnClick_Add()
    {
        //CNData[] bodyDatas = GameManager.Instance.GetDataContainer().GetDatas(mTableKind);

        //GameManager.Instance.GetUserData().BodyInfos.Add(
        //    new BodyInfomation(
        //        mTableKind,
        //        bodyDatas[UnityEngine.Random.Range(0, bodyDatas.Length)].GetIntValue(GameTableSetting.eKey.ID)));

        //RefreshInventory();
    }

    public void OnClick_Remove()
    {

    }

    //아이템을 Remove하는 것은 인벤토리에서 하는 것이 아니라 외부 요소에서 Remove를 한다라고 통보할 때 시작된다.
    //또한 Remove의 경우에는 단일삭제와 복수삭제가 존재한다. 복수삭제의 경우에는 단일삭제로써 사용될 수도 있으나 단일삭제의 로직을 그대로 사용할 수는 없다.
    //먼저 단일삭제의 로직을 살펴보자.
    //1. 아이템을 클릭한다.
    //2. 삭제를 하겠다는 요청을 받는다.(버튼등을 통해서...)
    //3. 해당 아이템의 GIItem에 해당되는 배열 인덱스를 가져온다.
    //   P.S : 초기 설계시에는 유저가 보유한 컨테이너에서 삭제해야되기 때문에 컨테이너를 기준으로 인덱스를 가져오기로 설계를 했었다.
    //          하지만 그렇게 할 경우 Item들을 웹퍼한 GIItems에서 보유한 아이템들의 연결하는 방식이 모호해지기 때문에 폐기하기로 했다.
    //          컨테이너 기준으로 인덱스를 가져오면 정렬이 될 경우 GIItems이 정렬이 되어있을 경우 서로 매칭이 맞지 않는 점이 발생해서 모호해진다.
    //          따라서 먼저 mGIRemove에서 삭제할 인덱스들을 미리 대입하고 GIRemove 안에서 있는 RemoveList를 GIItems로 넘겨서 GIItems에서 실제 데이터를 삭제하고 갱신하도록 변경함.
    //          이렇게 구현하면 GIRemove에서 따로 삭제에 대해서 따로 처리할 필요도 없고 GIItems에서 따로 처리할 필요없이 GIItems에서 모두 관리할 수 있게 된다.
    //4. 삭제 작업이 끝나면 인벤토리를 재갱신한다.
    public void OperatorRemoveItems()
    {
        if (mGIOptionContainer != null)
        {
            GIRemove remove = mGIOptionContainer.GetRemove();

            if (remove != null)
            {
                remove.Operation();

                UpdateInventory();
                UpdateLinkInventory();
            }
        }
    }

    public CNData GetGISettingData()
    {
        if (mGISettingDatas == null)
        {
            mGISettingDatas = GameManager.Instance.GetData(GameTableSetting.eTableKind.GISettingData, (int)mGI_KIND);
        }

        return mGISettingDatas;
    }

    public static Dictionary<GISettingData.eKind, GenericInventory> GetGIContainer()
    {
        return sGIContainer;
    }

    public static GenericInventory GetTargetInventory(GISettingData.eKind kind)
    {
        return sGIContainer[kind];
    }

    private static void AddGenericInventory(GenericInventory genericInventory)
    {
        if (sGIContainer == null)
        {
            sGIContainer = new Dictionary<GISettingData.eKind, GenericInventory>();
        }

        if (!sGIContainer.ContainsKey(genericInventory.GetKind()))
        {
            sGIContainer.Add(genericInventory.GetKind(), genericInventory);
        }
    }
    
    private void OnDestroy()
    {
        if (sGIContainer != null)
        {
            sGIContainer.Clear();
        }
    }
}