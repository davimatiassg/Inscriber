using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Class that stores a spell's rune graph agnostically of the data structured used
/// </summary>
/// 
using Node = ISpellGraph.Node;
public abstract class Graph : ISpellGraph, ICloneable
{
    public List<Node> nodes = new List<Node>();

    public Node this[int index] { get => nodes[index]; set => nodes[index] = value;}
    public List<Node> Nodes {get => nodes;

        set
        {
            if(value != nodes) nodes.Clear(); 
            foreach(Node n in value) Add(n);
        }
    }
    public abstract List<(Node, Node)> Edges {get; set;}
    public Node CreateNode(ICastable c) => new Node {castable = c, index = int.MinValue };
    public abstract void Add(Node node);
    public          void Add(ICastable castable) => Add(CreateNode(castable));

    public abstract bool Remove(Node node);
    public          bool Remove(int idx) => Remove(nodes[idx]);

    public abstract bool ReplaceNode(Node node, ICastable castable);
    public          bool ReplaceNode(int idx, ICastable castable) => ReplaceNode(nodes[idx], castable);
    

    public abstract bool Connect(Node sourceNode, Node targetNode);
    public          bool Connect(int sourceNodeIndex, Node targetNode) => Connect(nodes[sourceNodeIndex], targetNode);
    public          bool Connect(Node sourceNode, int targetNodeIndex) => Connect(sourceNode, nodes[targetNodeIndex]);
    public          bool Connect(int sourceNodeIndex, int targetNodeIndex) => Connect(nodes[sourceNodeIndex], nodes[targetNodeIndex]);
    public          bool Connect(List<int> sourceNodes, int targetNode) { 
        bool allDone = true; 
        foreach (int sourceNode in sourceNodes) 
        { 
            allDone = allDone && Connect(sourceNode, targetNode); 
        }
        return allDone; 
    }   
    public          bool Connect(int sourceNode, List<int> targetNodes) { 
        bool allDone = true; 
        foreach (int targetNode in targetNodes) 
        { 
            allDone = allDone && Connect(sourceNode, targetNode); 
        }
        return allDone; 
    }


    public abstract bool Disconnect(Node sourceNode, Node targetNode);
    public          bool Disconnect(int sourceNodeIndex, Node targetNode) => Disconnect(nodes[sourceNodeIndex], targetNode);
    public          bool Disconnect(Node sourceNode, int targetNodeIndex) => Disconnect(sourceNode, nodes[targetNodeIndex]);
    public          bool Disconnect(int sourceNodeIndex, int targetNodeIndex) => Disconnect(nodes[sourceNodeIndex], nodes[targetNodeIndex]);
    public          bool Disconnect(List<int> sourceNodes, int targetNode) { 
        bool allDone = true; 
        foreach (int sourceNode in sourceNodes) 
        { 
            allDone = allDone && Disconnect(sourceNode, targetNode); 
        }
        return allDone; 
    }
    public          bool Disconnect(int sourceNode, List<int> targetNodes) { 
        bool allDone = true; 
        foreach (int targetNode in targetNodes) 
        { 
            allDone = allDone && Disconnect(sourceNode, targetNode); 
        }
        return allDone; 
    }

    public abstract List<int> GetNextNodesOf(Node node);
    public          List<int> GetNextNodesOf(int idx) => GetNextNodesOf(nodes[idx]);
    public abstract void SetNextNodesOf(Node node, List<Node> nodes);
    public void SetNextNodesOf(int idx) => SetNextNodesOf(nodes[idx], nodes);
    
    
    public bool IsComplete() => EdgeAmmount() == nodes.Count*(nodes.Count - 1);
    public int NodeAmmount() => nodes.Count;


    /// <summary>
    /// Runs a repetitive DFS to get each Edge of the graph.
    /// </summary>
    /// <returns>A List of Node pairs, which each represent a edge on this graph</returns>
    public List<(Node, Node)> GetEdges()
    {
        List<(Node, Node)> edges = new List<(Node, Node)>  ();
        List<Node> remainingNodes = this.Nodes.ToArray().ToList();

        Action<Node, Node> newEdgeFound = (Node src, Node trg) => edges.Add((src, trg));

        Action<Node> visitedNewNode = (Node n) => remainingNodes.Remove(n);

        
        while(remainingNodes.Count > 0)
        {
            GraphUtil.ForEachNodeByDFSIn(this, remainingNodes[0], visitedNewNode, newEdgeFound);
        }
        
        return edges;
    }

    public virtual int EdgeAmmount() => GetEdges().Count;

    /// <summary>
    /// Verify the if two nodes are adjacent to each other.
    /// </summary>
    /// <param name="n1">The first nodes</param>
    /// <param name="n2">The second nodes</param>
    /// <returns></returns>
    public virtual bool AdjacenceBetween(Node n1, Node n2) => GetNextNodesOf(n1).Contains(n2.index);
    public int Degree(int n) =>  Degree(nodes[n]);
    public int Degree(Node n) => GetNextNodesOf(n).Count;

    public abstract Object Clone();
    

#region GRAPH_METHODS
    

#endregion GRAPH_METHODS



#region INTERFACE_METHODS

    public int Count => nodes.Count;

    public bool IsReadOnly => false;

    public void Clear() => nodes.Clear();

    public bool Contains(Node item) => nodes.Contains(item);

    public void CopyTo(Node[] array, int arrayIndex) => nodes.CopyTo(array, arrayIndex);

    public IEnumerator<Node> GetEnumerator() => nodes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => nodes.GetEnumerator();


    #endregion INTERFACE_METHODS


}