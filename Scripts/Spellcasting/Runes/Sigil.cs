using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SpellEditing;

public class Sigil : CastParam, IGraphDeployable
{
    public virtual string Name 
    { get => "Unknown Rune"; set {}  }
    public Rune.ERuneRarity rarity;
    private Texture2D portrait;
    public virtual Texture2D Portrait 
    {
        get => portrait; 
        set => portrait = value;
    }
    public virtual string Category { get => "Unknown Rune";  }
    public Color Color { get => Rune.ColorByRarity(rarity); }
    public IntPtr val;
    public Sigil(string n, ECastParamType p, IntPtr ptr) : base(n, p)
    { this.val = ptr; }

    public CastingResources AsCastingResource
    {
        get{ CastingResources c = new CastingResources(); c.Add(this, val); return c; }
    }

   
}
