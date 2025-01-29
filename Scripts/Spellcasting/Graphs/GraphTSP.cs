
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

    public const int MAX_ITERATIONS = 5;
    public const int MAX_GRASP_ITERATIONS = 1000;

    public const float GRASP_ALPHA = 0.1f;

#region GREEDY


    private static Path<TNode> GreedyLimitedTSP(TGraph graph, int pathSize) 
    {
        var random = new Random();

        if(pathSize > graph.Count) throw new ArgumentException("Can not create a path longer than the graph.");
        Path<TNode> path = new();
        var head = graph[random.Next(graph.Count)];

        bool[] visited = new bool[graph.Count];
        path.Add(head);
        for(int i = 1; i < pathSize; i++)
        {
            visited[head.Index] = true;
            TNode minNext = default;
            int minWeight = int.MaxValue;
            foreach( (TNode trg, int weight) in graph.GetTargetsOf(head))
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

    /// <summary>
    /// Runs the Greedy Algorithm solution to solve the Traveling Salesman Problem 
    /// </summary>
    /// <param name="graph">The graph where the greedy algorithm will run</param>
    /// <returns>The Traveling Salesman's path</returns>
	public static Path<TNode> GreedyTSP(TGraph graph)
    {
        Path<TNode> bestPath = new();
        bestPath.size = int.MaxValue;

        for(int i = 0; i < MAX_ITERATIONS; i++)
        {
            var path = GreedyLimitedTSP(graph, graph.Count);

            if(path.size < bestPath.size)
                bestPath = path;
        }
            
        return bestPath;
    }

#endregion 

#region CHEAPEST_INSERTION

    /// <summary>
    /// Runs the Cheapest Insertion Algorithm solution to solve the Traveling Salesman Problem 
    /// </summary>
    /// <param name="graph">The graph where the greedy algorithm will run</param>
    /// <returns>The Traveling Salesman's path</returns>
	public static Path<TNode> CheapestInsertWrappedTSP(TGraph graph, int initialPathSize)
    {
        Path<TNode> path = GreedyLimitedTSP(graph, initialPathSize);
        bool[] visited = new bool[graph.Count];
        foreach(var node in path) {visited[node.Index] = true; }
        
        var finalPathSize = graph.Count();

        while(path.Count < finalPathSize)
        {
            

            int pathPosition = 0;
            int graphPosition = -1;
            int cheapestInsert = int.MaxValue;

            for(int j = 1; j < path.Count; j++)
            {
                for(int h = 0; h < graph.Count; h++)
                {
                    if(visited[h]) 
                        continue;
                    
                    try
                    {
                        int i = j-1;

                        int i_j = graph.GetEdgeWeight(graph[path[i].Index], graph[path[j].Index]);
                        int i_h = graph.GetEdgeWeight(graph[path[i].Index], graph[h]);
                        int h_j = graph.GetEdgeWeight(graph[h], graph[path[j].Index]);

                        int currInsert = i_h+h_j - i_j; 

                        if(cheapestInsert > currInsert)
                        {
                            pathPosition = j;
                            cheapestInsert = currInsert;
                            graphPosition = h;
                        }
                    }
                    catch { continue; }       
                }            
            }

            path.Insert(pathPosition, graph[graphPosition]);  
            path.size += cheapestInsert;
            visited[graphPosition] = true;
        }
        return path;
    }


    public static Path<TNode> CheapestInsertTSP(TGraph graph)
    {
        
        

        Path<TNode> bestPath = new();
        bestPath.size = int.MaxValue;

        for(int i = 0; i < MAX_ITERATIONS; i++)
        {
            var path = CheapestInsertWrappedTSP(graph, 2);

            if(path.size < bestPath.size)
                bestPath = path;
        }
            
        return bestPath;
    }

#endregion

#region GRASP

    private static Path<TNode> GreedyRandomTSP(TGraph graph, float alpha) 
    {
        var random = new Random();
        var pathSize = graph.Count;
        if(pathSize > graph.Count) throw new ArgumentException("Can not create a path longer than the graph.");
        Path<TNode> path = new();
        var head = graph[random.Next(graph.Count)];

        bool[] visited = new bool[graph.Count];
        for(int i = 0; i < pathSize; i++)
        {
            
            var targets = graph.GetTargetsOf(head)
                .Where(targetEdge => visited[targetEdge.trg.Index] == false)
                .OrderBy(targetEdge => targetEdge.weight)
                .ToList();
            
            var next = targets[random.Next((int)(alpha*targets.Count()))];
            
            path.Add(next.trg, next.weight);
            head = next.trg;
            visited[head.Index] = true;
        }
        return path;
    }


    public static Path<TNode> SwapLocalSearch(Path<TNode> initialPath, TGraph graph)
    {
        
        Path<TNode> newPath = initialPath;
        int pathSize = initialPath.Count;
        for(int i = 1; i < pathSize; i++) {
            newPath = initialPath;
            TNode tmp = newPath[i-1];
            newPath[i-1] = newPath[i];
            newPath[i] = tmp;
            int newSize = 0;
            for(int j = 1; j < pathSize; j++) {
                newSize += graph.GetEdgeWeight(newPath[j-1], newPath[j]);
                       
            }
            if(newSize < initialPath.size) initialPath = newPath;
        }

        return newPath;
    }

     public static Path<TNode> OPT2LocalSearch(Path<TNode> initialPath, TGraph graph)
    {
        Path<TNode> newPath = initialPath;

        int pathSize = newPath.Count;
        for(int i = 1; i < pathSize-2; i++) {
            int i_weight = 
                graph.GetEdgeWeight(newPath[i-1], newPath[i]) +
                graph.GetEdgeWeight(newPath[i], newPath[i+1]);

            for(int j = i+2; j < pathSize-1; j++)
            {
                int j_weight = 
                    graph.GetEdgeWeight(newPath[j-1], newPath[j]) +
                    graph.GetEdgeWeight(newPath[j], newPath[j+1]);

                int i_new_weight = 
                    graph.GetEdgeWeight(newPath[j-1], newPath[i]) +
                    graph.GetEdgeWeight(newPath[i], newPath[j+1]);

                int j_new_weight = 
                    graph.GetEdgeWeight(newPath[i-1], newPath[j]) +
                    graph.GetEdgeWeight(newPath[j], newPath[i+1]);
                
                var original_weight = i_weight + j_weight ;
                var new_weight = i_new_weight + j_new_weight;
                if(original_weight > new_weight)
                {
                    newPath.size += new_weight - original_weight;

                    TNode tmp = newPath[j];
                    newPath[j] = newPath[i];
                    newPath[i] = tmp;
                }
                
            }
        }

        


        return newPath;
    }


    public static Path<TNode> GRASP(TGraph graph,
        Func<Path<TNode>, TGraph, Path<TNode>> LocalSearch)
    {
        

        Path<TNode> bestPath = new();
        bestPath.size = int.MaxValue;

        for(int i = 0; i < MAX_ITERATIONS; i++)
        {
            var path =  GRASPWrapped(graph, LocalSearch);

            if(path.size < bestPath.size)
                bestPath = path;
        }

        return bestPath;
    }


    public static Path<TNode> GRASPWrapped(TGraph graph,
        Func<Path<TNode>, TGraph, Path<TNode>> LocalSearch)
    {
        

        Path<TNode> bestPath = new();
        bestPath.size = int.MaxValue;

        for(int i = 0; i < MAX_GRASP_ITERATIONS; i++)
        {
            var path =  GreedyRandomTSP(graph, GRASP_ALPHA);
            path = LocalSearch(path, graph);

            if(path.size < bestPath.size)
                bestPath = path;
        }

        return bestPath;
    }


#endregion


}