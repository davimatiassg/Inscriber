
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;


public class GraphUtil
{

#region PREPARATIVE_FUNCTIONS
	public static string TestNodeString(ISpellGraphNode n) => "[color=" + ((Rune)n.Castable).Color.ToHtml() + "]" + n.Index + "[/color]";
	public static string NodeStringName(ISpellGraphNode n) => "[color=" + ((Rune)n.Castable).Color.ToHtml() + "]" + ((Rune)n.Castable).Name + "[/color]";	
	public static Dictionary<T1, T2> InitializePairType<T1, T2>(List<T1> nodes, T2 defValue)
	{
		Dictionary<T1, T2> dict = new Dictionary<T1,T2>();
		foreach(T1 n in nodes)
		{
			dict.Add(n, defValue);
		}
		return dict;
	}
	public static Dictionary<ISpellGraphNode, T> InitializePairType<T>(List<ISpellGraphNode> nodes) 
		where T : new() 
	=> InitializePairType(nodes, new T());


#endregion PREPARATIVE_FUNCTIONS

#region SEARCHES_ITERATIONS
/// <summary>
	/// Runs a full Breadth First Search trought the graph's nodes and executes a choosen expression at each node.
	/// </summary>
	/// <param name="spellGraph">The graph where the search will be performed </param>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <param name="VisitationProcess">An Action Delegate that operates after a node is visited.</param>
	/// <param name="UnmarkedVisitProcess">An Action Delegate that operates for each unvisited neighbor of the node being visited.</param>
	/// <param name="MarkedVisitProcess">An Action Delegate that operates for each visited neighbor of the node being visited.</param>
	public static void ForEachNodeByBFSIn(ISpellGraph<ISpellGraphNode> spellGraph, ISpellGraphNode startingNode, 
		Action<ISpellGraphNode>        VisitationProcess = null, 
		Action<ISpellGraphNode, ISpellGraphNode>  UnmarkedVisitProcess = null,
		Action<ISpellGraphNode, ISpellGraphNode>  MarkedVisitProcess = null
	){
		if(spellGraph.Count == 0) return;
		Dictionary<ISpellGraphNode, bool> markedNodes = InitializePairType(spellGraph.Nodes, false);
		Queue<ISpellGraphNode> queue = new Queue<ISpellGraphNode>();
		queue.Enqueue(startingNode);

		while (queue.Count > 0)
		{   
			ISpellGraphNode currNode = queue.Dequeue();
			if(markedNodes[currNode]) continue;
			
			foreach(var nextNode in spellGraph.GetNextNodesOf(currNode))
			{
				if(markedNodes[nextNode]) { MarkedVisitProcess?.Invoke(currNode, nextNode); }
				else{
					UnmarkedVisitProcess?.Invoke(currNode, nextNode);
					queue.Enqueue(nextNode);
				}
			}
			VisitationProcess?.Invoke(currNode);
			markedNodes[currNode] = true;
		}
	}
    public static void ForEachNodeByBFSIn(ISpellGraph<ISpellGraphNode> spellGraph) => ForEachNodeByBFSIn(spellGraph, spellGraph[0]);
	
