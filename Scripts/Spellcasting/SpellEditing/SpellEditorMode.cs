using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpellEditing
{
public abstract class SpellEditorMode 
{
    protected SpellEditorMode prevMode;
    public Vector2 position;
    public Control overlay;
    public virtual void EnterModeFrom(SpellEditorMode prevMode) 
    {
        this.prevMode = prevMode;
        SpellEditor.editorMode = this;
        this.position = prevMode.position;
        if(overlay != null) overlay.Visible = true;
    }
    public virtual void ExitModeTo(SpellEditorMode nextMode) 
    {
        this.prevMode = null; 
        if(overlay != null) overlay.Visible = false;
        nextMode.EnterModeFrom(this); 
    }
    public abstract void _Process(double delta);
    public abstract void _Input (InputEvent @event);
}
public class RuneSelectorMode : SpellEditorMode
{
    public RuneSelector selector;
    public SpellSlot selectedSlot;
    public override void _Process(double delta) {  }
    private void CheckTriggerSwap(InputEvent @event)
    {
        if(@event.IsActionPressed("game_fire_1", false)) 
        {
            selectedSlot = SpellEditor.AddSlot(selector.ConfirmSelection(), selector.GetViewportRect().GetCenter());
            ExitModeTo(SpellEditor.dragMode);
            return;
        }

        else if(@event.IsActionPressed("game_fire_2", false)) 
        {       
            if(selectedSlot == null) {
                GD.Print("lol no");
                selectedSlot = SpellEditor.AddSlot(selector.ConfirmSelection(), selector.GetViewportRect().GetCenter());
            }
            else {
                selectedSlot = SpellEditor.SubstituteSlot(selector.ConfirmSelection(), selectedSlot);   
            }
            
            ExitModeTo(SpellEditor.dragMode);
            return;
        }

        else if(@event.IsActionPressed("game_fire_5", false)) 
        {       
            ExitModeTo(prevMode);
            return;
        }

    }
    public override void _Input(InputEvent @event)
    {
        if(@event.IsAction("game_left", true)) { selector.SelectLeft(@event); }
        else if(@event.IsAction("game_right", true)) { selector.SelectRight(@event); }
        else if(@event.IsActionPressed("game_up", false)) { selector.SelectRarityUp(); }
        else if(@event.IsActionPressed("game_down", false)) { selector.SelectBackwards(@event); }
        CheckTriggerSwap(@event);
    }
    public override void EnterModeFrom(SpellEditorMode prevMode)
    {
        if(prevMode is FreeMode freeChildMode && freeChildMode.selectedSlot != null) 
            selectedSlot = freeChildMode.selectedSlot;
        
        base.EnterModeFrom(prevMode);
    }
    public override void ExitModeTo(SpellEditorMode nextMode)
    {
        
        if(nextMode is FreeMode freeChildMode && selectedSlot != null) 
            freeChildMode.selectedSlot = selectedSlot;
        else if (nextMode is ConnectMode connectChildMode && selectedSlot != null) 
            connectChildMode.selectedSlot = selectedSlot;

        base.ExitModeTo(nextMode);
        selectedSlot = null;
    }
}
public class ConnectMode : SpellEditorMode
{
    public SpellSlot selectedSlot;
    public SpellSlot startSlot = null;
    private bool mouseLocked;
    private SpellLine line;
    protected void MovePositionAlongAction()
    {
        Vector2 v = Input.GetVector("game_left", "game_right", "game_up", "game_down")*16;
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
    protected bool SelectSlotInDirection(Vector2 direction)
    {
        if(SpellEditor.Instance.plotableMaster.GetChildCount() == 0) return false;
        float proximity = Mathf.Inf;
        Vector2 prevPos = position;

        foreach(Control c in SpellEditor.Instance.plotableMaster.GetChildren())
        {
            Vector2 moveDirection = c.GetGlobalRect().GetCenter()-position;
            float lenght =  moveDirection.Length();
            if(lenght < 1f) { continue;}
            if((moveDirection/lenght).Dot(direction) < 0.4) { continue; }
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
        if(@event.IsActionPressed("game_fire_1", false) && !mouseLocked) 
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

        if(@event is InputEventMouse mouseEvent)
        { 
            
            if(mouseEvent is InputEventMouseButton && !mouseLocked) 
            {
                mouseLocked = true;
                return;
            }
            if(mouseLocked){ 
                position = mouseEvent.GlobalPosition;
                selectedSlot = FindClosestSpellSlotFromMouse(mouseEvent);
                line.Tip = selectedSlot;
                line.MoveTip(position);
            }
            
            //line.SetPointPosition(2, selectedSlot == null? position : selectedSlot.GetGlobalRect().GetCenter());
        }
        else if (!@event.IsEcho() && @event.IsPressed())
        {
            if(mouseLocked) { mouseLocked = false; return; }
            SelectSlotInDirection(Input.GetVector("game_left", "game_right", "game_up", "game_down").Normalized());
            line.Tip = selectedSlot;
            
        }
        

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
        if(prevMode is FreeMode freePrev) { 
            this.selectedSlot = freePrev.selectedSlot; 
            startSlot = selectedSlot; 
        }
        if(startSlot == null) { ExitModeTo(prevMode); }
        this.line = new SpellLine{ Bot = startSlot };
        SpellEditor.Instance.arrowMaster.AddChild(line);
        
    }
    public override void ExitModeTo(SpellEditorMode nextMode)
    {
        
        if(selectedSlot == null) { CancelLine(); return; }
        (bool, bool) toggleResult = SpellManager.ToggleConnectionOnGraph(startSlot.node, selectedSlot.node);
        
        //Connection Added?
        if(toggleResult.Item1) { if(!toggleResult.Item2) CancelLine(); }
        //Connection Removed?
        else { if (toggleResult.Item2) CancelLinesBetween(startSlot, selectedSlot); }
        
        base.ExitModeTo(nextMode);
        selectedSlot = null;
        
    }

}
public class FreeMode : SpellEditorMode 
{   
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
            ExitModeTo(SpellEditor.runeSelectorMode);
            return;
        }
    }
    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouse mouseMoveEvent) {
            if(mouseMoveEvent is InputEventMouseButton) { mouseLocked = true; }
            else if(mouseLocked) {
                selectedSlot = FindClosestSpellSlotFromMouse(mouseMoveEvent);
                if(selectedSlot != null) position = selectedSlot.GetGlobalRect().GetCenter();
            }
        }
        else {
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
        nextMode.position = position;
        if(nextMode is FreeMode freeChildMode) 
        {
            if(selectedSlot == null) return;
            freeChildMode.selectedSlot = selectedSlot;
        }
        else if (nextMode is ConnectMode connectChildMode)
        {
            if(selectedSlot == null) return;
            connectChildMode.selectedSlot = selectedSlot;
        }
        base.ExitModeTo(nextMode);
        selectedSlot = null;
        
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
        if(prevMode is FreeMode freePrev) { 
            this.selectedSlot = freePrev.selectedSlot; 
            
        }
        base.EnterModeFrom(prevMode);

    }
    public override void ExitModeTo(SpellEditorMode nextMode)
    {
        base.ExitModeTo(nextMode);
        selectedSlot = null;
        
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
        selectedSlot = null;
    }
}

}