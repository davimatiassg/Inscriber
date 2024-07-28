using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellEditing
{
    using SpellNode = Spell.Node;
public partial class SpellManager
{
    private static List<SpellNode> tableNodes = new List<SpellNode>();
    private static Spell currentSpell = new Spell();
    public Spell CurrentSpell { 
        get { return currentSpell; } 
        set { currentSpell = value; } 
    }

    public static SpellNode CreateNodeFromCastable(ICastable c)
    {

        SpellNode node = new SpellNode {castable = c};
        return node;
    }

    public static void AddNode(SpellNode node)
    {
        tableNodes.Add(node);
    }

    public static void RemoveNode(SpellNode node)
    {
        currentSpell.nodes.Remove(node);
        foreach(SpellNode n in node.nexts){ n.prevs.Remove(node); }
        foreach(SpellNode n in node.prevs){ n.nexts.Remove(node); }
    }
    public static void TransferNodeConnections(SpellNode receiver, SpellNode donnor)
    {
        foreach(SpellNode n in donnor.nexts)
        { 
            LinkNodes(receiver, n);
            n.prevs.Remove(donnor); 
        }
        donnor.nexts.Clear();
        foreach(SpellNode n in donnor.prevs){
            LinkNodes(n, receiver); 
            n.nexts.Remove(donnor); 
        }
        donnor.prevs.Clear();
    }
    enum ENodeState : int { OUT = 0, IN, PATH };
    private static bool GraphHasCycle(Dictionary<SpellNode, ENodeState> markedNodes, SpellNode currNode)
    {
        void MarkNode(SpellNode node, ENodeState state)
        {
            if(markedNodes.ContainsKey(node)) { markedNodes[node] = state; return;}
            markedNodes.Add(node, state);
        }

        bool CheckCycleRecursive(SpellNode currNode)
        {
            MarkNode(currNode, ENodeState.PATH);
            foreach(SpellNode nextNode in currNode.nexts)
            {
                if(!markedNodes.ContainsKey(nextNode)) { markedNodes.Add(nextNode, ENodeState.OUT); }
                switch(markedNodes[nextNode])
                {
                    case ENodeState.OUT:
                        if(CheckCycleRecursive(nextNode)) { return true; }
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
    public static bool GraphHasCycle(List<SpellNode> nodes, SpellNode currNode) => GraphHasCycle(Spell.InitializePairType<ENodeState>(nodes), currNode);
    public static bool GraphHasCycle(List<SpellNode> nodes) => GraphHasCycle(nodes, nodes[0]);
    private static bool GraphHasCycle(Dictionary<SpellNode, ENodeState> dict) => GraphHasCycle(dict, currentSpell[0]);
    public static bool GraphHasCycle() => GraphHasCycle(Spell.InitializePairType<ENodeState>(currentSpell.nodes), currentSpell[0]);

    private enum ELinkStatus { FIRST_EQUALS_LAST, HAS_CYCLE, LINKED };

    private static ELinkStatus LinkNodes(SpellNode first, SpellNode last)
    {
        if(first == last) { return ELinkStatus.FIRST_EQUALS_LAST; }
        if(first.nexts.Contains(last)) { return ELinkStatus.HAS_CYCLE; }
        List<SpellNode> nodes = new List<SpellNode>(currentSpell.nodes);
        if(!nodes.Contains(first)) { nodes.Add(first); }
        if(!nodes.Contains(last)) { nodes.Add(last); }

        first.ConnectNext(last);

        if(GraphHasCycle(nodes)){ first.DisconnectNext(last); return ELinkStatus.HAS_CYCLE; }

        return ELinkStatus.LINKED;
    }

    public static bool AddConnectionToSpellGraph(SpellNode first, SpellNode last) { 
        if(LinkNodes(first, last) != ELinkStatus.LINKED) return false;
        if(currentSpell.Contains(first)) { currentSpell.Add(last); }
        return true;
    }
    public static bool RemoveConnectionToSpellGraph(SpellNode first, SpellNode last) {
        first.DisconnectNext(last);
        last.DisconnectPrev(first);
        return true;
    }

    public static (bool, bool) ToggleConnectionOnGraph(SpellNode first, SpellNode last)
    {
        if(first.nexts.Contains(last) || last.nexts.Contains(first)) 
        return (false, RemoveConnectionToSpellGraph(first, last)); 
        else return (true, AddConnectionToSpellGraph(first, last)); 
    }

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