	/// <summary>
	/// Runs a full Depth First Search trought the graph's nodes and executes a choosen expression at each node.
	/// </summary>
	/// <param name="spellGraph">The graph where the search will be performed </param>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <param name="VisitationProcess">An Action Delegate that operates after a node is visited.</param>
	/// <param name="UnmarkedVisitProcess">An Action Delegate that operates for each unvisited neighbor of the node being visited.</param>
	/// <param name="MarkedVisitProcess">An Action Delegate that operates for each visited neighbor of the node being visited.</param>
	public static void ForEachNodeByDFSIn(ISpellGraph<ISpellGraphNode> spellGraph, ISpellGraphNode startingNode, 
		Action<ISpellGraphNode>        VisitationProcess = null, 
		Action<ISpellGraphNode, ISpellGraphNode>  UnmarkedVisitProcess = null,
		Action<ISpellGraphNode, ISpellGraphNode>  MarkedVisitProcess = null
	) 
	{
		if(spellGraph.Count == 0) return;
		Dictionary<ISpellGraphNode, bool> markedNodes = InitializePairType(spellGraph.Nodes, false);
		Stack<ISpellGraphNode> stack = new Stack<ISpellGraphNode>();
		stack.Push(startingNode);

		while (stack.Count > 0)
		{   
			ISpellGraphNode currNode = stack.Pop();
			if(markedNodes[currNode]) continue;

			foreach(var nextNode in spellGraph.GetNextNodesOf(currNode))
			{
				if(markedNodes[nextNode])
				{
					MarkedVisitProcess?.Invoke(currNode, nextNode);
				}
				else
				{
					UnmarkedVisitProcess?.Invoke(currNode, nextNode);
					stack.Push(nextNode);
				}
			}
			VisitationProcess?.Invoke(currNode);
			markedNodes[currNode] = true;
		}
	}
	public static void ForEachNodeByDFSIn(ISpellGraph<ISpellGraphNode> spellGraph) => ForEachNodeByDFSIn(spellGraph, spellGraph[0]);

	/// <summary>
	/// Runs a choosen expression at each node of a graph.
	/// </summary>
	/// <param name="spellGraph">The graph used </param>
	/// <param name="Process">An Action Delegate that executes with each node as parameter.</param>
	public static void ForEachNodeIn(ISpellGraph<ISpellGraphNode> spellGraph, Action<ISpellGraphNode> Process) {
		foreach(ISpellGraphNode node in spellGraph.Nodes) Process(node);
	}
#endregion SEARCHES_ITERATIONS

#region GRAPH_PROPERTY_CHECKS

	/// <summary>
	/// Returns whether the graph is Connected
	/// </summary>
	/// <param name="graph">The graph to check</param>
	/// <returns>true if there is only one connected component in the graph, false otherwise.</returns>
	public static bool IsGraphConnected<T>(Graph<T> graph) 
	where T : ISpellGraphNode, new()
		=> ListConnectedComponents(graph).Count == 1;

	/// <summary>
	/// Returns whether the graph is Bipartite
	/// </summary>
	/// <param name="graph">The graph to check</param>
	/// <returns>true the graph is bipartite, false otherwise.</returns>
	public static bool IsGraphBipartite<T>(Graph<T> graph)
		where T : ISpellGraphNode, new()
	{
		int brand = 1;
		if(graph.nodes.Count < 3) return true;
		Dictionary<int, (bool, int)> markedNodes = InitializePairType<int, (bool, int)>(graph.nodes.Select(n => n.Index).ToList(), (false, 0));
		Stack<T> stack = new Stack<T>();
		while (stack.Count > 0)
		{   
			T currNode = stack.Pop();
			markedNodes[currNode.Index] = (true, brand);
			foreach(var nextNode in graph.GetNextNodesOf(currNode))
			{
				if(!markedNodes[nextNode.Index].Item1)
				{
					markedNodes[nextNode.Index] = (true, -brand);
					stack.Push(nextNode);
				}
				else if(markedNodes[nextNode.Index].Item2 == brand) return false;
			}
			brand = -brand;
			
		}
		return true;
	}

	/// <summary>
	/// Uses a DFS algorithm to find out if the graph has a cycle.
	/// </summary>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <returns>True if this graph has a cycle</returns>
	public static bool HasCycle<T>(ISpellDigraph<T> graph, T startingNode)
	where T : ISpellGraphNode, new()
	{
		bool cycled = false;

		Action<ISpellGraphNode, ISpellGraphNode>  MarkedVisitProcess = (ISpellGraphNode n1, ISpellGraphNode n2) => 
		{  cycled = true; };

		ForEachNodeByDFSIn((ISpellGraph<ISpellGraphNode>)graph, startingNode, null, null, MarkedVisitProcess);

		return cycled;
	}


