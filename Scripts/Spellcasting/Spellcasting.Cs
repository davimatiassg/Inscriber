using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract partial class Spellcasting : Node2D
{
    public Node2D Caster;
    public enum ECastState {Ready, Casting, Cooldown};
	[Export] private int mana;
    [Export] private int manaMax;
    [Export] public ECastState state;
    [Export] private int selectedCastableIndex;  
    public List<(ICastable, bool)> castables = new List<(ICastable, bool)>();
    public int Mana { get { return mana; } set { mana = value; } }
    public int ManaMax { get { return manaMax; } set { manaMax = value; } }
    public int SelectedCastableIndex
    {
        get { 
            return selectedCastableIndex > 0 ? selectedCastableIndex : 0; 
        }
        set {
            int count = castables.Count;
            if(count == 0 || value < 0) { selectedCastableIndex = int.MinValue; return; }
            while (value > count) {value -= count;}
            selectedCastableIndex = value;
        }
         
    }

    public abstract CastingResources GenerateResources();
    private async Task CastingPipeline(int index)
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
        CastingPipeline(index).Start();
    }
    public void PrepareCast() { if(selectedCastableIndex >= 0) PrepareCast(selectedCastableIndex); }

}