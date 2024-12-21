using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Simple Graph by storing it on a Adjacence List 
/// </summary>

public partial class AdjacenceListGraph<T> : Graph<T> where T : ISpellGraphNode, new()
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

    public override void Add(T node)
    {
        
        if(node == null) return;
        node.Index = Nodes.Count;
        
        Nodes.Add(node);

        AdjList.Add(new List<int>());
        return;
    }

    public override bool Remove(T node)
    {
        if(node == null || !Nodes.Contains(node)) return false;
        foreach(List<int> adjs in AdjList) { adjs.Remove(node.Index); }
        AdjList.RemoveAt(node.Index);
        Nodes.RemoveAt(node.Index);
        return true;

    }

    public override bool ReplaceNode(T node, ICastable castable)
    {
        if(node == null || !Nodes.Contains(node)) return false;
        node.Castable = castable;
        return true;
    }
    public override bool Connect(T sourceNode, T targetNode)
    {
        if(AdjacenceBetween(sourceNode, targetNode)) return false;

        AdjList[sourceNode.Index].Add(targetNode.Index);
        AdjList[targetNode.Index].Add(sourceNode.Index);

        return true;
    }

    public override bool Disconnect(T sourceNode, T targetNode)
    {
        if(!AdjacenceBetween(sourceNode, targetNode)) return false;

        AdjList[sourceNode.Index].Remove(targetNode.Index);
        AdjList[targetNode.Index].Remove(sourceNode.Index);
        return true;
    }



    public override List<T> GetNextNodesOf(T node) => AdjList[node.Index].Select((n) => Nodes[n]).ToList();
    public override void SetNextNodesOf(T node, List<T> nodes)
    {
        foreach(int n in AdjList[node.Index]) { AdjList[n].Remove(node.Index); }
        AdjList[node.Index] = nodes.Select((T n) => n.Index).ToList();
        foreach(int n in AdjList[node.Index]) { AdjList[n].Add(node.Index); }
    }


    public override List<(T, T)> Edges { 
        get => GetEdges(); 
        set
        {
            foreach(List<int> l in AdjList) {l.Clear();}
            foreach((T src, T trg) in value) { Connect(src, trg); } 
        }
    }

    
    public override Object Clone() {
        AdjacenceListGraph<T> graph = new AdjacenceListGraph<T>();
        graph.Nodes = this.Nodes;
        graph.Edges = this.Edges;
        return graph;
    }


}

