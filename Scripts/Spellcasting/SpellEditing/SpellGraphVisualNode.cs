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

	private IGraphDeployable deployable;
	public IGraphDeployable Deployable 
	{
		get { return deployable; }
		set {
			deployable = value;
			UpdateVisuals();
		}
	}

	public void UpdateVisuals()
	{
		this.Modulate = Rune.ColorByRarity(((Rune)deployable).rarity);
		this.Texture = deployable.Portrait;
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
			this.arcs.Add(arc);
			node.arcs.Add(arc);
		}
		return arc;

	}
	public void ConnectTo(SpellGraphVisualNode node)
	{
		SpellGraphVisualArc arc = CreateSafeArcTowards(node);
		arc.Source = this;
		arc.Target = node;
	}

	public void ConnectFrom(SpellGraphVisualNode node)
	{
		SpellGraphVisualArc arc = CreateSafeArcTowards(node);
		arc.Source = node;
		arc.Target = this;
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


public partial class SpellGraphRuneVisualNode : SpellGraphVisualNode
{
	Spell.Node internalSpellNode;
}

}