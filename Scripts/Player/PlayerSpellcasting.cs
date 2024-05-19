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
		Vector2 casterPosition = this.Position;
		Vector2 position = GetViewport().GetMousePosition() + GetCanvasTransform().Origin;
		Vector2 direction = (position - casterPosition).Normalized();

		CastingResources res = new CastingResources();

		res.Add("CASTER", 			CastParam.ECastParamType.NODE2D, 		ref Caster);
		res.Add("CASTING_POSITION", CastParam.ECastParamType.VECTOR2,		ref casterPosition);
		res.Add("POSITION", 		CastParam.ECastParamType.VECTOR2, 		ref position);
		res.Add("DIRECTION", 		CastParam.ECastParamType.VECTOR2, 		ref direction);  
		
		return res;
    }


}
