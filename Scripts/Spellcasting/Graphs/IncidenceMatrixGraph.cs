using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Simple Graph by storing it on a Incidence Matrix 
/// </summary>

using Node = ISpellGraph.Node;
public partial class IncidenceMatrixGraph : Graph
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
    public override void Add(Node node)
    {
        nodes.Add(node);
        if(node == null) return;
        if(node.index == int.MinValue) node.index = (int)nodes.Count-1;

        for(int i = 0; i < IncMatrix.Count; i++)
        {
            IncMatrix[i].Insert(node.index, false);
        }       

        return;
    }
    public override bool Connect(Node sourceNode, Node targetNode)
    {
        if(sourceNode == null || targetNode == null) {  return false; }
        IncMatrix.Add(new List<bool>(nodes.Count));

        foreach(Node _n in nodes) IncMatrix[IncMatrix.Count-1].Add(false); 

        IncMatrix[IncMatrix.Count-1][sourceNode.index] = true;
        IncMatrix[IncMatrix.Count-1][targetNode.index] = true;
        return true;
    }
    public override bool Disconnect(Node sourceNode, Node targetNode)
    {
        if(sourceNode == null || targetNode == null) return false;

        for(int i = 0; i<IncMatrix.Count; i++)
        {
            if( IncMatrix[i][sourceNode.index] == true && 
                IncMatrix[i][targetNode.index] == true)
            {
                IncMatrix.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    

    public override List<int> GetNextNodesOf(Node node)
    {
        List<int> nexts = new List<int>();
        if(node == null) return nexts;
        for (int i = 0; i < IncMatrix.Count; i++)
        {
            if(IncMatrix[i][node.index] != true) continue;
            for(int j = 0; j < nodes.Count; j++)
            { if(j != node.index && IncMatrix[i][j]) nexts.Add(j); }
        }
        return nexts;
    }

    private bool DisconnectNode(Node node)
    {
        if(node == null) return false;
        for(int i = IncMatrix.Count-1; i >= 0; i--)
        {
            if(IncMatrix[i][node.index] == true) IncMatrix.RemoveAt(i);
        }
        return true;
    }


    /// <summary>
    /// Removes a node from the graph.
    /// </summary>
    /// <param name="node">The node to be taken out of the graph</param>
    /// <returns></returns>
    public override bool Remove(Node node)
    {
        if(!DisconnectNode(node)) return false;
        foreach(List<bool> arc in IncMatrix) arc.RemoveAt(node.index);
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
        for(int i = IncMatrix.Count-1; i >= 0; i--)
        {
            if(IncMatrix[i][node.index] == true) IncMatrix.RemoveAt(i);
        }
        Connect(node.index, nodes.Select((Node n) => n.index).ToList());
    }
    public override List<(Node, Node)> Edges { 
        get => GetEdges();
        set
        {
            IncMatrix.Clear();
            foreach((Node src, Node trg) in value) Connect(src, trg);
        }
    }


}

