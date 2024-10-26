using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Godot;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Reflection.Metadata.Ecma335;
using System.Numerics;
/// <summary>
/// Class that stores a spell's rune graph agnostically of the data structured used
/// </summary>
public abstract class GraphData : ICollection<GraphData.Node>
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

        /*public async Task<CastingResources> Cast()
        {
            foreach(Node dep in prevs)
            {
                await dep.castStatus;
            }
            return await castable.Cast(CastingResources.Merge(prevs.Select<Node, CastingResources>(i => i.castStatus?.Result).ToArray()));
        }*/

        public CastingResources GetSigilResources() 
        {
            CastingResources res = new CastingResources();
            foreach(Sigil s in sigils) res.Add(s, s.val);
            return res;
        }
    }

    public List<Node> nodes = new List<Node>();

    

    public Node this[int index] => nodes[index];

    public Node CreateNode(ICastable c) => new Node {castable = c, index = int.MinValue };
    public abstract int AddNode(Node node);
    public          int AddNode(ICastable castable) => AddNode(CreateNode(castable));

    public abstract bool RemoveNode(Node node);
    public          bool RemoveNode(int idx) => RemoveNode(nodes[idx]);

    public abstract bool ReplaceNode(Node node, ICastable castable);
    public          bool ReplaceNode(int idx, ICastable castable) => ReplaceNode(nodes[idx], castable);
    

    /// <summary>
    /// Creates a connection betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the connection was successful, false otherwise.</returns>
    public abstract bool Connect(Node sourceNode, Node targetNode);
    public          bool Connect(int sourceNodeIndex, Node targetNode) => Connect(nodes[sourceNodeIndex], targetNode);
    public          bool Connect(Node sourceNode, int targetNodeIndex) => Connect(sourceNode, nodes[targetNodeIndex]);
    public          bool Connect(int sourceNodeIndex, int targetNodeIndex) => Connect(nodes[sourceNodeIndex], nodes[targetNodeIndex]);
    public bool Connect(List<int> sourceNodes, int targetNode) { 
        bool allDone = true; 
        foreach (int sourceNode in sourceNodes) 
        { 
            allDone = allDone && Connect(sourceNode, targetNode); 
        }
        return allDone; 
    }
    
    public bool Connect(int sourceNode, List<int> targetNodes) { 
        bool allDone = true; 
        foreach (int targetNode in targetNodes) 
        { 
            allDone = allDone && Connect(sourceNode, targetNode); 
        }
        return allDone; 
    }

    /// <summary>
    /// Removes a connection betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the disconnection was successful, false otherwise.</returns>
    public abstract bool Disconnect(Node sourceNode, Node targetNode);
    public          bool Disconnect(int sourceNodeIndex, Node targetNode) => Disconnect(nodes[sourceNodeIndex], targetNode);
    public          bool Disconnect(Node sourceNode, int targetNodeIndex) => Disconnect(sourceNode, nodes[targetNodeIndex]);
    public          bool Disconnect(int sourceNodeIndex, int targetNodeIndex) => Disconnect(nodes[sourceNodeIndex], nodes[targetNodeIndex]);
    
    public bool Disconnect(List<int> sourceNodes, int targetNode) { 
        bool allDone = true; 
        foreach (int sourceNode in sourceNodes) 
        { 
            allDone = allDone && Disconnect(sourceNode, targetNode); 
        }
        return allDone; 
    }
    public bool Disconnect(int sourceNode, List<int> targetNodes) { 
        bool allDone = true; 
        foreach (int targetNode in targetNodes) 
        { 
            allDone = allDone && Disconnect(sourceNode, targetNode); 
        }
        return allDone; 
    }

    public abstract List<int> GetNextNodesOf(Node node);
    public          List<int> GetNextNodesOf(int idx) => GetNextNodesOf(nodes[idx]);
    public abstract List<int> GetPrevNodesOf(Node node);
    public          List<int> GetPrevNodesOf(int idx) => GetPrevNodesOf(nodes[idx]);
    public abstract void SetNextNodesOf(Node node, List<Node> nodes);
    public void SetNextNodesOf(int idx) => SetNextNodesOf(nodes[idx], nodes);
    public abstract void SetPrevNodesOf(Node node, List<Node> nodes);
    public void SetPrevNodesOf(int idx) => SetPrevNodesOf(nodes[idx], nodes);



