using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpellEditing
{
public abstract class SpellEditorMode 
{
    protected SpellEditorMode prevMode;
    public Control overlay;
    public virtual void EnterModeFrom(SpellEditorMode prevMode) 
    { 
        this.prevMode = prevMode;
        SpellEditor.editorMode = this;
        GD.Print(this.GetType());
        if(overlay != null) overlay.Visible = true;
    }
    public virtual void ExitModeTo(SpellEditorMode nextMode) 
    {
        if(overlay != null) overlay.Visible = false;
        this.prevMode = null; 
        nextMode.EnterModeFrom(this); 
    }
    public abstract void _Process(double delta);
    public abstract void _Input (InputEvent @event);
}
public class RuneSelectorMode : SpellEditorMode
{
    public RuneSelector selector;
    public override void EnterModeFrom(SpellEditorMode prevMode)
    {
        selector.ToggleDisplaying(true);
        base.EnterModeFrom(prevMode);
    }
    public override void ExitModeTo(SpellEditorMode nextMode)
    {
        selector.ToggleDisplaying(false);
        base.ExitModeTo(nextMode);
    }

    public override void _Process(double delta)
    {
        //Do Nothing        
    }

    public override void _Input(InputEvent @event)
    {
        if(@event.IsAction("game_left", true)) { selector.SelectLeft(@event); }
        else if(@event.IsAction("game_right", true)) { selector.SelectRight(@event); }
        else if(@event.IsActionPressed("game_up", false)) { selector.SelectRarityUp(); }
        else if(@event.IsActionPressed("game_down", false)) { selector.SelectBackwards(@event); }
        else if(@event.IsActionPressed("game_fire_1", false)) 
        {
            SpellEditor.dragMode.selectedSlot = 
            SpellEditor.AddSlot(selector.ConfirmSelection(), selector.GetViewportRect().GetCenter());
            ExitModeTo(SpellEditor.dragMode);
            return;
        }

        else if(@event.IsActionPressed("game_fire_2", false)) 
        {       
            if(prevMode is not FreeMode freeChildMode) { return ; }
            if(freeChildMode.selectedSlot == null) { return; }

            SpellEditor.dragMode.selectedSlot = 
            SpellEditor.SubstituteSlot(selector.ConfirmSelection(), freeChildMode.selectedSlot);
            ExitModeTo(SpellEditor.dragMode);
            return;
        }

        else if(@event.IsActionPressed("game_fire_5", false)) 
        {       
            ExitModeTo(prevMode);
            return;
        }
    }
}
public class ConnectMode : SpellEditorMode
{
    public SpellSlot selectedSlot;
    public SpellSlot startSlot = null;
    public Vector2 position;
    private bool mouseLocked;
    private SpellLine line;
    protected void MovePositionAlongAction()
    {
        Vector2 v = Input.GetVector("game_left", "game_right", "game_up", "game_down")*15;
        if(v == Vector2.Zero) { return; }
        position += v;
    }
    protected SpellSlot FindClosestSpellSlotFromMouse(InputEventMouse @event)
    {
        float proximity = Mathf.Inf;
        Vector2 pos = @event.GlobalPosition;
        SpellSlot closest = null;
        foreach(Control c in SpellEditor.Instance.plotableMaster.GetChildren())
        {
            float lenghtSQR = pos.DistanceSquaredTo(c.GetGlobalRect().GetCenter());
            if(lenghtSQR < proximity && c is SpellSlot s && s != selectedSlot)
            {
                proximity = lenghtSQR;
                closest = s;
            }
        }
        return closest;
    }
    protected bool SnapPositionFromKeyboard(InputEvent @event)
    {
        if(@event.IsEcho() || (!@event.IsPressed())) return true;
        if(SpellEditor.Instance.plotableMaster.GetChildCount() == 0) return false;
        float proximity = Mathf.Inf;
        Vector2 prevPos = position;
        Vector2 dir = Input.GetVector("game_left", "game_right", "game_up", "game_down").Normalized();
        foreach(Control c in SpellEditor.Instance.plotableMaster.GetChildren())
        {
            Vector2 moveDirection = c.GetGlobalRect().GetCenter()-position;
            float lenght =  moveDirection.Length();
            if(lenght < 1f) { continue;}
            if((moveDirection/lenght).Dot(dir) < 0.4) { continue; }
            if(lenght < proximity)
            {
                proximity = lenght;
                prevPos = c.GetGlobalRect().GetCenter();
                if(c is SpellSlot s) { selectedSlot = s; }
            }
        }
        position = prevPos;
        return proximity != Mathf.Inf;
    }
    private void CheckTriggerSwap(InputEvent @event)
    {
        if(@event.IsActionPressed("game_fire_1", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellEditor.freeMode);
            return;
        }
        else if(@event.IsActionPressed("game_fire_2", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellEditor.dragMode);
            return;
        }
        else if(@event.IsActionPressed("game_fire_3", false)) 
        {
            if(selectedSlot != null) ExitModeTo(prevMode);
            return;
        }
        //{ não tem o game_fire_4 
        //}
        else if(@event.IsActionPressed("game_fire_5", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellEditor.runeSelectorMode);
            return;
        }
    }

    public override void _Input(InputEvent @event)
    {

        if(@event is InputEventMouse mouseMoveEvent)
        {
            if(mouseMoveEvent is InputEventMouseButton) 
            { 
                mouseLocked = true; 
            }
            else if(mouseLocked) 
            { 
                position = mouseMoveEvent.GlobalPosition;
                selectedSlot = FindClosestSpellSlotFromMouse(mouseMoveEvent);
                line.MoveTip(position);
                line.SetPointPosition(2, selectedSlot == null? position : selectedSlot.GetGlobalRect().GetCenter());
            }
        }
        else
        {
            mouseLocked = false;
            SnapPositionFromKeyboard(@event);
        }
        line.Tip = selectedSlot;

        line.UpdatePosition();
        CheckTriggerSwap(@event);
        
    }

    public void CancelLinesBetween(SpellSlot first, SpellSlot last)
    {
        int i = 0;
        foreach(SpellLine line in SpellEditor.Instance.arrowMaster.GetChildren())
        {
            if(line.Tip == first && line.Bot == last) { CancelLine(line); i++; continue; }
            if(line.Bot == first && line.Tip == last) { CancelLine(line); i++; }
            if(i > 1) { return; }
        }
    }

    public void CancelLine(SpellLine line)
    {
        SpellEditor.Instance.arrowMaster.RemoveChild(line); 
        line.DisconnectAll();
        line.QueueFree();
        return;
    }
    public void CancelLine() => CancelLine(this.line);
    public override void _Process(double delta) 
    {
        SpellEditorCamera.Instance.Position = position; 
    }
    public override void EnterModeFrom(SpellEditorMode prevMode)
    {
        base.EnterModeFrom(prevMode);
        if(prevMode is FreeMode freePrev) { this.selectedSlot = freePrev.selectedSlot; startSlot = selectedSlot; }
        
        if(startSlot == null) { ExitModeTo(prevMode); }
        this.line = new SpellLine{ Bot = startSlot };
        GD.PrintRich(line==null?"[b]linha nula[/b]":"[b]linha startada[/b]");
        GD.PrintRich(selectedSlot == null?"[b]slot nula[/b]":"[b]slot startada[/b]");
        SpellEditor.Instance.arrowMaster.AddChild(line);
        GD.Print(line);
        
    }
    public override void ExitModeTo(SpellEditorMode nextMode)
    {
        base.ExitModeTo(nextMode);
        if(selectedSlot == null) { CancelLine(); }
        (bool, bool) toggleResult = SpellManager.ToggleConnectionOnGraph(startSlot.node, selectedSlot.node);
        
        //Connection Added?
        if(toggleResult.Item1) { if(!toggleResult.Item2) CancelLine(); }
        //Connection Removed?
        else { if (toggleResult.Item2) CancelLinesBetween(startSlot, selectedSlot); }
    }

}
public class FreeMode : SpellEditorMode 
{
    public Vector2 position;
    public SpellSlot selectedSlot;
    protected bool mouseLocked = false;
    protected SpellSlot FindClosestSpellSlotFromMouse(InputEventMouse @event)
    {
        float proximity = Mathf.Inf;
        Vector2 pos = @event.GlobalPosition;
        SpellSlot closest = null;
        foreach(Control c in SpellEditor.Instance.plotableMaster.GetChildren())
        {
            float lenght = pos.DistanceTo(c.GetGlobalRect().GetCenter());
            if(lenght < proximity && c is SpellSlot s)
            {
                proximity = lenght;
                closest = s;
            }
        }
        return closest;
    }
    protected bool SnapPositionToPlotable(InputEvent @event)
    {
        if(@event.IsEcho() || (!@event.IsPressed())) return true;
        if(SpellEditor.Instance.plotableMaster.GetChildCount() == 0) return false;
        float proximity = Mathf.Inf;
        Vector2 prevPos = position;
        Vector2 dir = Input.GetVector("game_left", "game_right", "game_up", "game_down").Normalized();
        foreach(Control c in SpellEditor.Instance.plotableMaster.GetChildren())
        {
            Vector2 moveDirection = c.GetGlobalRect().GetCenter()-position;
            float lenght =  moveDirection.Length();
            if(lenght < 1f) { continue;}
            if((moveDirection/lenght).Dot(dir) < 0.5) { continue; }
            if(lenght < proximity)
            {
                proximity = lenght;
                prevPos = c.GetGlobalRect().GetCenter();;
                if(c is SpellSlot s) { selectedSlot = s; }
            }
        }
        position = prevPos;
        return proximity != Mathf.Inf;
    }
    private void CheckTriggerSwap(InputEvent @event)
    {
        if(@event.IsActionPressed("game_fire_1", false)) 
        {
            SpellSlot closestSlot =  FindClosestSpellSlotFromMouse((InputEventMouse)@event);
            if(selectedSlot == closestSlot ) {
                ExitModeTo(SpellEditor.selectionMode);
            }
            return;
        }
        else if(@event.IsActionPressed("game_fire_2", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellEditor.dragMode);
            return;
        }
        else if(@event.IsActionPressed("game_fire_3", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellEditor.connectMode);
            return;
        }
        //{ não tem o game_fire_4 
        //}
        else if(@event.IsActionPressed("game_fire_5", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellEditor.runeSelectorMode);
            return;
        }
    }
    public override void _Input(InputEvent @event)
    {
        
        if(@event is InputEventMouse mouseMoveEvent)
        {
            if(mouseMoveEvent is InputEventMouseButton) { mouseLocked = true; }
            else if(mouseLocked) {
                selectedSlot = FindClosestSpellSlotFromMouse(mouseMoveEvent);
                position = selectedSlot.GetGlobalRect().GetCenter();
            }
            
        }
        else
        {
            mouseLocked = false;
            SnapPositionToPlotable(@event);
        }
        CheckTriggerSwap(@event);
        
    }

    public override void _Process(double delta) 
    {
        if (mouseLocked == false) SpellEditorCamera.Instance.Position = position;
    }
    public override void EnterModeFrom(SpellEditorMode prevMode)
    {
        base.EnterModeFrom(prevMode);
    }
    public override void ExitModeTo(SpellEditorMode nextMode)
    {
        base.ExitModeTo(nextMode);
        if(nextMode is FreeMode freeChildMode) 
        {
            freeChildMode.selectedSlot = selectedSlot;
            freeChildMode.position = position;
        }
        else if (nextMode is ConnectMode connectChildMode)
        {
            connectChildMode.selectedSlot = selectedSlot;
            connectChildMode.position = position;
        }
    }
}
public class DragMode : FreeMode
{
    protected void MovePositionAlongAction()
    {
        Vector2 v = Input.GetVector("game_left", "game_right", "game_up", "game_down")*50;
        if(v == Vector2.Zero) { return; }
        position += v;
    }
    private void CheckTriggerSwap(InputEvent @event)
    {
        if(@event.IsActionPressed("game_fire_1", false)) 
        {
            ExitModeTo(SpellEditor.selectionMode);
            return;
        }
        else if(@event.IsActionPressed("game_fire_2", false)) 
        {
            ExitModeTo(SpellEditor.freeMode);
            return;
        }
        else if(@event.IsActionPressed("game_fire_3", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellEditor.connectMode);
            return;
        }
        //{ não tem o game_fire_4 
        //}
        else if(@event.IsActionPressed("game_fire_5", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellEditor.runeSelectorMode);
            return;
        }
    }
    public override void _Input(InputEvent @event)
    {
        
        if(@event is InputEventMouse mouseMoveEvent)
        {
            if(mouseMoveEvent is InputEventMouseButton) { mouseLocked = true; }
            else if(mouseLocked) { position = mouseMoveEvent.GlobalPosition; }
        }
        else
        {
            mouseLocked = false;
            MovePositionAlongAction();
        }
        CheckTriggerSwap(@event);
        
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        selectedSlot.Position = SpellEditorCamera.Instance.GetScreenCenterPosition() - selectedSlot.GetGlobalRect().GetCenter() + selectedSlot.Position;
    }
    public override void EnterModeFrom(SpellEditorMode prevMode)
    {
        base.EnterModeFrom(prevMode);
    }
    public override void ExitModeTo(SpellEditorMode nextMode)
    {
        base.ExitModeTo(nextMode);
    }

}
public class SelectionMode : FreeMode
{
    public override void EnterModeFrom(SpellEditorMode prevMode) 
    { 
        base.EnterModeFrom(prevMode);
    }
    public override void ExitModeTo(SpellEditorMode nextMode) 
    {
        base.ExitModeTo(nextMode);
    }
}

}