using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Directed Graph by storing it on a Adjacence Matrix 
/// </summary>
public partial class AdjacenceMatrixDigraph<T> : IGraph<T>
    
    where T : ISpellGraphNode, new()
{

    public List<T> Nodes = new List<T>();

#region NODE_COLLECTION_FIELDS
    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public void Add(T item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

#endregion NODE_COLLECTION_FIELDS


