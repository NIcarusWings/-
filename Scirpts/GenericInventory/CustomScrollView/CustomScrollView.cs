using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This script, when attached to a panel turns it into a scroll view.
/// You can then attach UIDragScrollView to colliders within to make it draggable.
/// </summary>
/// 
[ExecuteInEditMode]
[RequireComponent(typeof(UIPanel))]
//[AddComponentMenu("NGUI/Interaction/CustomScroll View")]
public class CustomScrollView : UIScrollView
{
    private enum MoveState { None, Normal_Up, Normal_Down, Moment_Up, Moment_Down };
    public enum DragState { Normal_Drag, Moment_Drag, No_Update, Out_End };

    private MoveState mIsMoveLeftToRight = MoveState.None;
    private MoveState mIsMoveTopToDown = MoveState.None;

    private GIOperator mGIOperator;

    private SpringPanel mSpringPanel;

    private CustomGrid mGrid = null;
    private CPoint mCurField = CPoint.zero;
    
    private float mStart = 0f;
    //private float mLimit = 0f;

    private int mLastFieldIndex = 0;
    private int mRemainLastItemCount = 0;

    public override Bounds bounds
    {
        get
        {
            if (!mCalculatedBounds)
            {
                mCalculatedBounds = true;
                mTrans = transform;
            }

            return base.bounds;
        }
    }

    public void Initialize(GIOperator giOperator)
    {
        InitField();
        
        mGIOperator = giOperator;
        mGrid = mGIOperator.GetCustomGrid();

        switch (movement)
        {
            case Movement.Horizontal:
                mStart = mTrans.localPosition.x;
                break;

            case Movement.Vertical:
                mStart = mTrans.localPosition.y;
                break;
        }
        
        onMomentumMove = OnMomentumMove;
        onDragFinished = OnDragFinished;
    }

    private void InitField()
    {
        mPanel = GetComponent<UIPanel>();
        mTrans = transform;
        
        if (mPanel.clipping == UIDrawCall.Clipping.None)
            mPanel.clipping = UIDrawCall.Clipping.ConstrainButDontClip;

        // Auto-upgrade
        if (movement != Movement.Custom && scale.sqrMagnitude > 0.001f)
        {
            if (scale.x == 1f && scale.y == 0f)
            {
                movement = Movement.Horizontal;
            }
            else if (scale.x == 0f && scale.y == 1f)
            {
                movement = Movement.Vertical;
            }
            else if (scale.x == 1f && scale.y == 1f)
            {
                movement = Movement.Unrestricted;
            }
            else
            {
                movement = Movement.Custom;
                customMovement.x = scale.x;
                customMovement.y = scale.y;
            }
            scale = Vector3.zero;
#if UNITY_EDITOR
            NGUITools.SetDirty(this);
#endif
        }

        // Auto-upgrade
        if (contentPivot == UIWidget.Pivot.TopLeft && relativePositionOnReset != Vector2.zero)
        {
            contentPivot = NGUIMath.GetPivot(new Vector2(relativePositionOnReset.x, 1f - relativePositionOnReset.y));
            relativePositionOnReset = Vector2.zero;
#if UNITY_EDITOR
            NGUITools.SetDirty(this);
#endif
        }
    }

    public void UpdateBounds()
    {
        UpdateBounds(LastFieldIndex);
    }
    
    void Update()
    {
        if (mDragStarted && mShouldMove)
        {
            UpdateField(mStart);
        }
    }


