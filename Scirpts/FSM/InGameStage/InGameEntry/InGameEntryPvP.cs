using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameEntryPvP : InGameEntry
{
    public override void Initailize(InGameView inGameView)
    {
        base.Initailize(inGameView);

        SoundManager.PlayBGM(SoundManager.BGM.InGame_Arena_0, 0.5f);
    }

    protected override void SummonMobs()
    {
        List<List<FormationField>> list = new List<List<FormationField>>();
        FormationField field = null;
        GameObject moveObj = null;

        List<List<InformationCharacter>> pvpEnemys = InGameManager.Instance.PVPEnemys;

        for (int i = 0; i < pvpEnemys.Count; ++i)
        {
            list.Add(new List<FormationField>());

            moveObj = mInGameView.CreateMove(false, i);

            for (int j = 0; j < pvpEnemys[i].Count; ++j)
            {
                if (pvpEnemys[i][j] != null)
                {
                    field = SummonCharacter(
                        pvpEnemys[i][j],
                        moveObj.transform,
                        ePlayType.Enemy);

                    list[i].Add(field);
                }
            }

            SetFormationBuff(list[i], pvpEnemys[i]);

            moveObj.SetActive(false);
        }

        InGameManager.Instance.StageMobs = list;
    }
}
