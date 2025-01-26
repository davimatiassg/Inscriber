
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using Godot;
	using System.Diagnostics;


public class GraphUtil<TGraph, TNode>
	where TGraph : IGraph<TNode>, new()
	where TNode : IGraphNode, new()
{

#region PREPARATIVE_FUNCTIONS
	public static string TestNodeString<T>(T n) where T : ISpellGraphNode => "[color=" + ((Rune)n.Castable).Color.ToHtml() + "]" + n.Index + "[/color]";
	public static string NodeStringName<T>(T n) where T : ISpellGraphNode => "[color=" + ((Rune)n.Castable).Color.ToHtml() + "]" + ((Rune)n.Castable).Name + "[/color]";	
	public static Dictionary<T1, T2> InitializePairType<T1, T2>(IEnumerable<T1> nodes, T2 defValue)
	{
		Dictionary<T1, T2> dict = new Dictionary<T1,T2>();
		foreach(T1 n in nodes)
		{
			dict.Add(n, defValue);
		}
		return dict;
	}
	public static Dictionary<TNode, T> InitializePairType<T>(IEnumerable<TNode> nodes) 
		where T : new() 
	=> InitializePairType(nodes, new T());


#endregion PREPARATIVE_FUNCTIONS

#region SEARCHES_ITERATIONS
//REVIEW: Os chamadores das buscas DFS e BFS podem fazer bom uso da recém adicionada condição de parada.


/// <summary>
	/// Runs a full Breadth First Search trought the graph's nodes and executes a choosen expression at each node.
	/// </summary>
	/// <param name="spellGraph">The graph where the search will be performed </param>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <param name="VisitationProcess">An Action Delegate that operates after a node is visited.</param>
	/// <param name="UnmarkedVisitProcess">An Action Delegate that operates for each unvisited neighbor of the node being visited.</param>
	/// <param name="MarkedVisitProcess">An Action Delegate that operates for each visited neighbor of the node being visited.</param>
	/// <param name="BreakCondition"> A function that interrupts the search if it ever returns true. </param>
	public static void ForEachNodeByBFSIn(TGraph spellGraph, TNode startingNode, 
		Action<TNode>        	VisitationProcess = null, 
		Action<TNode, TNode>  	UnmarkedVisitProcess = null,
		Action<TNode, TNode>  	MarkedVisitProcess = null,
		Func<bool>			  	BreakCondition = null
	){
		if(spellGraph.Count == 0) return;
		Dictionary<TNode, bool> markedNodes = InitializePairType(spellGraph, false);
		Queue<TNode> queue = new Queue<TNode>();
		queue.Enqueue(startingNode);

		while (queue.Count > 0)
		{   
			TNode currNode = queue.Dequeue();
			if(markedNodes[currNode]) continue;
				
			foreach ((TNode nextNode, int weight) in spellGraph.GetTargetsOf(currNode))
			{
				if(markedNodes[nextNode]) { 
					MarkedVisitProcess?.Invoke(currNode, nextNode);
				}
				else 
				{
					UnmarkedVisitProcess?.Invoke(currNode, nextNode);
					queue.Enqueue(nextNode);
				}
				
			}

			VisitationProcess?.Invoke(currNode);
			markedNodes[currNode] = true;

			if(BreakCondition !=null && BreakCondition()) return;
		}
	}
    public static void ForEachNodeByBFSIn(TGraph spellGraph) => ForEachNodeByBFSIn(spellGraph, spellGraph[0]);
	
	/// <summary>
	/// Runs a full Depth First Search trought the graph's nodes and executes a choosen expression at each node.
	/// </summary>
	/// <param name="spellGraph">The graph where the search will be performed </param>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <param name="VisitationProcess">An Action Delegate that operates after a node is visited.</param>
	/// <param name="UnmarkedVisitProcess">An Action Delegate that operates for each unvisited neighbor of the node being visited.</param>
	/// <param name="MarkedVisitProcess">An Action Delegate that operates for each visited neighbor of the node being visited.</param>
	/// <param name="BreakCondition"> A function that interrupts the search if it ever returns true. </param> 
	public static void ForEachNodeByDFSIn(TGraph spellGraph, TNode startingNode, 
		Action<TNode>        	VisitationProcess = null, 
		Action<TNode, TNode>  	UnmarkedVisitProcess = null,
		Action<TNode, TNode>  	MarkedVisitProcess = null,
		Func<bool>			  	BreakCondition = null
	) 
	{
		if(spellGraph.Count == 0) return;
		Dictionary<TNode, bool> markedNodes = InitializePairType(spellGraph, false);
		Stack<TNode> stack = new Stack<TNode>();
		stack.Push(startingNode);

		while (stack.Count > 0)
		{   
			TNode currNode = stack.Pop();
			if(markedNodes[currNode]) continue;

			markedNodes[currNode] = true;
			foreach ((TNode nextNode, int weight) in spellGraph.GetTargetsOf(currNode))
			{
				if(markedNodes[nextNode]) { 
					MarkedVisitProcess?.Invoke(currNode, nextNode);
				}
				else
				{
					UnmarkedVisitProcess?.Invoke(currNode, nextNode);
					stack.Push(nextNode);
				}
			}
			VisitationProcess?.Invoke(currNode);

			if(BreakCondition !=null && BreakCondition()) return;
		}
	}
	public static void ForEachNodeByDFSIn(TGraph spellGraph) => ForEachNodeByDFSIn(spellGraph, spellGraph[0]);

	/// <summary>
	/// Runs a choosen expression at each node of a graph.
	/// </summary>
	/// <param name="spellGraph">The graph used </param>
	/// <param name="Process">An Action Delegate that executes with each node as parameter.</param>
	public static void ForEachNodeIn(TGraph spellGraph, Action<TNode> Process) {
		foreach(TNode node in spellGraph) Process(node);
	}
#endregion SEARCHES_ITERATIONS

#region GRAPH_PROPERTY_CHECKS

	/// <summary>
	/// Returns whether the graph is Connected
	/// </summary>
	/// <param name="graph">The graph to check</param>
	/// <returns>true if there is only one connected component in the graph, false otherwise.</returns>
	public static bool IsGraphConnected<T>(IGraph<T> graph) 
	where T : TNode, new()
		=> ListConnectedComponents(graph).Count == 1;

	/// <summary>
	/// Returns whether the graph is Bipartite
	/// </summary>
	/// <param name="graph">The graph to check</param>
	/// <returns>true the graph is bipartite, false otherwise.</returns>
	public static bool IsGraphBipartite<T>(IGraph<T> graph)
		where T : TNode, new()
	{
		int brand = 1;
		if(graph.Count < 3) return true;
		Dictionary<int, (bool, int)> markedNodes = InitializePairType<int, (bool, int)>(graph.Select(n => n.Index).ToList(), (false, 0));
		Stack<T> stack = new Stack<T>();
		while (stack.Count > 0)
		{   
			T currNode = stack.Pop();
			markedNodes[currNode.Index] = (true, brand);

			bool lastIteration = false;
			
			foreach ((T nextNode, int weight) in graph.GetTargetsOf(currNode))
			{
				if(!markedNodes[nextNode.Index].Item1)
				{
					markedNodes[nextNode.Index] = (true, -brand);
					stack.Push(nextNode);
				}
				else if(markedNodes[nextNode.Index].Item2 == brand) lastIteration = true;
			}
			if(lastIteration) return false;
			brand = -brand;
			
		}
		return true;
	}

	/// <summary>
	/// Uses a DFS algorithm to verify if the graph has a cycle.
	/// </summary>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <returns>True if this graph has a cycle</returns>
	public static bool HasCycle(TGraph graph, TNode startingNode)
	{
		bool cycled = false;

		List<int> predecessors = Enumerable.Repeat(-1, graph.Count).ToList();


		Action<TNode, TNode>  UnmarkedVisitProcess = (TNode n1, TNode n2) => 
		{
			predecessors[n2.Index] = n1.Index;
		};

		Action<TNode, TNode>  MarkedVisitProcess = (TNode n1, TNode n2) => 
		{ 
			if(graph.Flags.Contains(GraphFlag.DIRECTED) || predecessors[n1.Index] != n2.Index) cycled = true; 
		};

		Func<bool> breakCondition = () => cycled;

		ForEachNodeByDFSIn(graph, startingNode, null, UnmarkedVisitProcess, MarkedVisitProcess, breakCondition);

		return cycled;
	}
	/// <summary>
	/// Obtains the Prüffer sequence from certain tree graph.
	/// </summary>
	/// <param name="originalGraph">the tree graph</param>
	/// <returns>The pruffer sequence in a list..</returns>
	/// <exception cref="FormatException"></exception>
	/// 
	/*
	FIXME: public static List<int> TreeToPruffer<T>(IGraph<T> originalGraph)
	where T : TNode, new()
	{
		if(ListConnectedComponents(originalGraph).Count != 1)   	{GD.Print("The chosen graph is not connected."); return null;}
		if(HasCycle(originalGraph, originalGraph[0]))           	{GD.Print("The chosen graph has cycles."); return null;}
		if(originalGraph.Count < 3) return originalGraph.Select((T n) => n.Index).ToList();

		FIXME: IGraph<T> graph = (IGraph<T>)originalGraph.Clone();

		List<int> code = new List<int>();
		while(code.Count < graph.Count-2)
		{
			foreach(T n in graph)
			{
				if(graph.OutwardsDegree(n) == 1){ 
					graph.ForeachTargetOf(n, (T next, int weight) =>
					{
						code.Add(next.Index); 
						graph.Disconnect(n, next);
					});
					break; 
				}
			}
		}

		return code;
	}*/


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
	public static List<TNode> ListArticulations(TGraph graph, TNode root)
	{
		List<TNode> articulations = new List<TNode>();

		List<(TNode, TNode)> treeEdges = new List<(TNode, TNode)>();
		Dictionary<TNode, int> depths = new Dictionary<TNode, int> { { root, 0 }};
		Dictionary<TNode, TNode> lowpts = new Dictionary<TNode, TNode> { { root, root }};
		
		void UpdateLowPT(TNode node, TNode newLowPT)
		{
			(TNode, TNode)[] currEdges;
			TNode curr_node = node;
			do
			{
				if(depths[lowpts[curr_node]] > depths[newLowPT]) lowpts[curr_node] = newLowPT;
				else break;
				
				currEdges = treeEdges.Where(((TNode, TNode) n) => n.Item2.Equals(curr_node)).ToArray();
				curr_node = currEdges[0].Item1;
			} while(currEdges.Length > 0);
		}
				
		Action<TNode, TNode>  UnmarkedVisitProcess = (TNode current, TNode next) =>
		{
			treeEdges.Add((current, next));
			if(depths.Keys.Contains(next)) return;
			
			depths.Add(next, depths[current]+1);
			lowpts.Add(next, next);
			
		};

		Action<TNode, TNode>  MarkedVisitProcess = (TNode current, TNode next) =>
		{
			if(depths[lowpts[current]] > depths[next]) UpdateLowPT(current, next);
		};

        ForEachNodeByBFSIn(graph, root, null, UnmarkedVisitProcess, MarkedVisitProcess);
		
		foreach(TNode node in graph)
		{
			if(node.Equals(root)) continue;
			foreach ((TNode nextNode, int weight) in graph.GetTargetsOf(node))
			{
				if(lowpts[nextNode].Equals(nextNode) || lowpts[nextNode].Equals(node)) 
				if(!articulations.Contains(node)) articulations.Add(node);
			}
		}
		if(treeEdges.Where(((TNode, TNode) n) => n.Item1.Equals(root) || n.Item2.Equals(root)).Count() > 1) articulations.Add(root);
		return articulations;
	}

	/// <summary>
	/// Returns a list with the connected components of a given graph.
	/// </summary>
	/// <param name="graph">The graph to get connected components from</param>
	/// <returns>A List with the connected components of the graph</returns>
	public static List<List<T>> ListConnectedComponents<T>(IGraph<T> graph)
		where T : TNode, new()
	{
		List<List<T>> connectedComponents = new List<List<T>>();
		List<T> currentComponent = new List<T>();

        var remainingNodes = Enumerable.Range(0, graph.Count).ToList();

        Action<TNode> visitedNewNode = (TNode node) => {
			remainingNodes.RemoveAt(node.Index);
			currentComponent.Add((T)node);
		};

        while(remainingNodes.Count > 0)
        {
           	ForEachNodeByDFSIn((TGraph)graph, graph[remainingNodes[0]], visitedNewNode);
			connectedComponents.Add(currentComponent);
        }
        return connectedComponents;
	}

	

	
#endregion NODE_LISTING


#region SORTING


/*
	FIXME:
	public static void UpdateNodeTopSorting<T>(IGraph<T> digraph)
	where T: TNode, new()
	 => digraph = TopSortNodes(digraph);

	/// <summary>
	/// Runs a Topology Sort trough a IDigraph.
	/// </summary>
	/// <param name="spellGraph">The IDigraph to be sorted</param>
	/// <returns>The IGraph's nodes sorted by topology.</returns>
	public static List<T> TopSortNodes<T>(IDigraph<T> spellGraph)
	where T: TNode, new()
	{

		Dictionary<T, bool> markedNodes = InitializePairType(spellGraph, false);
		void TopologicalSortUtil(T node, ref Stack<TNode> stack)
		{
			markedNodes[node] = true;
			spellGraph.ForeachTargetOf(node, (T n, int i) =>
			{
				if (!markedNodes[n]) TopologicalSortUtil(n, ref stack);
			});
			stack.Push(node);
		}
		
		Stack<TNode> stack = new Stack<TNode>();
		foreach (T node in spellGraph) {
			if (!markedNodes[node] && (spellGraph.OutwardsDegree(node) + spellGraph.InwardsDegree(node) > 0)) 
				TopologicalSortUtil(node, ref stack);
		}
		List<T> sortedArray = new List<T>(spellGraph.Count);
		while(stack.Count > 0){ 
			sortedArray.Append((T)stack.Pop());
		}
		return sortedArray;
	}
*/
#endregion SORTING

}

public class Path<T> : List<T> 
	where T : IGraphNode
{
	public int size = 0;
	
	public void Add(T item, int weight)
	{
		size += weight;
		Add(item);
	}

}


