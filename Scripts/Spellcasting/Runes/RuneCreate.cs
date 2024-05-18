using Godot;
using System;
using System.Threading.Tasks;

//ERuneRarity { Dull = 0, Common, Uncommon, Rare, Mithic, Unique}
public partial class RuneCreate : Rune, ICastable
{
    public override CastingResources CastRequirements
    { get{
        CastingResources res = new CastingResources();
        res.Add("CASTER", CastParam.ECastParamType.VECTOR2);
        res.Add("POSITION", CastParam.ECastParamType.VECTOR2);
        res.Add("PACKED_OBJECT", CastParam.ECastParamType.STRING);
        return res;
    } }
    public override CastingResources CastReturns
    { get{
        CastingResources res = new CastingResources();
        res.Add("OBJECT", CastParam.ECastParamType.NODE2D);
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

        Node2D spawn;
        unsafe {
            CastParam param = new CastParam("OBJECT", CastParam.ECastParamType.NODE2D);
            if(res.ContainsKey(param)){
                spawn = *(Node2D*)res[param].ToPointer();
            }
            else{ 
                param.Refactor("PACKED_OBJECT", CastParam.ECastParamType.STRING);
                spawn = (Node2D)ResourceLoader.Load<PackedScene>(*(String*)res[param].ToPointer()).Instantiate();
            }
            MainScene.Node.AddChild(spawn);
            spawn.Position = *(Vector2*)res[param].ToPointer();

            if(res.ContainsKey(param.Refactor("ROTATION", CastParam.ECastParamType.VECTOR2))){
                spawn.Rotation = *(float*)res[param].ToPointer();
            }

            if(res.ContainsKey(param.Refactor("ANGULAR_", CastParam.ECastParamType.VECTOR2))){
                spawn.Rotation = *(float*)res[param].ToPointer();
            }

            res.Add<Node2D>("OBJECT", CastParam.ECastParamType.NODE2D, ref spawn);            
        }


        
        return res;
    }
}
