
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
	public static Dictionary<T1, T2> InitializePairType<T1, T2>(IEnumerable<T1> nodes, T2 defValue)
	{
		Dictionary<T1, T2> dict = new Dictionary<T1,T2>();
		foreach(T1 n in nodes)
		{
			dict.Add(n, defValue);
		}
		return dict;
	}
	public static Dictionary<ISpellGraphNode, T> InitializePairType<T>(IEnumerable<ISpellGraphNode> nodes) 
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
	public static void ForEachNodeByBFSIn(IGraph<ISpellGraphNode> spellGraph, ISpellGraphNode startingNode, 
		Action<ISpellGraphNode>        VisitationProcess = null, 
		Action<ISpellGraphNode, ISpellGraphNode>  UnmarkedVisitProcess = null,
		Action<ISpellGraphNode, ISpellGraphNode>  MarkedVisitProcess = null
	){
		if(spellGraph.Count == 0) return;
		Dictionary<ISpellGraphNode, bool> markedNodes = InitializePairType(spellGraph, false);
		Queue<ISpellGraphNode> queue = new Queue<ISpellGraphNode>();
		queue.Enqueue(startingNode);

		while (queue.Count > 0)
		{   
			ISpellGraphNode currNode = queue.Dequeue();
			if(markedNodes[currNode]) continue;
				
			spellGraph.ForeachTargetOf(currNode, (ISpellGraphNode nextNode, int weight) =>
			{
				if(markedNodes[nextNode]) { 
					MarkedVisitProcess?.Invoke(currNode, nextNode);
				}
				
				UnmarkedVisitProcess?.Invoke(currNode, nextNode);
				queue.Enqueue(nextNode);
			});

			VisitationProcess?.Invoke(currNode);
			markedNodes[currNode] = true;
		}
	}
    public static void ForEachNodeByBFSIn(IGraph<ISpellGraphNode> spellGraph) => ForEachNodeByBFSIn(spellGraph, spellGraph[0]);
	
	/// <summary>
	/// Runs a full Depth First Search trought the graph's nodes and executes a choosen expression at each node.
	/// </summary>
	/// <param name="spellGraph">The graph where the search will be performed </param>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <param name="VisitationProcess">An Action Delegate that operates after a node is visited.</param>
	/// <param name="UnmarkedVisitProcess">An Action Delegate that operates for each unvisited neighbor of the node being visited.</param>
	/// <param name="MarkedVisitProcess">An Action Delegate that operates for each visited neighbor of the node being visited.</param>
	public static void ForEachNodeByDFSIn(IGraph<ISpellGraphNode> spellGraph, ISpellGraphNode startingNode, 
		Action<ISpellGraphNode>        VisitationProcess = null, 
		Action<ISpellGraphNode, ISpellGraphNode>  UnmarkedVisitProcess = null,
		Action<ISpellGraphNode, ISpellGraphNode>  MarkedVisitProcess = null
	) 
	{
		if(spellGraph.Count == 0) return;
		Dictionary<ISpellGraphNode, bool> markedNodes = InitializePairType(spellGraph, false);
		Stack<ISpellGraphNode> stack = new Stack<ISpellGraphNode>();
		stack.Push(startingNode);

		while (stack.Count > 0)
		{   
			ISpellGraphNode currNode = stack.Pop();
			if(markedNodes[currNode]) continue;

			spellGraph.ForeachTargetOf(currNode, (ISpellGraphNode nextNode, int weight) =>
			{
				if(markedNodes[nextNode]) { 
					MarkedVisitProcess?.Invoke(currNode, nextNode);
				}
				
				UnmarkedVisitProcess?.Invoke(currNode, nextNode);
				stack.Push(nextNode);
			});
			VisitationProcess?.Invoke(currNode);
			markedNodes[currNode] = true;
		}
	}
	public static void ForEachNodeByDFSIn(IGraph<ISpellGraphNode> spellGraph) => ForEachNodeByDFSIn(spellGraph, spellGraph[0]);

	/// <summary>
	/// Runs a choosen expression at each node of a graph.
	/// </summary>
	/// <param name="spellGraph">The graph used </param>
	/// <param name="Process">An Action Delegate that executes with each node as parameter.</param>
	public static void ForEachNodeIn(IGraph<ISpellGraphNode> spellGraph, Action<ISpellGraphNode> Process) {
		foreach(ISpellGraphNode node in spellGraph) Process(node);
	}
#endregion SEARCHES_ITERATIONS

