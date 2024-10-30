using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Implements a Spell's Directed Graph by storing it on a Incidence Matrix 
/// </summary>
public partial class IncidenceMatrixDigraph : Digraph
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
    public override int AddNode(Node node)
    {
        nodes.Add(node);
        if(node == null) return int.MinValue;
        if(node.index == int.MinValue) node.index = (int)nodes.Count-1;

        for(int i = 0; i < IncMatrix.Count; i++)
        {
            IncMatrix[i].Insert(node.index, ArcIndicator.none);
        }       

        return node.index;
    }

    public override bool Connect(Node sourceNode, Node targetNode)
    {
        if(sourceNode == null || targetNode == null) {  return false; }
        IncMatrix.Add(new List<ArcIndicator>(nodes.Count));
        foreach(Node _n in nodes) IncMatrix[IncMatrix.Count-1].Add(ArcIndicator.none); 

        IncMatrix[IncMatrix.Count-1][sourceNode.index] = ArcIndicator.src;
        IncMatrix[IncMatrix.Count-1][targetNode.index] = ArcIndicator.trg;
        PrintArray();
        return true;
    }

    public override bool Disconnect(Node sourceNode, Node targetNode)
    {
        if(sourceNode == null || targetNode == null) return false;

        for(int i = 0; i<IncMatrix.Count; i++)
        {
            if( IncMatrix[i][sourceNode.index] == ArcIndicator.src && 
                IncMatrix[i][targetNode.index] == ArcIndicator.trg)
            {
                IncMatrix.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    private int FindIndicatorIndexAtArc(ArcIndicator indicator, int arcIndex)
    {
        
        for(int i = 0; i < nodes.Count; i++)
        {
            if(IncMatrix[arcIndex][i] == indicator) return i;
        }
        return int.MinValue;
    }

    public override List<int> GetNextNodesOf(Node node)
    {
        List<int> nexts = new List<int>();
        if(node == null) return nexts;
        for (int i = 0; i < IncMatrix.Count; i++)
        {
            if(IncMatrix[i][node.index] != ArcIndicator.src) continue;
            int result = FindIndicatorIndexAtArc(ArcIndicator.trg, i);
            if(result != int.MinValue) nexts.Add(result);
        }
        return nexts;
    }

    public override List<int> GetPrevNodesOf(Node node)
    {
        List<int> prevs = new List<int>();
        if(node == null) return prevs;
        for (int i = 0; i < IncMatrix.Count; i++)
        {
            if(IncMatrix[i][node.index] != ArcIndicator.trg) continue;
            int result = FindIndicatorIndexAtArc(ArcIndicator.src, i);
            if(result != int.MinValue) prevs.Add(result);
        }
        return prevs;
    }

    private bool DisconnectNode(Node node)
    {
        if(node == null) return false;
        for(int i = 0; i < IncMatrix.Count; i++)
        {
            if(IncMatrix[i][node.index] != ArcIndicator.none) IncMatrix.RemoveAt(i);
        }
        return true;
    }


    /// <summary>
    /// Removes a node from the graph.
    /// </summary>
    /// <param name="node">The node to be taken out of the graph</param>
    /// <returns></returns>
    public override bool RemoveNode(Node node)
    {
        if(!DisconnectNode(node)) return false;
        foreach(List<ArcIndicator> arc in IncMatrix) arc.RemoveAt(node.index);
        return true;
    }

    public override bool ReplaceNode(Node node, ICastable castable)
    {
        if(node == null) return false;
        node.castable = castable;
        return true;
    }

    public override void SetNextNodesOf(Node node, List<Node> nodes)
    {
        throw new NotImplementedException();
    }

    public void SetPrevNodesOf(Node node, List<Node> nodes)
    {
        throw new NotImplementedException();
    }

   
}