	public static bool HasCycle<T>(ISpellGraph<T> graph, T startingNode)
	where T : ISpellGraphNode, new()
	{
		bool cycled = false;
		Dictionary<ISpellGraphNode, ISpellGraphNode> Predecessors = new Dictionary<ISpellGraphNode, ISpellGraphNode>
        {
            { startingNode, startingNode }
        };

		Action<ISpellGraphNode, ISpellGraphNode>  UnmarkedVisitProcess = (ISpellGraphNode n1, ISpellGraphNode n2) => 
		{ 
			if(!Predecessors.Keys.Contains(n2)) Predecessors.Add(n2, n1); 
		};

		Action<ISpellGraphNode, ISpellGraphNode>  MarkedVisitProcess = (ISpellGraphNode n1, ISpellGraphNode n2) => 
		{ if(! Predecessors[n1].Equals(n1) && Predecessors[n1].Equals(n2)) cycled = true; };

		ForEachNodeByDFSIn((ISpellGraph<ISpellGraphNode>)graph, startingNode, null, UnmarkedVisitProcess, MarkedVisitProcess);

		return cycled;
	}
	/// <summary>
	/// Obtains the Prüffer sequence from certain tree graph.
	/// </summary>
	/// <param name="originalGraph">the tree graph</param>
	/// <returns>The pruffer sequence in a list..</returns>
	/// <exception cref="FormatException"></exception>
	public static List<int> TreeToPruffer<T>(Graph<T> originalGraph)
	where T : ISpellGraphNode, new()
	{
		if(ListConnectedComponents(originalGraph).Count != 1)   	{GD.Print("The chosen graph is not connected."); return null;}
		if(HasCycle(originalGraph, originalGraph[0]))           	{GD.Print("The chosen graph has cycles."); return null;}
		if(originalGraph.Count < 3) return originalGraph.Nodes.Select((T n) => n.Index).ToList();

		Graph<T> graph = (Graph<T>)originalGraph.Clone();

		List<int> code = new List<int>();
		while(code.Count < graph.Count-2)
		{
			foreach(T n in graph.Nodes)
			{
				var nexts = graph.GetNextNodesOf(n);
				if(nexts.Count == 1) { 
					code.Add(nexts[0].Index); 
					graph.SetNextNodesOf(n, new List<T>()); 
					break; 
				}
			}
		}

		return code;
	}


#endregion GRAPH_PROPERTY_CHECKS

#region NODE_LISTING

