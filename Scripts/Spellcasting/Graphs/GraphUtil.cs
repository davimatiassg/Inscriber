
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SpellEditing;


using Node = ISpellGraph.Node;


public class GraphUtil 
{
	///UTILITY FUNCTIONS
	public static string TestNodeString(Node n) => "[color=" + ((Rune)n.castable).Color.ToHtml() + "]" + n.index + "[/color]";
	public static string NodeStringName(Node n) => "[color=" + ((Rune)n.castable).Color.ToHtml() + "]" + ((Rune)n.castable).Name + "[/color]";
	public static Dictionary<T1, T2> InitializePairType<T1, T2>(List<T1> nodes, T2 defValue)
	{
		Dictionary<T1, T2> dict = new Dictionary<T1,T2>();
		foreach(T1 n in nodes)
		{
			dict.Add(n, defValue);
		}
		return dict;
	}
	public static Dictionary<Node, T> InitializePairType<T>(List<Node> nodes) where T : new() => InitializePairType<Node, T>(nodes, new T());   

	/// <summary>
	/// Finds all the articulations from certain graph.
	/// 
	/// Performs a Breadth-First Search to construct a graph tree 
	/// and classify the graph's edges, assert node's depth relative 
	/// to the search's root, calculate the LowPT for each node and 
	/// finally select the articulations, if there are any. 
	/// </summary>
	/// <param name="graph"></param>
	/// <returns></returns>
	public static List<Node> FindArticulations(ISpellGraph graph, Node root)
	{
		List<Node> articulations = new List<Node>();

		List<(Node, Node)> treeEdges = new List<(Node, Node)>();
		Dictionary<Node, int> depths = new Dictionary<Node, int> { { root, 0 }};
		Dictionary<Node, Node> lowpts = new Dictionary<Node, Node> { { root, root }};
		
		void UpdateLowPT(Node node, Node newLowPT)
		{
			(Node, Node)[] currEdges;
			Node curr_node = node;
			do
			{
				if(depths[lowpts[curr_node]] > depths[newLowPT]) lowpts[curr_node] = newLowPT;
				else break;
				
				currEdges = treeEdges.Where(((Node, Node) n) => n.Item2 == curr_node).ToArray();
				curr_node = currEdges[0].Item1;
			} while(currEdges.Length > 0);
		}
		

		
		Action<Node, Node>  UnmarkedVisitProcess = (Node current, Node next) =>
		{
			treeEdges.Add((current, next));
			if(depths.Keys.Contains(next)) return;
			
			depths.Add(next, depths[current]+1);
			lowpts.Add(next, next);
			
		};

		Action<Node, Node>  MarkedVisitProcess = (Node current, Node next) =>
		{
			if(depths[lowpts[current]] > depths[next]) UpdateLowPT(current, next);
		};

        ForEachNodeByBFSIn(graph, root, null, UnmarkedVisitProcess, MarkedVisitProcess);

		foreach(Node node in graph.Nodes)
		{
			if(node == root) continue;
			foreach(int next in graph.GetNextNodesOf(node))
			{
				if(lowpts[graph[next]] == graph[next] || lowpts[graph[next]] == node) 
				if(!articulations.Contains(node)) articulations.Add(node);
			}
		}

		if(treeEdges.Where(((Node, Node) n) => n.Item1 == root || n.Item2 == root).Count() > 1) articulations.Add(root);

		return articulations;
	}

	/// <summary>
	/// Returns whether the graph is Bipartite
	/// </summary>
	/// <param name="graph">The graph to check</param>
	/// <returns>true the graph is bipartite, false otherwise.</returns>
	public static bool IsGraphBipartite(Graph graph)
	{
		int brand = 1;
		if(graph.nodes.Count < 3) return true;
		Dictionary<int, (bool, int)> markedNodes = InitializePairType<int, (bool, int)>(graph.nodes.Select(n => n.index).ToList(), (false, 0));
		Stack<int> stack = new Stack<int>();
		while (stack.Count > 0)
		{   
			int currNode = stack.Pop();
			markedNodes[currNode] = (true, brand);
			foreach(int nextNode in graph.GetNextNodesOf(currNode))
			{
				if(!markedNodes[nextNode].Item1)
				{
					markedNodes[currNode] = (true, -brand);
					stack.Push(nextNode);
				}
				else if(markedNodes[nextNode].Item2 == brand) return false;
			}
			brand = -brand;
			
		}
		return true;
	}

