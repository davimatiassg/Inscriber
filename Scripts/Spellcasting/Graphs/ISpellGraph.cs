using Godot;
using System;
using System.Collections.Generic;

public interface ISpellGraph : ICollection<ISpellGraph.Node>
{
    public class Node
    {   
        /// <summary>
        /// The index of the spell Node on the node's list
        /// </summary>
        public int index;

        /// <summary>
        /// The castable associated with this Node
        /// </summary>
        public ICastable castable;

        /// <summary>
        /// The sigils associated with this Node
        /// </summary>
        public Sigil[] sigils;
        public CastingResources GetSigilResources() 
        {
            CastingResources res = new CastingResources();
            foreach(Sigil s in sigils) res.Add(s, s.val);
            return res;
        }

        public override string ToString()
        {
            return $"index: {index}, CastableName: {((Rune)castable).Name}";
        }
    }

    public Node this[int index] {get; set;}
    public List<Node> Nodes {get ; set;}
    public List<(Node, Node)> Edges {get; set;}

    public Node CreateNode(ICastable c);
    public void Add(ICastable c);
    
    /// <summary>
    /// Creates a connection betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the connection was successful, false otherwise.</returns>
    public bool Connect(Node sourceNode, Node targetNode);

    /// <summary>
    /// Removes a connection betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the disconnection was successful, false otherwise.</returns>
    /// 
    
    public bool ReplaceNode(Node node, ICastable castable);
    public bool Disconnect(Node sourceNode, Node targetNode);
    public List<int> GetNextNodesOf(Node node);
    public void SetNextNodesOf(Node node, List<Node> nodes);
    public int EdgeAmmount();
    public bool AdjacenceBetween(Node n1, Node n2);


    public int Degree(Node n);
}