	/// <summary>
	/// Lists all the articulations from certain graph.
	/// Performs a Breadth-First Search to construct a graph tree and classify the graph's edges, assert node's depth relative 
	/// to the search's root, calculate the LowPT for each node and 
	/// finally select the articulations, if there are any. 
	/// </summary>
	/// <param name="graph"></param>
	/// <returns></returns>
	public static List<ISpellGraphNode> ListArticulations(ISpellGraph<ISpellGraphNode> graph, ISpellGraphNode root)
	{
		List<ISpellGraphNode> articulations = new List<ISpellGraphNode>();

		List<(ISpellGraphNode, ISpellGraphNode)> treeEdges = new List<(ISpellGraphNode, ISpellGraphNode)>();
		Dictionary<ISpellGraphNode, int> depths = new Dictionary<ISpellGraphNode, int> { { root, 0 }};
		Dictionary<ISpellGraphNode, ISpellGraphNode> lowpts = new Dictionary<ISpellGraphNode, ISpellGraphNode> { { root, root }};
		
		void UpdateLowPT(ISpellGraphNode node, ISpellGraphNode newLowPT)
		{
			(ISpellGraphNode, ISpellGraphNode)[] currEdges;
			ISpellGraphNode curr_node = node;
			do
			{
				if(depths[lowpts[curr_node]] > depths[newLowPT]) lowpts[curr_node] = newLowPT;
				else break;
				
				currEdges = treeEdges.Where(((ISpellGraphNode, ISpellGraphNode) n) => n.Item2.Equals(curr_node)).ToArray();
				curr_node = currEdges[0].Item1;
			} while(currEdges.Length > 0);
		}
				
		Action<ISpellGraphNode, ISpellGraphNode>  UnmarkedVisitProcess = (ISpellGraphNode current, ISpellGraphNode next) =>
		{
			treeEdges.Add((current, next));
			if(depths.Keys.Contains(next)) return;
			
			depths.Add(next, depths[current]+1);
			lowpts.Add(next, next);
			
		};

		Action<ISpellGraphNode, ISpellGraphNode>  MarkedVisitProcess = (ISpellGraphNode current, ISpellGraphNode next) =>
		{
			if(depths[lowpts[current]] > depths[next]) UpdateLowPT(current, next);
		};

        ForEachNodeByBFSIn(graph, root, null, UnmarkedVisitProcess, MarkedVisitProcess);

		foreach(ISpellGraphNode node in graph.Nodes)
		{
			if(node.Equals(root)) continue;
			foreach(var next in graph.GetNextNodesOf(node))
			{
				if(lowpts[next].Equals(next) || lowpts[next].Equals(node)) 
				if(!articulations.Contains(node)) articulations.Add(node);
			}
		}
		if(treeEdges.Where(((ISpellGraphNode, ISpellGraphNode) n) => n.Item1.Equals(root) || n.Item2.Equals(root)).Count() > 1) articulations.Add(root);
		return articulations;
	}

	/// <summary>
	/// Returns a list with the connected components of a given graph.
	/// </summary>
	/// <param name="graph">The graph to get connected components from</param>
	/// <returns>A List with the connected components of the graph</returns>
	public static List<List<T>> ListConnectedComponents<T>(Graph<T> graph)
	where T : ISpellGraphNode, new()
	{
		List<List<T>> connectedComponents = new List<List<T>>();
		List<T> currentComponent = new List<T>();

        var remainingNodes = graph.Nodes.ToArray().ToList();

        Action<ISpellGraphNode> visitedNewNode = (ISpellGraphNode node) => {
			remainingNodes.Remove((T)node); 
			currentComponent.Add((T)node);
		};

        while(remainingNodes.Count > 0)
        {
           	ForEachNodeByDFSIn((ISpellGraph<ISpellGraphNode>)graph, remainingNodes[0], visitedNewNode);
			connectedComponents.Add(currentComponent);
        }
        return connectedComponents;
	}

	

	
#endregion NODE_LISTING

#region TREES
	
	/// <summary>
	/// Receives a void Graph and a Prüffer sequence, and returns the tree from that sequence.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="originalGraph">The void graph</param>
	/// <param name="pruffer">The list that stores a pruffer sequence</param>
	/// <returns></returns>
	/// <exception cref="FormatException"></exception>
	public static TGraph PrufferToTree<TGraph, TNode>(TGraph originalGraph, List<int> pruffer) 
	where TNode : ISpellGraphNode, new()
	where TGraph : Graph<TNode>, new()
	{
		if(originalGraph.EdgeAmmount() > 0)   		throw new FormatException("The chosen graph is not void.");
		if(originalGraph.Count != pruffer.Count+2) 	throw new FormatException("Incongruent graph and pruffer sequence sizes.");

		TGraph graph = new TGraph();
		graph.Nodes = originalGraph.Nodes;

		Dictionary<TNode, int> degrees = InitializePairType(graph.Nodes, 1);

		foreach(int p in pruffer) { degrees[graph[p]]++; }

		foreach(int p in pruffer)
		{
			foreach(TNode n in graph.Nodes)
			{
				if(degrees[n] == 1) {
					graph.Connect(n, p);
					degrees[graph[p]]--;
					degrees[n]--;
					break;
				}
			}
		}
		
		List<TNode> finalEdge = degrees
		.Where((pair)=> pair.Value == 1)
		.Select((pair) => pair.Key)
		.ToList();

		graph.Connect(finalEdge[0], finalEdge[1]);

		return graph;
	}


#endregion TREES

#region SORTING



