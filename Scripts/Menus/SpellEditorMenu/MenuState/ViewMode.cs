using System;
using System.Collections.Generic;
using Godot;

namespace SpellEditing
{

public partial class ViewMode : SpellEditorState
{
    protected virtual void ConfirmationInput(InputEvent @event)
    {
        if(@event.IsActionReleased("game_act_4", false)) 
        {
            editor.selectedNode = editor.FindClosestNodeFrom(graphCamera.Position);
            tempSelection = editor.selectedNode;
            ExitModeTo(prevMode);
            return;
        }
    }
    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseMotion mouseEvent) { 
            graphCamera.Position -= mouseEvent.Relative;
        }
        else {
            graphCamera.Position += 
            Input.GetVector("game_left", "game_right", "game_up", "game_down").Normalized()*25f;
        }

        ConfirmationInput(@event);
        
    }

    public override void _Process(double delta)
    {
    }
}

}    
