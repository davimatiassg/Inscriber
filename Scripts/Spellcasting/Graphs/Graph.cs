using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Class that stores a spell's rune graph agnostically of the data structured used
/// </summary>
public abstract class Graph<T> : ISpellGraph<T>, ICloneable 
where T : ISpellGraphNode, new()
{
    public List<T> nodes = new List<T>();

    public T this[int index] { get => nodes[index]; set => nodes[index] = value;}
    public List<T> Nodes {get => nodes;

        set
        {
            if(value != nodes) nodes.Clear(); 
            foreach(T n in value) Add(n);
        }
    }
    public abstract List<(T, T)> Edges {get; set;}
    public T CreateNode(ICastable c)
    {
        T node = new T();
        node.Castable = c;
        node.Index = int.MinValue;
        return node;
    }
    public abstract void Add(T node);
    public          void Add(ICastable castable) => Add(CreateNode(castable));

    public abstract bool Remove(T node);
    public          bool Remove(int idx) => Remove(nodes[idx]);

    public abstract bool ReplaceNode(T node, ICastable castable);
    public          bool ReplaceNode(int idx, ICastable castable) => ReplaceNode(nodes[idx], castable);
    

    public abstract bool Connect(T sourceNode, T targetNode);
    public          bool Connect(int sourceNodeIndex, T targetNode) => Connect(nodes[sourceNodeIndex], targetNode);
    public          bool Connect(T sourceNode, int targetNodeIndex) => Connect(sourceNode, nodes[targetNodeIndex]);
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


    public abstract bool Disconnect(T sourceNode, T targetNode);
    public          bool Disconnect(int sourceNodeIndex, T targetNode) => Disconnect(nodes[sourceNodeIndex], targetNode);
    public          bool Disconnect(T sourceNode, int targetNodeIndex) => Disconnect(sourceNode, nodes[targetNodeIndex]);
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

    public abstract List<T> GetNextNodesOf(T node);
    public          List<T> GetNextNodesOf(int idx) => GetNextNodesOf(nodes[idx]);
    public abstract void SetNextNodesOf(T node, List<T> nodes);
    public void SetNextNodesOf(int idx) => SetNextNodesOf(nodes[idx], nodes);
    
    
    public bool IsComplete() => EdgeAmmount() == nodes.Count*(nodes.Count - 1);
    public int NodeAmmount() => nodes.Count;


    /// <summary>
    /// Runs a repetitive DFS to get each Edge of the graph.
    /// </summary>
    /// <returns>A List of T pairs, which each represent a edge on this graph</returns>
    public List<(T, T)> GetEdges()
    {
        List<(T, T)> edges = new List<(T, T)>  ();
        List<T> remainingNodes = this.Nodes.ToArray().ToList();

        Action<ISpellGraphNode, ISpellGraphNode> newEdgeFound = (ISpellGraphNode src, ISpellGraphNode trg) => edges.Add(((T)src, (T)trg));

        Action<ISpellGraphNode> visitedNewNode = (ISpellGraphNode n) => remainingNodes.Remove((T)n);

        
        while(remainingNodes.Count > 0)
        {
            GraphUtil.ForEachNodeByDFSIn((ISpellGraph<ISpellGraphNode>)this, remainingNodes[0], visitedNewNode, newEdgeFound);
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
    public virtual bool AdjacenceBetween(T n1, T n2) => GetNextNodesOf(n1).Contains(n2);
    public int Degree(int n) =>  Degree(nodes[n]);
    public int Degree(T n) => GetNextNodesOf(n).Count;

    public abstract Object Clone();
    

#region GRAPH_METHODS
    

#endregion GRAPH_METHODS



#region INTERFACE_METHODS

    public int Count => nodes.Count;

    public bool IsReadOnly => false;

    public void Clear() => nodes.Clear();

    public bool Contains(T item) => nodes.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => nodes.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => nodes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => nodes.GetEnumerator();


    #endregion INTERFACE_METHODS


}