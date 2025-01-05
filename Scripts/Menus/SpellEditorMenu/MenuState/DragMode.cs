using System;
using System.Collections.Generic;
using Godot;

namespace SpellEditing
{


public class DragMode : SpellEditorState
{
    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseMotion mouseEvent) { 
            graphCamera.Position -= mouseEvent.Relative;
        }
        else {
            graphCamera.Position += 
            Input.GetVector("game_left", "game_right", "game_up", "game_down").Normalized()*25f;
        }

        tempSelection.UpdateArcPosition();

        if(@event.IsActionReleased("game_act_2", false))
        {
            ExitModeTo(editor.freeMode);
        }
    }
    public override void _Process(double delta) => tempSelection.Position = graphCamera.GetScreenCenterPosition();
    public override void EnterModeFrom(SpellEditorState prevMode)
    {
        base.EnterModeFrom(prevMode);
        if(tempSelection == null || !tempSelection.IsNodeReady()) { ExitModeTo(prevMode); return; }

        foreach(Control nodeView in editor.graphNodeMaster.GetChildren()){
            nodeView.FocusMode = Control.FocusModeEnum.None;
        }
        
        
        tempSelection.FocusMode = Control.FocusModeEnum.All;
        tempSelection.GrabFocus();
        
    }
    public override void ExitModeTo(SpellEditorState nextMode)
    {
        base.ExitModeTo(nextMode);
    }

}

}
