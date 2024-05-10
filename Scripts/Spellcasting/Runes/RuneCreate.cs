using Godot;
using System;
using System.Threading.Tasks;

//ERuneRarity { Dull = 0, Common, Uncommon, Rare, Mithic, Unique}
public partial class RuneCreate : Rune, ICastable
{
    public override CastingResources CastRequirements
    { get{
        CastingResources res = new CastingResources();
        res.Add(new CastParam("POSITION", CastParam.ECastParamType.VECTOR2), IntPtr.Zero);
        res.Add(new CastParam("ROTATION", CastParam.ECastParamType.FLOAT), IntPtr.Zero);
        res.Add(new CastParam("OBJECT", CastParam.ECastParamType.GAMEOBJECT), IntPtr.Zero);
        return res;
    } }
    public override CastingResources CastReturns
    { get{
        CastingResources res = new CastingResources();
        res.Add(new CastParam("OBJECT", CastParam.ECastParamType.GAMEOBJECT), IntPtr.Zero);
        return res;
    } }
    public override uint CastingTime
    {get{ return 500 - (uint)rarity*(uint)rarity*15; }}
    public override uint Cooldown
    {get{ return 500 - (uint)rarity*(uint)rarity*15; }}
    public override int Mana
    {get{ return 50 - (int)rarity*5; }}
    public override async Task<CastingResources> Cast(CastingResources res)
    {
        await Task.Delay((int)this.CastingTime);
        res.Merge(GatherResources());
        
        return;
    }
}
