using System.Threading.Tasks;

public interface ICastable 
{
    public CastingResources CastRequirements { get; }
    public CastingResources CastReturns { get; }
    public Task<CastingResources> Cast(CastingResources res);
    public uint Cooldown { get; }
    public int Mana { get; }
    public uint CastingTime { get; }
}


public class Castable : ICastable
{
    public CastingResources CastRequirements => new CastingResources();

    public CastingResources CastReturns => new CastingResources();
    public uint Cooldown { get { return 0; } }
    public int Mana { get { return 0; }  }
    public uint CastingTime { get { return 0; } }

    public async Task<CastingResources> Cast(CastingResources res){ return res; }
}