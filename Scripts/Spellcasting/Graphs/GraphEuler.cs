
using System.Collections.Generic;
using System.Linq;

using System;

public static class GraphEuler<TGraph, TNode>
	where TGraph : IGraph<TNode>, new()
	where TNode : IGraphNode, new()
{

	private static (List<int> inwards, List<int> outwards, List<int> total) GetGraphDegrees(TGraph graph)
	{
		List<int> inwards = Enumerable.Repeat(0, graph.Count).ToList();
		List<int> outwards = Enumerable.Repeat(0, graph.Count).ToList();
		List<int> total = Enumerable.Repeat(0, graph.Count).ToList();
		
		foreach ((TNode src, TNode trg, int weight) in graph.GetEdges())
		{
			outwards[src.Index] ++;
			inwards[trg.Index] ++;
			total[trg.Index] ++;
			total[src.Index] --;
		}
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
		foreach ((TNode src, TNode trg, int weight) in graph.GetEdges())
		{
			unmarkedEdges ++;
			edgeMarking.Add((src.Index, trg.Index), false);
		}
		

		//cycle.Add();
		Stack<TNode> nodes = new();
		TNode currNode = graph[0];
		do
		{

			bool gotEdge = false;
			foreach ((TNode nextNode, int weight) in graph.GetTargetsOf(currNode))
			{
				var edge = (currNode.Index, nextNode.Index);
				if(!edgeMarking[edge])
				{
					gotEdge = true;
					edgeMarking[edge] = true;
					unmarkedEdges --;
					nodes.Push(nextNode);
					break;
				}
			}
			
			if(!gotEdge)
			{
				circuit.Add(nodes.Pop()); 
			}

		} while(nodes.TryPeek(out currNode));

		circuit.Add(graph[0]);
		return circuit;
	}

}