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
    }


    /// <summary>
    /// Creates a connection betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the connection was successful, false otherwise.</returns>
    public abstract bool Connect(Node sourceNode, Node targetNode);

    /// <summary>
    /// Removes a connection betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the disconnection was successful, false otherwise.</returns>
    public abstract bool Disconnect(Node sourceNode, Node targetNode);
}
