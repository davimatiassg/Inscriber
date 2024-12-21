using Godot;
using System;
using System.Collections.Generic;


public interface ISpellGraphNode
{

    /// <summary>
    /// The index of the spell T on the node's list
    /// </summary>
    int Index {get; set;}

    /// <summary>
    /// The castable associated with this T
    /// </summary>
    ICastable Castable {get; set;}

    /// <summary>
    /// The position of this node on the spell graph's 
    /// </summary>
    Vector2 Position {get; set;}
    /// <summary>
    /// The sigils associated with this T
    /// </summary>
}

public class DefaultSpellGraphNode : ISpellGraphNode
{   


    public int Index { get; set; }
    public ICastable Castable { get; set; }
    public Vector2 Position { get; set; }
    public Sigil[] Sigils { get; set; }

    public CastingResources GetSigilResources() 
    {
        CastingResources res = new CastingResources();
        foreach(Sigil s in Sigils) res.Add(s, s.val);
        return res;
    }

    public override string ToString()
    {
        return $"index: {Index}, CastableName: {((Rune)Castable).Name}";
    }


}

public interface ISpellGraph : ISpellGraph<DefaultSpellGraphNode> {}
public interface ISpellGraph<T> : ICollection<T> where T : ISpellGraphNode
{
    

    public T this[int index] {get; set;}
    public List<T> Nodes {get ; set;}
    public List<(T, T)> Edges {get; set;}
    public T CreateNode(ICastable c);
    public void Add(ICastable c);
    
    /// <summary>
    /// Creates a connection betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the connection was successful, false otherwise.</returns>
    public bool Connect(T sourceNode, T targetNode);

    /// <summary>
    /// Removes a connection betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the disconnection was successful, false otherwise.</returns>
    /// 
    public bool Disconnect(T sourceNode, T targetNode);
    public bool ReplaceNode(T node, ICastable castable);
    public List<T> GetNextNodesOf(T node);
    public void SetNextNodesOf(T node, List<T> nodes);
    public int EdgeAmmount();
    public bool AdjacenceBetween(T n1, T n2);


    public int Degree(T n);
}

public interface ISpellDigraph<T> : ISpellGraph<T> where T : ISpellGraphNode
{
    

    public List<T> GetPrevNodesOf(T node);
    public void SetPrevNodesOf(T node, List<T> nodes);
    public int InwardsDegree(T n);
    public int OutwardsDegree(T n);

}

public interface IWeighted<T>  where T : ISpellGraphNode
{
    public Dictionary<(T, T), int> WeightedEdges {get; set;}

    public bool Connect(T sourceNode, T targetNode, int weight);
}
