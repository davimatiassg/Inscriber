using System;
using System.Collections.Generic;
using Godot;

namespace SpellEditing
{

public abstract class SpellEditorState
{
    protected static SpellEditorState prevMode;
    public Control overlay;

    public SpellGraphEditor editor;
    public SpellGraphView.VisualNode tempSelection;
    protected SpellGraphCamera graphCamera;
    public virtual void EnterModeFrom(SpellEditorState mode) 
    {
        prevMode = mode;
        graphCamera = editor.spellGraphCamera;
        editor.EditorState = this;
        if(overlay != null) overlay.Visible = true;
        tempSelection = editor.selectedNode;
    }
    public virtual void ExitModeTo(SpellEditorState nextMode) 
    {
        prevMode = null; 
        if(overlay != null) overlay.Visible = false;
        editor.selectedNode = tempSelection;
        nextMode.EnterModeFrom(this);
    }
    public abstract void _Process(double delta);
    public abstract void _Input (InputEvent @event);
}



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
        tempSelection = (SpellGraphView.VisualNode) selection;
        graphCamera.Position = tempSelection.Position;
    }
    protected virtual void UnFocusNode(Control selection)
    {
        if(selection == tempSelection) tempSelection = null;
    }

}
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
public class ConnectMode : SpellEditorState
{
    private SpellGraphView.VisualArc tryingArc = null;
    private Vector2 targetPosition = Vector2.Zero;
    public override void EnterModeFrom(SpellEditorState prevMode)
    {
        base.EnterModeFrom(prevMode);
        if(tempSelection == null) { ExitModeTo(prevMode); return; }
        

        targetPosition = tempSelection.Position;
        graphCamera.ZoomToFitGraph(editor);

        tryingArc = new SpellGraphView.VisualArc{ Source = editor.selectedNode };
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
                tempSelection = (SpellGraphView.VisualNode) nodeView;
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
