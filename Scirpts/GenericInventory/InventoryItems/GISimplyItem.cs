using UnityEngine;
using System.Collections.Generic;

//게임에서 나오는 인벤토리 아이템 중 썸네일을 기준으로 하여 간단한 정보를 표시할 때 사용된다.
//썸네일을 기준으로하여 표시하는 아이템의 용어를 몰라서 SmallItem 이라고 일단 칭하기로 하였다.
public class GISimplyItem : GIInventoryItem
{
    protected void OnPress(bool isPress)
    {
        if (GetGenericInventory() != null)
        {
            if (isPress)
            {
                UIManager.Instance.SetPressState(isPress, StartPress);
            }
            else
            {
                UIManager.Instance.SetPressState(isPress, EndPress);
            }
        }
    }

    public UISprite mRemoveTestImage;

    //공통점이 있으면 여기서 처리하면 되고 그렇지 않을 경우에는 상속받아서 처리하면 된다.
    protected virtual void StartPress(UIManager.PressState state)
    {
        switch (state)
        {
            //최종적으로 Press 상태가 Press로 나올 경우 StartPress함수에서 처리를 하지 않고 EndPress에서 처리한다.
            //왜냐하면 누르고 때었을 때 실제로 반영해야되기 때문이다.
            //case GIItemEvent.PressState.Press:
            //    //할일
            //    break;

            case UIManager.PressState.Scroll:
                //할일
                break;

            case UIManager.PressState.Drag:
                //할일
                break;
        }
    }

