
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;
using System.Diagnostics;


/// <summary>
/// A class to handle the Traveling Salesman Problem.
/// </summary>
/// <typeparam name="TGraph"></typeparam>
/// <typeparam name="TNode"></typeparam>
public class GraphTSP<TGraph, TNode>
	where TGraph : IGraph<TNode>, new()
	where TNode : IGraphNode, new()
{

#region GREEDY


    /// <summary>
    /// Runs the Greedy Algorithm solution to solve the Traveling Salesman Problem 
    /// </summary>
    /// <param name="spellGraph">The graph where the greedy algorithm will run</param>
    /// <returns>The Traveling Salesman's path</returns>
	public static Path<TNode> GreedyTSP(TGraph spellGraph)
    {
        Path<TNode> path = new();

        var head = spellGraph[0];
        var count = spellGraph.Count;

        bool[] visited = new bool[count];
        path.Add(head);
        for(int i = 1; i < count; i++)
        {
            visited[head.Index] = true;
            TNode minNext = default;
            int minWeight = int.MaxValue;
            foreach( (TNode trg, int weight) in spellGraph.GetTargetsOf(head))
            {
                if(!visited[trg.Index] && minWeight > weight)
                {
                    minNext = trg;
                    minWeight = weight;
                }
            }
            
            path.Add(minNext, minWeight);
            head = minNext;
        }
        return path;
    }

#endregion 

}