using System;
using System.Collections.Generic;
using Godot;

namespace SpellEditing
{


public class RuneSelectorMode : SpellEditorState
{
    public RuneSelector selector;

    public override void _Process(double delta) {  }
    private void ConfirmationInput(InputEvent @event)
    {
    
        //To Add new Rune;
        if(@event.IsActionPressed("game_act_1", false)) 
        {
            var selectedRune = selector.ConfirmSelection();
            if(selectedRune == null) return;
            if(prevMode is NodeFocusMode) editor.ReplaceNode(tempSelection, selectedRune);
            else tempSelection = editor.Add(selectedRune, graphCamera.Position);
            
            ExitModeTo(editor.freeMode);
            
            return;
        }
        else if(@event.IsActionPressed("game_act_2", false)) 
        {
            var selectedRune = selector.ConfirmSelection();
            if(selectedRune != null) 
                tempSelection = editor.Add(selector.ConfirmSelection(), graphCamera.Position);
            
            ExitModeTo(editor.dragMode);
            return;
        }

        //To Swap the current rune for this one;
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            var selectedRune = selector.ConfirmSelection();
            if(selectedRune == null ) { ExitModeTo(prevMode); return; }
            
            if(editor.selectedNode == null) editor.Add(selectedRune, graphCamera.Position);
            else                            editor.ReplaceNode(editor.selectedNode, selectedRune);
            
            ExitModeTo(editor.freeMode);
            return;
        }

        else if(@event.IsActionPressed("game_act_4", false)) 
        {
            ExitModeTo(editor.viewMode);
            return;
        }

        else if(@event.IsActionPressed("game_act_5", false)) 
        {       
            ExitModeTo(editor.freeMode);
            return;
        }

    }
    public override void _Input(InputEvent @event)
    {
        if(@event.IsAction("game_left", true)) { selector.SelectLeft(@event); }
        else if(@event.IsAction("game_right", true)) { selector.SelectRight(@event); }
        else if(@event.IsActionPressed("game_up", false)) { selector.SelectRarityUp(); }
        else if(@event.IsActionPressed("game_down", false)) { selector.SelectBackwards(@event); }
        else ConfirmationInput(@event);
    }
    public override void EnterModeFrom(SpellEditorState prevMode)
    {  
        base.EnterModeFrom(prevMode);
        tempSelection = null;
        
    }
    public override void ExitModeTo(SpellEditorState nextMode)
    {
        if(tempSelection != null && nextMode is ConnectMode)
            editor.Connect(tempSelection, editor.selectedNode);

        base.ExitModeTo(nextMode);
    }
}

}    