#region GRAPH_PROPERTY_CHECKS

	/// <summary>
	/// Returns whether the graph is Connected
	/// </summary>
	/// <param name="graph">The graph to check</param>
	/// <returns>true if there is only one connected component in the graph, false otherwise.</returns>
	public static bool IsGraphConnected<T>(IGraph<T> graph) 
	where T : ISpellGraphNode, new()
		=> ListConnectedComponents(graph).Count == 1;

	/// <summary>
	/// Returns whether the graph is Bipartite
	/// </summary>
	/// <param name="graph">The graph to check</param>
	/// <returns>true the graph is bipartite, false otherwise.</returns>
	public static bool IsGraphBipartite<T>(IGraph<T> graph)
		where T : ISpellGraphNode, new()
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
			
			graph.ForeachTargetOf(currNode, (T nextNode, int weight) => 
			{
				if(!markedNodes[nextNode.Index].Item1)
				{
					markedNodes[nextNode.Index] = (true, -brand);
					stack.Push(nextNode);
				}
				else if(markedNodes[nextNode.Index].Item2 == brand) lastIteration = true;
			});
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
	public static bool HasCycle(IGraph<ISpellGraphNode> graph, ISpellGraphNode startingNode)
	{
		bool cycled = false;

		Action<ISpellGraphNode, ISpellGraphNode>  MarkedVisitProcess = (ISpellGraphNode n1, ISpellGraphNode n2) => 
		{  cycled = true; };

		ForEachNodeByDFSIn(graph, startingNode, null, null, MarkedVisitProcess);

		return cycled;
	}


	public static bool HasCycle<T>(IGraph<T> graph, T startingNode)
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

		ForEachNodeByDFSIn((IGraph<ISpellGraphNode>)graph, startingNode, null, UnmarkedVisitProcess, MarkedVisitProcess);

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
	where T : ISpellGraphNode, new()
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
	public static List<ISpellGraphNode> ListArticulations(IGraph<ISpellGraphNode> graph, ISpellGraphNode root)
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
		
		foreach(ISpellGraphNode node in graph)
		{
			if(node.Equals(root)) continue;
			graph.ForeachTargetOf(node, (ISpellGraphNode next, int weight) =>
			{
				if(lowpts[next].Equals(next) || lowpts[next].Equals(node)) 
				if(!articulations.Contains(node)) articulations.Add(node);
			});
		}
		if(treeEdges.Where(((ISpellGraphNode, ISpellGraphNode) n) => n.Item1.Equals(root) || n.Item2.Equals(root)).Count() > 1) articulations.Add(root);
		return articulations;
	}

	/// <summary>
	/// Returns a list with the connected components of a given graph.
	/// </summary>
	/// <param name="graph">The graph to get connected components from</param>
	/// <returns>A List with the connected components of the graph</returns>
	public static List<List<T>> ListConnectedComponents<T>(IGraph<T> graph)
	where T : ISpellGraphNode, new()
	{
		List<List<T>> connectedComponents = new List<List<T>>();
		List<T> currentComponent = new List<T>();

        var remainingNodes = Enumerable.Range(0, graph.Count).ToList();

        Action<ISpellGraphNode> visitedNewNode = (ISpellGraphNode node) => {
			remainingNodes.RemoveAt(node.Index);
			currentComponent.Add((T)node);
		};

        while(remainingNodes.Count > 0)
        {
           	ForEachNodeByDFSIn((IGraph<ISpellGraphNode>)graph, graph[remainingNodes[0]], visitedNewNode);
			connectedComponents.Add(currentComponent);
        }
        return connectedComponents;
	}

	

	
#endregion NODE_LISTING

