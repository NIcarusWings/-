using UnityEngine;
using System.Collections;

public class OrganizationView : CView
{
    [SerializeField]
    private GameObject m_3DView;
    
    [SerializeField]
    private Transform m_CameraTr;

    private Vector3 mPrevMousePos = Vector3.zero;
    private Vector3 mDistance = Vector3.zero;
    
    private void Awake()
    {
        
    }

    protected override void OnEnable()
    {
        base.OnEnable();


    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mPrevMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            mDistance.Set(
                Input.mousePosition.x - mPrevMousePos.x,
                Input.mousePosition.y - mPrevMousePos.y,
                0f);
            
            if (mDistance.x != 0 || mDistance.y != 0)
            {
                Vector3 move = m_CameraTr.position;

                mDistance.Set(mDistance.x * 0.1f, mDistance.y * 0.1f, 0f);

                if ((mDistance.x < 0 && m_CameraTr.position.z > -5f) ||
                    (mDistance.x > 0 && m_CameraTr.position.z < 5f))
                {
                    move.z += mDistance.x;

                    if (move.z > 5f)
                    {
                        move.z = 5f;
                    }
                    else
                    {
                        if (move.z < -5f)
                        {
                            move.z = -5f;
                        }
                    }
                }

                if ((mDistance.y < 0 && m_CameraTr.position.y > 5f) ||
                    (mDistance.y > 0 && m_CameraTr.position.y < 15f))
                {
                    move.y += mDistance.y;

                    if (move.y < 5f)
                    {
                        move.y = 5f;
                    }
                    else
                    {
                        if (move.y > 15f)
                        {
                            move.y = 15f;
                        }
                    }
                }
                
                m_CameraTr.position = move;

                mPrevMousePos = Input.mousePosition;
            }
        }
    }
}
