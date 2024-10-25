using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Godot;
using System.Runtime.CompilerServices;
using System.Collections;
/// <summary>
/// Class that stores a spell's rune graph agnostically of the data structured used
/// </summary>
public abstract class GraphData : ICollection<GraphData.Node>
{
    public struct Node
    {   
        /// <summary>
        /// The index of the spell Node on the node's list
        /// </summary>
        public uint index;

        /// <summary>
        /// The castable associated with this Node
        /// </summary>
        public ICastable castable;

        /*public async Task<CastingResources> Cast()
        {
            foreach(Node dep in prevs)
            {
                await dep.castStatus;
            }
            return await castable.Cast(CastingResources.Merge(prevs.Select<Node, CastingResources>(i => i.castStatus?.Result).ToArray()));
        }*/
    }

    protected Node[] nodes;

    

    public Node this[uint index] => nodes[index];

    public Node CreateNode(ICastable c) => new Node {castable = c, index = (uint)nodes.Length };
    public abstract uint AddNode(Node node);
    public          uint AddNode(ICastable castable) => AddNode(CreateNode(castable));

    public abstract bool RemoveNode(Node node);
    public          bool RemoveNode(uint idx) => RemoveNode(nodes[idx]);

    public abstract bool ReplaceNode(Node node, ICastable castable);
    public          bool ReplaceNode(uint idx, ICastable castable) => ReplaceNode(nodes[idx], castable);
    

    /// <summary>
    /// Creates a connection betweeen two graph nodes
    /// </summary>
    /// <param name="sourceNode">The source node</param>
    /// <param name="targetNode">The target node</param>
    /// <returns> true when the connection was successful, false otherwise.</returns>
    public abstract bool Connect(Node sourceNode, Node targetNode);
    public          bool Connect(uint sourceNodeIndex, Node targetNode) => Connect(nodes[sourceNodeIndex], targetNode);
    public          bool Connect(Node sourceNode, uint targetNodeIndex) => Connect(sourceNode, nodes[targetNodeIndex]);
    public          bool Connect(uint sourceNodeIndex, uint targetNodeIndex) => Connect(nodes[sourceNodeIndex], nodes[targetNodeIndex]);
    public bool Connect(uint[] sourceNodes, uint targetNode) { 
        bool allDone = true; 
        foreach (uint sourceNode in sourceNodes) 
        { 
            allDone = allDone && Connect(sourceNode, targetNode); 
        }
        return allDone; 
    }
    
    public bool Connect(uint sourceNode, uint[] targetNodes) { 
        bool allDone = true; 
        foreach (uint targetNode in targetNodes) 
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
    public          bool Disconnect(uint sourceNodeIndex, Node targetNode) => Disconnect(nodes[sourceNodeIndex], targetNode);
    public          bool Disconnect(Node sourceNode, uint targetNodeIndex) => Disconnect(sourceNode, nodes[targetNodeIndex]);
    public          bool Disconnect(uint sourceNodeIndex, uint targetNodeIndex) => Disconnect(nodes[sourceNodeIndex], nodes[targetNodeIndex]);
    
    public bool Disconnect(uint[] sourceNodes, uint targetNode) { 
        bool allDone = true; 
        foreach (uint sourceNode in sourceNodes) 
        { 
            allDone = allDone && Disconnect(sourceNode, targetNode); 
        }
        return allDone; 
    }
    public bool Disconnect(uint sourceNode, uint[] targetNodes) { 
        bool allDone = true; 
        foreach (uint targetNode in targetNodes) 
        { 
            allDone = allDone && Disconnect(sourceNode, targetNode); 
        }
        return allDone; 
    }

    public abstract uint[] GetNextNodesOf(Node node);
    public          uint[] GetNextNodesOf(uint idx) => GetNextNodesOf(nodes[idx]);
    public abstract uint[] GetPrevNodesOf(Node node);
    public          uint[] GetPrevNodesOf(uint idx) => GetPrevNodesOf(nodes[idx]);
    public abstract void SetNextNodesOf(Node node, Node[] nodes);
    public void SetNextNodesOf(uint idx) => SetNextNodesOf(nodes[idx], nodes);
    public abstract void SetPrevNodesOf(Node node, Node[] nodes);
    public void SetPrevNodesOf(uint idx) => SetPrevNodesOf(nodes[idx], nodes);



#region GRAPH_METHODS
    public static Dictionary<Node, T> InitializePairType<T>(Node[] nodes, T defValue)
    {
        Dictionary<Node, T> dict = new Dictionary<Node,T>();
        foreach(Node n in nodes)
        {
            dict.Add(n, defValue);
        }
        return dict;
    }
    public static Dictionary<Node, T> InitializePairType<T>(Node[] nodes) where T : new() => InitializePairType<T>(nodes, new T());
  
    public void UpdateNodeTopSorting() => this.nodes = TopSortNodes(this);
    /// <summary>
    /// Runs a Topology Sort trough a Graph.
    /// </summary>
    /// <param name="spellGraph">The Graph to be sorted</param>
    /// <returns>The Graph's nodes sorted by topology.</returns>
    public static Node[] TopSortNodes(GraphData spellGraph)
    {

        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(spellGraph.nodes);
        void TopologicalSortUtil(Node node, ref Stack<Node> stack)
        {
            markedNodes[node] = true;
            foreach(uint n in spellGraph.GetNextNodesOf(node))
            {
                if (!markedNodes[spellGraph[n]]) TopologicalSortUtil(spellGraph[n], ref stack);
            }
            stack.Push(node);
        }
        
        Stack<Node> stack = new Stack<Node>();
        foreach (Node node in spellGraph.nodes) {
            if (!markedNodes[node] && (spellGraph.GetNextNodesOf(node).Length + spellGraph.GetPrevNodesOf(node).Length > 0)) 
                TopologicalSortUtil(node, ref stack);
        }
        Node[] sortedArray = new Node[spellGraph.nodes.Length];
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
        if(spellGraph.nodes.Length == 0) return;
        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(spellGraph.nodes);
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(spellGraph.nodes[0]);
        markedNodes[spellGraph.nodes[0]] = true;
        while (queue.Count > 0)
        {   
            Node currNode = queue.Dequeue();
            foreach(uint nextNode in spellGraph.GetNextNodesOf(currNode))
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
            foreach(uint prevNode in GetPrevNodesOf(node))
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

    public int Count => nodes.Length;

    public bool IsReadOnly => nodes.IsReadOnly;

    public void Add(Node item) => AddNode(item);

    public void Clear() => nodes = new Node[0];

    public bool Contains(Node item) => nodes.Contains(item);

    public void CopyTo(Node[] array, int arrayIndex) => nodes.CopyTo(array, arrayIndex);

    public bool Remove(Node item) => RemoveNode(item);

    public IEnumerator<Node> GetEnumerator() => (IEnumerator<Node>)nodes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => nodes.GetEnumerator();


    #endregion INTERFACE_METHODS


}