#region TREES
	
	/// <summary>
	/// Receives a void IGraph and a Prüffer sequence, and returns the tree from that sequence.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="originalGraph">The void graph</param>
	/// <param name="pruffer">The list that stores a pruffer sequence</param>
	/// <returns></returns>
	/// <exception cref="FormatException"></exception>
	/*
	FIXME: public static TGraph PrufferToTree<TGraph, TNode>(TGraph originalGraph, List<int> pruffer) 
		where TNode : ISpellGraphNode, new()
		where TGraph : IGraph<TNode>, new()
	{
		if(originalGraph.EdgeAmmount() > 0)   		throw new FormatException("The chosen graph is not void.");
		if(originalGraph.Count != pruffer.Count+2) 	throw new FormatException("Incongruent graph and pruffer sequence sizes.");

		TGraph graph = new TGraph();
		graph = originalGraph;

		Dictionary<TNode, int> degrees = InitializePairType(graph, 1);

		foreach(int p in pruffer) { degrees[graph[p]]++; }

		foreach(int p in pruffer)
		{
			foreach(TNode n in graph)
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
	}*/


	/// <summary>
	/// Auxiliar class to keep track of tree connections. Used to create forests that may eventualy be linked together into a single tree. 
	/// </summary>
	private class TreeRank
	{
		private TreeRank treeRoot;


		/// <summary>
		/// represents how many nodes are connected to this tree. Unused value if this tree is not the head. 
		/// </summary>
		public int rank = 1;

		public TreeRank()
		{
			treeRoot = this;
		}
		
		//this WILL break if there is a cycle on the tree (i.e. if its not a tree at all)
		private void UpdateHead()
		{
			while(this.treeRoot.treeRoot != this.treeRoot)
			{
				this.treeRoot = this.treeRoot.treeRoot;
			} 
		}

		public bool TryMergeTo(TreeRank tree)
		{
			UpdateHead();
			tree.UpdateHead();
			if(this.treeRoot == tree.treeRoot) return false;
			treeRoot = tree;
			treeRoot.rank += this.rank;
			return true;
		}
	}

	/// <summary>
	/// Runs the Kruskal algorithm to find the Minimum Spanning Tree using a given comparer function
	/// </summary>
	/// <typeparam name="TGraph">The type of the graph.</typeparam>
	/// <typeparam name="TNode">The type of the nodes.</typeparam>
	/// <typeparam name="TResult">The type of the resulting spanning tree graph.</typeparam>
	/// <param name="graph">The original graph to get a spanning tree from.</param>
	/// <param name="comparer">A comparison between the graph's edge's weights.</param>
	/// <returns>A minimum spanning tree from the graph</returns>
		public static TResult Kruskal<TGraph, TNode, TResult>(TGraph graph, Comparison<int> comparer)
		where TNode : ISpellGraphNode, new()
		where TGraph : IGraph<TNode>,  new()
		where TResult : IGraph<TNode>, new()
	{
		TResult mstree = new TResult();
		foreach(var node in graph ) mstree.Add(node);

		Dictionary<int, TreeRank> forest = InitializePairType(Enumerable.Range(0, graph.Count).ToList(), new TreeRank());

		List<(int, int, int)> edges = new List<(int, int, int)>();

		graph.ForeachEdge( (TNode src, TNode trg, int weight) => edges.Add((src.Index, trg.Index, weight)) );
		
		edges.AsEnumerable().OrderBy(edge => edge.Item3).ToList();		
		
		foreach(var weightedEdge in edges)
		{
			if(forest[weightedEdge.Item1].TryMergeTo(forest[weightedEdge.Item2]))
			{
				mstree.Connect(mstree[weightedEdge.Item1], mstree[weightedEdge.Item2], weightedEdge.Item3);
				if(forest[weightedEdge.Item2].rank == graph.Count) break;
			}

		}
		return mstree;
	}



#endregion TREES

#region SORTING


/*
	public static void UpdateNodeTopSorting<T>(IGraph<T> digraph)
	where T: ISpellGraphNode, new()
	 => digraph = TopSortNodes(digraph);

	/// <summary>
	/// Runs a Topology Sort trough a IDigraph.
	/// </summary>
	/// <param name="spellGraph">The IDigraph to be sorted</param>
	/// <returns>The IGraph's nodes sorted by topology.</returns>
	public static List<T> TopSortNodes<T>(IDigraph<T> spellGraph)
	where T: ISpellGraphNode, new()
	{

		Dictionary<T, bool> markedNodes = InitializePairType(spellGraph, false);
		void TopologicalSortUtil(T node, ref Stack<ISpellGraphNode> stack)
		{
			markedNodes[node] = true;
			spellGraph.ForeachTargetOf(node, (T n, int i) =>
			{
				if (!markedNodes[n]) TopologicalSortUtil(n, ref stack);
			});
			stack.Push(node);
		}
		
		Stack<ISpellGraphNode> stack = new Stack<ISpellGraphNode>();
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

#region GRAPH_CONVERSION
	/*
	FIXME:
	public static TResult ConvertGraphTo<TResult, TNode>(IGraph<TNode> originalGraph)
		where TNode : ISpellGraphNode, new()
		where TResult : IGraph<TNode>, new()
	{
		TResult result = new TResult();
		result = originalGraph;
		originalGraph.ForeachEdge((src, trg, w) => result.Connect(result[src.Index], result[trg.Index], w));
		return result;  
	}*/

#endregion GRAPH_CONVERSION

