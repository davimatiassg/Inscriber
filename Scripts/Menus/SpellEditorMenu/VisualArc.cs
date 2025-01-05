using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[GlobalClass]
public partial class VisualArc : Line2D
{
    private VisualNode[] vertices = { null, null };

    public int weight;
    public VisualNode Source
    {
        get => vertices[0];
        set => vertices[0] = value;        
    }

    public VisualNode Target
    {
        get => vertices[1];
        set => vertices[1] = value;
        
    }
    
    
    public void MoveSource(Vector2 vect) => SetPointPosition(0, vect); 
    public void MoveSource(VisualNode obj) => MoveSource(obj.GetRect().GetCenter());
    public void MoveSource() => MoveSource(Source);
    public void MoveTarget(Vector2 vect) => SetPointPosition(1, vect); 
    public void MoveTarget(VisualNode obj) => MoveTarget(obj.GetRect().GetCenter());
    public void MoveTarget() => MoveTarget(Target);

    public void UpdatePosition() { MoveSource(); MoveTarget(); }

    public VisualArc()
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