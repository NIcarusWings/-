using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//상황 1, 연출없이 단순 내포관계
//현재 인벤토리 뷰를 보고 있다가 인벤토리 안에 무기상점 뷰로 넘어가는 기능이 존재해서 무기상점 뷰를 활성화하는 경우에 대한 처리는 이 스크립트에서 처리한다.
//위와 같은 상황을 알고리즘을 생각하면 다음과 같다.
//1. 인벤토리 뷰에서 무기상점 뷰를 클릭했을 때 인벤토리 뷰는 Hide가 될 뿐 Close를 호출하면 안된다.(스택에서 Pop을 해버리는데 무기상점 뷰를 Close하면 되돌아갈 수가 없다.)
//2. 무기상점 뷰를 Push한다.
//3. 유저가 빽키 혹은 돌아가기 버튼을 클릭하면 무기상점 뷰를 Close(Hide & Pop)한다.
//4. 스택에서 Peek를 해서 인벤토리 뷰를 활성화한다.
//이런 경우 인벤토리 뷰나 무기상점 뷰는 해당 분류에서 메인 뷰로 취급되며 Push함수의 매개변수가 메인 뷰 일 경우 이전 뷰를 Hide 시킨다.
//즉 현재 메인 뷰(인벤토리 뷰)에서 특정 메인 뷰(무기상점 뷰)를 호출할 경우 현재 메인 뷰를 직접 Hide 시킬 필요는 없다.(시켜도 상관없긴 하다.)

//상황 2, 연출이 있는 내포관계(ㅂㄷㅂㄷ-_-)
//유저가 현재 월드 뷰를 보고 있고 로컬 뷰로 이동할려고 한다.
//기획에서 월드 뷰에 Close시 애니메이션을 연출하고 로컬 뷰가 활성화했을 때 애니메이션 연출을 넣어달라고 한다.(시벌...)
//위와 같은 상황의 알고리즘을 생각하면 상황 1을 토대로 고려사항과 추가사항을 포함해서 구현한다.
//고려사항
//1. 애니메이션을 실행하여 보여주는 것은 어디까지나 유저의 눈에서 사라지는 것이지 기능상으로 실제 Hide나 Close 되는 것이 아니다.
//2. 애니메이션을 실행하는 동안 유저가 다른 기능들을 선택했을 경우 실행이 되지 않게 막아야한다.
//추가사항
//1. 월드 뷰에서 CView의 Hide 함수를 override하여 연출을 구현한다.(Hide 연출 중에 로컬 뷰가 활성화되야한다면 연출 구현부분에서 코루틴을 사용하면 된다.)
//1-1. 연출이 끝나면 Hide한다.
//2. 로컬 뷰에서 CView의 Show 함수를 override하여 연출을 구현한다.
//2-2. 연출이 시작되기전에 Show한다.(Hide와 다르다.)
//만약 역으로 되돌아갈 때 역시 연출을 보여하므로 월드 뷰는 Show, 로컬 뷰는 Hide를 override하여 연출을 구현하면 된다.
public class ViewManager
{
    private Stack<CView> mStack;

#if UNITY_EDITOR
    private bool mIsEditorMode = true;
#endif
    public void Initialize(CView view, bool isEditorMode)
    {
        if (mStack == null)
        {
            mStack = new Stack<CView>();
        }

        //만약에 초기화를 하지 않고 그냥 넘어갔을 경우를 대비.
        Reset();

        CView.SOpen(view);

#if UNITY_EDITOR
        mIsEditorMode = isEditorMode;
#endif
    }
    
    public void Push(CView view)
    {
        //Push는 IsValidStack이 아니라 !IsNullStack()로 검사해야한다. 스택이 null이 아니면 count의 갯수와 상관없이 추가해야하기 때문.
        if (!IsNullStack() && Peek() != view)
        {
            mStack.Push(view);

            OutputLog("Push 스택 카운트 : " + GetStackCount());
        }
    }
    
    public CView Peek()
    {
        if (IsValidStack())
        {
            return mStack.Peek();
        }
        else
        {
            return null;
        }
    }

    public CView Pop()
    {
        CView view = null;

        if (IsValidStack())
        {
            view = mStack.Pop();
        }
        
        OutputLog("Pop 스택 카운트 : " + GetStackCount());
        
        return view;
    }
    
    public int GetStackCount()
    {
        if (IsNullStack())
        {
            return -1;
        }
        else
        {
            return mStack.Count;
        }
    }

    public bool IsValidStack()
    {
        return !(IsNullStack() || IsEmpty());
    }

    public bool IsFirstMainView()
    {
        return mStack.Count == 1;
    }

    private bool IsNullStack()
    {
        if (mStack == null)
        {
            OutputLog("스택이 초기화가 되지 않았습니다.");
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsEmpty()
    {
        if (mStack.Count == 0)
        {
            OutputLog("스택이 비어있습니다.");
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Reset()
    {
        mStack.Clear();
    }
    
    private void OutputLog(string value)
    {
#if UNITY_EDITOR
        Debug.Log(value);
#endif
    }
}