    //이 함수의 역할은 드래그 도중 탭을 눌렀을 경우 스크롤 뷰의 동작을 멈추게 하는 역할이다.
    //드래그 도중에 탭을 누르면 다음 탭에 해당되는 아이템 리스트를 출력하는데 이미 드래그 중이라는 현상과 겹쳐서 다음과 같은 현상이 발생한다.
    //1. 아이템이 적을 때는 인덱스범위를 벋어나는 에러가 발생한다.
    //2. 아이템이 같거나 많을 때에는 드래그 중인 인덱스에 맞추어서 갱신하기 때문에 제대로 출력이 되지 않는 현상이 발생한다.
    //위의 현상을 막기 위해서 드래그 중에 아이템이 갱신되어야될 상황이 발생하면 반드시 호출해줘야한다.
    //참고로 이 함수를 호출했다고 해서 강제로 재개시켜줄 필요는 없다.(여기서 재개란 mShouldMove를 true로 만드는 직접적인 호출을 말함.)
    //mShouldMove가 true가 되는 시점이 있는데 이 시점이 스크롤 뷰의 영역을 클릭해서 드래그 시작할 때이다.
    private void Stop()
    {
        mShouldMove = false;

        //dragEffect가 DragEffect.MomentumAndSpring일 경우에는 SpringPanel이 붙어있을 것이다.(처음 시작시에는 당연히 안 붙어있다.)
        //문제는 드래그를 하고 다음 탭으로 넘어가면 SpringPanel의 Update함수안에 구현된 AdvanceTowardsPosition함수가 지속적으로 호출된다.
        //mSpringPanel.enabled = false를 해봤지만 상관없이 계속 호출된다. 따라서 AdvanceTowardsPosition함수안에 target을 통해서 이동거리를 계산하는 로직이 있는데
        //이 target을 현 스크롤뷰의 localPosition과 동일하게 설정해주면 되는 것을 확인하였다.
        if (dragEffect == DragEffect.MomentumAndSpring)
        {
            //여기서 mSpringPanel == null일 때에도 작업을 한다. LateUpdate함수에서 SpringPanel이 할당되는 부분을 살펴보면 드래그가 끝난 다음에 SpringPanel이 할당된다.
            //문제는 드래그가 끝나기 전에 탭을 선택해버리면 드래그 종료이후 SpringPanel를 할당하여 처리를 해버리기 때문에 오작동이 발생한다.
            //또한 드래그 시작 -> 드래그 끝나기 전에 탭 클릭 -> 다시 이전 탭 혹은 다른 탭 클릭 시에도 오작동이 발생한다.
            //따라서 스크롤뷰의 기능을 멈출 때 SpringPanel의 존재여부를 검사해야한다.
            if (mSpringPanel == null)
            {
                mSpringPanel = GetComponent<SpringPanel>();
            }
            
            if (mSpringPanel != null)
            {
                mSpringPanel.target = mTrans.localPosition;
            }
        }
    }

