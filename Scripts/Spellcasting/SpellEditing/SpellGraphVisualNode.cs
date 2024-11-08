using Godot;
using System;
using System.Collections.Generic;
using System.Data;

namespace SpellEditing
{
/// <summary>
/// Displayer of Plotables - Sigils and Runes - on a spell graph.
/// </summary>

public partial class SpellGraphVisualNode : TextureRect
{
	public List<SpellGraphVisualArc> arcs = new List<SpellGraphVisualArc>();


	[Export] public Label nameLabel; 

	public new Vector2 Position
	{
		get => base.Position + GetRect().Size/2;
		set => base.Position = value - GetRect().Size/2;
		
	}

	private IGraphDeployable deployable;
	public IGraphDeployable Deployable 
	{
		get { return deployable; }
		set {
			deployable = value;
			UpdateVisuals();
		}
	}

	public SpellGraphVisualNode()
	{
		
	}

	private void InitNameLabel()
	{	
		nameLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
			LabelSettings = new LabelSettings{ FontSize = 10 }
        };
		AddChild(nameLabel);
		nameLabel.Position = new Vector2(-135/3, 50);
		nameLabel.Size = new Vector2I(135, 40);
	}

	public void UpdateVisuals()
	{
		this.Modulate = Rune.ColorByRarity(((Rune)deployable).rarity);
		this.Texture = deployable.Portrait;
		if(deployable is CharacterTextRune) { 
			if(this.nameLabel == null) InitNameLabel();
			this.nameLabel.Text = ((Rune)deployable).Name; 
		}
	}
	public void UpdateArcPosition() { foreach(SpellGraphVisualArc arc in arcs) arc.UpdatePosition(); }

	/// <summary>
	/// Finds arc in nodes
	/// </summary>
	/// <param name="node"> The node to check connection</param>
	/// <returns>An arc between this and node, if it exists. </returns>
	private SpellGraphVisualArc SearchConnectionArcFor(SpellGraphVisualNode node)
	{
		foreach(SpellGraphVisualArc arc in arcs)
		{
			if(arc.Source == node || arc.Target == node) return arc;
		}
		return null;
	}

	private SpellGraphVisualArc CreateSafeArcTowards(SpellGraphVisualNode node)
	{
		SpellGraphVisualArc arc = SearchConnectionArcFor(node);
		if(arc == null) { 
			arc = new SpellGraphVisualArc();
			arc.Source = this;
			arc.Target = node;
			this.arcs.Add(arc);
			node.arcs.Add(arc);
		}
		return arc;

	}
	public SpellGraphVisualArc ConnectTo(SpellGraphVisualNode node)
	{
		SpellGraphVisualArc arc = CreateSafeArcTowards(node);
		arc.Source = this;
		arc.Target = node;
		arc.UpdatePosition();
		return arc;
	}

	public SpellGraphVisualArc ConnectFrom(SpellGraphVisualNode node)
	{
		SpellGraphVisualArc arc = CreateSafeArcTowards(node);
		arc.Source = node;
		arc.Target = this;
		arc.UpdatePosition();
		return arc;
	}

	public void DisconnectFrom(SpellGraphVisualNode node)
	{
		var arc = SearchConnectionArcFor(node);
		if(arc == null) return;
		arc.Source = null;
		arc.Target = null;
		arc.Source.arcs.Remove(arc);
		arc.Target.arcs.Remove(arc);
	}

	public void DisconnectFromAll()
	{
		foreach(SpellGraphVisualArc arc in arcs)
		{
			(arc.Source==this?arc.Target:arc.Source).arcs.Remove(arc);
			arc.Source = null;	
			arc.Target = null;
		}
		arcs.Clear();
	}
}

}