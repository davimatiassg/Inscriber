using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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

public partial class RandomTestRune : Rune
{

    public RandomTestRune()
    {
        randomFactor = new Random().Next(0, 90);
        var names = new List<String>(DirAccess.Open("res://Sprites/Runes/").GetFiles());
        var nnames = new List<String>();
        foreach(string name in names) { if(!name.Contains(".import")) {nnames.Add(name);}}
        imgpath = nnames[randomFactor%nnames.Count()];
        rarity = (ERuneRarity)(randomFactor%9);
    }
    private string imgpath;
    private int randomFactor;
    public override CastingResources CastRequirements => throw new NotImplementedException();
    public override CastingResources CastReturns => throw new NotImplementedException();
    public override uint Cooldown => throw new NotImplementedException();
    public override int Mana => throw new NotImplementedException();
    public override uint CastingTime => throw new NotImplementedException();
    public override string Category { get => "??? - " + randomFactor; }
    public override string Name { get => Category; set => base.Name = value; }
    public override Texture2D Portrait { 
        get { return (Texture2D)ResourceLoader.Load<Texture2D>("res://Sprites/Runes/" + imgpath); }
    }
    public override Task<CastingResources> Cast(CastingResources res) => throw new NotImplementedException();
}