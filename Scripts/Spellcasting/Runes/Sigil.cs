using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Sigil : CastParam
{
    public Rune.ERuneRarity rarity;
    public IntPtr val;
    public Sigil(string n, ECastParamType p, IntPtr ptr) : base(n, p)
    { this.val = ptr; }

    public CastingResources AsCastingResource
    {
        get{ CastingResources c = new CastingResources(); c.Add(this, val); return c; }
    }
}
