

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;
using System.Diagnostics;


public static class GraphPathfinding<TGraph, TNode>
	where TGraph : IGraph<TNode>, new()
	where TNode : IGraphNode, new()
{


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
			foreach ((TNode src, TNode trg, int weight) in spellGraph.GetEdges()) 
			{
				if(distances[src.Index] != int.MaxValue && distances[src.Index] + weight < distances[trg.Index])
				{
					if(i == spellGraph.Count) negativeCycleFound = true;
					distances[trg.Index] = distances[src.Index]  + weight;
					predecessors[trg.Index] = src;
				}
			}
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
			 
			
			foreach ((TNode next, int weight) in spellGraph.GetTargetsOf(current))
			{
				if(distances[current.Index] + weight < distances[next.Index])
				{
					distances[next.Index] = distances[current.Index] + weight;
					predecessors[next.Index] = current;
				}
			}
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
			foreach ((TNode src, TNode trg, int weight) in spellGraph.GetEdges())
			{
				int i = src.Index;
				int j = trg.Index;
				distances[i,j] = weight;
				predecessors[i,j] =  i;

			}
		}
		else
		{
			foreach ((TNode src, TNode trg, int weight) in spellGraph.GetEdges())
			{
				int i = src.Index;
				int j = trg.Index;
				distances[i,j] = weight;
				predecessors[i,j] =  i;
			}
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

}