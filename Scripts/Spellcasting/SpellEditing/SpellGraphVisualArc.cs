using Godot;
using Godot.NativeInterop;
using System;
using System.Linq;

namespace SpellEditing
{
public partial class SpellGraphVisualArc : Line2D
{
    private SpellGraphVisualNode[] vertices = { null, null };
    public SpellGraphVisualNode Source
    {
        get => vertices[0];
        set => vertices[0] = value;        
    }

    public SpellGraphVisualNode Target
    {
        get => vertices[1];
        set => vertices[1] = value;
        
    }
    
    
    public void MoveSource(Vector2 vect) => SetPointPosition(0, vect); 
    public void MoveSource(SpellGraphVisualNode obj) => MoveSource(obj.GetRect().GetCenter());
    public void MoveSource() => MoveSource(Source);
    public void MoveTarget(Vector2 vect) => SetPointPosition(1, vect); 
    public void MoveTarget(SpellGraphVisualNode obj) => MoveTarget(obj.GetRect().GetCenter());
    public void MoveTarget() => MoveTarget(Target);

    public void UpdatePosition() { MoveSource(); MoveTarget(); }

    public SpellGraphVisualArc()
    {
        Points = new Vector2[2];
        this.WidthCurve = new Curve();
        this.WidthCurve.AddPoint(Vector2.Down);
        this.WidthCurve.AddPoint(Vector2.Right);
    }

    public void DisconnectAll()
    {
        Target = null;
        Source = null;
    }
}
}