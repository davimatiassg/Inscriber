using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Implements a Spell's Directed Graph by storing it on a Incidence Matrix 
/// </summary>

using T = DefaultSpellGraphNode;
public partial class IncidenceMatrixDigraph<T> : Digraph<T> where T : ISpellGraphNode, new()
{
    private enum ArcIndicator { src = -1, none = 0, trg = 1 };

    /// <summary>
    /// The incidence matrix that stores arcs data in the graph. 
    /// IncMatrix[x][y] returns whether the arc x has a source, target or none of them in node y.
    /// </summary>
    private List<List<ArcIndicator>> IncMatrix = new List<List<ArcIndicator>>();

    

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
            IncMatrix[i].Insert(node.Index, ArcIndicator.none);
        }       

        return;
    }
    public override bool Connect(T sourceNode, T targetNode)
    {
        if(sourceNode == null || targetNode == null) {  return false; }
        IncMatrix.Add(new List<ArcIndicator>(nodes.Count));

        foreach(T _n in nodes) IncMatrix[IncMatrix.Count-1].Add(ArcIndicator.none); 

        IncMatrix[IncMatrix.Count-1][sourceNode.Index] = ArcIndicator.src;
        IncMatrix[IncMatrix.Count-1][targetNode.Index] = ArcIndicator.trg;
        return true;
    }
    public override bool Disconnect(T sourceNode, T targetNode)
    {
        if(sourceNode == null || targetNode == null) return false;

        for(int i = 0; i<IncMatrix.Count; i++)
        {
            if( IncMatrix[i][sourceNode.Index] == ArcIndicator.src && 
                IncMatrix[i][targetNode.Index] == ArcIndicator.trg)
            {
                IncMatrix.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public override List<T> GetNextNodesOf(T node)
    {
        List<T> nexts = new List<T>();
        if(node == null) return nexts;
        for (int i = 0; i < IncMatrix.Count; i++)
        {
            if(IncMatrix[i][node.Index] != ArcIndicator.src) continue;
            for(int j = 0; j < nodes.Count; j++)
            {
                if(IncMatrix[i][j] == ArcIndicator.trg) nexts.Add(Nodes[j]);
            }
        }
        return nexts;
    }

    public override List<T> GetPrevNodesOf(T node)
    {
        List<T> prevs = new List<T>();
        if(node == null) return prevs;
        for (int i = 0; i < IncMatrix.Count; i++)
        {
            if(IncMatrix[i][node.Index] != ArcIndicator.trg) continue;
            for(int j = 0; j < nodes.Count; j++)
            {
                if(IncMatrix[i][j] == ArcIndicator.src) prevs.Add(Nodes[j]);
            }
        }
        return prevs;
    }

    private bool DisconnectNode(T node)
    {
        if(node == null) return false;
        for(int i = IncMatrix.Count-1; i >= 0; i--)
        {
            if(IncMatrix[i][node.Index] != ArcIndicator.none) IncMatrix.RemoveAt(i);
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
        foreach(List<ArcIndicator> arc in IncMatrix) arc.RemoveAt(node.Index);
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
            if(IncMatrix[i][node.Index] == ArcIndicator.src) IncMatrix.RemoveAt(i);
        }
        Connect(node.Index, nodes.Select((T n) => n.Index).ToList());
    }
    public override void SetPrevNodesOf(T node, List<T> nodes)
    {
        for(int i = IncMatrix.Count-1; i >= 0; i--)
        {
            if(IncMatrix[i][node.Index] == ArcIndicator.trg) IncMatrix.RemoveAt(i);
        }
        Connect(nodes.Select((T n) => n.Index).ToList(), node.Index);
    }


    public override List<(T, T)> Edges { 
        get => GetEdges();
        set
        {
            IncMatrix.Clear();
            foreach((T src, T trg) in value) Connect(src, trg);
        }
    }
    

}
