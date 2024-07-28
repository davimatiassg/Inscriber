using Godot;
using System;
using System.Collections.Generic;

namespace SpellEditing
{
using SpellNode = Spell.Node;
public partial class SpellSlot : TextureRect
{
	public Action getMoved;
	public List<SpellLine> spellLines = new List<SpellLine>();
	public Action<double> transition;
	[Export] public Vector2 finalPosition;
	public float gradValue;
	public SpellNode node;
	private IPlotable plotable;
	public IPlotable Plotable 
	{
		get { return plotable; }
		set {
			plotable = value;
			if(plotable is Rune)
			{
				this.Modulate = Rune.ColorByRarity(((Rune)plotable).rarity);
				this.Texture = plotable.Portrait;
			}
			if(plotable is ICastable)
			{
				node = SpellManager.CreateNodeFromCastable((ICastable)plotable);
			}
		}
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(plotable != null) PlaceSigilSlots();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		transition?.Invoke(delta);
	}

	public bool ProcessTransition(float interpolation)
	{
		if(GlobalPosition.Y == finalPosition.Y) { return true; }
		GlobalPosition = GlobalPosition.CubicInterpolateInTime(finalPosition, finalPosition, finalPosition, interpolation, 0, 0.5f, 1f);
		Scale = Scale.Lerp(Vector2.One*gradValue, interpolation);
		SelfModulate = new Color(gradValue,gradValue,1, gradValue*gradValue);

		return false;
	}

	public void PlaceSigilSlots()
	{
		//
	}

	public void UpdateLinePosition() { foreach(SpellLine line in spellLines) line.UpdatePosition(); }
}

}