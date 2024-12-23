using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SpellEditing;

public class Sigil : CastParam, IMagicSymbol
{
    public virtual string Name 
    { get => "Unknown Sigil"; set {}  }
    public Rune.ERuneRarity rarity;
    private Texture2D portrait;

    public Object val;
    public virtual Texture2D Portrait 
    {
        get => portrait; 
        set => portrait = value;
    }
    public virtual string Category { get => "Sigil";  }
    public virtual string Description { get => "No effects"; }
    public Color Color { get => Rune.ColorByRarity(rarity); }
    
    public Sigil(string n, ECastParamType p, IntPtr ptr) : base(n, p)
    { this.val = ptr; }
    public Sigil(string n, ECastParamType p, string s) : base(n, p, s){}
    public CastingResources AsCastingResource { get => new CastingResources{{ this, val }}; }

    public static Texture2D DefaultSigilPortrait(ECastParamType param)
    {
        string textureName;
        switch (param)
        {
            case ECastParamType.TARGET:
                textureName = "Target.png";
                break;
            default:
                textureName = "Unknown.png";
                break;
        }
        return ResourceLoader.Load<Texture2D>("res://Sprites/Sigils/" + textureName);
    }

    public static Texture2D LackingSigilPortrait()
    => ResourceLoader.Load<Texture2D>("res://Sprites/Sigils/Lacking.png");

    public static Texture2D FilledSigilPortrait()
    => ResourceLoader.Load<Texture2D>("res://Sprites/Sigils/Filled.png");
    
}
