using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SpellEditing
{


using SpellNode = GraphData.Node;

/// <summary>
/// Singleton used to alter the spell internally
/// </summary>

public partial class SpellManager
{
    private static Spell currentSpell = new Spell();
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
    enum ENodeState : int { OUT = 0, IN, PATH };
    private static bool GraphHasCycle(GraphData graph, SpellNode currNode)
    {
        var markedNodes = GraphData.InitializePairType<ENodeState>(graph.nodes);

        void MarkNode(SpellNode node, ENodeState state)
        {
            if(markedNodes.ContainsKey(node)) { markedNodes[node] = state; return;}
            markedNodes.Add(node, state);
        }

        bool CheckCycleRecursive(SpellNode currNode)
        {
            MarkNode(currNode, ENodeState.PATH);
            foreach(int nextNode in graph.GetNextNodesOf(currNode))
            {
                if(!markedNodes.ContainsKey(graph[nextNode])) { markedNodes.Add(graph[nextNode], ENodeState.OUT); }
                switch(markedNodes[graph[nextNode]])
                {
                    case ENodeState.OUT:
                        if(CheckCycleRecursive(graph[nextNode])) { return true; }
                        break;
                    case ENodeState.PATH:
                        return true;
                    default:
                        break;
                }
            }
            MarkNode(currNode, ENodeState.IN);
            return false;
        }
            
        return CheckCycleRecursive(currNode);
    }
    public static bool GraphHasCycle(SpellNode currNode) => GraphHasCycle(currentSpell.graphData, currNode);
    public static bool GraphHasCycle(GraphData graph) => GraphHasCycle(graph, graph[0]);
    public static bool GraphHasCycle() => GraphHasCycle(currentSpell.graphData, currentSpell.graphData[0]);

    private enum ELinkStatus { FIRST_EQUALS_LAST, HAS_CYCLE, LINKED, NULL_NODE_ERROR };

    private static ELinkStatus LinkNodes(GraphData graph, SpellNode first, SpellNode last)
    {
   
        if(first == null || last == null) return ELinkStatus.NULL_NODE_ERROR;
        if(first == last) return ELinkStatus.FIRST_EQUALS_LAST; 
        if(graph.GetNextNodesOf(first).Contains(last.index)) return ELinkStatus.HAS_CYCLE;

        graph.Connect(first, last);

        if(!GraphHasCycle(graph)) return ELinkStatus.LINKED;
        
        graph.Disconnect(first, last); 
        return ELinkStatus.HAS_CYCLE;
    }

    public static bool AddConnectionToSpellGraph(SpellNode first, SpellNode last)
        => LinkNodes(currentSpell.graphData, first, last) == ELinkStatus.LINKED;
    
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