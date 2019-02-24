using UnityEngine;
using System;

//View에는 현재 MainView와SubView가 존재한다.
//MainView : 한 화면에 반드시 하나만 나타나는 뷰를 의미한다.
//           월드맵, 지역맵과 같은 뷰를 메인 뷰로 볼 수가 있다.(인벤토리의 경우 상황에 따라 메인 뷰가 될 수도 있고 아닐 수도 있다.)
//           MainView와 MainView끼리는 겹칠 수도 동시에 활성화할 수도 없다.
//           MainView는 반드시 빽키를 눌렀을 때 반응을 해야되는 뷰이다. 따라서 뷰 안에 뷰를 여러개 생성하는 방식으로 제작하더라도
//           반드시 메인뷰는 하나이여야만한다.
//SubView : MainView가 켜진 상태에서 해당 MainView 위에 깔리는 View를 의미한다.
//          SubView는 MainView 위에서 여러 개의 뷰를 활성화할 수 있다.
//          빽키가 눌렀을 때 MainView보다 먼저 작동해야되는 뷰이며 활성화된 순서에서 뒤에서부터 비활성화되어야한다.
//테마를 소유한 뷰가 있을 수가 있고 그렇지 않을 경우도 있다. 따라서 테마를 별도로 관리하는 매니저가 존재해야한다.
public abstract class CView : MonoBehaviour
{
    [SerializeField]
    protected GameObject m_Thema;

    protected enum Type { MainView = 0, SubView  }
    protected Type mType = Type.MainView;
    
    protected byte mIndex = 0;

    private bool mIsPlayOpenedSound;

    private Action[] mActions;

    private Action<CView> mCallbackByViewManager;

    protected virtual void OnEnable()
    {
        m_Thema.SetActive(true);
    }

    protected virtual void OnDisable()
    {
        m_Thema.SetActive(false);
    }

    private void Open(bool isPlayOpenSound, params Action[] actions)
    {
        if (sViewManager != null)
        {
            mIsPlayOpenedSound = isPlayOpenSound;
            mActions = actions;

            Opening();
        }
    }

    protected virtual void Opening()
    {
        Opened();
    }

    protected void Opened()
    {
        CView view = sViewManager.Peek();

        //매개인자로 들어오는 view가 메인 뷰일 경우 이전 뷰(Peek)를 Hide시킨다.(메인 쓰레드가 2개일 수 없듯이 메인 뷰 역시 2개를 뛰울 수 없다.)
        if (view != null && view.IsMainView())
        {
            view.Hide();
        }
        
        sViewManager.Push(this);

        Show();
        PlayOpenedSound();
    }
    
    public bool IsMainView()
    {
        return mType == Type.MainView;
    }

    //보이게만 할 뿐 Push를 하지 않는다는 점에서 Open과는 다르다.
    public virtual void Show()
    {
        SetActive(true);
    }

    //안 보이게 할 뿐 Pop을 하지 않는다는 점에서 Close와 다르다.
    public virtual void Hide()
    {
        SetActive(false);
    }

    public bool IsOpen()
    {
        return gameObject.activeSelf;
    }
    
    protected virtual void Closing()
    {
        Closed();
    }

    protected void Closed()
    {
        Closed(true);
    }

    //애니메이션의 연출등과 관련되어서 Close에서 오브젝트를 끄는 것이 아니라
    //Closed에서 오브젝트를 끈다.
    //Closed에서 Hide 함수를 호출했는데 [GameObject is already being activated or deactivated.] 라는 에러 메시지가 뜨는 경우가 있다.
    //이 에러명이 의미하는 것은 유니티의 오브젝트 활성/비활성화 구조를 잘못쓰고 있다는 에러명이다.
    //비활성화시 SetActive(false) -> OnDisable인데 OnDisable에서 같은 클래스 오브젝트를 다시 활성화 혹은 비활성화를 했을 경우에 저런 에러가 나온다.
    //따라서 OnDisable에서는 오브젝트를 활성화, 비활성화를 하지 말아야한다.
    protected void Closed(bool isPop)
    {
        Hide();

        //Pop에서 리턴해주는 View는 현재 View와 동일한 뷰이다.
        CView view = sViewManager.Pop();

        //Pop를 했는데 할일이 있으면 작업을 하면 된다.(현재는 없음.)
        //if (view != null)
        //{

        //}

        view = sViewManager.Peek();

        if (view != null)
        {
            if (view.IsMainView())
            {
                view.Show();
            }
        }
    }
    
    protected void Callback(int index)
    {
        if (mActions != null &&
            (index >= 0 && index < mActions.Length) &&
            mActions[index] != null)
        {
            mActions[index].Invoke();
            mActions[index] = null;
        }
    }
    
    private void SetActive(bool value)
    {
        if (gameObject.activeSelf != value)
        {
            gameObject.SetActive(value);
        }
    }

    //혹시 뷰 별로 출력해야되는 사운드 방식이 다를 경우를 대비해서 만들어둠.
    private void PlayOpenedSound()
    {
        if (mIsPlayOpenedSound)
        {
            PlaySound();
        }
    }

    private void PlaySound()
    {
#if UNITY_EDITOR
        Debug.Log("CView PlaySound 작업을 해야됨");
#endif
        //SoundManager.PlayFx(SoundManager.FxSound.ButtonTouch);
    }
    
    private static ViewManager sViewManager;

    public static void Initialize(CView view)
    {
        if (sViewManager == null)
        {
            sViewManager = new ViewManager();
#if UNITY_EDITOR
            sViewManager.Initialize(view, true);
#else
            sViewManager.Initialize(view, false);
#endif
        }
    }
    
    public static void SOpen(CView view, params Action[] actions)
    {
        SOpen(view, true, actions);
    }

    public static void SOpen(CView view, bool isPlayOpenSound, params Action[] actions)
    {
        view.Open(isPlayOpenSound, actions);
    }

    public static void SClose()
    {
        if (sViewManager.IsValidStack())
        {
            if (sViewManager.IsFirstMainView())
            {
                //여기서 ExitMenu를 호출하면 된다.
#if UNITY_EDITOR
                //음... 커스텀 디버그 창을 시간되면 만들어봐야겠음.
                Debug.Log("SClose ExitMenu 작업을 해야됨.");
#endif
            }
            else
            {
                CView view = sViewManager.Peek();

                if (view != null)
                {
                    view.Closing();
                }
            }
        }   
    }

    public static void SReset()
    {
        if (sViewManager != null)
        {
            sViewManager.Reset();
        }
    }
}
