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
public abstract class SpellGraphStructural
{
    public class Node : ICastable
    {
        uint index;
        public ICastable castable;
        public Task<CastingResources> castStatus;
        public CastingResources CastRequirements { get { return castable.CastRequirements; } }
        public CastingResources CastReturns { get { return castable.CastReturns; } }
        public async Task<CastingResources> Cast(CastingResources data)
        {
            return await castable.Cast(data);
        }
        public async Task<CastingResources> Cast()
        {
            foreach(Node dep in prevs)
            {
                await dep.castStatus;
            }
            return await castable.Cast(CastingResources.Merge(prevs.Select<Node, CastingResources>(i => i.castStatus?.Result).ToArray()));
        }

    }
     
    public bool InsertNode(ICastable castable) = 0;
    public bool RemoveNode(ICastable castable) = 0;
    public bool SubstituteNode(Node node, ICastable castable) = 0;

    private Node GetNodeFromIndex(uint idx) = 0;

    public bool Connect(Node sourceNode, Node targetNode) = 0;
    public bool Connect(Node[] sourceNodes, Node targetNode) { 
        bool allDone = true; 
        foreach (Node sourceNode in sourceNodes) 
        { 
            allDone = allDone && Connect(sourceNode, targetNode); 
        }
        return allDone; 
    }
    public bool Connect(Node sourceNode, Node[] targetNodes) { 
        bool allDone = true; 
        foreach (Node targetNode in targetNodes) 
        { 
            allDone = allDone && Connect(sourceNode, targetNode); 
        }
        return allDone; 
    }

    public bool Disconnect(Node sourceNode, Node targetNode) = 0;
    public bool Disconnect(Node[] sourceNodes, Node targetNode) { 
        bool allDone = true; 
        foreach (Node sourceNode in sourceNodes) 
        { 
            allDone = allDone && Disconnect(sourceNode, targetNode); 
        }
        return allDone; 
    }
    public bool Disconnect(Node sourceNode, Node[] targetNodes) { 
        bool allDone = true; 
        foreach (Node targetNode in targetNodes) 
        { 
            allDone = allDone && Disconnect(sourceNode, targetNode); 
        }
        return allDone; 
    }
    
    public Node[] GetNextNodesOf(Node node) = 0;
    public Node[] GetNextNodesOf(uint idx) => GetNextNodesOf(GetNodeFromIndex(idx));

    public Node[] GetPrevNodesOf(Node node) = 0;
    public Node[] GetPrevNodesOf(uint idx) => GetPrevNodesOf(GetNodeFromIndex(idx));

    public void SetNextNodesOf(Node node, Node[] nodes) = 0;
    public void SetNextNodesOf(uint idx) => SetNextNodesOf(GetNodeFromIndex(idx), Node[] nodes);

    public void SetPrevNodesOf(Node node, Node[] nodes) = 0;
    public void SetPrevNodesOf(uint idx) => SetPrevNodesOf(GetNodeFromIndex(idx), Node[] nodes);

    
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
   

    public void UpdateNodeTopSorting() => this.nodes = TopSortNodes(this.nodes);

    public static Node[] TopSortNodes(Node[] nodes)
    {
        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(nodes);
        void TopologicalSortUtil(Node node, ref Stack<Node> stack)
        {
            markedNodes[node] = true;
            foreach(Node n in node.nexts)
            {
                if (!markedNodes[n]) TopologicalSortUtil(n, ref stack);
            }
            stack.Push(node);
        }
        
        Stack<Node> stack = new Stack<Node>();
        foreach (Node node in nodes) {
            if (!markedNodes[node] && (node.nexts.Count + node.prevs.Count > 0)) TopologicalSortUtil(node, ref stack);
        }

        Node[] list = new Node[]();
        while(stack.Count > 0){ 
            list.Insert(0, stack.Pop());
        }

        return list;
    }
    
    private TResult BFSNodes<TResult>(ref TResult results, Func<Node, TResult> Process)
    {
        if(nodes.Count == 0) return results;
        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(nodes);
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(nodes[0]);
        markedNodes[nodes[0]] = true;

        while (queue.Count > 0)
        {   
            Node currNode = queue.Dequeue();
            foreach(Node nextNode in currNode.nexts)
            {
                if(!markedNodes[nextNode])
                {
                    queue.Enqueue(nextNode);
                    markedNodes[nextNode] = true;
                }
            }
            Process(currNode);
        }

        return results;
    }
    public int WalkFullPath( Func<Node, int> Process) 
    {
        Dictionary<Node, int> weightedDict = InitializePairType<int>(nodes, -1);
        int result = 0;
        foreach(Node node in nodes)
        {
            int p = 0;
            foreach(Node prevNode in node.prevs)
            {
                p = Mathf.Max(p, weightedDict[prevNode]);
                result = Mathf.Max(result, p);
            }
            weightedDict[node] = Process(node) + p;
        }
        return result;
    }
#endregion GRAPH_METHODS
}