using System;
using System.Collections.Generic;
using Godot;

namespace SpellEditing
{

public class NodeFocusMode : SpellEditorState
{
    
    private const float CAMERA_SMOOTH_SPEED = 25f;
    private const float CAMERA_ZOOM = 4F;

    protected virtual void ConfirmationInput(InputEvent @event)
    {
        if(@event.IsActionPressed("game_act_1", false)) 
        {
            if(tempSelection != null) {
                ExitModeTo(editor.freeMode);
            }
            return;
        }
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            if(tempSelection != null) ExitModeTo(editor.connectMode);
            return;
        }
        
        else if(@event.IsActionPressed("game_act_5", false)) 
        {
            ExitModeTo(editor.runeSelectorMode);
            return;
        }
    }

    public override void EnterModeFrom(SpellEditorState prevMode) 
    { 
        base.EnterModeFrom(prevMode);
        
        if(tempSelection == null || !tempSelection.IsNodeReady()) { ExitModeTo(prevMode); return; }
        
        foreach(Control nodeView in editor.graphNodeMaster.GetChildren()){
            nodeView.FocusMode = Control.FocusModeEnum.None;
        }

        tempSelection.FocusMode = Control.FocusModeEnum.All;
        tempSelection.GrabFocus();

        graphCamera.PositionSmoothingSpeed = CAMERA_SMOOTH_SPEED;
        graphCamera.Zoom = Vector2.One*CAMERA_ZOOM;
        graphCamera.Position = editor.selectedNode.Position;
        
    }

    
    public override void ExitModeTo(SpellEditorState nextMode) 
    {
        graphCamera.Position = editor.selectedNode.Position;
        graphCamera.Zoom = Vector2.One;
        base.ExitModeTo(nextMode);
        
    }

    public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("game_act_4", false)) 
        {
            editor.Remove(tempSelection);
            ExitModeTo(editor.viewMode);
            tempSelection = null;
            return;
        }

        ConfirmationInput(@event);
    }

    public override void _Process(double delta)
    {

    }
}

}    
