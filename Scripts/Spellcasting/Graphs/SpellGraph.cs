using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Simple Graph by storing it on a Adjacence Matrix 
/// </summary>

public partial class SpellGraph<T> : ISpellGraph<T>
    where T : ISpellGraphNode, new()
{

    public const int DEFAULT_WEIGHT = 1;
    public GraphFlag[] Flags => new GraphFlag[] { GraphFlag.DIRECTED, GraphFlag.ALLOW_LOOPS, GraphFlag.WEIGHTED };
    
    protected List<T> Nodes = new List<T>();

    protected List<List<int>> EdgeMatrix = new List<List<int>>();


#region NODE_COLLECTION_FIELDS
    public int Count => Nodes.Count;

    public bool IsReadOnly => false;
    
    public T this[int i] { get => Nodes[i]; set => Nodes[i] = value; }
    public void Add(T item)
    {
        if(item == null) return;
        
        item.Index = Count;
        Nodes.Add(item);

        EdgeMatrix.Add(Enumerable.Repeat(int.MaxValue, Count-1).ToList());
        foreach( var row in EdgeMatrix ) row.Add(int.MaxValue);
    }

    public void Add(ICastable item)
    {
        T newItem = new T{Castable = item};
        Add(newItem);
    }

    public void Clear()
    {
        Nodes.Clear();
        EdgeMatrix.Clear();
    }

    public bool Contains(T item) => Nodes.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => Nodes.CopyTo(array, arrayIndex);
    public IEnumerator<T> GetEnumerator() => Nodes.GetEnumerator();


    private void DisposeMatrixIndex(int disposed)
    {
        int writor = Count-1;
        for(int i = 0; i < disposed; i++)
        {
            EdgeMatrix[i][disposed] = EdgeMatrix[i][Count-1];
            EdgeMatrix[disposed][i] = EdgeMatrix[Count-1][i];
        }
        for(int i = disposed + 1; i < Count-1; i++)
        {
            EdgeMatrix[i][disposed] = EdgeMatrix[i][Count-1];
            EdgeMatrix[disposed][i] = EdgeMatrix[Count-1][i];
        }
        EdgeMatrix[disposed][disposed] = EdgeMatrix[Count-1][Count-1];
    }
    public bool Remove(T item)
    {
        if(Count == 0) 
            return false;
        if(Count == 1) {
            Clear();
            return true;
        }
        
        if(item.Index < Count-1) DisposeMatrixIndex(item.Index);
        
        Nodes[Count-1].Index = item.Index;
        Nodes[item.Index] = Nodes[Count-1];
        
        Nodes.RemoveAt(Count-1);
        EdgeMatrix.RemoveAt(Count-1);
        foreach(var list in EdgeMatrix) list.RemoveAt(Count-1);

        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Nodes).GetEnumerator();


#endregion NODE_COLLECTION_FIELDS

#region GRAPH_FIELDS


    public bool AdjacenceBetween(T n1, T n2) => EdgeMatrix[n1.Index][n2.Index] != int.MaxValue;

    public bool Connect(T sourceNode, T targetNode, int weight)
    {
        if(EdgeMatrix[sourceNode.Index][targetNode.Index] != int.MaxValue) return false;
        EdgeMatrix[sourceNode.Index][targetNode.Index] = weight;
        return true;
    }
    public bool Connect(T sourceNode, T targetNode) => Connect(sourceNode, targetNode, DEFAULT_WEIGHT);

    public bool Disconnect(T sourceNode, T targetNode)
    {
        if(sourceNode.Index == targetNode.Index) return false;
        if(EdgeMatrix[sourceNode.Index][targetNode.Index] == int.MaxValue) return false;
        EdgeMatrix[sourceNode.Index][targetNode.Index] = int.MaxValue;
        return true;
    }
    
    public int EdgeAmmount()
    {
        int k = 0;

        foreach(var row in EdgeMatrix) {foreach(int weight in row){
            k += weight == int.MaxValue ? 0 : 1;
        }}

        return k;
    }

    public IEnumerable<(T src, int weight)> GetSourcesOf(T node)
    {
        var count = Nodes.Count;
        var column = node.Index;
        for (int row = 0; row < count; row++) {
            var weight = EdgeMatrix[row][column];
            if(weight == int.MaxValue) continue;
            yield return (Nodes[row], weight);
        }
    }

    public IEnumerable<(T trg, int weight)> GetTargetsOf(T node)
    {
        var count = Nodes.Count;
        var row = node.Index;
        for (int column = 0; column < count; column++) {
            var weight = EdgeMatrix[row][column];
            if(weight == int.MaxValue) continue;
            yield return (Nodes[column], weight);
        }
    }

    public IEnumerable<(T src, T trg, int weight)> GetEdges()
    {
        int range = Nodes.Count;
        for(int row = 0; row < range; row++) {
        for(int column = 0; column < range; column++) {
            int weight = EdgeMatrix[row][column];
            if(weight == int.MaxValue) continue;
            yield return (Nodes[row], Nodes[column], weight);
        }}
    }

    public void SetEdgeWeight(T src, T trg, int weight)
    {
        if (EdgeMatrix[src.Index][trg.Index] == int.MaxValue) 
            throw new InvalidOperationException("No Edge found between provided nodes");
        EdgeMatrix[src.Index][trg.Index] = weight;
        
    }

    public int GetEdgeWeight(T src, T trg)
    {
        var w = EdgeMatrix[src.Index][trg.Index];
        if(w != int.MaxValue) return w;
        throw new InvalidOperationException("No Edge found between provided nodes");
    }

    public int InwardsDegree(T node)
    {
        int degree = 0;
        var count = Nodes.Count;
        var column = node.Index;
        for (int row = 0; row < count; row++) {
            var weight = EdgeMatrix[row][column];
            if(weight == int.MaxValue) continue;
            degree++;
        }
        return degree;
    }

    public int Degree(T node) => InwardsDegree(node) - OutwardsDegree(node);

    public int OutwardsDegree(T node)
    {
        int degree = 0;
        var count = Nodes.Count;
        var row = node.Index;
        for (int column = 0; column < count; column++) {
            var weight = EdgeMatrix[row][column];
            if(weight == int.MaxValue) continue;
            degree++;
        }
        return degree;
    }

    public bool ReplaceNode(T node, ICastable castable)
    {
        if(!Nodes.Contains(node)) return false;
        node.Castable = castable;
        return true;
    }

#endregion GRAPH_FIELDS

}

