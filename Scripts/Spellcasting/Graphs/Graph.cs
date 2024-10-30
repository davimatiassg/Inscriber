using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Godot;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Reflection.Metadata.Ecma335;
using System.Numerics;
using System.IO;
/// <summary>
/// Class that stores a spell's rune graph agnostically of the data structured used
/// </summary>
public abstract class Graph : ICollection<Graph.Node>
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

    public class Edge 
    {
        (int, int) nodes;
        public int Left 
        {
            get => nodes.Item1;
            set => nodes = (value, nodes.Item2);
        }
        public int Right 
        {
            get => nodes.Item2;
            set => nodes = (nodes.Item1, value);
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
    public abstract void SetNextNodesOf(Node node, List<Node> nodes);
    public void SetNextNodesOf(int idx) => SetNextNodesOf(nodes[idx], nodes);
    
    
    
    public bool IsComplete() => EdgeAmmount() == (nodes.Count*nodes.Count) - nodes.Count;
    public int NodeAmmount() => nodes.Count;

    /// <summary>
    /// Performs a small DFS to find out how many unique edges this graph bears.
    /// </summary>
    /// <returns>The ammount of edges in the graph</returns>
    public virtual int EdgeAmmount()
    {
        int ammount = 0;
        if(nodes.Count < 2) return 0; 
        Dictionary<int, bool> markedNodes = InitializePairType<int, bool>(nodes.Select(n => n.index).ToList(), false);
        Stack<int> stack = new Stack<int>();
        while (stack.Count > 0)
        {   
            int currNode = stack.Pop();
            foreach(int nextNode in GetNextNodesOf(currNode))
            {
                if(!markedNodes[nextNode])
                {
                    stack.Push(nextNode);
                    ammount++;
                }
            }
            markedNodes[currNode] = true;
        }
        return ammount;
    }
    /// <summary>
    /// Verify the if two nodes are adjacent to each other.
    /// </summary>
    /// <param name="n1">The first nodes</param>
    /// <param name="n2">The second nodes</param>
    /// <returns></returns>
    public virtual bool AdjacenceBetween(Node n1, Node n2) => GetNextNodesOf(n1).Contains(n2.index);
    public int Degree(int n) =>  Degree(nodes[n]);
    public int Degree(Node n) => GetNextNodesOf(n).Count;

    public bool isConnected() => ConnectedComponents().Count == 1;
    public List<List<Node>> ConnectedComponents()
    {
        List<List<Node>> connectedComponents = new List<List<Node>>();
        List<Node> remainingNodes = new List<Node>(this.nodes);
        
        List<Node> currentComponent = new List<Node>();
        Action<Node> process = (Node n) => 
        {
            currentComponent.Add(n);
            remainingNodes.RemoveAt(n.index);
        };


        while(remainingNodes.Count > 0)
        {
            ForEachNodeByDFSIn(this,nodes[0], process);
            connectedComponents.Add(currentComponent);
            currentComponent = new List<Node>();
        }
        
        return connectedComponents;
    }
    public bool IsBipartite()
    {
        int brand = 1;
        if(nodes.Count < 3) return true;
        Dictionary<int, (bool, int)> markedNodes = InitializePairType<int, (bool, int)>(nodes.Select(n => n.index).ToList(), (false, 0));
        Stack<int> stack = new Stack<int>();
        while (stack.Count > 0)
        {   
            int currNode = stack.Pop();
            markedNodes[currNode] = (true, brand);
            foreach(int nextNode in GetNextNodesOf(currNode))
            {
                if(!markedNodes[nextNode].Item1)
                {
                    markedNodes[currNode] = (true, -brand);
                    stack.Push(nextNode);
                }
                else if(markedNodes[nextNode].Item2 == brand) return false;
            }
            brand = -brand;
            
        }
        return true;
    }

#region GRAPH_METHODS
    public static Dictionary<T1, T2> InitializePairType<T1, T2>(List<T1> nodes, T2 defValue)
    {
        Dictionary<T1, T2> dict = new Dictionary<T1,T2>();
        foreach(T1 n in nodes)
        {
            dict.Add(n, defValue);
        }
        return dict;
    }
    public static Dictionary<Node, T> InitializePairType<T>(List<Node> nodes) where T : new() => InitializePairType<Node, T>(nodes, new T());
  
    
    
    /// <summary>
    /// Runs a full Breadth First Search trought the graph's nodes and executes a choosen expression at each node.
    /// </summary>
    /// <param name="spellGraph">The graph where the search will be performed </param>
    /// <param name="startingNode">The spellGraph's node from where the search will start</param>
    /// <param name="Process">An Action Delegate that executes with each node as parameter.</param>
    /// 
    public static void ForEachNodeByBFSIn(Graph spellGraph, Node startingNode, Action<Node> Process)
    {
        if(spellGraph.nodes.Count == 0) return;
        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(spellGraph.nodes);
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(spellGraph.nodes[0]);
        markedNodes[startingNode] = true;
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
    public static void ForEachNodeByBFSIn(Graph spellGraph, Action<Node> Process) => ForEachNodeByBFSIn(spellGraph, spellGraph[0], Process);
    

    /// <summary>
    /// Runs a full Depth First Search trought the graph's nodes and executes a choosen expression at each node.
    /// </summary>
    /// <param name="spellGraph">The graph where the search will be performed </param>
    /// <param name="startingNode">The spellGraph's node from where the search will start</param>
    /// <param name="Process">An Action Delegate that executes with each node as parameter.</param>
    public static void ForEachNodeByDFSIn(Graph spellGraph, Node startingNode, Action<Node> Process)
    {
        if(spellGraph.nodes.Count == 0) return;
        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(spellGraph.nodes);
        Stack<Node> stack = new Stack<Node>();
        markedNodes[startingNode] = true;
        while (stack.Count > 0)
        {   
            Node currNode = stack.Pop();
            foreach(int nextNode in spellGraph.GetNextNodesOf(currNode))
            {
                if(!markedNodes[spellGraph[nextNode]])
                {
                    stack.Push(spellGraph[nextNode]);
                    markedNodes[spellGraph[nextNode]] = true;
                }
            }
            Process(currNode);
        }
    }
    public static void ForEachNodeByDFSIn(Graph spellGraph, Action<Node> Process) => ForEachNodeByDFSIn(spellGraph, spellGraph[0], Process);

    /// <summary>
    /// Runs a choosen expression at each node of a graph.
    /// </summary>
    /// <param name="spellGraph">The graph used </param>
    /// <param name="Process">An Action Delegate that executes with each node as parameter.</param>
    public static void ForEachNodeIn(Graph spellGraph, Action<Node> Process) {
        foreach(Node node in spellGraph.nodes) Process(node);
    }
    
    public int WalkFullPath(Func<Node, int> Process) 
    {
        Dictionary<Node, int> weightedDict = InitializePairType(nodes, -1);
        int result = 0;
        foreach(Node node in nodes)
        {
            int p = 0;
            foreach(int nextNode in GetNextNodesOf(node))
            {
                p = Mathf.Max(p, weightedDict[this[nextNode]]);
                result = Mathf.Max(result, p);
            }
            weightedDict[node] = Process(node) + p;
        }
        return result;
    }

    
 
     private enum ESearchState : int { OUT = 0, STACKED, VISITED };
    /// <summary>
    /// Uses a DFS algorithm to find out if the graph has a cycle.
    /// </summary>
    /// <param name="startingNode">The spellGraph's node from where the search will start</param>
    /// <returns>True if this graph has a cycle</returns>
    public virtual bool HasCycle(Node startingNode)
    {
        if(nodes.Count == 0) return false;
        Dictionary<int, (ESearchState, int)> markedNodes = InitializePairType(nodes.Select(n => n.index).ToList(), (ESearchState.OUT, -1));
        Stack<int> stack = new Stack<int>();
        markedNodes[startingNode.index] = (ESearchState.STACKED, startingNode.index);
        stack.Push(startingNode.index);
        int currNode = startingNode.index;
        while (stack.Count > 0)
        {   
            
            foreach(int nextNode in GetNextNodesOf(currNode))
            {   
                GD.Print($"current: {currNode}, next: {nextNode} entered by {markedNodes[nextNode].Item2}. nextState: {markedNodes[nextNode]}");

                switch( markedNodes[nextNode].Item1 )
                {
                    case ESearchState.OUT:
                        stack.Push(nextNode);
                        markedNodes[nextNode] = (ESearchState.STACKED, currNode);
                        break;
                    case ESearchState.VISITED:
                        if(markedNodes[currNode].Item2 != nextNode) { 
                            GD.PrintRich($"[color=red]current: {currNode}, next: {nextNode} entered by {markedNodes[nextNode].Item2}. nextState: {markedNodes[nextNode]} [/color]");

                            return true;}
                        break;
                    default:
                        continue;
                }
            }

            markedNodes[currNode] = (ESearchState.VISITED, markedNodes[currNode].Item2);
            currNode = stack.Pop();
            
        }  
        return false;
    }
    public bool HasCycle() => HasCycle(nodes[0]);









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