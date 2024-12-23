using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class RuneCreate : Rune
{
    public override string Name { 
        get { return rarity.ToString() + " Create"; } 
        set => throw new System.InvalidOperationException("You can not rename the Rune of Creation!"); 
        }

    public override CastingResources CastDefaults
    { get{
        CastingResources res = new CastingResources
        {
            { "CASTER", CastParam.ECastParamType.TARGET },
            { "POSITION", CastParam.ECastParamType.VECTOR2 },
            { "PACKED_OBJECT", CastParam.ECastParamType.STRING }
        };
        return res;
    } }
    public override CastingResources CastRequirements
    { get{
        CastingResources res = new CastingResources
        {
            { "CASTER", CastParam.ECastParamType.TARGET },
            { "POSITION", CastParam.ECastParamType.VECTOR2 },
            { "PACKED_OBJECT", CastParam.ECastParamType.STRING }
        };
        return res;
    } }
    public override CastingResources CastReturns
    { get{
        CastingResources res = new CastingResources
        {
            { "OBJECT", CastParam.ECastParamType.NODE2D }
        };
        return res;
    } }
    public override uint CastingTime
    {get{ return 500 - (uint)rarity*(uint)rarity*15; }}
    public override uint Cooldown
    {get{ return 500 - (uint)rarity*(uint)rarity*15; }}
    public override int Mana
    {get{ return 50 - (int)rarity*5; }}
    public override string Category { get => "Create Rune"; }
    public override Texture2D Portrait { 
        get { return ResourceLoader.Load<Texture2D>("res://Sprites/Runes/Wave.png"); }
    }

    public override async Task<CastingResources> Cast(CastingResources res)
    {
        await Task.Delay((int)this.CastingTime);
        res.Merge(SigilResources);

        Node2D spawn;
        unsafe {
            CastParam param = new CastParam("OBJECT", CastParam.ECastParamType.NODE2D);
            if(res.ContainsKey(param)){
                spawn = (Node2D)res[param];
            }
            else{ 
                param.Refactor("PACKED_OBJECT", CastParam.ECastParamType.STRING);
                spawn = (Node2D)ResourceLoader.Load<PackedScene>((String)res[param]).Instantiate();
            }
            MainScene.Node.AddChild((Node2D) spawn);
            spawn.Position = (Vector2)res[param.Refactor("POSITION", CastParam.ECastParamType.VECTOR2)];

            if(res.ContainsKey(param.Refactor("ROTATION", CastParam.ECastParamType.VECTOR2))){
                spawn.Rotation = (float)res[param];
            }

            if(res.ContainsKey(param.Refactor("ANGULAR_", CastParam.ECastParamType.VECTOR2))){
                spawn.Rotation = (float)res[param];
            }

            res.Add<Node2D>("OBJECT", CastParam.ECastParamType.NODE2D, spawn);            
        }


        
        return res;
    }
}
