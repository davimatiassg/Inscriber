/// <summary>
/// Class that stores a spell's rune graph agnostically of the data structured used
/// </summary>
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using Node = ISpellGraph.Node;

public class GraphUtil 
{

#region GENERAL_METHODS
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

    public static bool IsGraphConnected(Graph graph) => GraphUtil.GetConnectedComponents(graph).Count == 1;
    public static bool IsGraphBipartite(Graph graph)
    {
        int brand = 1;
        if(graph.nodes.Count < 3) return true;
        Dictionary<int, (bool, int)> markedNodes = InitializePairType<int, (bool, int)>(graph.nodes.Select(n => n.index).ToList(), (false, 0));
        Stack<int> stack = new Stack<int>();
        while (stack.Count > 0)
        {   
            int currNode = stack.Pop();
            markedNodes[currNode] = (true, brand);
            foreach(int nextNode in graph.GetNextNodesOf(currNode))
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

    public static List<List<Node>> GetConnectedComponents(Graph graph)
    {
        List<List<Node>> connectedComponents = new List<List<Node>>();
        List<Node> remainingNodes = new List<Node>(graph.nodes);
        
        List<Node> currentComponent = new List<Node>();
        Action<Node> process = (Node n) => 
        {
            currentComponent.Add(n);
            remainingNodes.RemoveAt(n.index);
        };


        while(remainingNodes.Count > 0)
        {
            ForEachNodeByDFSIn(graph, graph.nodes[0], process);
            connectedComponents.Add(currentComponent);
            currentComponent = new List<Node>();
        }
        
        return connectedComponents;
    }
    
    /// <summary>
    /// Runs a full Breadth First Search trought the graph's nodes and executes a choosen expression at each node.
    /// </summary>
    /// <param name="spellGraph">The graph where the search will be performed </param>
    /// <param name="startingNode">The spellGraph's node from where the search will start</param>
    /// <param name="Process">An Action Delegate that executes within the search context.</param>
    /// 
    public static void ForEachNodeByBFSIn(Graph spellGraph, Node startingNode, 
        Action<Node>        VisitationProcess = null, 
        Action<Node, Node>  UnmarkedVisitProcess = null,
        Action<Node, Node>  MarkedVisitProcess = null
    ){
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
                    UnmarkedVisitProcess?.Invoke(currNode, spellGraph[nextNode]);
                    queue.Enqueue(spellGraph[nextNode]);
                    markedNodes[spellGraph[nextNode]] = true;
                }
                else { MarkedVisitProcess?.Invoke(currNode, spellGraph[nextNode]); }
            }
            VisitationProcess?.Invoke(currNode);
        }
    }
    public static void ForEachNodeByBFSIn(Graph spellGraph) => ForEachNodeByBFSIn(spellGraph, spellGraph[0]);
    

    /// <summary>
    /// Runs a full Depth First Search trought the graph's nodes and executes a choosen expression at each node.
    /// </summary>
    /// <param name="spellGraph">The graph where the search will be performed </param>
    /// <param name="startingNode">The spellGraph's node from where the search will start</param>
    /// <param name="Process">An Action Delegate that operates after a node is visited.</param>
    /// <param name="UnmarkedVisitProcess">An Action Delegate that operates for each unvisited neighbor of the node being visited.</param>
    public static void ForEachNodeByDFSIn(Graph spellGraph, Node startingNode, 
        Action<Node>        VisitationProcess = null, 
        Action<Node, Node>  UnmarkedVisitProcess = null,
        Action<Node, Node>  MarkedVisitProcess = null
    ){
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
                    UnmarkedVisitProcess?.Invoke(currNode, spellGraph[nextNode]);
                    stack.Push(spellGraph[nextNode]);
                    markedNodes[spellGraph[nextNode]] = true;
                }
                else { MarkedVisitProcess?.Invoke(currNode, spellGraph[nextNode]); }
            }
            VisitationProcess?.Invoke(currNode);
        }
    }
    public static void ForEachNodeByDFSIn(Graph spellGraph) => ForEachNodeByDFSIn(spellGraph, spellGraph[0]);

    /// <summary>
    /// Runs a choosen expression at each node of a graph.
    /// </summary>
    /// <param name="spellGraph">The graph used </param>
    /// <param name="Process">An Action Delegate that executes with each node as parameter.</param>
    public static void ForEachNodeIn(Graph spellGraph, Action<Node> Process) {
        foreach(Node node in spellGraph.nodes) Process(node);
    }
    
 
     private enum ESearchState : int { OUT = 0, STACKED, VISITED };
    /// <summary>
    /// Uses a DFS algorithm to find out if the graph has a cycle.
    /// </summary>
    /// <param name="startingNode">The spellGraph's node from where the search will start</param>
    /// <returns>True if this graph has a cycle</returns>
    public static bool HasCycle(Graph graph, Node startingNode)
    {
        var nodes = graph.nodes;
        if(nodes.Count == 0) return false;
        Dictionary<int, (ESearchState, int)> markedNodes = InitializePairType(nodes.Select(n => n.index).ToList(), (ESearchState.OUT, -1));
        Stack<int> stack = new Stack<int>();
        markedNodes[startingNode.index] = (ESearchState.STACKED, startingNode.index);
        stack.Push(startingNode.index);
        int currNode = startingNode.index;
        while (stack.Count > 0)
        {   
            
            foreach(int nextNode in graph.GetNextNodesOf(currNode))
            {   
                switch( markedNodes[nextNode].Item1 )
                {
                    case ESearchState.OUT:
                        stack.Push(nextNode);
                        markedNodes[nextNode] = (ESearchState.STACKED, currNode);
                        break;
                    case ESearchState.VISITED:
                        if(markedNodes[currNode].Item2 != nextNode) { 
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
}



public class DigraphUtil : GraphUtil
{
        private enum ESearchState : int { OUT = 0, STACKED, VISITED };
    /// <summary>
    /// Uses a DFS algorithm to find out if the graph has a cycle.
    /// </summary>
    /// <param name="startingNode">The spellGraph's node from where the search will start</param>
    /// <returns>True if this graph has a cycle</returns>
    public static bool HasCycle(Graph graph, Node startingNode)
    {
        var nodes = graph.nodes;
        if(nodes.Count == 0) return false;
        Dictionary<Node, ESearchState> markedNodes = InitializePairType(nodes, ESearchState.OUT);
        Stack<Node> stack = new Stack<Node>();
        markedNodes[startingNode] = ESearchState.STACKED;
        stack.Push(startingNode);
        while (stack.Count > 0)
        {   
            Node currNode = stack.Pop();
            markedNodes[currNode] = ESearchState.VISITED;
            foreach(int nextNode in graph.GetNextNodesOf(currNode))
            {
                switch(markedNodes[nodes[nextNode]])
                {
                    
                    case ESearchState.OUT:
                        stack.Push(nodes[nextNode]);
                        markedNodes[nodes[nextNode]] = ESearchState.STACKED;
                        break;
                    case ESearchState.VISITED:
                        return true;
                    default:
                        continue;
                }
            }
        }  
        return false;
    }


    public static void UpdateNodeTopSorting(Digraph digraph) => digraph.nodes = TopSortNodes(digraph);

    /// <summary>
    /// Runs a Topology Sort trough a Graph.
    /// </summary>
    /// <param name="spellGraph">The Graph to be sorted</param>
    /// <returns>The Graph's nodes sorted by topology.</returns>
    public static List<Node> TopSortNodes(Digraph spellGraph)
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

    public TGraph UnderlyingGraph<TGraph>() where TGraph : Graph, new()
    {
        return  
    }
}