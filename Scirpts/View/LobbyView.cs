using UnityEngine;

public class LobbyView : CView
{
    [SerializeField]
    private Transform m_SubViewsTr;

    private CView[] mMainViews;

    private void Awake()
    {
        if (m_SubViewsTr != null)
        {
            mMainViews = new CView[m_SubViewsTr.childCount];

            for (int i = 0; i < m_SubViewsTr.childCount; ++i)
            {
                mMainViews[i] = m_SubViewsTr.GetChild(i).GetComponent<CView>();
            }
        }
    }

    protected override void Opening()
    {
        base.Opening();        
    }

    public void OnClick_OpenView()
    {
        OpenView(UIButton.current.transform.GetSiblingIndex());
    }

    public void OpenView(int index)
    {
        if (mMainViews != null && 
            index < mMainViews.Length &&
            mMainViews[index] != null)
        {
            CView.SOpen(mMainViews[index]);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("OpenView에서 에러가 발생했습니다.");
#endif
        }
    }
}
