using Godot;
using System;

public partial class PlayerSpellcasting : Spellcasting
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	//TODO
    public override CastingResources GenerateResources()
    {
        throw new NotImplementedException();
    }
}