	/// <summary>
	/// Returns a list with the connected components of a given graph.
	/// </summary>
	/// <param name="graph">The graph to get connected components from</param>
	/// <returns>A List with the connected components of the graph</returns>
	public static List<List<Node>> GetConnectedComponents(Graph graph)
	{
		List<List<Node>> connectedComponents = new List<List<Node>>();
		List<Node> currentComponent = new List<Node>();

        List<Node> remainingNodes = graph.Nodes.ToArray().ToList();

        Action<Node> visitedNewNode = (Node n) => {remainingNodes.Remove(n); currentComponent.Add(n);};

        
        while(remainingNodes.Count > 0)
        {
           	ForEachNodeByDFSIn(graph, remainingNodes[0], visitedNewNode);
			connectedComponents.Add(currentComponent);
        }
		
        
        return connectedComponents;
	}

	/// <summary>
	/// Returns whether the graph is Connected
	/// </summary>
	/// <param name="graph">The graph to check</param>
	/// <returns>true if there is only one connected component in the graph, false otherwise.</returns>
	public static bool IsGraphConnected(Graph graph) => GetConnectedComponents(graph).Count == 1;
	
	/// <summary>
	/// Runs a full Breadth First Search trought the graph's nodes and executes a choosen expression at each node.
	/// </summary>
	/// <param name="spellGraph">The graph where the search will be performed </param>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <param name="VisitationProcess">An Action Delegate that operates after a node is visited.</param>
	/// <param name="UnmarkedVisitProcess">An Action Delegate that operates for each unvisited neighbor of the node being visited.</param>
	/// <param name="MarkedVisitProcess">An Action Delegate that operates for each visited neighbor of the node being visited.</param>
	public static void ForEachNodeByBFSIn(ISpellGraph spellGraph, Node startingNode, 
		Action<Node>        VisitationProcess = null, 
		Action<Node, Node>  UnmarkedVisitProcess = null,
		Action<Node, Node>  MarkedVisitProcess = null
	){


		if(spellGraph.Count == 0) return;
		Dictionary<Node, bool> markedNodes = InitializePairType<bool>(spellGraph.Nodes);
		Queue<Node> queue = new Queue<Node>();
		queue.Enqueue(startingNode);

		while (queue.Count > 0)
		{   
			Node currNode = queue.Dequeue();
			if(markedNodes[currNode]) continue;
			
			foreach(int nextNode in spellGraph.GetNextNodesOf(currNode))
			{
				
				if(markedNodes[spellGraph[nextNode]]) { MarkedVisitProcess?.Invoke(currNode, spellGraph[nextNode]); }
				else{
					UnmarkedVisitProcess?.Invoke(currNode, spellGraph[nextNode]);
					queue.Enqueue(spellGraph[nextNode]);
				}
			}
			VisitationProcess?.Invoke(currNode);
			markedNodes[currNode] = true;
			
		}
	}
	public static void ForEachNodeByBFSIn(ISpellGraph spellGraph) => ForEachNodeByBFSIn(spellGraph, spellGraph[0]);
	



	/// <summary>
	/// Runs a full Depth First Search trought the graph's nodes and executes a choosen expression at each node.
	/// </summary>
	/// <param name="spellGraph">The graph where the search will be performed </param>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <param name="VisitationProcess">An Action Delegate that operates after a node is visited.</param>
	/// <param name="UnmarkedVisitProcess">An Action Delegate that operates for each unvisited neighbor of the node being visited.</param>
	/// <param name="MarkedVisitProcess">An Action Delegate that operates for each visited neighbor of the node being visited.</param>
	public static void ForEachNodeByDFSIn(ISpellGraph spellGraph, Node startingNode, 
		Action<Node>        VisitationProcess = null, 
		Action<Node, Node>  UnmarkedVisitProcess = null,
		Action<Node, Node>  MarkedVisitProcess = null
	){

		
		if(spellGraph.Count == 0) return;
		Dictionary<Node, bool> markedNodes = InitializePairType<bool>(spellGraph.Nodes);
		Stack<Node> stack = new Stack<Node>();
		stack.Push(startingNode);

		while (stack.Count > 0)
		{   
			Node currNode = stack.Pop();
			if(markedNodes[currNode]) continue;

			foreach(int nextNode in spellGraph.GetNextNodesOf(currNode))
			{
				if(markedNodes[spellGraph[nextNode]])
				{
					MarkedVisitProcess?.Invoke(currNode, spellGraph[nextNode]);
				}
				else
				{
					UnmarkedVisitProcess?.Invoke(currNode, spellGraph[nextNode]);
					stack.Push(spellGraph[nextNode]);
				}
			}
			
			VisitationProcess?.Invoke(currNode);
			markedNodes[currNode] = true;

			
		}
	}
	public static void ForEachNodeByDFSIn(ISpellGraph spellGraph) => ForEachNodeByDFSIn(spellGraph, spellGraph[0]);

