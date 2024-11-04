using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Simple Graph by storing it on a Adjacence List 
/// </summary>

using Node = ISpellGraph.Node;
public partial class AdjacenceListGraph : Graph
{

    /// <summary>
    /// An array that stores nodes neighbors by index. 
    /// AdjList[x] is the list of indexes for nodes adjacent to x .
    /// </summary>
    protected List<List<int>> AdjList = new List<List<int>>();

    public void PrintArray()
    {
        string s = "Printando Lista de Adjacencia: \n";
        for(int i = 0; i < AdjList.Count; i++){
            s += i + ": "; 
            for (int j = 0; j < AdjList[i].Count; j++)
            {
                s += AdjList[i][j];
                s += ";\t";
            } 
            s+="\n"; 
        }

        GD.Print(s);
    }

    public override void Add(Node node)
    {
        node.index = nodes.Count;
        if(node == null) return;

        nodes.Add(node);

        AdjList.Add(new List<int>());
        return;
    }

    public override bool Remove(Node node)
    {
        if(node == null || !nodes.Contains(node)) return false;
        foreach(List<int> adjs in AdjList) { adjs.Remove(node.index); }
        AdjList.RemoveAt(node.index);
        nodes.RemoveAt(node.index);
        return true;

    }

    public override bool ReplaceNode(Node node, ICastable castable)
    {
        if(node == null || !nodes.Contains(node)) return false;
        node.castable = castable;
        return true;
    }
    public override bool Connect(Node sourceNode, Node targetNode)
    {
        if(AdjacenceBetween(sourceNode, targetNode)) return false;

        AdjList[sourceNode.index].Add(targetNode.index);
        AdjList[targetNode.index].Add(sourceNode.index);

        return true;
    }

    public override bool Disconnect(Node sourceNode, Node targetNode)
    {
        if(!AdjacenceBetween(sourceNode, targetNode)) return false;

        AdjList[sourceNode.index].Remove(targetNode.index);
        AdjList[targetNode.index].Remove(sourceNode.index);
        return true;
    }

    public override List<int> GetNextNodesOf(Node node) => AdjList[node.index];

    public override void SetNextNodesOf(Node node, List<Node> nodes)
    {
        throw new NotImplementedException();
    }


}

