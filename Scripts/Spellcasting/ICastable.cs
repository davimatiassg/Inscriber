using System.Threading.Tasks;

public interface ICastable 
{
    public CastingResources CastRequirements { get; }
    public CastingResources CastReturns { get; }
    public Task<CastingResources> Cast(CastingResources res);
    public float Cooldown { get; protected set;}
    public int Mana { get; protected set;}
    public float CastingTime { get; protected set;}
}


public class Castable : ICastable
{
    public CastingResources CastRequirements => new CastingResources();

    public CastingResources CastReturns => new CastingResources();
    float ICastable.Cooldown { get { return 0; }  set { return; } }
    int ICastable.Mana { get { return 0; }  set { return; } }
    float ICastable.CastingTime { get { return 0; }  set { return; } }

    public async Task<CastingResources> Cast(CastingResources res){ return res; }
}