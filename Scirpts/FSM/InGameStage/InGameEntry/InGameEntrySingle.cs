using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameEntrySingle : InGameEntry
{
    public override void Initailize(InGameView inGameView)
    {
        base.Initailize(inGameView);

        SoundManager.PlayBGM(SoundManager.BGM.InGame_0, 0.5f);
    }

    protected override void SummonMobs()
    {
        List<List<FormationField>> list = new List<List<FormationField>>();
        FormationField field = null;
        GameObject moveObj = null;

        DataStage ds = InGameManager.Instance.GetStage();
        DataWaveEnemy[] dwes = new DataWaveEnemy[ds.MaxWave];

        int[] formations = null;

        GameObject fieldObj = null;

        //웨이브마다 등장하는 적들을 초기화함.(dwes 는 웨이브마다 등장하는 적들을 설정하기 위한 데이터)
        //즉 1 웨이브 = dwes[0], 2 웨이브 = dwes[1]이라고 보면 됨.
        for (int i = 0; i < dwes.Length; ++i)
        {
            moveObj = mInGameView.CreateMove(false, i);

            list.Add(new List<FormationField>());

            dwes[i] = GameTableContainer.Instance.GetDataWaveEnemy(ds.EnemyFormationIndex + i);

            formations = dwes[i].GetFormations(BaseData.NONE_VALUE);

            //해당 웨이브에서 각 진형마다 등장하는 적들을 초기화함.
            //하나의 진형공간에는 적이 여러명 등장할 수 있음.
            for (int j = 0; j < formations.Length; ++j)
            {
                if (formations[j] != BaseData.NONE_VALUE && formations[j] != 0)
                {
                    fieldObj = GameManager.Summon(DataPrefab.eIndex.FormationField);
                    fieldObj.transform.SetParent(moveObj.transform);
                    fieldObj.transform.localEulerAngles = Vector3.up * 180f;
#if UNITY_EDITOR
                    fieldObj.name = "Formation_" + j;
#endif
                    field = fieldObj.GetComponent<FormationField>();
                    field.Summon(dwes[i], j);

                    list[i].Add(field);
                }
            }

            moveObj.SetActive(false);
        }

        //mInGameView.MoveMob.localScale = new Vector3(1f, 1f, -1f);
        InGameManager.Instance.StageMobs = list;
    }

    //protected override void Start()
    //{
    //    base.Start();
    //}

    //protected override void Update()
    //{
    //    base.Update();
    //}
}
