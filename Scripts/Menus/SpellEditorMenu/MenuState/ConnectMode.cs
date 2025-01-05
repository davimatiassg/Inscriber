using System;
using System.Collections.Generic;
using Godot;

namespace SpellEditing
{

public class ConnectMode : SpellEditorState
{
    private VisualArc tryingArc = null;
    private Vector2 targetPosition = Vector2.Zero;
    public override void EnterModeFrom(SpellEditorState prevMode)
    {
        base.EnterModeFrom(prevMode);
        if(tempSelection == null) { ExitModeTo(prevMode); return; }
        

        targetPosition = tempSelection.Position;
        graphCamera.ZoomToFitGraph(editor);

        tryingArc = new VisualArc{ Source = editor.selectedNode };
        editor.graphArcsMaster.AddChild(tryingArc);
        
        tryingArc.MoveSource();

        Action<Control> focusFirstNodeOnce = (Control node) => { 
            if( node != null && node.IsNodeReady()) node.CallDeferred(Control.MethodName.GrabFocus);
            focusFirstNodeOnce = (Control _node) => {};
        };

        foreach(Control nodeView in editor.graphNodeMaster.GetChildren()){
            if(nodeView == tempSelection) continue;
            focusFirstNodeOnce(nodeView);
            nodeView.FocusMode = Control.FocusModeEnum.All;

            Action enter = () => { 
                tempSelection = (VisualNode) nodeView;
                targetPosition = tempSelection.Position;
                tryingArc.MoveTarget(targetPosition);
            };
            stashedEnterActions.Add(nodeView, enter);            
            nodeView.FocusEntered += enter;
            
        }
          
    }
    private Dictionary<Control, Action> stashedEnterActions = new Dictionary<Control, Action>();
    public override void ExitModeTo(SpellEditorState nextMode)
    {
        tryingArc.QueueFree();
        tryingArc = null;

        foreach(Control nodeView in editor.graphNodeMaster.GetChildren()){
            Action enter;
            if(stashedEnterActions.TryGetValue(nodeView, out enter)) { nodeView.FocusEntered -= enter; }
            nodeView.FocusMode = Control.FocusModeEnum.None;
        }
        stashedEnterActions.Clear();
        
        graphCamera.Position = tempSelection.Position;

        base.ExitModeTo(nextMode); 
    }
    protected void ConfirmationInput(InputEvent @event)
    {
        if(@event.IsActionPressed("game_act_1", false)) 
        {
            if(tempSelection != null && tempSelection != editor.selectedNode) ConnectSelection();
            ExitModeTo(editor.freeMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_2", false)) 
        {
            ExitModeTo(editor.dragMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            ExitModeTo(prevMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_5", false)) 
        {
            ExitModeTo(editor.runeSelectorMode);
            return;
        }
    }
    
    public void ConnectSelection(){ 
        editor.Connect(editor.selectedNode, tempSelection);
        tryingArc.Target = tempSelection;
    }
    
    public override void _Input(InputEvent @event)
    {
        ConfirmationInput(@event); 
    }
    public override void _Process(double delta)
    {  
    }
    
}
}    
