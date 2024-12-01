using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Simple Graph by storing it on a Incidence Matrix 
/// </summary>

public partial class IncidenceMatrixGraph<T> : Graph<T> where T : ISpellGraphNode, new()
{
    /// <summary>
    /// The incidence matrix that stores arcs data in the graph. 
    /// IncMatrix[x][y] returns whether the arc x has a source, target or none of them in node y.
    /// </summary>
    private List<List<bool>> IncMatrix = new List<List<bool>>();

    private void PrintArray()
    {
        string s = "Printando Array: \n";
        for(int i = 0; i < IncMatrix.Count; i++){ for (int j = 0; j < nodes.Count; j++)
        {
            s += IncMatrix[i][j].ToString();
            s += "\t";
        } s+="\n"; }
        GD.Print(s);
    }
    public override void Add(T node)
    {
        nodes.Add(node);
        if(node == null) return;
        if(node.Index == int.MinValue) node.Index = (int)nodes.Count-1;

        for(int i = 0; i < IncMatrix.Count; i++)
        {
            IncMatrix[i].Insert(node.Index, false);
        }       

        return;
    }
    public override bool Connect(T sourceNode, T targetNode)
    {
        if(sourceNode == null || targetNode == null) {  return false; }
        IncMatrix.Add(new List<bool>(nodes.Count));

        foreach(T _n in nodes) IncMatrix[IncMatrix.Count-1].Add(false); 

        IncMatrix[IncMatrix.Count-1][sourceNode.Index] = true;
        IncMatrix[IncMatrix.Count-1][targetNode.Index] = true;
        return true;
    }
    public override bool Disconnect(T sourceNode, T targetNode)
    {
        if(sourceNode == null || targetNode == null) return false;

        for(int i = 0; i<IncMatrix.Count; i++)
        {
            if( IncMatrix[i][sourceNode.Index] == true && 
                IncMatrix[i][targetNode.Index] == true)
            {
                IncMatrix.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    

    public override List<int> GetNextNodesOf(T node)
    {
        List<int> nexts = new List<int>();
        if(node == null) return nexts;
        for (int i = 0; i < IncMatrix.Count; i++)
        {
            if(IncMatrix[i][node.Index] != true) continue;
            for(int j = 0; j < nodes.Count; j++)
            { if(j != node.Index && IncMatrix[i][j]) nexts.Add(j); }
        }
        return nexts;
    }

    private bool DisconnectNode(T node)
    {
        if(node == null) return false;
        for(int i = IncMatrix.Count-1; i >= 0; i--)
        {
            if(IncMatrix[i][node.Index] == true) IncMatrix.RemoveAt(i);
        }
        return true;
    }


    /// <summary>
    /// Removes a node from the graph.
    /// </summary>
    /// <param name="node">The node to be taken out of the graph</param>
    /// <returns></returns>
    public override bool Remove(T node)
    {
        if(!DisconnectNode(node)) return false;
        foreach(List<bool> arc in IncMatrix) arc.RemoveAt(node.Index);
        return true;
    }

    public override bool ReplaceNode(T node, ICastable castable)
    {
        if(node == null) return false;
        node.Castable = castable;
        return true;
    }

    public override void SetNextNodesOf(T node, List<T> nodes)
    {
        for(int i = IncMatrix.Count-1; i >= 0; i--)
        {
            if(IncMatrix[i][node.Index] == true) IncMatrix.RemoveAt(i);
        }
        Connect(node.Index, nodes.Select((T n) => n.Index).ToList());
    }
    public override List<(T, T)> Edges { 
        get => GetEdges();
        set
        {
            IncMatrix.Clear();
            foreach((T src, T trg) in value) Connect(src, trg);
        }
    }

    
    public override Object Clone() {
        IncidenceMatrixGraph<T> graph = new IncidenceMatrixGraph<T>();
        graph.Nodes = this.Nodes;
        graph.Edges = this.Edges;
        return graph;
    }

}

