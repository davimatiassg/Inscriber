using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Directed Graph by storing it on a Adjacence Matrix 
/// </summary>
using Node = ISpellGraph.Node;
public partial class AdjacenceMatrixDigraph : Digraph
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

    

    public override void Add(Node node)
    {
        nodes.Add(node);
        activeSize++;
        if(node == null) return;

        AdjMatrix.Add(new List<bool>(nodes.Count));
        while(AdjMatrix.Last().Count < AdjMatrix.Count-1) AdjMatrix.Last().Add(false);
        for(int i = 0; i < AdjMatrix.Count; i++) AdjMatrix[i].Add(false);

        if(node.index == int.MinValue) node.index = (int)nodes.Count-1;

        return;
    }

    public override bool Connect(Node sourceNode, Node targetNode)
    {
        if(sourceNode == null || targetNode == null) {  return false; }
        AdjMatrix[sourceNode.index][targetNode.index] = true;
        return true;
    }

    public override bool Disconnect(Node sourceNode, Node targetNode)
    {
        if(sourceNode == null || targetNode == null) return false;
        if(!AdjMatrix[sourceNode.index][targetNode.index]) return false;
        AdjMatrix[sourceNode.index][targetNode.index] = false;
        return true;
    }

    public override List<int> GetNextNodesOf(Node node)
    {
        List<int> nexts = new List<int>();
        if(node == null) return nexts;
        for(int i = 0; i < nodes.Count; i++) { if(AdjMatrix[node.index][i]) { nexts.Add(i); } }
        return nexts;
    }

    public override List<int> GetPrevNodesOf(Node node)
    {
        List<int> prevs = new List<int>();
        if(node == null) return prevs;
        for(int i = 0; i < nodes.Count; i++) { if(AdjMatrix[i][node.index]) { prevs.Add(i); } }
        return prevs;
    }

    private bool DisconnectNode(Node node)
    {
        if(node == null) return false;
        for(int i = 0; i < nodes.Count; i++)
        {
           AdjMatrix[node.index][i] = false;
           AdjMatrix[i][node.index] = false;
        }
        return true;
    }


    /// <summary>
    /// Removes all arcs from a node and disables it from the graph.
    /// <see> The Defragment Method </see> to clean the removed nodes.
    /// </summary>
    /// <param name="node">The node to be taken out of the graph</param>
    /// <returns></returns>
    public override bool Remove(Node node)
    {
        if(!DisconnectNode(node)) return false;
        removedIndexes.Add(node.index);
        nodes[node.index] = null;
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
        nodes[n].index = n;
    }

    void ReduceArray()
    {
        for(int i = 0; i < activeSize; i++)
        {
            AdjMatrix[i].RemoveRange(activeSize, AdjMatrix.Count-activeSize);
        }
        AdjMatrix.RemoveRange(activeSize, AdjMatrix.Count-activeSize);
    }

    public override List<(Node, Node)> Edges { 
        get => GetEdges(); 
        set
        {
            foreach(List<bool> adjacences in AdjMatrix) for(int i = 0; i < adjacences.Count; i++) adjacences[i] = false;
            foreach((Node src, Node trg) in value) AdjMatrix[src.index][trg.index] = true;
        }
    }

    public override void SetNextNodesOf(Node node, List<Node> nodes)
    {
        Disconnect(node.index, this.nodes.Select((Node n) => n.index).ToList());
        Connect(node.index, nodes.Select((Node n) => n.index).ToList());
    }
    public override void SetPrevNodesOf(Node node, List<Node> nodes)
    {
        Disconnect(this.nodes.Select((Node n) => n.index).ToList(), node.index);
        Connect(nodes.Select((Node n) => n.index).ToList(), node.index);
    }

    public override bool ReplaceNode(Node node, ICastable castable)
    {
        if(node == null || !nodes.Contains(node)) return false;
        node.castable = castable;
        return true;
    }
}

