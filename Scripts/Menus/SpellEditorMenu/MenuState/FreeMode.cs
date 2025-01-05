using System;
using System.Collections.Generic;
using Godot;

namespace SpellEditing
{


public class FreeMode : SpellEditorState 
{
    private const float CAMERA_SMOOTH_SPEED = 10f;
    private const float CAMERA_ZOOM = 1f;
    protected virtual void ConfirmationInput(InputEvent @event)
    {
        if(@event.IsActionPressed("game_act_1", false)) 
        {
            if(tempSelection != null) {
                ExitModeTo(editor.selectionMode);
            }
            return;
        }
        else if(@event.IsActionPressed("game_act_2", false)) 
        {
            if(tempSelection != null) ExitModeTo(editor.dragMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            if(tempSelection != null) ExitModeTo(editor.connectMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_4", false)) 
        {
            ExitModeTo(editor.viewMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_5", false)) 
        {
            ExitModeTo(editor.runeSelectorMode);
            return;
        }
    }
    public override void _Input(InputEvent @event)
    {
        ConfirmationInput(@event); 
    }
    public override void _Process(double delta) { }
    public override void EnterModeFrom(SpellEditorState prevMode)
    {
        
        base.EnterModeFrom(prevMode);
        
        foreach(Control nodeView in editor.graphNodeMaster.GetChildren()){
            Action enter = () => { FocusNode(nodeView); };
            stashedEnterActions.Add(nodeView, enter);
            nodeView.FocusEntered += enter;
            

            Action exit = () => { UnFocusNode(nodeView); };
            stashedExitActions.Add(nodeView, exit);
            nodeView.FocusExited += exit;
            

            nodeView.FocusMode = Control.FocusModeEnum.All;
        }

        graphCamera.PositionSmoothingSpeed = CAMERA_SMOOTH_SPEED;
        graphCamera.Zoom = Vector2.One * CAMERA_ZOOM;

        if( tempSelection != null && tempSelection.IsNodeReady()) 
        { 
            graphCamera.Position = tempSelection.Position; 
            tempSelection.CallDeferred(Control.MethodName.GrabFocus);
        }      
    }

    private Dictionary<Control, Action> stashedEnterActions = new Dictionary<Control, Action>();
    private Dictionary<Control, Action> stashedExitActions = new Dictionary<Control, Action>();
    public override void ExitModeTo(SpellEditorState nextMode)
    {
        
        foreach(Control nodeView in editor.graphNodeMaster.GetChildren()){
            nodeView.FocusEntered -= stashedEnterActions[nodeView];
            nodeView.FocusExited -= stashedExitActions[nodeView];
            nodeView.FocusMode = Control.FocusModeEnum.None;
        }
        stashedEnterActions.Clear();
        stashedExitActions.Clear();
        base.ExitModeTo(nextMode); 
    }
    protected virtual void FocusNode(Control selection)
    {
        tempSelection = (VisualNode) selection;
        graphCamera.Position = tempSelection.Position;
    }
    protected virtual void UnFocusNode(Control selection)
    {
        if(selection == tempSelection) tempSelection = null;
    }

}

}    
