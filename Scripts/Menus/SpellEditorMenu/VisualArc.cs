using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[GlobalClass]
public partial class VisualArc : Line2D
{
    private VisualNode[] vertices = { null, null };

    private RichTextLabel label; 

    private int weight;

    public int Weight 
    {
        get => weight;
        set {
            weight = value;
            label.Text = value.ToString();
        }
    }
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
    
    
    public void MoveSource(Vector2 vect)  => SetPointPosition(0, vect); 
    public void MoveSource(VisualNode obj) => MoveSource(obj.GetRect().GetCenter());
    public void MoveSource() { MoveSource(Source); UpdateLabelPosition(); }
    public void MoveTarget(Vector2 vect) => SetPointPosition(1, vect); 
    public void MoveTarget(VisualNode obj) => MoveTarget(obj.GetRect().GetCenter());
    public void MoveTarget() { MoveTarget(Target); UpdateLabelPosition(); } 

    public void UpdatePosition() 
    { 
        MoveSource(Source); 
        MoveTarget(Target);
        UpdateLabelPosition();
    }

    public void UpdateLabelPosition()
    {
        label.Position = (GetPointPosition(1) - GetPointPosition(0))/2 + GetPointPosition(0) - Vector2.One*20;
    }

    public VisualArc()
    {
        Points = new Vector2[2];

        this.Width = 40;
        this.WidthCurve = new Curve();
        this.WidthCurve.AddPoint(new Vector2(0, 0.25f));
        this.WidthCurve.AddPoint(new Vector2(0.95f, 0.25f));
        this.WidthCurve.AddPoint(new Vector2(0.951f, 1f));
        this.WidthCurve.AddPoint(Vector2.Right);

        //STUB:
        label = new RichTextLabel();
        label.Modulate = Colors.DarkOrchid;
        label.Size = Vector2.One*50;
        this.AddChild(label);
    }

    public void DisconnectAll()
    {
        Target = null;
        Source = null;
    }
}