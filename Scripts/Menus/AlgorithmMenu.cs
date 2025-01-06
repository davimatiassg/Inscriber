using Godot;
using SpellEditing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


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
	}
}