using System.Collections.Generic;

public abstract class InGamePlayFSM : NFSM
{
    public virtual void Initailize(InGamePlay inGamePlay)
    {
        mInGamePlay = inGamePlay;
    }

    public override void Pause(bool isPause)
    {
        for (int i = 0; i < Pacs.Count; ++i)
        {
            Pacs[i].Pause(isPause);
        }

        for (int i = 0; i < WaveMobs.Count; ++i)
        {
            WaveMobs[i].Pause(isPause);
        }

        base.Pause(isPause);
    }

    public List<FormationField> Pacs
    {
        get
        {
            return mInGamePlay.pacs;
        }
    }

    public List<FormationField> WaveMobs
    {
        get
        {
            return mInGamePlay.WaveMobs;
        }
    }

    protected int CurPlatoonIndex
    {
        get
        {
            return mInGamePlay.CurPlatoonIndex;
        }
        set
        {
            mInGamePlay.CurPlatoonIndex = value;
        }
    }

    protected int CurWaveIndex
    {
        get
        {
            return mInGamePlay.CurWaveIndex;
        }
        set
        {
            mInGamePlay.CurWaveIndex = value;
        }
    }

    public virtual void OnNoticeByServer(){;}
	public virtual void OnNoticePVPStartNextTag() {; }

	protected InGamePlay mInGamePlay;
}
