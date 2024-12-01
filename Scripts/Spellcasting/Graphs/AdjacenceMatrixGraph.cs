using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Simple Graph by storing it on a Adjacence Matrix 
/// </summary>
public partial class AdjacenceMatrixGraph<T> : Graph<T> where T : ISpellGraphNode, new()
{

    /// <summary>
    /// An array that stores nodes by Index. 
    /// AdjMatrix[x][y] returns whether there is an arc from node x to node y.
    /// </summary>
    protected List<List<bool>> AdjMatrix = new List<List<bool>>();
    public void PrintArray()
    {
        string s = "Printando Array: \n";
        for(int i = 0; i < AdjMatrix.Count; i++){ for (int j = 0; j < AdjMatrix.Count; j++)
        {
            if(i < j) {s += " \t"; continue;}
            s += AdjMatrix[i][j].ToString();
            s += "\t";
        } s+="\n"; }

        GD.Print(s);
    }

    public override void Add(T node)
    {
        node.Index = nodes.Count;
        if(node == null) return;
        
        nodes.Add(node);

        AdjMatrix.Add(new List<bool>(nodes.Count));
        foreach(var _n in nodes) AdjMatrix[nodes.Count-1].Add(false);

        return;
    }

    public override bool Remove(T node)
    {
        if(node == null || !nodes.Contains(node)) return false;
        int lastIndex = nodes.Count()-1;
        for(int i = 0; i < node.Index; i++)             AdjMatrix[node.Index][i] = AdjMatrix[lastIndex][i];
        for(int i = node.Index+1; i < lastIndex; i++)   AdjMatrix[i][node.Index] = AdjMatrix[lastIndex][i];
                                                        AdjMatrix[node.Index][node.Index] = AdjMatrix[lastIndex][lastIndex];
        nodes.RemoveAt(lastIndex);
        AdjMatrix.RemoveAt(lastIndex);
        return true;

    }

    public override bool ReplaceNode(T node, ICastable castable)
    {
        if(node == null || !nodes.Contains(node)) return false;
        node.Castable = castable;
        return true;
    }
    public override bool Connect(T sourceNode, T targetNode)
    {
        (int, int) pair = sourceNode.Index > targetNode.Index? (sourceNode.Index, targetNode.Index) : (targetNode.Index, sourceNode.Index);
        GD.Print($"({pair.Item1}, {pair.Item2}). max INDEX: ({AdjMatrix.Count-1},{AdjMatrix[AdjMatrix.Count-1].Count-1})");
        if(sourceNode == null || targetNode == null) {  return false; }
        AdjMatrix[pair.Item1][pair.Item2] = true;   

        PrintArray();
        return true;
    }

    public override bool Disconnect(T sourceNode, T targetNode)
    {
        (int, int) pair = sourceNode.Index > targetNode.Index? (sourceNode.Index, targetNode.Index) : (targetNode.Index, sourceNode.Index);
        if(sourceNode == null || targetNode == null) {  return false; }
        AdjMatrix[pair.Item1][pair.Item2] = false;
        return true;
    }

    public override List<int> GetNextNodesOf(T node)
    {
        List<int> nexts = new List<int>();
        if(node == null) return nexts;

        for(int i = 0; i < node.Index; i++)             if(AdjMatrix[node.Index][i]) { nexts.Add(i); } 
        for(int i = node.Index; i < nodes.Count; i++)   if(AdjMatrix[i][node.Index]) { nexts.Add(i); }

        return nexts;
    }

    public override void SetNextNodesOf(T node, List<T> nodes)
    {
        Disconnect(node.Index, this.nodes.Select((T n) => n.Index).ToList());
        Connect(node.Index, nodes.Select((T n) => n.Index).ToList());
    }

    public override List<(T, T)> Edges { 
        get => GetEdges(); 
        set
        {
            foreach(List<bool> adjacences in AdjMatrix) for(int i = 0; i < adjacences.Count; i++) adjacences[i] = false;
            foreach((T src, T trg) in value) AdjMatrix[Math.Max(src.Index, trg.Index)][Math.Min(src.Index, trg.Index)] = true;
        }
    }



    public override Object Clone() {
        AdjacenceMatrixGraph<T> graph = new AdjacenceMatrixGraph<T>();
        graph.Nodes = this.Nodes;
        graph.Edges = this.Edges;
        return graph;
    }



}

