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
        node.index = Nodes.Count;
        if(node == null) return;

        Nodes.Add(node);

        AdjList.Add(new List<int>());
        return;
    }

    public override bool Remove(Node node)
    {
        if(node == null || !Nodes.Contains(node)) return false;
        foreach(List<int> adjs in AdjList) { adjs.Remove(node.index); }
        AdjList.RemoveAt(node.index);
        Nodes.RemoveAt(node.index);
        return true;

    }

    public override bool ReplaceNode(Node node, ICastable castable)
    {
        if(node == null || !Nodes.Contains(node)) return false;
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
        foreach(int n in AdjList[node.index]) { AdjList[n].Remove(node.index); }
        AdjList[node.index] = nodes.Select((Node n) => n.index).ToList();
        foreach(int n in AdjList[node.index]) { AdjList[n].Add(node.index); }
    }


    public override List<(Node, Node)> Edges { 
        get => GetEdges(); 
        set
        {
            foreach(List<int> l in AdjList) {l.Clear();}
            foreach((Node src, Node trg) in value) { Connect(src, trg); } 
        }
    }

    
    public override Object Clone() {
        AdjacenceListGraph graph = new AdjacenceListGraph();
        graph.Nodes = this.Nodes;
        graph.Edges = this.Edges;
        return graph;
    }


}

