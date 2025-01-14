
using System;
using System.Diagnostics.Debug;
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
				
			spellGraph.ForeachTargetOf(currNode, (TNode nextNode, int weight) =>
			{
				if(markedNodes[nextNode]) { 
					MarkedVisitProcess?.Invoke(currNode, nextNode);
				}
				else 
				{
					UnmarkedVisitProcess?.Invoke(currNode, nextNode);
					queue.Enqueue(nextNode);
				}
				
			});

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
			spellGraph.ForeachTargetOf(currNode, (TNode nextNode, int weight) =>
			{
				if(markedNodes[nextNode]) { 
					MarkedVisitProcess?.Invoke(currNode, nextNode);
				}
				else
				{
					UnmarkedVisitProcess?.Invoke(currNode, nextNode);
					stack.Push(nextNode);
				}
			});
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
			graph.ForeachTargetOf(node, (TNode next, int weight) =>
			{
				if(lowpts[next].Equals(next) || lowpts[next].Equals(node)) 
				if(!articulations.Contains(node)) articulations.Add(node);
			});
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
		where TNode : TNode, new()
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
		private TreeRank treeHead;


		/// <summary>
		/// represents how many nodes are connected to this tree. Unused value if this tree is not the head. 
		/// </summary>
		public int rank = 1;

        public int Index {get; set; }

        public TreeRank(int idx)
		{
			treeHead = this;
			Index = idx;
		}
		
		//this WILL break if there is a cycle on the tree (i.e. if its not a tree at all)
		private void UpdateHead()
		{
			while(treeHead != treeHead.treeHead)
			{
				treeHead = treeHead.treeHead;
			} 
		}

		public bool TryMergeTo(TreeRank tree)
		{
			UpdateHead();
			tree.UpdateHead(); 
			
			if(tree.treeHead == treeHead) 
				return false;
			tree.treeHead.rank += treeHead.rank;
			treeHead.treeHead = tree.treeHead;
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
	public static TResult Kruskal<TResult>(TGraph graph, Comparison<int> comparer)
		where TResult : IGraph<TNode>, new()
	{
		TResult mstree = new TResult();
		foreach(var node in graph ) mstree.Add(node);

		List<TreeRank> forest = new List<TreeRank>();
		for(int i = 0; i < graph.Count; i++) forest.Add( new TreeRank(i) ); 
		
		List<(int, int, int)> edges = new List<(int, int, int)>();

		graph.ForeachEdge( (TNode src, TNode trg, int weight) => edges.Add((src.Index, trg.Index, weight)) );
		
		edges = edges.AsEnumerable().OrderBy(edge => edge.Item3).ToList();		
		
		foreach(var weightedEdge in edges)
		{
			if(forest[weightedEdge.Item1].TryMergeTo(forest[weightedEdge.Item2]))
			{
				mstree.Connect(mstree[weightedEdge.Item1], mstree[weightedEdge.Item2], weightedEdge.Item3);
				if(Mathf.Max(forest[weightedEdge.Item1].rank, forest[weightedEdge.Item2].rank) >= graph.Count) 
					break;
			}

		}
		return mstree;
	}


	public static TResult Prim<TResult>(TGraph graph, Comparison<int> comparer)
		where TResult : IGraph<TNode>, new()
	{
		TResult mstree = new TResult();
		foreach(var node in graph) mstree.Add(node);
		
		int unlinkedNodes = graph.Count;
		List<bool> linked = Enumerable.Repeat(false, unlinkedNodes).ToList();
		
		
		List<(int src, int trg, int weight)> viableEdges = new();
		graph.ForeachEdge( (TNode src, TNode trg, int weight) => 
			{
				viableEdges.Add((src.Index, trg.Index, weight));
			}
		);
		viableEdges = viableEdges.OrderBy(edge => edge.weight).ToList();

		//Avoid inconsistences in reference-type TNode's Connection
		foreach(TNode src in graph) foreach(TNode trg in graph) graph.Disconnect(src, trg);


		unlinkedNodes--;
		linked[0] = true;

		while(unlinkedNodes > 0 && viableEdges.Count > 0)
		{
			//Find the correct Edge

			for(int i = 0; i < viableEdges.Count; i++)
			{
				var currEdge = viableEdges[i];
				if(linked[currEdge.src] && !linked[currEdge.trg])
				{
					viableEdges.RemoveAll(edge => edge.trg == currEdge.trg);
					linked[currEdge.trg] = true;
					unlinkedNodes--;
					mstree.Connect(mstree[currEdge.src], mstree[currEdge.trg], currEdge.weight);
					break;
				}	
			}
		}

		return mstree;
	}


    public static TResult ChuliuEdmonds<TResult>(TGraph graph, Comparison<int> comparer)
		where TResult : IGraph<TNode>, new()

	{
		
		if(graph.Count < 2) 
			return new TResult();

		var rootIndex = 0;
		foreach(var node in graph)
		{
			int visitedCount = 0;

			ForEachNodeByDFSIn(graph, graph[rootIndex], (TNode _) => visitedCount++);

			if(visitedCount == graph.Count) {break;}
			else {rootIndex ++;}
		}
		if(rootIndex == graph.Count) 
			throw new InvalidOperationException("The chosen directed graph does not have a spanning arborescence.");

		var clusterGraph = InitClusterGraph(graph);
		clusterGraph = ChuliuEdmondsRecursive(clusterGraph, clusterGraph[rootIndex], comparer);


		List<(TNode src, TNode trg)> edges = new();
		graph.ForeachEdge((TNode src, TNode trg, int weight) => {
			edges.Add((src, trg));
		});
		foreach(var edge in edges) graph.Disconnect(edge.src, edge.trg);

		return Convert<TResult, TNode, ClusterNode>(clusterGraph, (ClusterNode node) => (TNode)node.clusteredNodes[0]);
		
	}

	public static MatrixGraph<ClusterNode> ChuliuEdmondsRecursive(MatrixGraph<ClusterNode> graph, ClusterNode root, Comparison<int> comparer)
	{
		var clusteredGraph = InitClusterGraph(graph);

		var minimumEdges = FindMinimumEdges(clusteredGraph, root);

		var unreachedNodes = new List<ClusterNode>(graph);
		GraphUtil<MatrixGraph<ClusterNode>, ClusterNode>.ForEachNodeByDFSIn(graph, root, (ClusterNode current) => unreachedNodes.Remove(current) );

		if(unreachedNodes.Count == 0)
		{
			Uncluster(clusteredGraph);
			return clusteredGraph;
		}

		var lastUnreached = unreachedNodes.Last();
		unreachedNodes.RemoveAt(lastUnreached.Index);
		
		GraphUtil<ClusteredGraph<T>, T>.ForEachNodeByDFSIn(graph, r, null, null,
		(T source, T target) => 
		{
			cycle.Add(target);
			//retrieves the cycle and the points where it exits
			T cycleNode = source;
			
			while(cycleNode.Index != target.Index)
			{
				cycle.Add(cycleNode);
				graph.ForeachSourceOf(cycleNode,(T src, int weight) => {
					cycleNode = src;
				});
			}

		},
		() => cycle.Count > 0
			);


		return clusteredGraph;
	}

	private static MatrixGraph<ClusterNode> InitClusterGraph<T>(IGraph<T> graph) where T : IGraphNode 
	{
		MatrixGraph<ClusterNode> clusteredGraph = new();

		for(int i = 0; i < graph.Count; i++)
		{
			var node = graph[i];

			Debug.Assert(i == node.Index);

			var clusterNode = new ClusterNode();
			clusterNode.clusteredNodes.Add(node);
			clusteredGraph.Add(clusterNode);
		}

		graph.ForeachEdge((T src, T trg, int weight) => {
			clusteredGraph.Connect(clusteredGraph[src.Index], clusteredGraph[trg.Index], weight);
		});

		return clusteredGraph;
	}

	public static List<(ClusterNode src, ClusterNode trg)> FindMinimumEdges(MatrixGraph<ClusterNode> graph, ClusterNode root)
	{
		List<(ClusterNode src, ClusterNode trg)> minimumEdges = new();

		foreach(var node in graph) {	
			if(node.Index == root.Index)
				continue;
			
			
			int minWeight = int.MaxValue;
			ClusterNode minWeightSource = default;
			graph.ForeachSourceOf(node, (ClusterNode src, int weight) => {
				if(weight < minWeight)
				{
					minWeightSource = src;
					minWeight = weight;
				}
			});
			Debug.Assert(minWeight != int.MaxValue);
			minimumEdges.Add((minWeightSource, node));
		}

		return minimumEdges;
	}

	
	public static void Uncluster(MatrixGraph<ClusterNode> graph)
	{	
		//TODO:
	}



		/*
	{
		TResult mstree = new TResult();

		foreach(var node in graph) mstree.Add(node.Castable);

		if(graph.Count < 2) 
			return mstree;


		int rootIndex = 0;

		foreach(var node in graph)
		{
			int visitedCount = 0;

			ForEachNodeByDFSIn(graph, graph[rootIndex], (TNode _) => visitedCount++);

			if(visitedCount == graph.Count) {break;}
			else {rootIndex ++;}
		}

		if(rootIndex == graph.Count) 
			throw new InvalidOperationException("The chosen directed graph does not have a spanning arborescence.");

		ClusteredGraph<TNode> clusteredGraph = new();
		foreach(var node in graph) clusteredGraph.Add(node);
		graph.ForeachEdge((TNode src, TNode trg, int weight) => {
			clusteredGraph.Connect(src, trg, weight);
		});

		clusteredGraph.root = graph[rootIndex];
		clusteredGraph.cycleEdges = new();

		clusteredGraph = ChuliuEdmondsRecursive(clusteredGraph, comparer, graph[rootIndex]);

		int edgecount = 0;
		clusteredGraph.ForeachEdge((TNode src, TNode trg, int weight) => {
			edgecount++;
			mstree.Connect(mstree[src.Index], mstree[trg.Index], weight);
		});

		return mstree;
	}

    private static ClusteredGraph<T> ChuliuEdmondsRecursive<T>(ClusteredGraph<T> original, Comparison<int> comparer, T root)
		where T : ISpellGraphNode, new()
	{

		ClusteredGraph<T> graph = new();
		foreach(var node in original) graph.Add(node);
		original.ForeachEdge((T src, T trg, int weight) => {
			graph.Connect(src, trg, weight);
		}); 
		graph.root = root;

		graph.MakeMinimumArborescence();

		List<T> unreachedNodes = new ();
		foreach(var node in graph) unreachedNodes.Add(node);

		GraphUtil<ClusteredGraph<T>, T>.ForEachNodeByDFSIn(graph, root, (T current) => unreachedNodes.Remove(current) );

		if(unreachedNodes.Count == 0) return graph;
		List<T> cycle = new();
		do{		
			var r = unreachedNodes.Last();
			unreachedNodes.Remove(r);
			
			GraphUtil<ClusteredGraph<T>, T>.ForEachNodeByDFSIn(graph, r, null, null,
			(T source, T target) => 
			{
				cycle.Add(target);
				//retrieves the cycle and the points where it exits
				T cycleNode = source;
				
				while(cycleNode.Index != target.Index)
				{
					cycle.Add(cycleNode);
					graph.ForeachSourceOf(cycleNode,(T src, int weight) => {
						cycleNode = src;
					});
				}

			},
			() => cycle.Count > 0
			);
		}
		while(cycle.Count == 0 && unreachedNodes.Count > 0);

		var clusteredGraph = new ClusteredGraph<T>(graph, original, root, cycle);
		clusteredGraph.edges = ChuliuEdmondsRecursive(clusteredGraph, comparer, root).edges;
		clusteredGraph.Cycle = cycle;
		clusteredGraph.Uncluster();
		graph = clusteredGraph;
		graph.Disconnect(cycle[1], cycle[0]);


		return graph;
	}
*/


#endregion TREES

#region SORTING


/*
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

#region GRAPH_CONVERSION
	
	FIXME:
	public static T Convert<T, TN, TNO>(this IGraph<TNO> graph, Func<TNO, TN> convertNodeProcess = null)
		where T : IGraph<TN>, new()
		where TN : IGraphNode, new()
		where TNO : IGraphNode, new()
	{
		if(convertNodeProcess == null)
			convertNodeProcess = (TNO node) => new TN{Index = node.Index, Position = node.Position};
		T result = new T();
		foreach(var node in graph) 
		{
			result.Add(convertNodeProcess(node));
		}
		graph.ForeachEdge((src, trg, w) => result.Connect(result[src.Index], result[trg.Index], w));
		return result;  
	}


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
	public static Path<TNode> BellmanFord(TGraph spellGraph, TNode startingNode, TNode endingNode)
	{		
		if(!spellGraph.Contains(startingNode) || !spellGraph.Contains(endingNode)) 
			throw new InvalidOperationException("The starting or ending nodes are not contained in the graph");

		if(spellGraph.Count <= 1)
		{
			Path<TNode> p = new Path<TNode>();
			p.AddRange(spellGraph.AsEnumerable().Cast<TNode>());
			return p;
		}

		var distances = new List<int>();
		var predecessors = new List<TNode>();
		
		foreach(var node in spellGraph)
		{
			distances.Add(int.MaxValue);
			predecessors.Add(default(TNode));
		}

		distances[startingNode.Index] = 0;

		bool negativeCycleFound = false;
		for(int i = 0; i < spellGraph.Count; i++)
		{
			spellGraph.ForeachEdge((TNode src, TNode trg, int weight) => 
			{
				if(distances[src.Index] != int.MaxValue && distances[src.Index] + weight < distances[trg.Index])
				{
					if(i == spellGraph.Count) negativeCycleFound = true;
					distances[trg.Index] = distances[src.Index]  + weight;
					predecessors[trg.Index] = src;
				}
			});
		}

		Path<TNode> path = new Path<TNode>();
		if(predecessors[endingNode.Index] == null || negativeCycleFound)
			return path;

		path.Add(endingNode);

		while(predecessors[path.Last().Index] != null)
			path.Add(predecessors[path.Last().Index]);

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
	public static Path<TNode> Dijkstra(TGraph spellGraph, TNode startingNode, TNode endingNode)
	{
		
		if(!spellGraph.Contains(startingNode) || !spellGraph.Contains(endingNode)) 
			throw new InvalidOperationException("The starting or ending nodes are not contained in the graph");

		if(spellGraph.Count <= 1) 
		{
			Path<TNode> p = new Path<TNode>();
			p.AddRange(spellGraph.AsEnumerable().Cast<TNode>());
			return p;
		}

		var distances = Enumerable.Repeat(int.MaxValue, spellGraph.Count).ToList();
		var predecessors = Enumerable.Repeat<TNode>(default, spellGraph.Count).ToList();
		var unvisited = spellGraph.ToList();

		distances[startingNode.Index] = 0;
		
		do
		{
			TNode current = unvisited.MinBy((TNode n) => distances[n.Index]);
			unvisited.Remove(current);
			 
			
			spellGraph.ForeachTargetOf(current, (TNode next, int weight) => 
			{
				if(distances[current.Index] + weight < distances[next.Index])
				{
					distances[next.Index] = distances[current.Index] + weight;
					predecessors[next.Index] = current;
				}
			});
		}
		while (unvisited.Count > 0);

		Path<TNode> path = new Path<TNode>();
		if(predecessors[endingNode.Index] == null)
			return path;

		path.Add(endingNode);

		while(predecessors[path.Last().Index] != null)
			path.Add(predecessors[path.Last().Index]);
			
		return path;

	}

	/// <summary>
	///	Runs Floyd-Warshall algorithm to find the shortest paths between any two nodes on the graph
	/// </summary>
	/// <typeparam name="TGraph"></typeparam>
	/// <typeparam name="TNode"></typeparam>
	/// <param name="spellGraph"></param>
	/// <returns>A bidimentional array with were arr[x][y] is the index of the predecessor of the node y, on the path from x to y.</returns>
	public static int[,] FloydWarshall(TGraph spellGraph)
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
		
		for(int k = 0; k < size; k++){ 
			for(int i = 0; i < size; i++){ for(int j = 0; j < size; j++)
			{
				if(distances[i, k] == int.MaxValue) continue;
				if(distances[k, j] == int.MaxValue) continue;
				if(distances[i, j] == int.MaxValue || distances[i, j] > distances[i, k] + distances[k,  j])
				{
					distances[i, j] = distances[i, k] + distances[k,  j];
					predecessors[i, j] = predecessors[k, j];
				}
			}}
		}

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

    public static Path Dijkstra<TGraph>(TGraph spellGraph, TNode startingNode, TNode endingNode, 
        Action<TNode>           			VisitationProcess = null, 
        Action<TNode, TNode>    UnmarkedVisitProcess = null,
        Action<TNode, TNode>    MarkedVisitProcess = null
	)	
		where TGraph : TGraph, new() 
    {
		

        if(spellGraph.Count == 0) return new Path();
        Dictionary<TNode, PathNode> nodeStats = InitializePairType(spellGraph.Cast<TNode>().ToList(), new PathNode());
        		
		var edgeWeight = ((TNode, TNode) edge) => { return 1;};
		if(spellGraph is IWeighted<TNode>)
		{
			Dictionary<(TNode, TNode), int> weightedEdges = ((IWeighted<TNode>)spellGraph).WeightedEdges;
			edgeWeight = ((TNode, TNode) edge) => { return weightedEdges[edge]; };
		}
		

        nodeStats[startingNode].Distance = 0;
        
        UnmarkedVisitProcess += (TNode current, TNode next) => 
		{
			nodeStats[next].Distance = edgeWeight((current, next)) + nodeStats[current].Distance;
			nodeStats[next].Parent = current;
		};

        MarkedVisitProcess += (TNode current, TNode next) => 
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
            for(TNode current = endingNode; !current.Equals(endingNode); current = nodeStats[current].Parent) path.Add(current); 
        }
        path.Reverse();
	
		return path;
    }

*/
#endregion

#region  EULER_PATHS


	private static (List<int> inwards, List<int> outwards, List<int> total) GetGraphDegrees(TGraph graph)
	{
		List<int> inwards = Enumerable.Repeat(0, graph.Count).ToList();
		List<int> outwards = Enumerable.Repeat(0, graph.Count).ToList();
		List<int> total = Enumerable.Repeat(0, graph.Count).ToList();
		
		graph.ForeachEdge(
			
			(TNode src, TNode trg, int weight) => 
			
			{
				outwards[src.Index] ++;
				inwards[trg.Index] ++;
				total[trg.Index] ++;
				total[src.Index] --;
			}
		);

		return (inwards, outwards, total);
	}

	private static bool CheckEuler(TGraph graph)
	{
		if(graph.Count <= 1) return true;
		var degrees =  GetGraphDegrees(graph);
		
		for(int i = 0; i < graph.Count; i++)
		{
			if(degrees.inwards[i] != degrees.outwards[i]) return false;
		}

		return true;
	}


	/// <summary>
	/// Runs the Hierholzer Algorithm to find a Eulerian Cycle on a directed graph
	/// </summary>
	/// <param name="graph">The graph to find the cycle on</param>
	/// <returns>A path containing a Eulerian Cycle.</returns>
	public static Path<TNode> HierholzerDigraphCycles(TGraph graph)
	{
		Path<TNode> circuit = new();
		if(!CheckEuler(graph)) throw new InvalidOperationException("The provided graph is not Eulerian.");
		if(graph.Count <= 1) return (Path<TNode>)graph.ToList();
		
		//Ao invés de tirar as arestas do grafo, marcar quais já foram visitadas para facilitar o processo
		int unmarkedEdges = 0;
		Dictionary<(int, int), bool> edgeMarking = new();
		graph.ForeachEdge( (TNode src, TNode trg, int weight) => 
			{
				unmarkedEdges ++;
				edgeMarking.Add((src.Index, trg.Index), false);
			}
		);

		//cycle.Add();
		Stack<TNode> nodes = new();
		TNode currNode = graph[0];
		do
		{

			bool gotEdge = false;
			graph.ForeachTargetOf(currNode, 
				(TNode nextNode, int weight) =>
				{
					if(gotEdge) return;
					var edge = (currNode.Index, nextNode.Index);
					if(!edgeMarking[edge])
					{
						gotEdge = true;
						edgeMarking[edge] = true;
						unmarkedEdges --;
						nodes.Push(nextNode);
					}
				}
			);

			if(!gotEdge)
			{
				circuit.Add(nodes.Pop()); 
			}

		} while(nodes.TryPeek(out currNode));

		circuit.Add(graph[0]);
		return circuit;
	}

#endregion

}