#region PATHFINDING

	
	//REVIEW:
	/// <summary>
    /// Executes a breadth-first search and finds the shortest path between two nodes of a graph, if it exists, using Dijkstra algorithm.
    /// </summary>
    /// <param name="spellGraph">The graph where the search will be performed </param>
    /// <param name="startingNode">The spellGraph's node where the path starts</param>
    /// <param name="endingNode">The spellGraph's node where the path ends</param>

    /// <returns> A List of nodes forming the path </returns>
	public static Path BellmanFord<TGraph>(TGraph spellGraph, ISpellGraphNode startingNode, ISpellGraphNode endingNode
	)
		where TGraph : IGraph<ISpellGraphNode>, new() 
	{		
		if(!spellGraph.Contains(startingNode) || !spellGraph.Contains(endingNode)) 
			throw new InvalidOperationException("The starting or ending nodes are not contained in the graph");

		if(spellGraph.Count <= 1)
		{
			Path p = new Path();
			p.AddRange(spellGraph);
			return p;
		}

		var distances = new List<int>();
		var predecessors = new List<ISpellGraphNode>();
		
		foreach(var node in spellGraph)
		{
			distances.Add(int.MaxValue);
			predecessors.Add(null);
		}

		distances[startingNode.Index] = 0;
		
		var queue = new PriorityQueue<ISpellGraphNode, int>();
		queue.Enqueue(startingNode, 0);
	
		foreach(var current in spellGraph)
		{
			spellGraph.ForeachTargetOf(current, (ISpellGraphNode next, int weight) => 
			{
				if(distances[next.Index] > distances[current.Index] + weight)
				{
					distances[next.Index] = distances[current.Index] + weight;
					predecessors[next.Index] = current;
					queue.Enqueue(spellGraph[next.Index], distances[next.Index]);
				}
			});
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

	//REVIEW:
	/// <summary>
    /// Executes a breadth-first search and finds the shortest path between two nodes of a graph, if it exists, using Dijkstra algorithm.
    /// </summary>
    /// <param name="spellGraph">The graph where the search will be performed </param>
    /// <param name="startingNode">The spellGraph's node where the path starts</param>
    /// <param name="endingNode">The spellGraph's node where the path ends</param>
    /// <returns> A List of nodes forming the path </returns>
	public static Path Dijkstra<TGraph>(TGraph spellGraph, ISpellGraphNode startingNode, ISpellGraphNode endingNode)
		where TGraph : IGraph<ISpellGraphNode>, new() 
	{
		
		if(!spellGraph.Contains(startingNode) || !spellGraph.Contains(endingNode)) 
			throw new InvalidOperationException("The starting or ending nodes are not contained in the graph");

		if(spellGraph.Count <= 1) 
		{
			Path p = new Path();
			p.AddRange(spellGraph);
			return p;
		}

		var distances = new List<int>();
		var predecessors = new List<ISpellGraphNode>();

		foreach(var node in spellGraph)
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
			
			spellGraph.ForeachTargetOf(current, (ISpellGraphNode next, int weight) => 
			{
				if(weight < 0) throw new InvalidOperationException("IGraph contains negative edges.");

				if(distances[next.Index] > distances[current.Index] + weight)
				{
					distances[next.Index] = distances[current.Index] + weight;
					predecessors[next.Index] = current;
					queue.Enqueue(spellGraph[next.Index], distances[next.Index]);
				}
			});
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

	/// <summary>
	///	Runs Floyd-Warshall algorithm to find
	/// </summary>
	/// <typeparam name="TGraph"></typeparam>
	/// <typeparam name="TNode"></typeparam>
	/// <param name="spellGraph"></param>
	/// <returns></returns>
	public static int[,] FloydWarshall<TGraph, TNode>(TGraph spellGraph)
		where TGraph : IGraph<TNode>, new() 
		where TNode : ISpellGraphNode, new() 
	{
		int size = spellGraph.Count();
		int[,] distances = new int[size, size];
		int[,] predecessors = new int[size, size];

		for(int i = 0; i < size; i++){ 
		for(int j = 0; j < size; j++){
			distances[i, j] = int.MaxValue;
			predecessors[i, j] = -1;	
		}}
		if(spellGraph.Flags.Contains(GraphFlag.WEIGHTED))
		{
			spellGraph.ForeachEdge((src, trg, weight) => {
				int i = src.Index;
				int j = trg.Index;
				distances[i,j] = weight;
				predecessors[i,j] =  i;

			});
		}
		else
		{
			spellGraph.ForeachEdge((src, trg, weight) => {
				int i = src.Index;
				int j = trg.Index;
				distances[i,j] = weight;
				predecessors[i,j] =  i;
			});
		}
		
		for(int i = 0; i < size; i++){ 
		for(int j = 0; j < size; j++){
			for(int h = 0; h < size; h++){
				if(distances[i, j] > distances[i, h] + distances[h,  j])
				{
					distances[i, j] = distances[i, h] + distances[h,  j];
					predecessors[i, j] = predecessors[h, j];
				}
			}
		}}

		return predecessors;
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
		where TGraph : IGraph<ISpellGraphNode>, new() 
    {
		

        if(spellGraph.Count == 0) return new Path();
        Dictionary<ISpellGraphNode, PathNode> nodeStats = InitializePairType(spellGraph.Cast<ISpellGraphNode>().ToList(), new PathNode());
        		
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