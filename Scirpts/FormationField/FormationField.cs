using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//GameUnitObject를 FormationField에서 소환하는 건 맞는데 FormationField에 GameUnitObject를 종속을 시켜버린 건 잘못함...
public class FormationField : MonoBehaviour
{
    public static bool IsFormationIn(int currentFormationValue, int targetFormationValue, int baseFormationValue, int[] formations)
    {
        bool result = false;

        int extendCenterValue = Mathf.CeilToInt(Mathf.Pow(GameManager.FSquare, 4) * 0.5f);
        int extendStartValue = GetExtendValueBy(
            extendCenterValue,
            new NPoint(-(int)(GameManager.FSquare * 0.5f), -(int)(GameManager.FSquare * 0.5f)),
            (int)Mathf.Pow(GameManager.FSquare, 2));

        int[] extendFormations = ConvertFormationsToExtendFormations(currentFormationValue, baseFormationValue, extendStartValue, formations);
        int extendTargetFormation = ConvertDataFormationToExtendFormation(targetFormationValue, GameManager.FSquare, extendStartValue);

        for (int i = 0; i < extendFormations.Length; ++i)
        {
            if (extendFormations[i] == extendTargetFormation)
            {
                result = true;
                break;
            }
        }

        return result;
    }

    private static int[] ConvertFormationsToExtendFormations(int currentFormationValue, int baseFormationValue, int extendStartValue, int[] formations)
    {
        int[] extendFormationBuffs = new int[formations.Length];
        int extendCurrentFormationValue = ConvertDataFormationToExtendFormation(currentFormationValue, GameManager.FSquare, extendStartValue);
        int extendBaseFormationValue = ConvertDataFormationToExtendFormation(baseFormationValue, GameManager.FSquare, extendStartValue);
        int distance = extendCurrentFormationValue - extendBaseFormationValue;

        for (int i = 0; i < formations.Length; ++i)
        {
            extendFormationBuffs[i] = ConvertDataFormationToExtendFormation(formations[i], GameManager.FSquare, extendStartValue);
            extendFormationBuffs[i] += distance + 1;//+1을 더하는 이유는 테이블에 기재된 데이터가 0부터 시작하기 때문.
        }

        return extendFormationBuffs;
    }

    private static int ConvertDataFormationToExtendFormation(int dataFormationValue, int squareValue, int extendStartValue)
    {
        int quotient = BaseData.NONE_VALUE;
        int dsqr = squareValue * squareValue;//double square

        quotient = ((dataFormationValue - 1) / squareValue);

        return (quotient * dsqr) +
                ((dataFormationValue - 1) - (quotient * squareValue)) +
                extendStartValue;
    }

    private static int GetExtendValueBy(int target, NPoint distance, int squareValue)
    {
        int result = 0;
        int y = 0;
        int loop = Mathf.Abs(distance.y);

        for (int i = 0; i < loop; ++i)
        {
            y += distance.y * squareValue;
        }

        result = target + y + distance.x;

        return result;
    }

    private List<GameUnitObject> mUnits;
    [HideInInspector] public int CurSortie = 0;
    [HideInInspector] public int FormationIndex;
}