using System.Collections.Generic;

public abstract class InGameFSM : NFSM
{
	public virtual void Initailize(InGameView inGameView)
    {
        mInGameView = inGameView;
    }

    public InGameView GetView()
    {
        return mInGameView;
    }

    public int GetMaxWave()
    {
        return mInGameView.GetMaxWave();
    }

    public List<FormationField> pacs
    {
        get
        {
            return mInGameView.Pacs;
        }
    }

    public List<FormationField> WaveMobs
    {
        get
        {
            return mInGameView.WaveMobs;
        }
    }

    public int CurPlatoonIndex
    {
        get
        {
            return mInGameView.CurPlatoonIndex;
        }
        set
        {
            mInGameView.CurPlatoonIndex = value;
        }
    }

    public int CurWaveIndex
    {
        get
        {
            return mInGameView.CurWaveIndex;
        }
        set
        {
            mInGameView.CurWaveIndex = value;
        }
    }

    public virtual int GetGameMode()
    {
        return BaseData.NONE_VALUE;
    }

    public virtual void OnNoticeByServer(){; }
	public virtual void OnNoticePVPStartNextTag() {; }

	protected InGameView mInGameView;

}
