using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract partial class Spellcasting : Node2D
{
    public enum ECastState {Ready, Casting, Cooldown};
	[Export] private int mana;
    [Export] private int manaMax;
    [Export] public ECastState state;
    public List<(ICastable, bool)> castables;
    public int Mana { get { return mana; } set { mana = value; } }
    public int ManaMax { get { return manaMax; } set { manaMax = value; } }

    public abstract CastingResources GenerateResources();

    public async void CastingPipeline(int index)
    {
        CastingResources res = GenerateResources();
        Task casting = castables[index].Item1.Cast(res);
        Mana -= castables[index].Item1.Mana;
        state = ECastState.Casting;
        castables[index] = (castables[index].Item1, true);
        await Task.Delay((int)(1000*castables[index].Item1.CastingTime));
        casting.Start();
        state = ECastState.Ready;
        await Task.Delay((int)(1000*castables[index].Item1.Cooldown));
        castables[index] = (castables[index].Item1, false);
    }
    public void PrepareCast(int index)
    {
        if(state != ECastState.Ready) return;
        (ICastable, bool) castSlot = castables[index];
        if(castables[index].Item2) return;
        if(castables[index].Item1.Mana > Mana) return;
        Task t = new Task(() => { CastingPipeline(index); } );
    }


}