public class Path<T> : List<T> 
	where T : IGraphNode
{}

public class ClusterNode : IGraphNode
{
    public int Index { get; set; }
    public Vector2 Position { get; set; }

	public List<IGraphNode> clusteredNodes = new();

	public ClusterNode()
	{	}
}
/*
public class ClusteredGraph<T> : IGraph<T> where T : ISpellGraphNode, new()
{
	public List<T> Cycle = new();
	public List<int> cycleWeights = new();
	public List<T> nodes = new();
	public T root;
	public T clusteredCycle;
	public Dictionary<(T src, T trg), int> edges = new();
	public Dictionary<(T src, T trg), int> cycleEdges = new();
	public Dictionary<T, (T trg, int weight)> cycleEdgesIn = new();
	public Dictionary<T, (T src, int weight)> cycleEdgesOut = new();

	public ClusteredGraph() {}

	public ClusteredGraph(List<T> nodes, T root) 
	{
		foreach(var node in nodes)
		{
			this.nodes.Add(node);
		}
		this.root = root;
	}
	public ClusteredGraph(IGraph<T> graph, IGraph<T> original, T root, List<T> cycle)
	{
		
		foreach( var node in graph ) nodes.Add(node);
		this.root = root;
		this.Cycle = cycle;

		if(Cycle.Count > 0)
		{
			foreach(var node in cycle) { nodes.Remove(node); }
			clusteredCycle = new();
			Add(clusteredCycle);
		}
		
		graph.ForeachEdge((T src, T trg, int weight) => {
			var containSrc = Cycle.Contains(src);
			var containTrg = Cycle.Contains(trg);
			
			
			if(containSrc && containTrg)
				cycleEdges.TryAdd((src, trg), weight);
			else if(!containSrc && !containTrg)
				Connect(src, trg, weight);

			return;
		});

		foreach(var node in cycle)
		{
			original.ForeachSourceOf(node, (T src, int weight) => {
				cycleEdgesIn.TryAdd(src, (node, weight));
				int cycleBreakWeight = graph.GetEdgeWeight(cycle[(cycle.IndexOf(node)+1)%cycle.Count], node);
				Connect(src, clusteredCycle, weight - cycleBreakWeight);
			});
			original.ForeachTargetOf(node, (T trg, int weight) => {
				cycleEdgesOut.TryAdd(trg, (node, weight));
				Connect(clusteredCycle, trg, weight);
			});
		}

	}

	public void Uncluster()
	{
		foreach(var node in Cycle)
		{
			nodes.Add(node);
		}
		foreach(var edge in cycleEdges)
		{
			Connect(edge.Key.src, edge.Key.trg, edge.Value);
		}

		List<(T src, T trg, int weight)> connections = new();
		ForeachEdge((T src, T trg, int weight) => 
		{
			if(src.Index == clusteredCycle.Index)
			{
				var originalEdge = cycleEdgesOut[trg];
				connections.Add((originalEdge.src, trg, originalEdge.weight));
			}
			else if(trg.Index == clusteredCycle.Index)
			{
				var originalEdge = cycleEdgesIn[src];
				connections.Add((src, originalEdge.trg, originalEdge.weight));
			}
			else
				connections.Add((src, trg, weight));
		});
		Remove(clusteredCycle);

		foreach(var edge in connections)
		{
			Connect(edge.src, edge.trg, edge.weight);
		}
	}


	public void MakeMinimumArborescence()
	{
		Dictionary<(T src, T trg), int> newEdges = new();
		foreach(var node in nodes) {	
			if(node.Index == root.Index) continue;
			
			int minWeight = int.MaxValue;
			T minWeightSource = default;
			ForeachSourceOf(node, (T src, int weight) => {
				if(weight < minWeight)
				{
					minWeightSource = src;
					minWeight = weight;
				}
			});
			newEdges.Add((minWeightSource, node), minWeight);
		}
		this.edges = newEdges;
	}


    public T this[int index] { get => nodes[index]; set => nodes[index] = value; }

    public GraphFlag[] Flags => throw new NotImplementedException();

    public int Count => nodes.Count;

    public bool IsReadOnly => false;

    public void Add(ICastable c)
    {
        throw new NotImplementedException();
    }

    public void Add(T item)
    {
		item.Index = nodes.Count;
        nodes.Add(item);
    }

    public bool AdjacenceBetween(T n1, T n2) => edges.ContainsKey((n1, n2));
    

    public void Clear()
    {
        edges.Clear();
		nodes.Clear();
    }

    public bool Connect(T sourceNode, T targetNode) => Connect(sourceNode, targetNode, 1);

    public bool Connect(T sourceNode, T targetNode, int weight)
    {
        if(!nodes.Contains(sourceNode) || !nodes.Contains(targetNode)) return false;

		edges.TryAdd((sourceNode, targetNode), weight);
		return true;
    }

    public bool Contains(T item) => nodes.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => nodes.CopyTo(array, arrayIndex);
    public bool Disconnect(T sourceNode, T targetNode)
    {
        if(!nodes.Contains(sourceNode) || !nodes.Contains(targetNode)) return false;
		return edges.Remove((sourceNode, targetNode));
    }

    public void ForeachEdge(Action<T, T, int> process)
    {
        foreach(var edge in edges)
		{
			process(edge.Key.Item1, edge.Key.Item2, edge.Value);
		}
    }

    public void ForeachSourceOf(T node, Action<T, int> process)
    {
        foreach(var edge in edges)
		{
			if(edge.Key.Item2.Index != node.Index) continue;
			process(edge.Key.Item1, edge.Value);
		}
    }

    public void ForeachTargetOf(T node, Action<T, int> process)
    {
        foreach(var edge in edges)
		{
			if(edge.Key.Item1.Index != node.Index) continue;
			process(edge.Key.Item2, edge.Value);
		}
    }

    public int GetEdgeWeight(T src, T trg)
    {
        return edges[(src, trg)];
    }

    public IEnumerator<T> GetEnumerator()
    {
       return nodes.GetEnumerator();
    }

    public int InwardsDegree(T n)
    {
        int degree = 0;
        ForeachSourceOf(n, (_, _) => degree++);
		return degree;
    }

    public int OutwardsDegree(T n)
    {
		int degree = 0;
        ForeachTargetOf(n, (_, _) => degree++);
		return degree;
    }

    public bool Remove(T item)
    {
		if(!nodes.Contains(item)) return false;
		List<(T src, T trg)> valid_edges = new();
        foreach(var edge in edges)
		{
			if(edge.Key.Item1.Index == item.Index || edge.Key.Item2.Index == item.Index)
			{
				valid_edges.Add(edge.Key);
			}
		}

		foreach(var edge in valid_edges) { edges.Remove(edge); }
		return nodes.Remove(item);
    }

    public bool ReplaceNode(T node, ICastable castable)
    {
		if(!nodes.Contains(node)) return false;
		node.Castable = castable;
		return true;
    }

    public void SetEdgeWeight(T src, T trg, int weight)
    {
		edges[(src, trg)] = weight;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
	
}
*/
