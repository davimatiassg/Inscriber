using Godot;
using System;
using System.Collections.Specialized;
using System.Linq;


/// <summary>
/// Implements a Spell's Directed Graph by storing it on a Adjacence Matrix 
/// </summary>
public partial class AdjacenceMatrixDigraph : GraphData
{

    /// <summary>
    /// An array that stores nodes by index. Arr[pivotIndex][y] returns whether there is an arc from node pivotIndex to node y.
    /// </summary>
    private bool[][] AdjMatrix = new bool[0][];
    private int activeSize = 0;
    private uint[] removedIndexes = new uint[0];
    public override uint AddNode(Node node)
    {
        if(node == null) return uint.MaxValue;
        AdjMatrix.Append(new bool[nodes.Length]);
        for(int i = 0; i < nodes.Length-1; i++) AdjMatrix[i].Append(false);
        if(node.index == uint.MaxValue) node.index = (uint)nodes.Length;
        nodes.Append(node);
        activeSize++;
        return node.index;
    }

    public override bool Connect(Node sourceNode, Node targetNode)
    {
        if(sourceNode == null || targetNode == null) return false;
        if(AdjMatrix[sourceNode.index][targetNode.index]) return false;
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

    public override uint[] GetNextNodesOf(Node node)
    {
        uint[] nexts = new uint[0];
        if(node == null) return nexts;
        for(uint i = 0; i < nodes.Length; i++)
        {
            if(AdjMatrix[node.index][i]) {nexts.Append(i);}
        }
        return nexts;
    }

    public override uint[] GetPrevNodesOf(Node node)
    {
        uint[] prevs = new uint[0];
        if(node == null) return prevs;
        for(uint i = 0; i < nodes.Length; i++)
        {
            if(AdjMatrix[i][node.index]) {prevs.Append(i);}
        }
        return prevs;
    }

    private bool DisconnectNode(Node node)
    {
        if(node == null) return false;
        for(uint i = 0; i < nodes.Length; i++)
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
    public override bool RemoveNode(Node node)
    {
        if(!DisconnectNode(node)) return false;
        removedIndexes.Append(node.index);
        nodes[node.index] = null;
        return true;
    }

    public override bool ReplaceNode(Node node, ICastable castable)
    {
        throw new NotImplementedException();
    }

    public override void SetNextNodesOf(Node node, Node[] nodes)
    {
        throw new NotImplementedException();
    }

    public override void SetPrevNodesOf(Node node, Node[] nodes)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes "holes" in the graph's array, adjusting the node's indexes.
    /// </summary>
    public void DefragmentArray()
    {
        removedIndexes = removedIndexes.OrderBy((uint i) => -(int)i ).ToArray();
        for(int i = 0; i < removedIndexes.Length; i++) DefragmentIndex(removedIndexes[i]);
        ReduceArray();
        Array.Resize(ref nodes, nodes.Length - removedIndexes.Length);
    }

    private void DefragmentIndex(uint n)
    {
        activeSize--;
        for(uint i = 0; i < activeSize; i++)
        {
            if(i == n) continue;
            AdjMatrix[i][n] = AdjMatrix[i][activeSize];   
        }
        for(uint i = 0; i < activeSize; i++)
        {
            if(i == n) continue;
            AdjMatrix[n][i] = AdjMatrix[activeSize][i];   
        }
        AdjMatrix[n][n] = AdjMatrix[activeSize][activeSize];
        nodes[n] = nodes[activeSize];
        nodes[n].index = n;
        activeSize--;
    }

    void ReduceArray()
    {
        for(int i = 0; i < activeSize; i++)
        {
            Array.Resize<bool>(ref AdjMatrix[i], activeSize);
        }
        Array.Resize<bool[]>(ref AdjMatrix, activeSize);
    }
}

