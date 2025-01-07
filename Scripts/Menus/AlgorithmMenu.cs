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

#endregion

#region SHORTEST_PATH
        dijkstraButton.Pressed += async () => 
        {
            graphView.CloseAlgMenu();
            Path<VisualNode> result = GraphUtil<SpellGraphView, VisualNode>.Dijkstra(graphView, graphView[0], graphView[graphView.Count-1]);
            
            if(result.Count < 2) 
                return;
            

            for(int i = 1; i < result.Count; i++)
            {
                var n1 = result[i-1];
                var n2 = result[i];
                await Task.Delay(250);
                foreach(VisualArc arc in n2.arcs)
                {
                    if(arc.Target != n1) continue;
                    arc.Modulate = Colors.Green; 
                    break;
                }
            }

            await Task.Delay(10000);

            for(int i = 1; i < result.Count; i++)
            {
                var n1 = result[i-1];
                var n2 = result[i];
                await Task.Delay(250);
                foreach(VisualArc arc in n2.arcs)
                {
                    if(arc.Target != n1) continue;
                    arc.Modulate = Colors.White; 
                    break;
                }
            }

        };
        dijkstraButton.Disabled = false;

#endregion
	}
}