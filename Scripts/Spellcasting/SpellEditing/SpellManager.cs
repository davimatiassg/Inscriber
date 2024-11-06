using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SpellEditing
{


using SpellNode = ISpellGraph.Node;

/// <summary>
/// Singleton used to alter the spell internally
/// </summary>

public partial class SpellManager
{
	private static Spell currentSpell = new Spell();

	public static Func<ISpellGraph, SpellNode, SpellNode, bool> ConnectionMethod = ConnectWithoutCycles;
	public Spell CurrentSpell { 
		get { return currentSpell; } 
		set { currentSpell = value; } 
	}

	public static SpellNode AddNode(ICastable castable)
	{
		currentSpell.graphData.Add(castable);
		return currentSpell.graphData.Last();
	}

	public static void RemoveNode(SpellNode node)
	{
		currentSpell.graphData.Remove(node);
	}

	private static bool DirectConnect(ISpellGraph graph, SpellNode first, SpellNode last)
	{
		if(first == null || last == null) return false;
		graph.Connect(first, last);
		return true;
	}

	private static bool ConnectWithoutCycles(ISpellGraph graph, SpellNode first, SpellNode last)
	{
        
		if(first == null || last == null ) return false;

		graph.Connect(first, last);
		if(graph is Graph) { if(!GraphUtil.HasCycle((Graph)graph, first)) return true; }
        else if(graph is Digraph) { if(!GraphUtil.HasCycle((Digraph)graph, first)) return true; }

		graph.Disconnect(first, last);
		return false;
	}

	public static bool AddConnectionToSpellGraph(SpellNode first, SpellNode last)
		=> ConnectionMethod(currentSpell.graphData, first, last);
	
	public static bool RemoveConnectionToSpellGraph(SpellNode first, SpellNode last) 
		=> currentSpell.graphData.Disconnect(first, last);

	

	public static void ReplaceNode(SpellNode node, ICastable castable)
	=> currentSpell.graphData.ReplaceNode(node, castable);

}


}



/*
GD.PrintRich("[rainbow freq=1.0 sat=0.8 val=0.8] Entangling process start! [/rainbow]");
GD.PrintRich("[b]Printing the current spell's nodes[/b]");
foreach(SpellNode n in currentSpell)
{
	GD.PrintRich("[b]Node [color=" + Rune.ColorByRarity(((Rune)n.castable).rarity).ToHtml() + "]" + n + "[/color][/b]");
}
GD.PrintRich("[b]Printing the current spell's inactive nodes[/b]");
foreach(SpellNode n in currentSpell.inactiveNodes)
{
	GD.PrintRich("[b]Node [color=" + Rune.ColorByRarity(((Rune)n.castable).rarity).ToHtml() + "]" + n.ToString() + "[/color][/b]");
}
*/
