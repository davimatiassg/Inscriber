using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Simple Graph by storing it on a Adjacence Matrix 
/// </summary>
public partial class AdjacenceMatrixGraph : Graph
{

    /// <summary>
    /// An array that stores nodes by index. 
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

    public override int AddNode(Node node)
    {
        node.index = nodes.Count;
        nodes.Add(node);

        if(node == null) return int.MinValue;

        AdjMatrix.Add(new List<bool>(nodes.Count));
        foreach(var _n in nodes) AdjMatrix[nodes.Count-1].Add(false);

        return node.index;
    }

    public override bool RemoveNode(Node node)
    {
        if(node == null || !nodes.Contains(node)) return false;
        int lastIndex = nodes.Count()-1;
        for(int i = 0; i < node.index; i++)             AdjMatrix[node.index][i] = AdjMatrix[lastIndex][i];
        for(int i = node.index+1; i < lastIndex; i++)   AdjMatrix[i][node.index] = AdjMatrix[lastIndex][i];
                                                        AdjMatrix[node.index][node.index] = AdjMatrix[lastIndex][lastIndex];
        nodes.RemoveAt(lastIndex);
        AdjMatrix.RemoveAt(lastIndex);
        return true;

    }

    private void Swap((int, int) p1, (int, int) p2)
    {
        bool temp = AdjMatrix[p2.Item1][p2.Item2];
        AdjMatrix[p2.Item1][p2.Item2] = AdjMatrix[p1.Item1][p1.Item2];
        AdjMatrix[p1.Item1][p1.Item2] = temp;

    }
    public override bool ReplaceNode(Node node, ICastable castable)
    {
        if(node == null || !nodes.Contains(node)) return false;
        int lastIndex = nodes.Count()-1;
        for(int i = 0; i < node.index; i++)             Swap((node.index,i), (lastIndex,i));
        for(int i = node.index+1; i < lastIndex; i++)   Swap((i,node.index), (lastIndex,i));
                                                        Swap((node.index,node.index), (lastIndex,lastIndex));
        return true;
    }
    public override bool Connect(Node sourceNode, Node targetNode)
    {
        (int, int) pair = sourceNode.index > targetNode.index? (sourceNode.index, targetNode.index) : (targetNode.index, sourceNode.index);
        GD.Print($"({pair.Item1}, {pair.Item2}). max INDEX: ({AdjMatrix.Count-1},{AdjMatrix[AdjMatrix.Count-1].Count-1})");
        if(sourceNode == null || targetNode == null) {  return false; }
        AdjMatrix[pair.Item1][pair.Item2] = true;   

        PrintArray();
        return true;
    }

    public override bool Disconnect(Node sourceNode, Node targetNode)
    {
        (int, int) pair = sourceNode.index > targetNode.index? (sourceNode.index, targetNode.index) : (targetNode.index, sourceNode.index);
        if(sourceNode == null || targetNode == null) {  return false; }
        AdjMatrix[pair.Item1][pair.Item2] = false;
        return true;
    }

    public override List<int> GetNextNodesOf(Node node)
    {
        List<int> nexts = new List<int>();
        if(node == null) return nexts;

        for(int i = 0; i < node.index; i++)             if(AdjMatrix[node.index][i]) { nexts.Add(i); } 
        for(int i = node.index; i < nodes.Count; i++)   if(AdjMatrix[i][node.index]) { nexts.Add(i); }

        GD.Print($"{nexts.Count}");
        return nexts;
    }

    public override void SetNextNodesOf(Node node, List<Node> nodes)
    {
        throw new NotImplementedException();
    }


}

