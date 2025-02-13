using Godot;
using SpellEditing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


public partial class AlgorithmMenu : Control
{
    [Export] public Button kruskalButton;
    [Export] public Button primButton;
    [Export] public Button boruvkaButton;
    [Export] public Button chuLiuEdmondsButton;
    [Export] public Button dijkstraButton;
    [Export] public Button bellmanFordButton;
    [Export] public Button floydWarshallButton;
    [Export] public Button hierholzerCyclesButton;
    [Export] public Button hierholzerPathsButton;
    [Export] public Button fordFulkersonButton;
    [Export] public Button edmondsKarpButton;

    public SpellGraphView graphButton;

    public override void _Ready()
    {
        base._Ready();
        
        
    }
    public void SetupButtons(SpellGraphEditor graphView)
	{

#region SPANNING_TREE
		kruskalButton.Pressed += async () => 
        {
            var result = GraphUtil<SpellGraphView, VisualNode>.Kruskal<SpellGraph<VisualNode>>(graphView, null);

            var resource = SpellRepository.SaveSpell<SpellGraph<VisualNode>, VisualNode>(
                result, 
                graphView.metaMenu.titleField.Text + "-> kruskal result", 
                "result of kruskal minimun spanning tree",
                await graphView.metaMenu.GetPhoto()
            );

            SceneManager.SaveData("SELECTED_SPELL", resource);
		    SceneManager.LoadScene("SpellEditor");
        };
        kruskalButton.Disabled = false;

        primButton.Pressed += async () => 
        {
            var result = GraphUtil<SpellGraphView, VisualNode>.Prim<SpellGraph<VisualNode>>(graphView, null);

            var resource = SpellRepository.SaveSpell<SpellGraph<VisualNode>, VisualNode>(
                result, 
                graphView.metaMenu.titleField.Text + "-> prim result", 
                "result of prim minimun spanning tree",
                await graphView.metaMenu.GetPhoto()
            );

            SceneManager.SaveData("SELECTED_SPELL", resource);
		    SceneManager.LoadScene("SpellEditor");
        };
        primButton.Disabled = false;

        chuLiuEdmondsButton.Pressed += async () => 
        {
            var result = GraphUtil<SpellGraphView, VisualNode>.ChuliuEdmonds<SpellGraph<VisualNode>>(graphView, null);

            var resource = SpellRepository.SaveSpell<SpellGraph<VisualNode>, VisualNode>(
                result, 
                graphView.metaMenu.titleField.Text + "-> chu-liu_Edmonds result", 
                "result of Chu-Liu/Edmonds minimun spanning tree",
                await graphView.metaMenu.GetPhoto()
            );

            SceneManager.SaveData("SELECTED_SPELL", resource);
		    SceneManager.LoadScene("SpellEditor");
        };
        chuLiuEdmondsButton.Disabled = false;

#endregion

#region SHORTEST_PATH
        dijkstraButton.Pressed += async () => 
        {
            graphView.CloseAlgMenu();
            Path<VisualNode> result = GraphUtil<SpellGraphView, VisualNode>.Dijkstra(graphView, graphView[0], graphView[graphView.Count-1]);
            
            if(result.Count < 2) 
                return;
            

            await HighLightPath(result, Colors.Green, 250);

            await Task.Delay(10000);

            await HighLightPath(result, Colors.White, 250);
        };
        dijkstraButton.Disabled = false;


        bellmanFordButton.Pressed += async () => 
        {
            graphView.CloseAlgMenu();
            Path<VisualNode> result = GraphUtil<SpellGraphView, VisualNode>.BellmanFord(graphView, graphView[0], graphView[graphView.Count-1]);
            
            if(result.Count < 2) 
                return;
            

            await HighLightPath(result, Colors.Green, 250);

            await Task.Delay(10000);

            await HighLightPath(result, Colors.White, 250);
        };
        
        bellmanFordButton.Disabled = false;

        floydWarshallButton.Pressed += async () => 
        {
            graphView.CloseAlgMenu();
            Path<VisualNode> result =RunFloydWarshall(graphView, graphView[0], graphView[graphView.Count-1]);
            
            if(result.Count < 2) 
                return;
            
            await HighLightPath(result, Colors.Green, 250);

            await Task.Delay(10000);

            await HighLightPath(result, Colors.White, 250);
        };
        
        floydWarshallButton.Disabled = false;

#endregion
	
#region EULERIAN_CYCLES

        hierholzerCyclesButton.Pressed += async () => 
        {
            graphView.CloseAlgMenu();
            Path<VisualNode> result = GraphUtil<SpellGraphView, VisualNode>.HierholzerDigraphCycles(graphView);
            GD.Print(result);
            
            if(result.Count < 2) 
                return;
            

            await HighLightPath(result, Colors.Green, 100);

            
            await Task.Delay(10000);

            await HighLightPath(result, Colors.White, 50);
            
        };
        hierholzerCyclesButton.Disabled = false;

#endregion
    
    
    }



    public async Task HighLightPath(Path<VisualNode> path, Color color, int betweenDelay = 250)
    {
        for(int i = 1; i < path.Count; i++)
        {
            var n1 = path[i-1];
            var n2 = path[i];     
            foreach(VisualArc arc in n2.arcs)
            {
                if(arc.Target != n1) continue;
                arc.Modulate = color; 
                break;
            }
            await Task.Delay(betweenDelay);
        }
    }

    public Path<VisualNode> RunFloydWarshall(SpellGraphEditor graph, VisualNode startingNode, VisualNode endingNode)
    {
        var result = GraphUtil<SpellGraphView, VisualNode>.FloydWarshall(graph);

        Path<VisualNode> resultPath = new();
        if(startingNode.Index == endingNode.Index) return resultPath;

        int x = startingNode.Index;
        int y = endingNode.Index;

        resultPath.Add(endingNode);
        
        do
        {
            y = result[x, y];
            if(y == -1) 
                return new Path<VisualNode>();
            resultPath.Add(graph[y]);
        }
        while(x != y);

        return resultPath;

    }
}