	public static void UpdateNodeTopSorting<T>(Digraph<T> digraph)
	where T: ISpellGraphNode, new()
	 => digraph.nodes = TopSortNodes(digraph);

	/// <summary>
	/// Runs a Topology Sort trough a Digraph.
	/// </summary>
	/// <param name="spellGraph">The Digraph to be sorted</param>
	/// <returns>The Graph's nodes sorted by topology.</returns>
	public static List<T> TopSortNodes<T>(Digraph<T> spellGraph)
	where T: ISpellGraphNode, new()
	{

		Dictionary<T, bool> markedNodes = InitializePairType(spellGraph.nodes, false);
		void TopologicalSortUtil(T node, ref Stack<ISpellGraphNode> stack)
		{
			markedNodes[node] = true;
			foreach(var n in spellGraph.GetNextNodesOf(node))
			{
				if (!markedNodes[n]) TopologicalSortUtil(n, ref stack);
			}
			stack.Push(node);
		}
		
		Stack<ISpellGraphNode> stack = new Stack<ISpellGraphNode>();
		foreach (T node in spellGraph.nodes) {
			if (!markedNodes[node] && (spellGraph.GetNextNodesOf(node).Count + spellGraph.GetPrevNodesOf(node).Count > 0)) 
				TopologicalSortUtil(node, ref stack);
		}
		List<T> sortedArray = new List<T>(spellGraph.nodes.Count);
		while(stack.Count > 0){ 
			sortedArray.Append((T)stack.Pop());
		}
		return sortedArray;
	}

#endregion SORTING

#region GRAPH_CONVERSION

	/// <summary>
	/// Gets the underlying undirected Graph of given Digraph. 
	/// </summary>
	/// <typeparam name="TGraph">The representation of the Graph to be returned.</typeparam>
	/// <param name="digraph">The Digraph to find the undelying graph from.</param>
	/// <returns>The undelying graph from the given Digraph.</returns>
	public static TGraph UnderlyingGraph<TGraph, TNode>(TGraph digraph) 
	where TNode : ISpellGraphNode, new()
	where TGraph : Digraph<TNode>, new()
	{
		TGraph graph = new TGraph();
		graph.Nodes = digraph.Nodes;
		List<(TNode, TNode)> edges = digraph.Edges;
		for(int i = 0; i < edges.Count; i++)
		{
			(TNode, TNode) duplicate = (edges[i].Item2, edges[i].Item1);
			edges.Remove(duplicate);
		}
		graph.Edges = digraph.Edges;
		return graph;  
	}

	public static TResult ConvertGraphTo<TGraph, TNode, TResult>(TGraph originalGraph)
		where TGraph : ISpellGraph<TNode>, new()
		where TNode : ISpellGraphNode, new()
		where TResult : ISpellGraph<TNode>, new()
	{
		TResult result = new TResult();
		result.Nodes = originalGraph.Nodes;
		result.Edges = originalGraph.Edges;
		return result;  
	}

#endregion GRAPH_CONVERSION

#region PATHFINDING

	

	/// <summary>
    /// Executes a breadth-first search and finds the shortest path between two nodes of a graph, if it exists, using Dijkstra algorithm.
    /// </summary>
    /// <param name="spellGraph">The graph where the search will be performed </param>
    /// <param name="startingNode">The spellGraph's node where the path starts</param>
    /// <param name="endingNode">The spellGraph's node where the path ends</param>

