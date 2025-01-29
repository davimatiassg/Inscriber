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

    protected List<VisualNode> Nodes = new List<VisualNode>();
    protected List<List<VisualArc>> EdgeMatrix = new();
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

    public IEnumerable<(VisualNode trg, int weight)> GetTargetsOf(VisualNode node)
    {
        var count = Nodes.Count;
        var row = node.Index;
        for (int column = 0; column < count; column++) {
            var arc = EdgeMatrix[row][column];
            if(arc == null) continue;
            yield return (Nodes[column], arc.Weight);
        }
    }

    public IEnumerable<(VisualNode src, int weight)> GetSourcesOf(VisualNode node)
    {
        var count = Nodes.Count;
        var column = node.Index;
        for (int row = 0; row < count; row++) {
            var arc = EdgeMatrix[row][column];
            if(arc == null) continue;
            yield return (Nodes[row], arc.Weight);
        }
    }
    

    public IEnumerable<(VisualNode src, VisualNode trg, int weight)> GetEdges()
    {
        foreach(VisualArc arc in graphArcsMaster.GetChildren())
            yield return (arc.Source, arc.Target, arc.Weight);
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
        
        if(src.Index == trg.Index) throw new InvalidOperationException("Tried to get the weight of a loop, which are not allowed.");
        var arc = EdgeMatrix [src.Index][trg.Index];
        if(arc == null) { throw new InvalidOperationException("No Edge found between provided nodes."); }

        return arc.Weight;
    } 
    public void SetEdgeWeight(VisualNode src, VisualNode trg, int weight){
        if(src.Index == trg.Index) throw new InvalidOperationException("Tried to get the weight of a loop, which are not allowed.");
        var arc = EdgeMatrix [src.Index][trg.Index];
        if(arc == null) { throw new InvalidOperationException("No Edge found between provided nodes."); }

        arc.Weight = weight;
    }
    public int EdgeAmmount() => graphArcsMaster.GetChildCount();
    public bool AdjacenceBetween(VisualNode n1, VisualNode n2) => 
        EdgeMatrix[n1.Index][n2.Index] != null;
    public int InwardsDegree(VisualNode n) => GetSourcesOf(n).Count();
    public int OutwardsDegree(VisualNode n) => GetTargetsOf(n).Count();
    public int Degree(VisualNode n) => InwardsDegree(n) - OutwardsDegree(n);


    int lastIndex = 0;
    public void Add(VisualNode node)
    {
        graphNodeMaster.AddChild(node);
        node.Index = lastIndex;
        lastIndex++;
        
        Nodes.Add(node);
        EdgeMatrix.Add(Enumerable.Repeat<VisualArc>(null, Count-1).ToList());
        foreach( var row in EdgeMatrix ) row.Add(null);
    }

    public virtual void Clear()
    {
        foreach(Node n in graphNodeMaster.GetChildren()){ n.QueueFree(); }
        foreach(Node n in graphArcsMaster.GetChildren()){ n.QueueFree(); }
        Nodes.Clear();
        EdgeMatrix.Clear();
    }

    public bool Contains(VisualNode item) => graphNodeMaster.GetChildren().Contains(item);

    public void CopyTo(VisualNode[] array, int arrayIndex)
    => graphNodeMaster.GetChildren().CopyTo(array, arrayIndex);
    

    private void DisposeMatrixIndex(int disposed)
    {
        for(int i = 0; i < disposed; i++)
        {
            EdgeMatrix[i][disposed] = EdgeMatrix[i][Count-1];
            EdgeMatrix[disposed][i] = EdgeMatrix[Count-1][i];
        }
        for(int i = disposed + 1; i < Count-1; i++)
        {
            EdgeMatrix[i][disposed] = EdgeMatrix[i][Count-1];
            EdgeMatrix[disposed][i] = EdgeMatrix[Count-1][i];
        }
        EdgeMatrix[disposed][disposed] = EdgeMatrix[Count-1][Count-1];
    }

    public virtual bool Remove(VisualNode item)
    {
        if(Count == 0) 
            return false;
        if(Count == 1) {
            Clear();
            return true;
        }

        if(item.Index < Count-1) DisposeMatrixIndex(item.Index);

        Nodes[Count-1].Index = item.Index;
        Nodes[item.Index] = Nodes[Count-1];

        Nodes.RemoveAt(Count-1);
        EdgeMatrix.RemoveAt(Count-1);
        foreach(var list in EdgeMatrix) list.RemoveAt(Count-1);

        graphNodeMaster.RemoveChild(item);
        item.DisconnectFromAll();
        item.QueueFree();
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
        if(sourceNode.Index == targetNode.Index) return false;
        if(AdjacenceBetween(sourceNode, targetNode)) 
            return false;
        var arc = sourceNode.CreateArcTowards(targetNode);
        arc.Weight = weight;
        /*if(GraphUtil<SpellGraphView, VisualNode>.HasCycle(this, sourceNode)) {
            sourceNode.DestroyArc(arc);
            return false;
        }*/
        sourceNode.AssembleConnetion(arc);      
        graphArcsMaster.AddChild(arc);
        EdgeMatrix[sourceNode.Index][targetNode.Index] = arc;
        return true;
    }

    public bool Disconnect(VisualNode sourceNode, VisualNode targetNode)
    {
        if(sourceNode.Index == targetNode.Index) return false;
        if(!AdjacenceBetween(sourceNode, targetNode)) return false;
        
        sourceNode.DisconnectFrom(targetNode);

        EdgeMatrix[sourceNode.Index][targetNode.Index] = null;
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
        foreach(var edge in graph.GetEdges())
        {
            Connect(
                (VisualNode)graphNodeMaster.GetChild(edge.src.Index), 
                (VisualNode)graphNodeMaster.GetChild(edge.trg.Index),
                edge.weight
            );
        }
    }

}
}