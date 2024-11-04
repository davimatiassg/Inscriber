using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Directed Graph by storing it on a Adjacence Matrix 
/// </summary>

using Node = ISpellGraph.Node;
public abstract partial class Digraph : ISpellGraph
{
    public abstract List<int> GetPrevNodesOf(Node node);
    public override abstract void SetNextNodesOf(Node node, List<Node> nodes);


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

