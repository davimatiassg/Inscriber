using Godot;
using System.Collections.Generic;
using System.Linq;

namespace SpellEditing
{

using SpellNode = ISpellGraph.Node;
/// <summary>
/// Control-inherited class that displays a Rune Graph of a Spell.
/// </summary>
    
public partial class SpellGraphViewer : Control
{   
    [Export] public Control graphNodeMaster;
    [Export] public Control graphArcsMaster;
    [Export] public SpellGraphCamera spellGraphCamera;
    public Dictionary<SpellNode, SpellGraphVisualNode> viewPairs = new Dictionary<SpellNode, SpellGraphVisualNode>();
    public Dictionary<SpellGraphVisualNode, SpellNode> viewPairsReverse = new Dictionary<SpellGraphVisualNode, SpellNode>();

    public void AddNodeViewPair(SpellNode node, SpellGraphVisualNode nodeView)
    {
        viewPairs.Add(node, nodeView);
        viewPairsReverse.Add(nodeView, node);

        ///GOTTA CLEAN THESE DICTIONARIES
        ///NEVER REMOVES ANYTHING FROM THEM, JUST ADDS
        ///THAT WILL MESS UP EDGE CASES's MEMORY
    }

    public SpellGraphVisualNode GetPairNodeFrom(SpellNode node) => viewPairs[node];
    public SpellNode GetPairNodeFrom(SpellGraphVisualNode node) => viewPairsReverse[node];
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

    public SpellGraphVisualNode FindClosestNodeFrom(Vector2 position)
    {
        Control node = null;
        float distSQR = float.MaxValue;
        foreach(Control n in graphNodeMaster.GetChildren())
        {
            float d = n.Position.DistanceSquaredTo(position);
            if(d <distSQR)
            {
                node = n;
                distSQR = d;
            }
        }

        return (SpellGraphVisualNode)node;
    }

    public void ClearView()
    {
        foreach(Node n in graphNodeMaster.GetChildren()){ n.QueueFree(); }
        foreach(Node n in graphArcsMaster.GetChildren()){ n.QueueFree(); }
        viewPairs.Clear();
        viewPairsReverse.Clear();
    }
}
}