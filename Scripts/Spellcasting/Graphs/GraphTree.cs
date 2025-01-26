using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public static class GraphTree<TGraph, TNode>
	where TGraph : IGraph<TNode>, new()
	where TNode : IGraphNode, new()
{


#region KRUSKAL
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


		foreach(var edge in graph.GetEdges())
		{
			edges.Add((edge.src.Index, edge.trg.Index, edge.weight));
		}
		
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
#endregion

#region PRIM
	public static TResult Prim<TResult>(TGraph graph, Comparison<int> comparer)
		where TResult : IGraph<TNode>, new()
	{
		TResult mstree = new TResult();
		foreach(var node in graph) mstree.Add(node);
		
		int unlinkedNodes = graph.Count;
		List<bool> linked = Enumerable.Repeat(false, unlinkedNodes).ToList();
		
		
		List<(int src, int trg, int weight)> viableEdges = new();

		foreach((TNode src, TNode trg, int weight) in graph.GetEdges())
		{
			viableEdges.Add((src.Index, trg.Index, weight));
		}

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

#endregion


#region CHU-LIU/EDMONDS

    /// <summary>
    /// Helper Graph node class to handle chu-liu/edmonds algorithm's cycle clustering
    /// </summary>
    public class ClusterNode : IGraphNode
    {
        public int Index { get; set; }
        public Vector2 Position { get; set; }

        public List<IGraphNode> clusteredNodes = new();
        public ClusterNode()
        {	}
        public override string ToString()
        {
			string k = "[ ";
			foreach(var node in clusteredNodes) k+= node;
			k += "]";
            return "idx: " + Index + "|clustered:" + k; 
        }

    }
 

    public static TResult ChuliuEdmonds<TResult>(TGraph graph, Comparison<int> comparer = null)
		where TResult : IGraph<TNode>, new()

	{
		if(graph.Count < 2) 
			return new TResult();

		var rootIndex = 0;
		foreach(var node in graph)
		{
			int visitedCount = 0;

			GraphUtil<TGraph, TNode>.ForEachNodeByDFSIn(graph, graph[rootIndex], (TNode _) => visitedCount++);

			if(visitedCount == graph.Count) {break;}
			else {rootIndex ++;}
		}
		if(rootIndex == graph.Count) 
			throw new InvalidOperationException("The chosen directed graph does not have a spanning arborescence.");

		var clusterGraph = graph.Convert<MatrixGraph<ClusterNode>, ClusterNode, TNode>(
            (TNode node) => new ClusterNode{ clusteredNodes =  new List<IGraphNode>{node}} );

		clusterGraph = ChuliuEdmondsRecursive(clusterGraph, clusterGraph[rootIndex], comparer);

		List<(TNode src, TNode trg)> edges = new();

		foreach((TNode src, TNode trg, int weight) in graph.GetEdges())
		{
			edges.Add((src, trg));
		}

		foreach(var edge in edges) graph.Disconnect(edge.src, edge.trg);

		return clusterGraph.Convert<TResult, TNode, ClusterNode>( (ClusterNode node) => (TNode)node.clusteredNodes[0]);
		
	}

	public static MatrixGraph<ClusterNode> ChuliuEdmondsRecursive(MatrixGraph<ClusterNode> graph, ClusterNode root, Comparison<int> comparer)
	{
		var clusteredGraph = graph.Clone((ClusterNode node) => new ClusterNode{clusteredNodes = new List<IGraphNode>{node.clusteredNodes[0]}});

		ClusterNode clusteredRoot  = clusteredGraph[root.Index];

        Debug.Assert(clusteredRoot.Index == root.Index);
		
		List<(ClusterNode src, ClusterNode trg, int weight)> allEdges;

		var minimumEdges = FindMinimumEdges(clusteredGraph, clusteredRoot, out allEdges);

		var cycleIndexes = FindCycle(minimumEdges, root.Index);

		var cycle = cycleIndexes.Select((int idx) => clusteredGraph[idx]);

		if(cycle == null)
		{
			return clusteredGraph.Convert<MatrixGraph<ClusterNode>, ClusterNode, ClusterNode>( (ClusterNode node) => (ClusterNode)node.clusteredNodes[0]);
		}

        ClusterNode newNode = new(){
        Position = new Vector2(cycle.Average((ClusterNode node) => node.Position.X), cycle.Average((ClusterNode node) => node.Position.X)),
        clusteredNodes = cycle.Cast<IGraphNode>().ToList()
        };

        foreach(var cycleNode in cycle){ clusteredGraph.Remove(cycleNode); }
        clusteredGraph.Add(newNode);

        List<(ClusterNode src, ClusterNode trg)> inCycleEdges = new();
        int minSrcWeight = int.MaxValue;
        ClusterNode minSrc = default;
        ClusterNode cycleTrg = default;
        int minTrgWeight = int.MaxValue;
        ClusterNode minTrg = default;
        ClusterNode cycleSrc = default;

		foreach((ClusterNode src, ClusterNode trg, int weight) in allEdges)
        {
            bool containsSrc = cycle.Contains(src);
            bool containsTrg = cycle.Contains(trg);
            if(containsSrc && !containsTrg)
            {
                if(minTrgWeight > weight)
                {
                    minTrgWeight = weight;
                    cycleSrc = src;
                    minTrg = trg;
                }
            }
            else if(!containsSrc && containsTrg)
            {
				var pi_trg = minimumEdges.Find(edge => edge.trg == trg);
                if(minSrcWeight > weight - pi_trg.weight)
                {
                    minSrcWeight = weight - pi_trg.weight;
                    minSrc = src;
                    cycleTrg = trg;
                }
            }

        }
        
        Debug.Assert(minSrcWeight != int.MaxValue);

        clusteredGraph.Connect(minSrc, newNode, minSrcWeight);
        if(minTrgWeight != int.MaxValue) clusteredGraph.Connect(cycleSrc, newNode, minTrgWeight);

        clusteredGraph = ChuliuEdmondsRecursive(clusteredGraph, clusteredRoot, comparer);

        clusteredGraph.Remove(newNode);
        foreach(var node in cycle) clusteredGraph.Add(node);
        clusteredGraph.Connect(minSrc, cycleTrg, minSrcWeight);
        clusteredGraph.Connect(cycleSrc, minTrg, minTrgWeight);

        foreach(var edge in minimumEdges)
            if(cycle.Contains(edge.src) && cycle.Contains(edge.trg))
                clusteredGraph.Connect(edge.src, edge.trg, edge.weight);
        
        clusteredGraph.Disconnect(cycle.Last(), cycle.First());

		return clusteredGraph;
	}


    private static List<(ClusterNode src, ClusterNode trg, int weight)> FindMinimumEdges(
		MatrixGraph<ClusterNode> graph, ClusterNode root, 
		out List<(ClusterNode src, ClusterNode trg, int weight)> allEdges
	)
    {
        List<(ClusterNode src, ClusterNode trg, int weight)> edges = new();
		List<(ClusterNode src, ClusterNode trg, int weight)> outEdges = new();
        foreach(var node in graph)
        {
            if(node.Index == root.Index) continue;

            int minW = int.MaxValue;
            ClusterNode minSrc = default;
            foreach ((ClusterNode src, int weight) in graph.GetSourcesOf(node))
            {
                if(minW > weight)
                {
                    minSrc = src;
                    minW = weight;
                }
				outEdges.Add((src, node, weight));
            }
            edges.Add((minSrc, node, minW));
        }

		allEdges = outEdges;
		Debug.Assert(edges.Count == graph.Count-1);

        return edges;
    }
    

    /// <summary>
    /// Finds cycles in a dictionary of edges.
    /// </summary>
    /// <param name="edges">A dictionary storing edge sources as keys and targets as values. </param>
    /// <param name="root">The root of the arborescence. Optimization catalyst.</param>
    /// <returns> The first cycle found in a Dictionary of edges, or null if there is not one.</returns>
    private static List<int> FindCycle(List<(ClusterNode src, ClusterNode trg, int weight)> edges, int root)
    {
        if(edges.Count < 2)
            return null;
        
        Dictionary<int, bool> visited = new();
        Dictionary<int, int> arcs = new();
        foreach(var edge in edges){ 
            visited.Add(edge.trg.Index, false);
            arcs.Add(edge.trg.Index, edge.src.Index);
        }

		visited.Add(root, true);

		foreach(var keypair in visited)
		{
			if(keypair.Value) 
				continue;
			

			List<int> path = new(){keypair.Key};
			while(true)
			{
				var prev = arcs[path.Last()];
				if(visited[prev])
				{
					foreach(var node in path) visited[node] = true;
					break;
				}
				else if(path.Contains(arcs[prev])) { return path; }
				path.Add(arcs[path.Last()]);
			}
		}

        List<int> cycledNodes = new();
        foreach(var visitPairing in visited) if(!visitPairing.Value) cycledNodes.Add(visitPairing.Key); 


        ///Como cada nó possui grau de entrada 1, exceto a root (grau de entrada 0),
        ///Não é possível ter um cíclo sem ter mais de uma componente conexa.
        ///Não haveriam arestas suficientes!
        if(cycledNodes.Count == 0)
            return null;
        



        //GARANTIDAMENTE existe um ciclo em edges

        foreach(var cycleStart in cycledNodes)
        {
            Stack<int> stack = new();
            stack.Push(cycleStart);
            visited = new();
            foreach(var node in cycledNodes){ visited.Add(node, false); }

            while(stack.Count > 0)
            {
                var currNode = stack.Pop();
                if(visited[currNode])
                {
                    var start = currNode;
                    List<int> cycle = new();
                    do
                    {
                        cycle.Add(currNode);
                        currNode = arcs[currNode];
                    } while(currNode != start);
                    return cycle;
                }
                visited[currNode] = true;
                stack.Push(arcs[currNode]);
            }
        }

        Debug.Assert(false);
        return null;
    }






#endregion



}