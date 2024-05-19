using Godot;
using System;

public class SpellManager
{
    public Spell CreateSpell()
    {
        Spell spell = new Spell();
        return spell;
    }

    public void ModifySpell(ref Spell spell)
    {
        //TODO
    }
}
