using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract partial class Rune : Resource, ICastable
{
    public enum ERuneRarity : int { Dull = 0, Common, Uncommon, Rare, Mithic, Unique}
    public ERuneRarity rarity;
    public List<Sigil> sigils = new List<Sigil>();
    public abstract CastingResources CastRequirements
    { get; }
    public abstract CastingResources CastReturns
    { get; }
    public abstract uint Cooldown
    { get; }
    public abstract int Mana
    { get; }
    public abstract uint CastingTime
    { get; }
    public CastingResources GatherResources()
    {
        return CastingResources.Merge(sigils.Select<Sigil, CastingResources>(i => i.AsCastingResource).ToArray());
    }
    public abstract Task<CastingResources> Cast(CastingResources res);
}
