using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


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
    
    public void AddSigil(Sigil sigil);

    public Sigil GetSigil(int index);

    public IEnumerable<Sigil> GetSigils();

    public int GetSigilCount();
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

    public void AddSigil(Sigil sigil) => Sigils.Append(sigil);

    public Sigil GetSigil(int index) => Sigils[index];
    public int GetSigilCount() => Sigils.Count();

    public IEnumerable<Sigil> GetSigils() => Sigils;

} 

public enum GraphFlag {WEIGHTED, DIRECTED, ALLOW_CYCLES, ALLOW_LOOPS}
public interface IGraph<T> : ICollection<T> where T : ISpellGraphNode
{
    public GraphFlag[] Flags { get; }

    public T this[int index] {get; set;}
    public void Add(ICastable c);
    
    /// <summary>
    /// Creates a arc betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the connection was successful, false otherwise.</returns>
    public bool Connect(T sourceNode, T targetNode);
    
    /// <summary>
    /// Creates a arc betweeen two graph nodes, specifying the weight of the arc
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <param name="weight">The created arc's weight</param>
    /// <returns> true when the connection was successful, false otherwise.</returns>
    public bool Connect(T sourceNode, T targetNode, int weight);

    /// <summary>
    /// Removes a arc betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the disconnection was successful, false otherwise.</returns>
    /// 
    public bool Disconnect(T sourceNode, T targetNode);
    public bool ReplaceNode(T node, ICastable castable);
    public void ForeachTargetOf(T node, Action<T, int> process);
    public void ForeachSourceOf(T Node, Action<T, int> process);
    public void ForeachEdge(Action<T, T, int> process);
    public bool AdjacenceBetween(T n1, T n2);
    public int InwardsDegree(T n);
    public int OutwardsDegree(T n);
    public int GetEdgeWeight(T src, T trg);
    public void SetEdgeWeight(T src, T trg, int weight);
}
