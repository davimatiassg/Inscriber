using System.Threading.Tasks;
using Godot;
using SpellEditing;

public interface ICastable : IMagicSymbol
{
    public CastingResources CastDefaults { get; }
    public CastingResources CastRequirements { get; }
    public CastingResources CastReturns { get; }
    public Task<CastingResources> Cast(CastingResources res);
    public uint CastingTime { get; }
    public uint Cooldown { get; }
    public int Mana { get; }
}

/*
public class CastablePlaceHolder : ICastable
{

    public Texture2D Portrait => throw new System.NotImplementedException();

    public string Category => throw new System.NotImplementedException();

    public string Description => throw new System.NotImplementedException();

    public Color Color => throw new System.NotImplementedException();

    public CastingResources CastRequirements => new CastingResources();
    public CastingResources CastReturns => new CastingResources();
    public uint Cooldown { get { return 0; } }
    public int Mana { get { return 0; }  }
    public uint CastingTime { get { return 0; } }

    public async Task<CastingResources> Cast(CastingResources res){ return res; }
}*/