    //공통점이 있으면 여기서 처리하면 되고 그렇지 않을 경우에는 상속받아서 처리하면 된다.
    protected virtual void EndPress(UIManager.PressState state)
    {
        switch (state)
        {
            case UIManager.PressState.Press:
                //할일
                switch (GetGenericInventory().GetKind())
                {
                    case GISettingData.eKind.GI_WHOLE_AMOUNT_RECYCLE:
                        GIOptionContainer optionContainer = GetGenericInventory().GetOptionContainer();

                        if (optionContainer != null)
                        {
                            Dictionary<GISettingData.eKind, GenericInventory> giContainer = GenericInventory.GetGIContainer();

                            if (giContainer != null && giContainer.Count != 0)
                            {
                                List<GISettingData.eKind> kindList = optionContainer.GetLinkKindList();

                                if (kindList != null)
                                {
                                    GenericInventory linkGI = null;
                                    List<GIFieldLine> linkFieldLines = null;
                                    GIInventoryItem[] linkItems = null;
                                    CNData linkData = null;

                                    GIRemove selfRemove = null;
                                    bool isRefresh = true;

                                    for (int i = 0; (i < kindList.Count && isRefresh); ++i)
                                    {
                                        if (giContainer.ContainsKey(kindList[i]))
                                        {
                                            linkGI = giContainer[kindList[i]];
                                            linkFieldLines = linkGI.GetOperator().GetFieldLines();

                                            //음... 좀 더 스마트한 최적화 방법이 있긴하다.
                                            //먼저 인벤토리 아이템들의 첫 번째 인자의 클래스 데이터와 
                                            //마지막 활성화된(활성화되어있지 않으면 패스한다.) 인자의 범위에 속하지 않으면 비교검사를 하지 않는 초기구문을 넣는 방법이다.
                                            //현재는 이런 기능이 필요는 없다만 기획상에서 한 화면에 100개 이상을 보여야한다는 기획이 나온다면 한번 고려를 해봐야될 문제이다.(폰 액정에서 이런 일이 있을지 의문)
                                            //P.S 혹시 실제 삭제 기능을 실행했을 때에는 이렇게 일일이 비교할 필요없이 해당 데이터를 모두 다 지운 다음에 업데이트가 되므로 신경쓰지 않아도 된다.
                                            //Vertical의 경우에는 상관없으나 Horizontal의 경우에는 데이터의 출력방식이 달라지기 때문에 이 부분도 같이 신경써서 고민해야한다.(고려를 할 필요가 없을 수도 있다.)
                                            GIInventoryItem.SSetState(GetGenericInventory().GetCurrentTap(), mData.GetIntValue(GameTableSetting.eKey.I_UniqueID), eState.Default);

                                            for (int row = 0; (row < linkFieldLines.Count && isRefresh); ++row)
                                            {
                                                linkItems = linkFieldLines[row].GetItems();

                                                for (int col = 0; (col < linkItems.Length && isRefresh); ++col)
                                                {
                                                    linkData = linkItems[col].GetData();

                                                    if (linkData != null)
                                                    {
                                                        if (mData.GetIntValue(GameTableSetting.eKey.I_UniqueID) ==
                                                            linkData.GetIntValue(GameTableSetting.eKey.I_UniqueID))
                                                        {
                                                            linkItems[col].UpdateItem();
                                                            isRefresh = false;
                                                        }
                                                    }
                                                }
                                            }

                                            selfRemove = GetGenericInventory().GetOptionContainer().GetRemove();
                                            selfRemove.RevertData(0, mData);
                                            GetGenericInventory().UpdateInventory();
                                        }
                                    }
                                }
                            }
                        }
                        //정렬된 상태에서 아이템 인덱스로 해당 아이템을 찾는 방법.
                        //지금 당장 쓰이지는 않지만 장착된 장비를 해제할 때와 같은 상황에서 쓰이게 된다.
                        //장착을 해제하면 연결된 클래스 데이터를 찾아서 해제 옵션을 설정해줘야한다. 이 때는 인벤토리에서 찾는 것이 아니라
                        //유저 데이터의 클래스 리스트에서 찾아서 해당 클래스에 해제 옵션을 설정한 뒤 인벤토리 아이템을 갱신시키면 된다.
                        //GIOptionContainer optionContainer = GetGenericInventory().GetOptionContainer();

                        //if (optionContainer != null)
                        //{
                        //    Dictionary<GISettingData.eKind, GenericInventory> giContainer = GenericInventory.GetGIContainer();

                        //    if (giContainer != null && giContainer.Count != 0)
                        //    {

                        //        List<GISettingData.eKind> kindList = optionContainer.GetLinkKindList();
                        //        GenericInventory linkGI = null;
                        //        GISort linkSort = null;
                        //        GIItems linkItems = null;

                        //        for (int i = 0; i < kindList.Count; ++i)
                        //        {
                        //            if (giContainer.ContainsKey(kindList[i]))
                        //            {
                        //                linkGI = giContainer[kindList[i]];
                        //                linkSort = linkGI.GetOptionContainer().GetSort();
                        //                linkItems = linkGI.GetOperator().GetItems();

                        //                int linkIndex = Utility.BinarySearchInCurrentSort(linkItems.GetDatas(), linkSort.GetKeys(linkGI.GetCurrentTap()), mData, true);

                        //                if (linkIndex != CNData.NONE_VALUE)
                        //                {

                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                        break;

                    case GISettingData.eKind.GI_SELECT_RECYCLE:
                        if (mData != null)
                        {
                            GIRemove remove = GetGenericInventory().GetOptionContainer().GetRemove();

                            if (remove != null)
                            {
                                if (mRemoveTestImage.gameObject.activeSelf)
                                {
                                    //삭제 등록 해제
                                    mRemoveTestImage.gameObject.SetActive(false);
                                    remove.RevertData(mGIFieldLine.GetGenericInventory().GetCurrentTap(), mData);

                                    GIInventoryItem.SSetState(GetGenericInventory().GetCurrentTap(), mData.GetIntValue(GameTableSetting.eKey.I_UniqueID), eState.Default);
                                }
                                else
                                {
                                    //삭제등록
                                    mRemoveTestImage.gameObject.SetActive(true);
                                    remove.AddData(mGIFieldLine.GetGenericInventory().GetCurrentTap(), mData);

                                    GIInventoryItem.SSetState(GetGenericInventory().GetCurrentTap(), mData.GetIntValue(GameTableSetting.eKey.I_UniqueID), eState.Remove);
                                }
                            }
                        }
                        break;
                }
                break;

            case UIManager.PressState.Scroll:
                //할일
                break;

            case UIManager.PressState.Drag:
                //할일
                break;
        }
    }

    protected override void AppendUpdateItem()
    {
        switch (GetGenericInventory().GetKind())
        {
            case GISettingData.eKind.GI_SELECT_RECYCLE:
                if (GIInventoryItem.IsValidState(GetGenericInventory().GetCurrentTap(), mData.GetIntValue(GameTableSetting.eKey.I_UniqueID)) &&
                    GIInventoryItem.SGetState(GetGenericInventory().GetCurrentTap(), mData.GetIntValue(GameTableSetting.eKey.I_UniqueID)) == eState.Remove)
                {
                    mRemoveTestImage.gameObject.SetActive(true);
                }
                else
                {
                    mRemoveTestImage.gameObject.SetActive(false);
                }
                break;
        }
    }
}
