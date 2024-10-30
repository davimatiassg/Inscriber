using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Directed Graph by storing it on a Adjacence Matrix 
/// </summary>
public abstract partial class Digraph : Graph
{
    public abstract List<int> GetPrevNodesOf(Node node);
    public override abstract void SetNextNodesOf(Node node, List<Node> nodes);

#region GRAPH_METHODS
     private enum ESearchState : int { OUT = 0, STACKED, VISITED };
    /// <summary>
    /// Uses a DFS algorithm to find out if the graph has a cycle.
    /// </summary>
    /// <param name="startingNode">The spellGraph's node from where the search will start</param>
    /// <returns>True if this graph has a cycle</returns>
    public override bool HasCycle(Node startingNode)
    {
        GD.Print("cycle detection");
        if(nodes.Count == 0) return false;
        Dictionary<Node, ESearchState> markedNodes = InitializePairType(nodes, ESearchState.OUT);
        Stack<Node> stack = new Stack<Node>();
        markedNodes[startingNode] = ESearchState.STACKED;
        stack.Push(startingNode);
        GD.Print("loop:");
        while (stack.Count > 0)
        {   
            GD.Print("loopin");
            Node currNode = stack.Pop();
            GD.Print("node " + currNode.index);
            markedNodes[currNode] = ESearchState.VISITED;
            foreach(int nextNode in GetNextNodesOf(currNode))
            {
                GD.Print("next: " + markedNodes[nodes[nextNode]]);
                switch(markedNodes[nodes[nextNode]])
                {
                    
                    case ESearchState.OUT:
                        stack.Push(nodes[nextNode]);
                        markedNodes[nodes[nextNode]] = ESearchState.STACKED;
                        break;
                    case ESearchState.VISITED:
                        return true;
                    default:
                        continue;
                }
            }
        }  
        return false;
    }


    public void UpdateNodeTopSorting() => this.nodes = TopSortNodes(this);

    /// <summary>
    /// Runs a Topology Sort trough a Graph.
    /// </summary>
    /// <param name="spellGraph">The Graph to be sorted</param>
    /// <returns>The Graph's nodes sorted by topology.</returns>
    public static List<Node> TopSortNodes(Digraph spellGraph)
    {

        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(spellGraph.nodes);
        void TopologicalSortUtil(Node node, ref Stack<Node> stack)
        {
            markedNodes[node] = true;
            foreach(int n in spellGraph.GetNextNodesOf(node))
            {
                if (!markedNodes[spellGraph[n]]) TopologicalSortUtil(spellGraph[n], ref stack);
            }
            stack.Push(node);
        }
        
        Stack<Node> stack = new Stack<Node>();
        foreach (Node node in spellGraph.nodes) {
            if (!markedNodes[node] && (spellGraph.GetNextNodesOf(node).Count + spellGraph.GetPrevNodesOf(node).Count > 0)) 
                TopologicalSortUtil(node, ref stack);
        }
        List<Node> sortedArray = new List<Node>(spellGraph.nodes.Count);
        while(stack.Count > 0){ 
            sortedArray.Append(stack.Pop());
        }
        return sortedArray;
    }  

    

#endregion GRAPH_METHODS
}