    /// <returns> A List of nodes forming the path </returns>
	public static Path BellmanFord<TGraph>(TGraph spellGraph, ISpellGraphNode startingNode, ISpellGraphNode endingNode
	)
		where TGraph : ISpellGraph<ISpellGraphNode>, new() 
	{		
		if(!spellGraph.Contains(startingNode) || !spellGraph.Contains(endingNode)) 
			throw new InvalidOperationException("The starting or ending nodes are not contained in the graph");

		if(spellGraph.Count <= 1) 
			return (Path)spellGraph.Nodes;

		var distances = new List<int>();
		var predecessors = new List<ISpellGraphNode>();

		Func<ISpellGraphNode, ISpellGraphNode, int> weights = spellGraph is IWeighted<ISpellGraphNode> g? 
		(ISpellGraphNode src, ISpellGraphNode trg) => g.WeightedEdges[(src, trg)] :  
		(ISpellGraphNode src, ISpellGraphNode trg) => 1;
		
		foreach(var node in spellGraph.Nodes)
		{
			distances.Add(int.MaxValue);
			predecessors.Add(null);
		}

		distances[startingNode.Index] = 0;
		
		var queue = new PriorityQueue<ISpellGraphNode, int>();
		queue.Enqueue(startingNode, 0);
	
		foreach(var current in spellGraph.Nodes)
		{
			foreach(var next in spellGraph.GetNextNodesOf(current))
			{
				var weight = weights(current, next);
				if(weight < 0) throw new InvalidOperationException("Graph contains negative edges.");

				if(distances[next.Index] > distances[current.Index] + weight)
				{
					distances[next.Index] = distances[current.Index] + weight;
					predecessors[next.Index] = current;
					queue.Enqueue(spellGraph[next.Index], distances[next.Index]);
				}
			}
		}

		Path path = new Path();
		if(predecessors[endingNode.Index] == null)
			return path;

		path.Add(endingNode);

		while(predecessors[path.Last().Index] != null)
			path.Add(predecessors[path.Last().Index]);

		path.Reverse();
		return path;

	
	}

