using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpellEditing;

public abstract partial class Rune : Resource, ICastable, SpellEditing.IPlotable
{
    public enum ERuneRarity : int { Dull = 0, Common, Uncommon, Rare, Mithic, Arcane, Primal, Profane, Divine }
    public ERuneRarity rarity;
    public abstract Texture2D Portrait 
    { get; }
    public virtual string Name 
    {   get { return rarity.ToString() + " ???"; } 
        set => throw new System.InvalidOperationException("You can not rename a unknown rune!"); }
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
    public virtual Color Color { get => ColorByRarity(rarity); }
    public virtual string Category { get => "Unknown Rune";  }

    public CastingResources GatherResources()
    {
        return CastingResources.Merge(sigils.Select<Sigil, CastingResources>(i => i.AsCastingResource).ToArray());
    }
    public abstract Task<CastingResources> Cast(CastingResources res);

    public static Color ColorByRarity(ERuneRarity rarity)
    {
        switch (rarity)
        {
            case ERuneRarity.Divine: 
                return new Color( 1f, 0.781f, 0f, 1f );             //Yellow
            case ERuneRarity.Profane: 
                return new Color( 0.878f, 0.1f,  0.1f, 1f );        //Red
            case ERuneRarity.Arcane: 
                return new Color( 0.1f, 0.1f, 0.878f, 1f );         //Blue
            case ERuneRarity.Primal:
                return new Color( 0.1f, 0.878f, 0.1f, 1f );         //Green
            case ERuneRarity.Mithic:
                return new Color( 0.6f, 0.1f, 0.7f, 1f );           //Purple
            case ERuneRarity.Rare:
                return new Color( 0.039f, 0.628f, 0.628f, 1f );     //Cyan
            case ERuneRarity.Uncommon:
                return new Color( 0.917f, 0.328f, 0.074f, 1f );     //Orange
            case ERuneRarity.Common:
                return new Color( 0.8f, 0.8f, 0.8f, 1f );           //White
            default: //Dull
                return new Color( 0.1f, 0.1f, 0.1f, 1f );           //Black
        }
    }
}
