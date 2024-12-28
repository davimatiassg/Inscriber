using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellEditing
{
public partial class SpellGraphCamera : Camera2D 
{
    private const float ZOOM_OUT_SCALE = 1.1f;
    public override void _Ready()
    {
        base._Ready();
        Vector2 viewSize = this.GetViewport().GetVisibleRect().Size;
    }

    
    public void ZoomToFitGraph(SpellGraphView graph, Vector2 center) => ZoomToFitChildren(graph.graphNodeMaster, center);


    /// <summary>
    /// Adjusts camera zoom to fit a whole graph in screen space
    /// </summary>
    /// <param name="graph"> The graph to fit </param>
    /// <param name="center">The position to center the screen on</param>
    public void ZoomToFitGraph(SpellGraphView graph)
    {
        if(graph.graphNodeMaster.GetChildCount() < 2) { return; }

        Vector2 firstPoint = FindFurthestChild(graph.graphNodeMaster, ((Control)graph.graphNodeMaster.GetChild(0)).Position).Position;
        Vector2 secondPoint = FindFurthestChild(graph.graphNodeMaster, firstPoint).Position;
        ZoomToFit(firstPoint.DistanceTo(secondPoint));

        Position = (firstPoint+secondPoint)/2 + ((Control)graph.graphNodeMaster.GetChild(0)).GetRect().Size/2;
    }

    /// <summary>
    /// Finds the furthest child of certain control from given position
    /// </summary>
    /// <param name="control">The Parent control</param>
    /// <param name="position">The position to get the furthest child from</param>
    /// <returns></returns>
    public Control FindFurthestChild(Control control, Vector2 position)
    {
        Control furthest = null;
        float maxDistanceSqrd = 0;
        foreach(Control child in control.GetChildren()) {
            if(position.DistanceSquaredTo(child.Position) > maxDistanceSqrd)
            {
                furthest = child;
                maxDistanceSqrd = child.Position.DistanceSquaredTo(position);
            }
        }
        return furthest;
    }

    /// <summary>
    /// Adjusts camera zoom to fit all of a Control's children on screen space
    /// </summary>
    /// <param name="control">A node whose children will be fit on screen</param>
    /// <param name="center">The position to center the screen on</param>

    public void ZoomToFitChildren(Control control, Vector2 center)
    {
        Position = center;
        if(control.GetChildCount() < 2) return;

        Control furthest = FindFurthestChild(control, center);
        ZoomToFit(furthest.Position.DistanceTo(center));
        //GD.PrintRich("[color=" + furthest.Modulate.ToHtml() + "]" + furthest.Modulate + "[/color]");
    }


    
    /// <summary>
    /// Adjusts zoom to fit a line 
    /// </summary>
    /// <param name="length">the distance that must be contained in the screen </param>
    private void ZoomToFit(float length)
    {
        if(length <= 0) throw new DivideByZeroException();
        Vector2 viewSize = this.GetViewport().GetVisibleRect().Size;
        Zoom = Vector2.One * Mathf.Min(viewSize.X, viewSize.Y) / (ZOOM_OUT_SCALE*length);
    }


}

}