#region GRAPH_METHODS
    public static Dictionary<Node, T> InitializePairType<T>(List<Node> nodes, T defValue)
    {
        Dictionary<Node, T> dict = new Dictionary<Node,T>();
        foreach(Node n in nodes)
        {
            dict.Add(n, defValue);
        }
        return dict;
    }
    public static Dictionary<Node, T> InitializePairType<T>(List<Node> nodes) where T : new() => InitializePairType<T>(nodes, new T());
  
    public void UpdateNodeTopSorting() => this.nodes = TopSortNodes(this);
    /// <summary>
    /// Runs a Topology Sort trough a Graph.
    /// </summary>
    /// <param name="spellGraph">The Graph to be sorted</param>
    /// <returns>The Graph's nodes sorted by topology.</returns>
    public static List<Node> TopSortNodes(GraphData spellGraph)
    {

        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(spellGraph.nodes);
        void TopologicalSortUtil(Node node, ref Stack<Node> stack)
        {
            markedNodes[node] = true;
            foreach(int n in spellGraph.GetNextNodesOf(node))
            {
                if (!markedNodes[spellGraph[n]]) TopologicalSortUtil(spellGraph[n], ref stack);
            }
            stack.Push(node);
        }
        
        Stack<Node> stack = new Stack<Node>();
        foreach (Node node in spellGraph.nodes) {
            if (!markedNodes[node] && (spellGraph.GetNextNodesOf(node).Count + spellGraph.GetPrevNodesOf(node).Count > 0)) 
                TopologicalSortUtil(node, ref stack);
        }
        List<Node> sortedArray = new List<Node>(spellGraph.nodes.Count);
        while(stack.Count > 0){ 
            sortedArray.Append(stack.Pop());
        }
        return sortedArray;
    }
    
    /// <summary>
    /// Runs a full Breadth First Search trought the graph's nodes and executes a choosen expression at each node.
    /// </summary>

    /// <param name="spellGraph">The graph where the search will be performed </param>
    /// <param name="Process">An Action Delegate that executes with each node as parameter.</param>
    
    public static void ForEachNodeByBFSIn(GraphData spellGraph, Action<Node> Process)
    {
        if(spellGraph.nodes.Count == 0) return;
        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(spellGraph.nodes);
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(spellGraph.nodes[0]);
        markedNodes[spellGraph.nodes[0]] = true;
        while (queue.Count > 0)
        {   
            Node currNode = queue.Dequeue();
            foreach(int nextNode in spellGraph.GetNextNodesOf(currNode))
            {
                if(!markedNodes[spellGraph[nextNode]])
                {
                    queue.Enqueue(spellGraph[nextNode]);
                    markedNodes[spellGraph[nextNode]] = true;
                }
            }
            Process(currNode);
        }
    }

    /// <summary>
    /// Runs a choosen expression at each node of a graph.
    /// </summary>
    /// <param name="spellGraph">The graph used </param>
    /// <param name="Process">An Action Delegate that executes with each node as parameter.</param>
    public static void ForEachNodeIn(GraphData spellGraph, Action<Node> Process) {
        foreach(Node node in spellGraph.nodes) Process(node);
    }
    
    public int WalkFullPath(Func<Node, int> Process) 
    {
        Dictionary<Node, int> weightedDict = InitializePairType<int>(nodes, -1);
        int result = 0;
        foreach(Node node in nodes)
        {
            int p = 0;
            foreach(int prevNode in GetPrevNodesOf(node))
            {
                p = Mathf.Max(p, weightedDict[this[prevNode]]);
                result = Mathf.Max(result, p);
            }
            weightedDict[node] = Process(node) + p;
        }
        return result;
    }
#endregion GRAPH_METHODS



#region INTERFACE_METHODS

    public int Count => nodes.Count;

    public bool IsReadOnly => false;

    public void Add(ICastable castable) => AddNode(CreateNode(castable));
    public void Add(Node item) => AddNode(item);

    public void Clear() => nodes.Clear();

    public bool Contains(Node item) => nodes.Contains(item);

    public void CopyTo(Node[] array, int arrayIndex) => nodes.CopyTo(array, arrayIndex);

    public bool Remove(Node item) => RemoveNode(item);

    public IEnumerator<Node> GetEnumerator() => nodes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => nodes.GetEnumerator();


    #endregion INTERFACE_METHODS


}