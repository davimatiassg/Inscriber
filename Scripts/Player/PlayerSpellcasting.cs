using Godot;
using System;

public partial class PlayerSpellcasting : Spellcasting
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Mana = this.ManaMax;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
    public override CastingResources GenerateResources()
    {
		Object casterPosition = this.Position;
		Object position = GetViewport().GetMousePosition() + GetCanvasTransform().Origin;
		Object direction = ((Vector2)position - (Vector2)casterPosition).Normalized();

		CastingResources res = new CastingResources
        {
            { "CASTER", CastParam.ECastParamType.NODE2D, Caster },
            { "CASTING_POSITION", CastParam.ECastParamType.VECTOR2, casterPosition },
            { "POSITION", CastParam.ECastParamType.VECTOR2, position },
            { "DIRECTION", CastParam.ECastParamType.VECTOR2, direction }
        };		
		return res;
    }


}
