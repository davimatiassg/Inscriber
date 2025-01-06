using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpellEditing
{
/// <summary>
/// Control-inherited class that displays a Rune Graph of a Spell.
/// </summary>

[GlobalClass]
public partial class SpellGraphView : Control, IGraph<VisualNode>
{
    public const int DEFAULT_WEIGHT = 1;
    public GraphFlag[] Flags => new GraphFlag[] { GraphFlag.DIRECTED, GraphFlag.ALLOW_LOOPS, GraphFlag.WEIGHTED };

#region SPELLGRAPH_INTERFACE

    public VisualNode this[int index] { 
        get => (VisualNode)graphNodeMaster.GetChild(index);
        set => this[index].Castable = value.Castable;
    }    
    public void ForeachSourceOf(VisualNode node, Action<VisualNode, int> process)
    {
        foreach(VisualArc arc in node.arcs)
            if(arc.Target == node) 
                process(arc.Source, arc.Weight);
    }
    public void ForeachTargetOf(VisualNode node, Action<VisualNode, int> process)
    {
        foreach(VisualArc arc in node.arcs)
            if(arc.Source == node) 
                process(arc.Target, arc.Weight);
        
    }
    public void ForeachEdge(Action<VisualNode, VisualNode, int> process)
    {
        foreach(VisualArc arc in graphArcsMaster.GetChildren())
            process(arc.Source, arc.Target, arc.Weight);
    }

    public int Count =>  graphNodeMaster.GetChildCount();

    public bool IsReadOnly => false;

    public VisualNode CreateNode(ICastable castable)
    {
        VisualNode node = new VisualNode(){ 
            Castable = castable, 
        };
        return node;
    }
    public int GetEdgeWeight(VisualNode src, VisualNode trg)
    {
        try { return src.arcs.Where(arc => arc.Target == trg).Single().Weight; }
        catch(InvalidOperationException) { throw new InvalidOperationException("No Edge found between provided nodes"); }
    } 
    public void SetEdgeWeight(VisualNode src, VisualNode trg, int weight){
        try { src.arcs.Where(arc => arc.Target == trg).Single().Weight = weight; }
        catch(InvalidOperationException) { throw new InvalidOperationException("No Edge found between provided nodes"); }
    }
    public int EdgeAmmount() => graphArcsMaster.GetChildCount();
    public bool AdjacenceBetween(VisualNode n1, VisualNode n2) => 
        n1.arcs.Where(arc => arc.Source == n1).Select(arc => arc.Target).Contains(n2);
    public int InwardsDegree(VisualNode n) => n.arcs.Where(arc => arc.Source==n).Count();
    public int OutwardsDegree(VisualNode n) => n.arcs.Where(arc => arc.Target==n).Count();
    public int Degree(VisualNode n) => InwardsDegree(n) - OutwardsDegree(n);

    public void Add(VisualNode node)
    {
        graphNodeMaster.AddChild(node);
    }

    public virtual void Clear()
    {
        foreach(Node n in graphNodeMaster.GetChildren()){ n.QueueFree(); }
        foreach(Node n in graphArcsMaster.GetChildren()){ n.QueueFree(); }
    }

    public bool Contains(VisualNode item) => graphNodeMaster.GetChildren().Contains(item);

    public void CopyTo(VisualNode[] array, int arrayIndex)
    => graphNodeMaster.GetChildren().CopyTo(array, arrayIndex);
    

    public virtual bool Remove(VisualNode node)
    {
        graphNodeMaster.RemoveChild(node);
        node.DisconnectFromAll();
        node.QueueFree();
        return true;
    }

    public IEnumerator<VisualNode> GetEnumerator() 
    => graphNodeMaster.GetChildren().Cast<VisualNode>().ToList().GetEnumerator();
    

    IEnumerator IEnumerable.GetEnumerator()
    => graphNodeMaster.GetChildren().Cast<VisualNode>().GetEnumerator();

    public virtual VisualNode Add(ICastable castable, Vector2 position)
    {
        VisualNode node = CreateNode(castable);
        node.Position = position;
        Add(node);
        return node;
    }
    public void Add(ICastable castable) => Add(castable, Vector2.Zero);

    public bool Connect(VisualNode sourceNode, VisualNode targetNode) => Connect(sourceNode, targetNode, 1);

    public bool Connect(VisualNode sourceNode, VisualNode targetNode, int weight)
    {
        var arc = sourceNode.CreateArcTowards(targetNode);
        arc.Weight = weight;
        /*if(GraphUtil<SpellGraphView, VisualNode>.HasCycle(this, sourceNode)) {
            sourceNode.DestroyArc(arc);
            return false;
        }*/
        sourceNode.AssembleConnetion(arc);      
        graphArcsMaster.AddChild(arc);  
        return true;
        throw new InvalidOperationException("Graph does not support connections");
    }

    public bool Disconnect(VisualNode sourceNode, VisualNode targetNode)
    {
        sourceNode.DisconnectFrom(targetNode);
        return true;
    }

    public bool ReplaceNode(VisualNode node, ICastable castable)
    {
        node.Castable = castable;
        return false;
    }

    

#endregion SPELLGRAPH_INTERFACE


    [Export] public Control graphNodeMaster;
    [Export] public Control graphArcsMaster;
    [Export] public SpellGraphCamera spellGraphCamera;
    

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

    public VisualNode FindClosestNodeFrom(Vector2 position)
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

        return (VisualNode)node;
    }


    public virtual void LoadSpellGraph<TGraph, TNode>(TGraph graph)
        where TGraph : IGraph<TNode>, new()
        where TNode :  ISpellGraphNode, new()
    {
        Clear();
        if(graph.Count == 0) return;

        foreach(var node in graph)
        {
            Add(node.Castable, node.Position);
        }

        graph.ForeachEdge((src, trg, weight) => {
            Connect(
                (VisualNode)graphNodeMaster.GetChild(src.Index), 
                (VisualNode)graphNodeMaster.GetChild(trg.Index),
                weight
            );
        });
    }

}
}