	/// <summary>
    /// Executes a breadth-first search and finds the shortest path between two nodes of a graph, if it exists, using Dijkstra algorithm.
    /// </summary>
    /// <param name="spellGraph">The graph where the search will be performed </param>
    /// <param name="startingNode">The spellGraph's node where the path starts</param>
    /// <param name="endingNode">The spellGraph's node where the path ends</param>
    /// <returns> A List of nodes forming the path </returns>
	public static Path Dijkstra<TGraph>(TGraph spellGraph, ISpellGraphNode startingNode, ISpellGraphNode endingNode)
		where TGraph : ISpellGraph<ISpellGraphNode>, new() 
	{
		
		if(!spellGraph.Contains(startingNode) || !spellGraph.Contains(endingNode)) 
			throw new InvalidOperationException("The starting or ending nodes are not contained in the graph");

		if(spellGraph.Count <= 1) 
			return (Path)spellGraph.Nodes;

		var distances = new List<int>();
		var predecessors = new List<ISpellGraphNode>();

		Func<ISpellGraphNode, ISpellGraphNode, int> weights = spellGraph is IWeighted<ISpellGraphNode> g? 
		(ISpellGraphNode src, ISpellGraphNode trg) => g.WeightedEdges[(src, trg)] :  
		(ISpellGraphNode src, ISpellGraphNode trg) => 1;
		
		foreach(var node in spellGraph.Nodes)
		{
			distances.Add(int.MaxValue);
			predecessors.Add(null);
		}

		distances[startingNode.Index] = 0;
		
		var queue = new PriorityQueue<ISpellGraphNode, int>();
		queue.Enqueue(startingNode, 0);
	
		do
		{
			ISpellGraphNode current = queue.Dequeue();
			foreach(var next in spellGraph.GetNextNodesOf(current))
			{
				var weight = weights(current, next);
				if(weight < 0) throw new InvalidOperationException("Graph contains negative edges.");

				if(distances[next.Index] > distances[current.Index] + weight)
				{
					distances[next.Index] = distances[current.Index] + weight;
					predecessors[next.Index] = current;
					queue.Enqueue(spellGraph[next.Index], distances[next.Index]);
				}
			}
		}
		while (queue.Count > 0);

		Path path = new Path();
		if(predecessors[endingNode.Index] == null)
			return path;

		path.Add(endingNode);

		while(predecessors[path.Last().Index] != null)
			path.Add(predecessors[path.Last().Index]);

		path.Reverse();
		return path;

	}
/*
	/// <summary>
    /// Executes a breadth-first search and finds the shortest path between two nodes of a graph, if it exists, using Dijkstra algorithm.
    /// </summary>
    /// <param name="spellGraph">The graph where the search will be performed </param>
    /// <param name="startingNode">The spellGraph's node where the path starts</param>
    /// <param name="endingNode">The spellGraph's node where the path ends</param>
    /// <param name="VisitationProcess">An Action Delegate that operates after a node is visited.</param>
    /// <param name="UnmarkedVisitProcess">An Action Delegate that operates for each unvisited neighbor of the node being visited.</param>
    /// <param name="MarkedVisitProcess">An Action Delegate that operates for each visited neighbor of the node being visited.</param>
    /// <returns> A List of nodes forming the path </returns>

    public static Path Dijkstra<TGraph>(TGraph spellGraph, ISpellGraphNode startingNode, ISpellGraphNode endingNode, 
        Action<ISpellGraphNode>           			VisitationProcess = null, 
        Action<ISpellGraphNode, ISpellGraphNode>    UnmarkedVisitProcess = null,
        Action<ISpellGraphNode, ISpellGraphNode>    MarkedVisitProcess = null
	)	
		where TGraph : ISpellGraph<ISpellGraphNode>, new() 
    {
		

        if(spellGraph.Count == 0) return new Path();
        Dictionary<ISpellGraphNode, PathNode> nodeStats = InitializePairType(spellGraph.Nodes.Cast<ISpellGraphNode>().ToList(), new PathNode());
        		
		var edgeWeight = ((ISpellGraphNode, ISpellGraphNode) edge) => { return 1;};
		if(spellGraph is IWeighted<ISpellGraphNode>)
		{
			Dictionary<(ISpellGraphNode, ISpellGraphNode), int> weightedEdges = ((IWeighted<ISpellGraphNode>)spellGraph).WeightedEdges;
			edgeWeight = ((ISpellGraphNode, ISpellGraphNode) edge) => { return weightedEdges[edge]; };
		}
		

        nodeStats[startingNode].Distance = 0;
        
        UnmarkedVisitProcess += (ISpellGraphNode current, ISpellGraphNode next) => 
		{
			nodeStats[next].Distance = edgeWeight((current, next)) + nodeStats[current].Distance;
			nodeStats[next].Parent = current;
		};

        MarkedVisitProcess += (ISpellGraphNode current, ISpellGraphNode next) => 
        {
            if(nodeStats[next].Distance > edgeWeight((current, next)) + nodeStats[current].Distance)
            {
                nodeStats[next].Distance = edgeWeight((current, next)) + nodeStats[current].Distance;
				nodeStats[next].Parent = current;
            }
        };

        ForEachNodeByDFSIn(spellGraph, startingNode, VisitationProcess, UnmarkedVisitProcess, MarkedVisitProcess);
        
        Path path = new Path();
	
        if(nodeStats[endingNode].Distance < int.MaxValue) 
        { 
            for(ISpellGraphNode current = endingNode; !current.Equals(endingNode); current = nodeStats[current].Parent) path.Add(current); 
        }
        path.Reverse();
	
		return path;
    }

*/
#endregion

}

public class Path : List<ISpellGraphNode> {}