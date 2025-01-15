
using System;

public static class GraphConversion
{

    public static TGraph Clone<TGraph, TNode>(this TGraph graph, Func<TNode, TNode> nodeConversor = null)
		where TGraph : IGraph<TNode>, new()
		where TNode : IGraphNode, new()
	=> graph.Convert<TGraph, TNode, TNode>(nodeConversor);
	

	public static TResult Convert<TResult, TNode, TNodeOriginal>(this IGraph<TNodeOriginal> graph, Func<TNodeOriginal, TNode> nodeConversor = null)
		where TResult : IGraph<TNode>, new()
		where TNode : IGraphNode, new()
		where TNodeOriginal : IGraphNode, new()
	{
		if(nodeConversor == null)
			nodeConversor = (TNodeOriginal node) => new TNode();
        
		TResult result = new TResult();
		foreach(var node in graph) 
		{
            var newNode = nodeConversor(node);
            newNode.Index = node.Index;
            newNode.Position = node.Position;
			result.Add(newNode);
		}
		graph.ForeachEdge((src, trg, w) => result.Connect(result[src.Index], result[trg.Index], w));
		return result;  
	}



}