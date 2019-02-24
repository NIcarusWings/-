using UnityEngine;

//UI 아이템의 추상 클래스. 부모로써 정의할 뿐 실제로 사용되지 않음.
public abstract class UIBaseItem : MonoBehaviour
{
    public Vector3 GetPosition(Space space)
    {
        Vector3 p = Vector3.zero;

        switch (space)
        {
            case Space.World:
                p = transform.position;
                break;

            case Space.Self:
                p = transform.localPosition;
                break;
        }

        return p;
    }

    public void SetPosition(Vector3 pos, Space space)
    {
        switch (space)
        {
            case Space.World:
                transform.position = pos;
                break;

            case Space.Self:
                transform.localPosition = pos;
                break;
        }
    }

    public bool IsOpen()
    {
        return gameObject.activeSelf;
    }
    
    public void SetActive(bool value)
    {
        if (gameObject.activeSelf != value)
        {
            gameObject.SetActive(value);
        }
    }
}
