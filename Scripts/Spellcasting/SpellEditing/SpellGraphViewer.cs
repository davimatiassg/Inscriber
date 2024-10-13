using Godot;
using System.Collections.Generic;
using System.Linq;

namespace SpellEditing
{


/// <summary>
/// Control-inherited class that displays a Rune Graph of a Spell.
/// </summary>
    
public partial class SpellGraphViewer : Control
{   
    [Export] public Control graphNodeMaster;
    [Export] public Control graphArcsMaster;
    [Export] public Camera2D spellGraphCamera;
    public Dictionary<Spell.Node, SpellGraphVisualNode> viewPairs = new Dictionary<Spell.Node, SpellGraphVisualNode>();
    private Dictionary<SpellGraphVisualNode, Spell.Node> viewPairsReverse = new Dictionary<SpellGraphVisualNode, Spell.Node>();

    public void AddNodeViewPair(Spell.Node node, SpellGraphVisualNode nodeView)
    {
        viewPairs.Add(node, nodeView);
        viewPairsReverse.Add(nodeView, node);

        ///GOTTA CLEAN THESE DICTIONARIES
        ///NEVER REMOVES ANYTHING FROM THEM, JUST ADDS
        ///THAT WILL MESS UP EDGE CASES's MEMORY
    }

    public SpellGraphVisualNode GetPairNodeFrom(Spell.Node node) => viewPairs[node];
    public Spell.Node GetPairNodeFrom(SpellGraphVisualNode node) => viewPairsReverse[node];
    public SpellGraphVisualNode DeployNewNode(IGraphDeployable deployable, Vector2 position)
    {
        SpellGraphVisualNode node = new SpellGraphVisualNode(){ 
            Deployable = deployable, 
            Position = position
        };

        graphNodeMaster.AddChild(node);
        
        return node;
    }

    public void SubstituteGraphNode(IGraphDeployable deployable, SpellGraphVisualNode node) => node.Deployable = deployable;

    public void RemoveGraphNode(SpellGraphVisualNode node)
    {
        graphNodeMaster.RemoveChild(node);
        node.DisconnectFromAll();
        node.QueueFree();
    }


    /// SpellGraph's Camera Methods
    public void SetCameraGlobalPosition(Vector2 position) => spellGraphCamera.GlobalPosition = position;

    public void SetCameraZoomPosition(Vector2 zoom) => spellGraphCamera.Zoom = zoom;

    public void SetCameraFocus(List<Vector2> positionList)
    {
        Vector2 pos = new Vector2();
        foreach (Vector2 position in positionList) pos+=position;
        pos /= positionList.Count;
        SetCameraGlobalPosition(pos);


    }
}
}