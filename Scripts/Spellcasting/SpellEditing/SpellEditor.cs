using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellEditing
{
public partial class SpellEditor : Control
{

    [Export] private RuneSelector selector;
    [Export] private SpellCursor cursor;
    [Export] private Container arrowMaster;
    [Export] private Container plotableMaster;

    private SpellLine tracingLine = null;
    private SpellManager manager = new SpellManager();

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        if(cursor.Slot == null)
        {
            
            Rect2 cursorRect = cursor.GetRect();
            if(@event.IsActionPressed("game_fire_1", false)) { EmptyCursorPick(); }
            if(@event.IsActionPressed("game_fire_2", false)) { tracingLine = EmptyCursorTrail(); }
            if(tracingLine != null && @event.IsActionReleased("game_fire_2", false)){ EmptyCursorUnTrail(); }
        }
        else
        {
            if(@event.IsActionReleased("game_fire_1", false)) {  CarriedCursorDrop(); }
            
        }
            
    }

    private RuneSlot EmptyCursorPick()
    {
        RuneSlot slot = CheckCursorIntersection();
        if(slot == null) { return null; }
        if(slot.GetParent() == selector)
        {
            selector.Selected = selector.slots.IndexOf(slot);
            PickPlotable(slot);
        }
        cursor.Slot = slot;
        return slot;
    }
    private SpellLine EmptyCursorTrail()
    {
        RuneSlot slot = CheckCursorIntersection();
        if(slot == null) { return null; }
        SpellLine line = new SpellLine();

        arrowMaster.AddChild(line);
        
        line.ConnectBot(slot);
        line.ConnectCursor(cursor);
        
        slot.getMoved?.Invoke();
        return line;
    }

    private void EmptyCursorUnTrail()
    {
        RuneSlot slot = CheckCursorIntersection();
        if(slot == null) 
        { 
            tracingLine.Disconnect(); 
            arrowMaster.RemoveChild(tracingLine);
        }
        
        if(manager.AddToSpellGraph(tracingLine.bot.node, tracingLine.tip.node))
        {
            tracingLine.DisconnectCursor();
            tracingLine.ConnectTip(slot);
            return;
        }

        tracingLine.Disconnect(); 
        arrowMaster.RemoveChild(tracingLine);
    }

    private void CarriedCursorDrop()
    {
        if(selector.GetGlobalRect().HasPoint(cursor.GetGlobalRect().GetCenter())){ DropPlotable(cursor.Slot); return; }
        RuneSlot slot = cursor.Slot;
        PlacePlotable(cursor.Slot);
        RuneSlot bumper = CheckRectIntersection(slot.GetGlobalRect());
        /*do
        {
            var b_rect = bumper.GetGlobalRect();
            var s_rect = slot.GetGlobalRect();
            slot.GlobalPosition += (b_rect.GetCenter() - s_rect.GetCenter()).Normalized()*(s_rect.Intersection(b_rect).Size.Length());
            bumper = CheckRectIntersection(slot.GetGlobalRect());
            GD.Print("lol");
        }while(bumper != null);*/
        slot.getMoved?.Invoke();        

    }
    private void PickPlotable(RuneSlot slot)
    {

        selector.RemovePlotable(slot);
        plotableMaster.AddChild(slot);
        cursor.Slot = slot;
    }
    private void DropPlotable(RuneSlot slot)
    {
        plotableMaster.RemoveChild(slot);
        selector.AddPlotable(slot);
        cursor.Slot = null;
    }
    private void PlacePlotable(RuneSlot slot)
    {
        if(slot.node == null){ DropPlotable(slot); }
        SpellManager.AddNode(slot.node);
        cursor.Slot = null;
    }


    private RuneSlot CheckCursorIntersection()
    {
        return CheckRectIntersection(cursor.GetGlobalRect());
        
    }
    private RuneSlot CheckRectIntersection(Rect2 rectValue)
    {
        List<RuneSlot> intersections = new List<RuneSlot>();
        foreach(Control child in selector.GetChildren())
        {
            if(rectValue.Intersects(((RuneSlot)child).GetGlobalRect())){
                intersections.Add((RuneSlot)child);
            }
        }
        foreach(Control child in plotableMaster.GetChildren())
        {
            if(rectValue.Intersects(((RuneSlot)child).GetGlobalRect())){
                intersections.Add((RuneSlot)child);
            }
        }
        if(intersections.Count == 0) { return null; }
        RuneSlot best = intersections[0];
        for(int i = 1; i < intersections.Count; i++)
        {
            if(best.GetGlobalRect().GetCenter().DistanceTo(rectValue.GetCenter()) > intersections[i].GetGlobalRect().GetCenter().DistanceTo(rectValue.GetCenter()))
            {
                best = intersections[i];
            }
        }
        return best;
    }

}
}
