using Godot;
using System;

public partial class SpellcastingBehavior : PhysicalBehavior
{
    [Export] protected Spellcasting spellcastingNode;
	public Spellcasting SpellcastingNode {
        get { return spellcastingNode; }
        set { spellcastingNode = value; value.Owner = this; }        
    }

    public override void _Ready()
    {
        base._Ready();
        if(spellcastingNode != null) { SpellcastingNode = spellcastingNode; }
    }

}