	/// <summary>
	/// Runs a choosen expression at each node of a graph.
	/// </summary>
	/// <param name="spellGraph">The graph used </param>
	/// <param name="Process">An Action Delegate that executes with each node as parameter.</param>
	public static void ForEachNodeIn(ISpellGraph spellGraph, Action<Node> Process) {
		foreach(Node node in spellGraph.Nodes) Process(node);
	}
	
 
	 private enum ESearchState : int { OUT = 0, STACKED, VISITED };
	/// <summary>
	/// Uses a DFS algorithm to find out if the graph has a cycle.
	/// </summary>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <returns>True if this graph has a cycle</returns>
	public static bool HasCycle(Graph graph, Node startingNode)
	{
		bool cycled = false;
		Dictionary<Node, Node> Predecessors = new Dictionary<Node, Node>
        {
            { startingNode, startingNode }
        };

		Action<Node, Node>  UnmarkedVisitProcess = (Node n1, Node n2) => 
		{ if(!Predecessors.Keys.Contains(n2)) Predecessors.Add(n2, n1); };

		Action<Node, Node>  MarkedVisitProcess = (Node n1, Node n2) => 
		{ if(Predecessors[n1] != n1 && Predecessors[n1] != n2) cycled = true; };

		ForEachNodeByDFSIn(graph, startingNode, null, UnmarkedVisitProcess, MarkedVisitProcess);

		return cycled;
	}


	public static bool HasCycle(Digraph graph, Node startingNode)
	{
		bool cycled = false;

		Action<Node, Node>  MarkedVisitProcess = (Node n1, Node n2) => 
		{  cycled = true; };

		ForEachNodeByDFSIn(graph, startingNode, null, null, MarkedVisitProcess);

		return cycled;
	}

	public static void PrintPruffer()
	{
		string s = "[b]Pruffer code: { [/b]";
		foreach(int i in TreeToPruffer((Graph)SpellManager.currentSpell.graphData)) 
			s += NodeStringName(SpellManager.currentSpell.graphData[i]) + " " ;
		s += "[b]}[/b]" ;

		GD.PrintRich(s);
	}

	/// <summary>
	/// Obtains the Prüffer sequence from certain tree graph.
	/// </summary>
	/// <param name="originalGraph">the tree graph</param>
	/// <returns>The pruffer sequence in a list..</returns>
	/// <exception cref="FormatException"></exception>
	public static List<int> TreeToPruffer(Graph originalGraph)
	{
		if(GetConnectedComponents(originalGraph).Count != 1)   	{GD.Print("The chosen graph is not connected."); return null;}
		if(HasCycle(originalGraph, originalGraph[0]))           {GD.Print("The chosen graph has cycles."); return null;}
		if(originalGraph.Count < 3) return originalGraph.Nodes.Select((Node n) => n.index).ToList();

		Graph graph = (Graph)originalGraph.Clone();

		List<int> code = new List<int>();
		while(code.Count < graph.Count-2)
		{
			foreach(Node n in graph.Nodes)
			{
				var nexts = graph.GetNextNodesOf(n);
				if(nexts.Count == 1) { 
					code.Add(nexts[0]); 
					graph.SetNextNodesOf(n, new List<Node>()); 
					break; 
				}
			}
		}

		return code;
	}

	/// <summary>
	/// Receives a void Graph and a Prüffer sequence, and returns the tree from that sequence.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="originalGraph">The void graph</param>
	/// <param name="pruffer">The list that stores a pruffer sequence</param>
	/// <returns></returns>
	/// <exception cref="FormatException"></exception>
	public static T PrufferToTree<T>(Graph originalGraph, List<int> pruffer) where T : Graph, new()
	{
		if(originalGraph.EdgeAmmount() > 0)   		throw new FormatException("The chosen graph is not void.");
		if(originalGraph.Count != pruffer.Count+2) 	throw new FormatException("Incongruent graph and pruffer sequence sizes.");

		T graph = new T();
		graph.Nodes = originalGraph.Nodes;

		Dictionary<Node, int> degrees = InitializePairType<Node, int>(graph.Nodes, 1);

		foreach(int p in pruffer) { degrees[graph[p]]++; }

		foreach(int p in pruffer)
		{
			foreach(Node n in graph.Nodes)
			{
				if(degrees[n] == 1) {
					graph.Connect(n, p);
					degrees[graph[p]]--;
					degrees[n]--;
					break;
				}
			}
		}
		
		List<Node> finalEdge = degrees.Where((pair)=> pair.Value == 1).Select((pair) => pair.Key).ToList();

		graph.Connect(finalEdge[0], finalEdge[1]);

		return graph;
	}