    void LateUpdate()
    {
        if (!Application.isPlaying) return;
        float delta = RealTime.deltaTime;

        // Fade the scroll bars if needed
        if (showScrollBars != ShowCondition.Always && (verticalScrollBar || horizontalScrollBar))
        {
            bool vertical = false;
            bool horizontal = false;

            if (showScrollBars != ShowCondition.WhenDragging || mDragID != -10 || mMomentum.magnitude > 0.01f)
            {
                vertical = shouldMoveVertically;
                horizontal = shouldMoveHorizontally;
            }

            if (verticalScrollBar)
            {
                float alpha = verticalScrollBar.alpha;
                alpha += vertical ? delta * 6f : -delta * 3f;
                alpha = Mathf.Clamp01(alpha);
                if (verticalScrollBar.alpha != alpha) verticalScrollBar.alpha = alpha;
            }

            if (horizontalScrollBar)
            {
                float alpha = horizontalScrollBar.alpha;
                alpha += horizontal ? delta * 6f : -delta * 3f;
                alpha = Mathf.Clamp01(alpha);
                if (horizontalScrollBar.alpha != alpha) horizontalScrollBar.alpha = alpha;
            }
        }

        if (!mShouldMove) return;

        // Apply momentum
        if (!mPressed)
        {
            if (mMomentum.magnitude > 0.0001f || Mathf.Abs(mScroll) > 0.0001f)
            {
                if (movement == Movement.Horizontal)
                {
                    mMomentum -= mTrans.TransformDirection(new Vector3(mScroll * 0.05f, 0f, 0f));
                }
                else if (movement == Movement.Vertical)
                {
                    mMomentum -= mTrans.TransformDirection(new Vector3(0f, mScroll * 0.05f, 0f));
                }
                else if (movement == Movement.Unrestricted)
                {
                    mMomentum -= mTrans.TransformDirection(new Vector3(mScroll * 0.05f, mScroll * 0.05f, 0f));
                }
                else
                {
                    mMomentum -= mTrans.TransformDirection(new Vector3(
                        mScroll * customMovement.x * 0.05f,
                        mScroll * customMovement.y * 0.05f, 0f));
                }
                mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);

                // Move the scroll view
                Vector3 offset = NGUIMath.SpringDampen(ref mMomentum, dampenStrength, delta);
                MoveAbsolute(offset);
                
                // Restrict the contents to be within the scroll view's bounds
                if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None)
                {
                    if (NGUITools.GetActive(centerOnChild))
                    {
                        if (centerOnChild.nextPageThreshold != 0f)
                        {
                            mMomentum = Vector3.zero;
                            mScroll = 0f;
                        }
                        else centerOnChild.Recenter();
                    }
                    else
                    {
                        RestrictWithinBounds(false, canMoveHorizontally, canMoveVertically);
                    }
                }

                if (onMomentumMove != null)
                    onMomentumMove();
            }
            else
            {
                mScroll = 0f;
                mMomentum = Vector3.zero;

                if (dragEffect == DragEffect.MomentumAndSpring)
                {
                    if (mSpringPanel == null)
                    {
                        mSpringPanel = GetComponent<SpringPanel>();
                    }
                }

                SpringPanel sp = mSpringPanel;
                if (sp != null && sp.enabled) return;

                mShouldMove = false;
                if (onStoppedMoving != null)
                    onStoppedMoving();
            }
        }
        else
        {
            // Dampen the momentum
            mScroll = 0f;
            NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
        }
    }

    void OnMomentumMove()
    {
        if (mShouldMove)
        {
            UpdateField(mStart);
        }
    }

    void OnDragFinished()
    {
        CPoint curField = GetCurrentField();

        ProofreadingField(curField);

        mCurField = curField;

        mDragStarted = false;
    }
    
    public void UpdateBounds(int fieldCount)
    {
        //OnValidate에서 호출될 경우 mGrid가 null
        if (mGrid != null)
        {
            InvalidateBounds();
            //실제 문제가 되고 있는 부분이 이 부분이다.
            mBounds = CalculateBounds(fieldCount);
            RestrictWithinBounds(true);
        }
    }


    //맨 처음 시작할 때 fieldIndex 가 0이 들어오는데 이 때 dragState 의 값이 OUT_START이다.
    //최초 시작할 때는 이미 데이터가 초기화가 되어있는 상태이므로 반복해서 처리할 필요가 없기 때문이다.
    public void UpdateField(float originalStartValue)
    {
        CPoint curField = GetCurrentField();

        if (mCurField == curField) return;

        SetMoveState(curField);

        DragState dragState = GetDragState(curField, originalStartValue);

        if (dragState != DragState.No_Update)
        {
            switch (dragState)
            {
                case DragState.Normal_Drag:
                    ChangingField(curField);
                    break;

                case DragState.Moment_Drag:
                    ProofreadingMoveState();
                    ProofreadingField(curField);
                    break;
            }
        }

        mCurField = curField;
    }

    private void SetMoveState(CPoint curField)
    {
        switch (movement)
        {
            case Movement.Horizontal:
                mIsMoveLeftToRight =
                    (curField.x == mCurField.x) ? MoveState.None :
                    (curField.x < mCurField.x) ? MoveState.Normal_Up : MoveState.Normal_Down;
                break;

            case Movement.Vertical:
                mIsMoveTopToDown =
                    (curField.y == mCurField.y) ? MoveState.None :
                    (curField.y < mCurField.y) ? MoveState.Normal_Up : MoveState.Normal_Down;
                break;
        }
    }

    private void ProofreadingMoveState()
    {
        if (mIsMoveLeftToRight != MoveState.None)
        {
            mIsMoveLeftToRight = (mIsMoveLeftToRight == MoveState.Normal_Up) ? MoveState.Moment_Up : MoveState.Moment_Down;
        }

        if (mIsMoveTopToDown != MoveState.None)
        {
            mIsMoveTopToDown = (mIsMoveTopToDown == MoveState.Normal_Up) ? MoveState.Moment_Up : MoveState.Moment_Down;
        }
    }

    private DragState GetDragState(CPoint curField, float originalStartValue)
    {
        if (mIsMoveLeftToRight == MoveState.None && mIsMoveTopToDown == MoveState.None)
        {
            return DragState.No_Update;
        }
        else
        {
            if (mIsMoveLeftToRight == MoveState.Normal_Down)
            {
                if (curField.x <= originalStartValue)
                {
                    return DragState.No_Update;
                }
            }

            if (mIsMoveTopToDown == MoveState.Normal_Down)
            {
                if (curField.y <= originalStartValue)
                {
                    return DragState.No_Update;
                }
            }
        }

        //순간 드래그일 경우.
        //OnDragFinished에서 처리하면 되지 왜 굳이 Update문에서 지속적으로 비교해서 처리해야되는지 의문이 들 수 있으나 OnDragFinished는 터치를 Up했을 경우에 발생합니다. 
        //드래그 중 순간 드래그를 하면서 터치 Up을 안할 수도 있다는 점을 생각하면 Update문에서 지속적으로 비교해서 처리해야됩니다.
        //필드의 간격이 1보다 클 경우면 순간 드래그로 판별합니다.
        if ((Mathf.Abs(mCurField.x - curField.x) > 1) || (Mathf.Abs(mCurField.y - curField.y) > 1))
        {
            return DragState.Moment_Drag;
        }
        else
        {
            return DragState.Normal_Drag;
        }
    }

    /* mCurrentFieldIndex가 fieldIndex보다 클 경우 밑에 TopLeft 기준으로 볼 때 인덱스가 증가한다는 의미가 됨.
     * 인덱스가 증가한다는 건 스크롤 뷰를 위로 올리고 있다는 것을 의미하므로 다음 아이템들을 밑으로 이동시키면 됨.
     * ----------------------------------------------------------------------------------------------------
     * mCurrentFieldIndex가 fieldIndex보다 작을 경우 밑에 TopLeft 기준으로 볼 때 인덱스가 감소한다는 의미가 됨.
     * 인덱스가 감소한다는 건 스크롤 뷰를 밑으로 내리고 있다는 것을 의미하므로 다음 아이템들을 위로 이동시키면 됨.
     */
    private void ChangingField(CPoint curField)
    {
        int lastFieldStartIndex = GetLastViewStartIndex();

        if (!CheckMoveField(mIsMoveLeftToRight, curField.x, lastFieldStartIndex))
        {
            mIsMoveLeftToRight = MoveState.None;
        }

        if (!CheckMoveField(mIsMoveTopToDown, curField.y, lastFieldStartIndex))
        {
            mIsMoveTopToDown = MoveState.None;
        }

        if (mIsMoveLeftToRight != MoveState.None || mIsMoveTopToDown != MoveState.None)
        {
            //NONE_LINE을 매개변수로 받는 이유는 NONE_LINE을 선언할 곳을 확실하게 결정하지 못했기 때문
            mGIOperator.ChangeField(GetDragMatrix(curField));
        }
    }

    private bool CheckMoveField(MoveState state, int value, int lastFieldStartIndex)
    {
        if (state == MoveState.None) return false;
        if (state == MoveState.Normal_Up && (value < 0 || value >= lastFieldStartIndex)) return false;
        if (state == MoveState.Normal_Down && value > lastFieldStartIndex) return false;

        return true;
    }

    protected virtual void ProofreadingField(CPoint curField)
    {
        if (mCurField == curField) return;

        mGIOperator.MomentField(GetDragMatrix(curField));

        mGrid.Reposition();
    }

    //matrix.m00(10) 와 matrix.m01(11)은 같을 수도 있고 다를 수도 있다.
    //matrix.m00(10)은 처음 시작할 때 0부터 시작하지만 matrix.m01(11)은 스크롤 방향에 따라서 값이 달라진다.
    //Vertical기준이고 터치를 밑에서 위로 올릴 경우 matrix.m01(11)의 값은 matrix.m00(10) + (visibleCount - 1)이다.(-1을 하는 이유는 0부터 시작하기 때문)
    //Vertical기준이고 터치를 위에서 밑으로 내릴 경우 matrix.m01(11)의 값은 matrix.m00(10)과 동일하다.
    //값이 다른 이유는 vertical에서 밑에서 위로 올릴 경우에는 visibleCount -1 의 다음 값들을 출력해야되기 때문이고 위에서 밑으로 내릴 경우에는 최상단의 이전 값을 찾아서 출력하면 되기 때문이다.
    //간단히 설명하면 위로 올라갈 때는 이전 라인 값(현재 라인 -1)을 출력하면 되고 내려갈 때는 visibleCount -1의 다음 값((visibleCount -1) + 1)을 출력하면 된다.
    private Matrix4x4 GetDragMatrix(CPoint curField)
    {
        Matrix4x4 matrix = Matrix4x4.zero;
        CPoint startDataIdx = GetStartDataIndex(curField);
        
        curField = GetDecideFieldIndex(curField);

        matrix.m00 = curField.x;//필드의 시작인덱스.
        matrix.m01 = startDataIdx.x;//해당 필드에 해당되는 필드 인덱스.
        matrix.m02 = mGIOperator.GetItemCountInOwnLine();//해당 필드(한 라인)에 출력되는 최대수.

        matrix.m10 = curField.y;//필드의 시작인덱스.
        matrix.m11 = startDataIdx.y;//해당 필드에 해당되는 필드 인덱스.
        matrix.m12 = mGIOperator.GetItemCountInOwnLine();//해당 필드(한 라인)에 출력되는 최대수.

        //스크롤 할 때 움직이는 범위를 계산.
        matrix.m30 = (mGrid.cellWidth * ((mIsMoveLeftToRight == MoveState.None) ? 0 :
                                        (mIsMoveLeftToRight == MoveState.Normal_Up || mIsMoveLeftToRight == MoveState.Moment_Up) ? -1f :
                                        1f));
        
        //스크롤 할 때 움직이는 범위를 계산.
        matrix.m31 = (mGrid.cellHeight * ((mIsMoveTopToDown == MoveState.None) ? 0 :
                                        (mIsMoveTopToDown == MoveState.Normal_Up || mIsMoveTopToDown == MoveState.Moment_Up) ? 1f :
                                        -1f));
        
        return matrix;
    }
    
    private CPoint GetStartDataIndex(CPoint curField)
    {
        CPoint v = CPoint.zero;

        v.Set(
            (mIsMoveLeftToRight == MoveState.None) ? 0 :
            (mIsMoveLeftToRight == MoveState.Normal_Down) ? (curField.x + (GetVisibleFieldCount() - 1)) ://인덱스를 구하는 것이므로 보여지는 전체 카운트 수에서 -1을 해야 인덱스로 나온다.
            curField.x,//NORMAL_UP, MOMENT_UP, MOMENT_DOWN
            (mIsMoveTopToDown == MoveState.None) ? 0 :
            (mIsMoveTopToDown == MoveState.Normal_Down) ? (curField.y + (GetVisibleFieldCount() - 1)) ://인덱스를 구하는 것이므로 보여지는 전체 카운트 수에서 -1을 해야 인덱스로 나온다.
            curField.y);//NORMAL_UP, MOMENT_UP, MOMENT_DOWN
        
        return v;
    }

    private int GetLastViewStartIndex()
    {
        return (mLastFieldIndex < GetVisibleFieldCount()) ? 0 : mLastFieldIndex - GetVisibleFieldCount();
    }

    public CPoint GetCurrentField()
    {
        Vector3 curPosition = GetPosition();
        CPoint curField = CPoint.zero;
        int lastViewStartIndex = GetLastViewStartIndex();

        switch (movement)
        {
            case Movement.Horizontal:
                curField.Set(curPosition.x / mGrid.cellWidth, 0);

                curField.Set(
                    (curField.x < 0) ? 0 :
                    (curField.x >= lastViewStartIndex) ?
                    lastViewStartIndex :
                    curField.x, //x
                    0 //y
                    );
                break;

            case Movement.Vertical:
                curField.Set(0, curPosition.y / mGrid.cellHeight);
                curField.Set(
                    0, //x
                    (curField.y < 0) ? 0 ://y
                    (curField.y >= lastViewStartIndex) ?
                    lastViewStartIndex :
                    curField.y
                    );
                break;
        }

        return curField;
    }

    private CPoint GetDecideFieldIndex(CPoint curFiled)
    {
        switch (movement)
        {
            case Movement.Horizontal:
                curFiled.y = GenericInventory.NONE_LINE;
                break;

            case Movement.Vertical:
                curFiled.x = GenericInventory.NONE_LINE;
                break;
        }

        return curFiled;
    }

    public int LastFieldIndex
    {
        get
        {
            return mLastFieldIndex;
        }
        set
        {
            mLastFieldIndex = value;
        }
    }

    public int RemainLastItemCount
    {
        get
        {
            return mRemainLastItemCount;
        }
        set
        {
            mRemainLastItemCount = value;
        }
    }

    public int GetVisibleFieldCount()
    {
        return mGIOperator.GetVisibleFieldCount();
    }

    //CustomGrid의 위치가 TopLeft일 경우를 기준으로 하여 만듬. 그 이외의 좌표에서는 작업이 안됨.
    public virtual Bounds CalculateBounds(int fieldCount)
    {
        Bounds bounds = default(Bounds);

        if (mTrans == null) return bounds;
        
        Transform gridTrans = mGrid.transform;
        Vector3 bottomRight = gridTrans.localPosition;

        switch (movement)
        {
            case Movement.Horizontal:
                bottomRight.x = gridTrans.localPosition.x + (mGrid.cellWidth * (fieldCount/* + 1*/));
                bottomRight.y = -gridTrans.localPosition.y;
                bottomRight.z = gridTrans.localPosition.z;
                break;

            case Movement.Vertical:
                bottomRight.x = -gridTrans.localPosition.x;
                bottomRight.y = gridTrans.localPosition.y - (mGrid.cellHeight * (fieldCount/* + 1*/));
                bottomRight.z = gridTrans.localPosition.z;
                break;
        }
        
        //두 번쨰 인자에 zero가 아니라 one을 대입했는데 이는 패딩효과를 주기 위함.
        bounds = new Bounds(gridTrans.localPosition, Vector3.one);
        //TopLeft(transform.localPosition) ~ bottomRight까지의 영역을 설정한다.(패딩효과는 자동계산이 된다.)
        bounds.Encapsulate(bottomRight);
        
        return bounds;
    }

    /*스크롤뷰의 Movement에 의해서 가져오는 값이 다름.
          Movement.Horizontal의 경우 스크롤 뷰의 패널의 Size의 x값을,
          Movement.Vertical의 경우 스크롤 뷰의 패널의 Size의 y값을 가져옴.*/
    public int GetVisibleItemFieldCount()
    {
        float value = -1;//가려지는 것과 관계없이 보이는 라인 갯수.
        
        switch (movement)
        {
            case Movement.Horizontal:
                value = mPanel.GetViewSize().x / mGrid.cellWidth;
                break;

            case Movement.Vertical:
                value = mPanel.GetViewSize().y / mGrid.cellHeight;
                break;
        }
        
        //여기서 +1은 예비용으로 미리 만들어둬야되는 아이템들을 의미한다.
        return Mathf.CeilToInt(value) + 1;
    }

    public int GetItemCountInOwnLine()
    {
        float value = -1;//가려지는 것과 관계없이 보이는 라인 갯수.

        switch (movement)
        {
            case Movement.Horizontal:
                value = mPanel.GetViewSize().y / mGrid.cellHeight;
                break;

            case Movement.Vertical:
                value = mPanel.GetViewSize().x / mGrid.cellWidth;
                break;
        }
        
        //필드 카운트를 구하는 함수(GetVisibleItemFieldCount)의 마지막 리턴에는 +1을 하지만 한 라인의 아이템 개수를 구하는 이 함수에서는
        //+1을 하지 않는다.!!! 왜냐하면 한 라인의 아이템 개수는 예비용이 필요없으며 무조건 유저의 눈에 다 보여야되기 때문이다.
        return Mathf.CeilToInt(value);
    }
    
    public Vector3 GetPosition()
    {
        Vector3 p = Vector3.zero;
        
        switch (movement)
        {
            //행 스크롤하는 기능 중 오브젝트가 오른쪽에서 왼쪽으로 이동할 때 음수가 된다.
            //- Vertical의 기능을 공유할 경우 오른쪽에서 왼쪽으로 가는 기능은 Down 기능에 해당된다. 문제는 Down 기능을 사용할 때 스크롤 오브젝트가 양수가 된다.
            //- 따라서 좌표값을 맞춰주는 작업을 해야한다.
            //Mathf.Abs 함수를 사용해서 처리할 수도 있으나 정작 음수로 판별해야되는 작업을 해야될 경우 판별할 수가 없게 된다.(오른쪽에서 왼쪽으로 갈 때)
            //따라서 스크롤뷰하면 항상 따라다니는 패널의 offset 변수를 이용하기로 했다.
            //이 변수의 경우에는 오른쪽에서 왼쪽으로 갈 때는 양수, 왼쪽에서 오른쪽으로 갈 때는 음수로 수렴된다.
            case Movement.Horizontal:
                p = mPanel.clipOffset;
                break;

            case Movement.Vertical:
                p = mTrans.localPosition;
                break;
        }

        return p;
    }

    public void Resetting()
    {
        mCurField = CPoint.zero;
        
        MoveRelative(-transform.localPosition);

        Stop();
    }

#if UNITY_EDITOR

    /// <summary>
    /// Draw a visible orange outline of the bounds.
    /// </summary>
    /// 
    private UIWidget mDragingWidget;
    
    void OnDrawGizmos()
    {
        if (mPanel != null)
        {
            if (!Application.isPlaying) mCalculatedBounds = false;
            Bounds b = bounds;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.4f, 0f);
            Gizmos.DrawWireCube(new Vector3(b.center.x, b.center.y, b.min.z), new Vector3(b.size.x, b.size.y, 0f));

            if (!Application.isPlaying)
            {
                if (mDragingWidget == null)
                {
                    Transform tr = transform.parent.Find("DragingWidget");

                    if (tr != null)
                    {
                        mDragingWidget = tr.gameObject.GetComponent<UIWidget>();
                    }
                }

                transform.localPosition = Vector3.zero;
            }
        }
    }
#endif
}
