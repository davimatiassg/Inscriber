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
    public override CastingResources GenerateResources()
    {
		Node2D parent = (Node2D)this.GetParent();
		Vector2 casterPosition = this.Position;
		Vector2 position = GetViewport().GetMousePosition() + GetCanvasTransform().Origin;
		Vector2 direction = (position - casterPosition).Normalized();

		CastingResources res = new CastingResources();

		res.Add("CASTER", 			CastParam.ECastParamType.NODE2D, 		ref parent);
		res.Add("CASTING_POSITION", CastParam.ECastParamType.VECTOR2,		ref casterPosition);
		res.Add("POSITION", 		CastParam.ECastParamType.VECTOR2, 		ref position);
		res.Add("DIRECTION", 		CastParam.ECastParamType.VECTOR2, 		ref direction);  
		
		return res;
    }


}
