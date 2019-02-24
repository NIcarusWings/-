using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAI
{
    public UnitAI(GameUnitObject from)
    {
        mFrom = from;
    }

    //public GameUnitObject GetTarget()
    //{

    //}

    public GameUnitObject NormalTarget//일반 공격할 때 사용되는 타켓임.
    {
        get
        {
            if (mFrom.IsRetire() || mFrom.PlayType == ePlayType.None)
            {
                mTo = null;
            }
            else
            {
                if (mTo == null)
                {
                    GameUnitObject target = null;//result
                    List<FormationField> targets = null;

                    switch (mFrom.PlayType)
                    {
                        case ePlayType.Player:
                        case ePlayType.NPC:
                            targets = InGameManager.Instance.GetWaveMobs();
                            break;

                        case ePlayType.Enemy:
                            targets = InGameManager.Instance.GetPlatoonPlayers();
                            break;
                    }

                    if (targets != null)
                    {
                        NPoint selfPoint = mFrom.GetFormationPoint();

                        switch (selfPoint.x)
                        {
                            case 0:
                                target = NormalTargetOneRow(targets);
                                break;

                            case 1:
                                target = NormalTargetTwoRow(targets);
                                break;

                            case 2:
                                target = NormalTargetThreeRow(targets);
                                break;
                        }

                        mTo = target;

                        //적들을 모두 격파하고 나면 더 이상 타켓이 존재하지 않게 되므로 체크를 해줘야함.
                        if (mTo != null)
                        {
                            MechBody mechBody = mFrom.GetMechBody();

                            if (mechBody != null)
                            {
                                mechBody.SetRot();
                            }

                            mTo.AddTargetOn(mFrom);
                        }
                    }
                }
            }

            return mTo;
        }
        set
        {
            mTo = value;
        }
    }

    private GameUnitObject NormalTargetOneRow(List<FormationField> targets)
    {
        GameUnitObject result = null;

        for (int i = 0; i < GameManager.FSquare; ++i)
        {
            result = GetNormalTargetBy(targets, i);

            if (result != null)
            {
                break;
            }
        }

        return result;
    }

    private GameUnitObject NormalTargetTwoRow(List<FormationField> targets)
    {
        GameUnitObject result = GetNormalTargetBy(targets, 1);

        if (result == null)
        {
            result = GetNormalTargetBy(targets, 0);

            if (result == null)
            {
                result = GetNormalTargetBy(targets, 2);
            }
        }

        return result;
    }

    private GameUnitObject NormalTargetThreeRow(List<FormationField> targets)
    {
        GameUnitObject result = null;

        for (int i = GameManager.FSquare - 1; i >= 0; --i)
        {
            result = GetNormalTargetBy(targets, i);

            if (result != null)
            {
                break;
            }
        }

        return result;
    }

    private GameUnitObject GetNormalTargetBy(List<FormationField> targets, int rowIndex)
    {
        GameUnitObject result = null;
        NPoint prevPoint = new NPoint(9999, 9999);
        NPoint targetPoint;

        for (int i = 0; i < targets.Count; ++i)
        {
            if (!targets[i].IsRetire() &&
                targets[i].CurUnit != null &&
                !targets[i].CurUnit.IsRetire())
            {
                targetPoint = targets[i].CurUnit.GetFormationPoint();

                if (rowIndex == targetPoint.x)
                {
                    //더 작은 값이 나올 수도 있는 것과 캐릭터마다 순서를 보장하지 않기 때문에 정렬 순서가 맞지 않음.
                    //그렇기에 break를 할 수가 없음
                    if (targetPoint.y <= prevPoint.y)
                    {
                        prevPoint = targetPoint;
                        result = targets[i].CurUnit;
                    }
                }
            }
        }

        return result;
    }

    private GameUnitObject mFrom;
    private GameUnitObject mTo;

    public enum eTargetAI { NormalTarget }
}
