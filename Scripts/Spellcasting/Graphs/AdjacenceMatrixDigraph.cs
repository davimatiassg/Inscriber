using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Directed Graph by storing it on a Adjacence Matrix 
/// </summary>
public partial class AdjacenceMatrixDigraph<T> : Digraph<T> where T : ISpellGraphNode, new()
{

    /// <summary>
    /// An array that stores nodes by index. 
    /// AdjMatrix[x][y] returns whether there is an arc from node x to node y.
    /// </summary>
    protected List<List<bool>> AdjMatrix = new List<List<bool>>();

    public void PrintArray()
    {
        string s = "Printando Array: \n";
        for(int i = 0; i < AdjMatrix.Count; i++){ for (int j =0; j<AdjMatrix.Count; j++)
        {
            s += AdjMatrix[i][j].ToString();
            s += "\t";
        } s+="\n"; }

        GD.Print(s);
    }
    private int activeSize = 0;
    private List<int> removedIndexes = new List<int>();

    

    public override void Add(T node)
    {
        nodes.Add(node);
        activeSize++;
        if(node == null) return;

        AdjMatrix.Add(new List<bool>(nodes.Count));
        while(AdjMatrix.Last().Count < AdjMatrix.Count-1) AdjMatrix.Last().Add(false);
        for(int i = 0; i < AdjMatrix.Count; i++) AdjMatrix[i].Add(false);

        if(node.Index == int.MinValue) node.Index = (int)nodes.Count-1;

        return;
    }

    public override bool Connect(T sourceNode, T targetNode)
    {
        if(sourceNode == null || targetNode == null) {  return false; }
        AdjMatrix[sourceNode.Index][targetNode.Index] = true;
        return true;
    }

    public override bool Disconnect(T sourceNode, T targetNode)
    {
        if(sourceNode == null || targetNode == null) return false;
        if(!AdjMatrix[sourceNode.Index][targetNode.Index]) return false;
        AdjMatrix[sourceNode.Index][targetNode.Index] = false;
        return true;
    }

    public override List<T> GetNextNodesOf(T node)
    {
        List<T> nexts = new List<T>();
        if(node == null) return nexts;
        for(int i = 0; i < nodes.Count; i++) { if(AdjMatrix[node.Index][i]) { nexts.Add(Nodes[i]); } }
        return nexts;
    }

    public override List<T> GetPrevNodesOf(T node)
    {
        List<T> prevs = new List<T>();
        if(node == null) return prevs;
        for(int i = 0; i < nodes.Count; i++) { if(AdjMatrix[i][node.Index]) { prevs.Add(Nodes[i]); } }
        return prevs;
    }

    private bool DisconnectNode(T node)
    {
        if(node == null) return false;
        for(int i = 0; i < nodes.Count; i++)
        {
           AdjMatrix[node.Index][i] = false;
           AdjMatrix[i][node.Index] = false;
        }
        return true;
    }


    /// <summary>
    /// Removes all arcs from a node and disables it from the graph.
    /// <see> The Defragment Method </see> to clean the removed nodes.
    /// </summary>
    /// <param name="node">The node to be taken out of the graph</param>
    /// <returns></returns>
    public override bool Remove(T node)
    {
        if(!DisconnectNode(node)) return false;
        removedIndexes.Add(node.Index);
        nodes[node.Index] = default;
        DefragmentArray();
        return true;
    }

    /// <summary>
    /// Removes "holes" in the graph's array, adjusting the node's indexes.
    /// </summary>
    public void DefragmentArray()
    {

        List<int> indexes = removedIndexes.OrderByDescending(n => n).ToList();
        removedIndexes.Clear();
        for(int i = 0; i < indexes.Count; i++) {
            if(indexes[i] < activeSize-1) DefragmentIndex(indexes[i]);
            else activeSize --;
        }
        ReduceArray();
        nodes.RemoveRange(nodes.Count - indexes.Count, indexes.Count);
        PrintArray();
    }

    private void DefragmentIndex(int n)
    {
        activeSize--;
        for(int i = 0; i <= activeSize; i++)
        {
            if(i == n) continue;
            AdjMatrix[i][n] = AdjMatrix[i][activeSize];   
        }
        for(int i = 0; i <= activeSize; i++)
        {
            if(i == n) continue;
            AdjMatrix[n][i] = AdjMatrix[activeSize][i];   
        }
        AdjMatrix[n][n] = AdjMatrix[activeSize][activeSize];
        
        
        nodes[n] = nodes[activeSize];
        nodes[n].Index = n;
    }

    void ReduceArray()
    {
        for(int i = 0; i < activeSize; i++)
        {
            AdjMatrix[i].RemoveRange(activeSize, AdjMatrix.Count-activeSize);
        }
        AdjMatrix.RemoveRange(activeSize, AdjMatrix.Count-activeSize);
    }

    public override List<(T, T)> Edges { 
        get => GetEdges(); 
        set
        {
            foreach(List<bool> adjacences in AdjMatrix) for(int i = 0; i < adjacences.Count; i++) adjacences[i] = false;
            foreach((T src, T trg) in value) AdjMatrix[src.Index][trg.Index] = true;
        }
    }

    public override void SetNextNodesOf(T node, List<T> nodes)
    {
        Disconnect(node.Index, this.nodes.Select((T n) => n.Index).ToList());
        Connect(node.Index, nodes.Select((T n) => n.Index).ToList());
    }
    public override void SetPrevNodesOf(T node, List<T> nodes)
    {
        Disconnect(this.nodes.Select((T n) => n.Index).ToList(), node.Index);
        Connect(nodes.Select((T n) => n.Index).ToList(), node.Index);
    }

    public override bool ReplaceNode(T node, ICastable castable)
    {
        if(node == null || !nodes.Contains(node)) return false;
        node.Castable = castable;
        return true;
    }
}