	public static void UpdateNodeTopSorting(Digraph digraph) => digraph.nodes = TopSortNodes(digraph);

	/// <summary>
	/// Runs a Topology Sort trough a Graph.
	/// </summary>
	/// <param name="spellGraph">The Graph to be sorted</param>
	/// <returns>The Graph's nodes sorted by topology.</returns>
	public static List<Node> TopSortNodes(Digraph spellGraph)
	{

		Dictionary<Node, bool> markedNodes = InitializePairType<bool>(spellGraph.nodes);
		void TopologicalSortUtil(Node node, ref Stack<Node> stack)
		{
			markedNodes[node] = true;
			foreach(int n in spellGraph.GetNextNodesOf(node))
			{
				if (!markedNodes[spellGraph[n]]) TopologicalSortUtil(spellGraph[n], ref stack);
			}
			stack.Push(node);
		}
		
		Stack<Node> stack = new Stack<Node>();
		foreach (Node node in spellGraph.nodes) {
			if (!markedNodes[node] && (spellGraph.GetNextNodesOf(node).Count + spellGraph.GetPrevNodesOf(node).Count > 0)) 
				TopologicalSortUtil(node, ref stack);
		}
		List<Node> sortedArray = new List<Node>(spellGraph.nodes.Count);
		while(stack.Count > 0){ 
			sortedArray.Append(stack.Pop());
		}
		return sortedArray;
	} 
	/// <summary>
	/// Gets the underlying undirected Graph of given Digraph. 
	/// </summary>
	/// <typeparam name="TGraph">The representation of the Graph to be returned.</typeparam>
	/// <param name="digraph">The Digraph to find the undelying graph from.</param>
	/// <returns>The undelying graph from the given Digraph.</returns>
	public static TGraph UnderlyingGraph<TGraph>(Digraph digraph) where TGraph : Graph, new()
	{
		TGraph graph = new TGraph();
		graph.Nodes = digraph.Nodes;
		List<(Node, Node)> edges = digraph.Edges;
		for(int i = 0; i < edges.Count; i++)
		{
			(Node, Node) duplicate = (edges[i].Item2, edges[i].Item1);
			edges.Remove(duplicate);
		}
		graph.Edges = digraph.Edges;
		return graph;  
	}

	public static TGraph ConvertGraphTo<TGraph>(Graph originalGraph) where TGraph : Graph, new()
	{
		TGraph graph = new TGraph();
		graph.Nodes = originalGraph.Nodes;
		graph.Edges = originalGraph.Edges;
		return graph;  
	}
}











/*
public class DigraphUtil : GraphUtil
{
		private enum ESearchState : int { OUT = 0, STACKED, VISITED };
	/// <summary>
	/// Uses a DFS algorithm to find out if the graph has a cycle.
	/// </summary>
	/// <param name="startingNode">The spellGraph's node from where the search will start</param>
	/// <returns>True if this graph has a cycle</returns>
	public static bool HasCycle(Digraph graph, Node startingNode)
	{
		var nodes = graph.nodes;
		if(nodes.Count == 0) return false;
		Dictionary<Node, ESearchState> markedNodes = InitializePairType(nodes, ESearchState.OUT);
		Stack<Node> stack = new Stack<Node>();
		markedNodes[startingNode] = ESearchState.STACKED;
		stack.Push(startingNode);
		while (stack.Count > 0)
		{   
			Node currNode = stack.Pop();
			markedNodes[currNode] = ESearchState.VISITED;
			foreach(int nextNode in graph.GetNextNodesOf(currNode))
			{
				switch(markedNodes[nodes[nextNode]])
				{
					
					case ESearchState.OUT:
						stack.Push(nodes[nextNode]);
						markedNodes[nodes[nextNode]] = ESearchState.STACKED;
						break;
					case ESearchState.VISITED:
						return true;
					default:
						continue;
				}
			}
		}  
		return false;
	}


	
}
*/
