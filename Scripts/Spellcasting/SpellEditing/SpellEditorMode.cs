using System;
using System.Collections.Generic;
using Godot;

namespace SpellEditing
{

public abstract class SpellGraphEditorMode
{
    protected static SpellGraphEditorMode prevMode;
    public Control overlay;
    public SpellGraphVisualNode tempSelection;
    protected SpellGraphCamera graphCamera;
    public virtual void EnterModeFrom(SpellGraphEditorMode mode) 
    {
        prevMode = mode;
        graphCamera = SpellGraphEditor.Instance.graphView.spellGraphCamera;
        SpellGraphEditor.editorMode = this;
        if(overlay != null) overlay.Visible = true;
        tempSelection = SpellGraphEditor.selectedNode;
    }
    public virtual void ExitModeTo(SpellGraphEditorMode nextMode) 
    {
        prevMode = null; 
        if(overlay != null) overlay.Visible = false;
        SpellGraphEditor.selectedNode = tempSelection;
        nextMode.EnterModeFrom(this);
    }
    public abstract void _Process(double delta);
    public abstract void _Input (InputEvent @event);
}
public class NodeFocusMode : SpellGraphEditorMode
{
    
    private const float CAMERA_SMOOTH_SPEED = 25f;
    private const float CAMERA_ZOOM = 4F;
    protected virtual void ConfirmationInput(InputEvent @event)
    {
        if(@event.IsActionPressed("game_act_1", false)) 
        {
            if(tempSelection != null) {
                ExitModeTo(SpellGraphEditor.freeMode);
            }
            return;
        }
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            if(tempSelection != null) ExitModeTo(SpellGraphEditor.connectMode);
            return;
        }
        
        else if(@event.IsActionPressed("game_act_5", false)) 
        {
            ExitModeTo(SpellGraphEditor.runeSelectorMode);
            return;
        }
    }
    public override void EnterModeFrom(SpellGraphEditorMode prevMode) 
    { 
        base.EnterModeFrom(prevMode);
        
        if(tempSelection == null || !tempSelection.IsNodeReady()) { ExitModeTo(prevMode); return; }
        
        foreach(Control nodeView in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren()){
            nodeView.FocusMode = Control.FocusModeEnum.None;
        }

        tempSelection.FocusMode = Control.FocusModeEnum.All;
        tempSelection.GrabFocus();

        graphCamera.PositionSmoothingSpeed = CAMERA_SMOOTH_SPEED;
        graphCamera.Zoom = Vector2.One*CAMERA_ZOOM;
        graphCamera.Position = SpellGraphEditor.selectedNode.Position;
        
    }
    public override void ExitModeTo(SpellGraphEditorMode nextMode) 
    {
        graphCamera.Position = SpellGraphEditor.selectedNode.Position;
        graphCamera.Zoom = Vector2.One;
        base.ExitModeTo(nextMode);
        
    }

    public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("game_act_4", false)) 
        {
            SpellGraphEditor.RemoveGraphNode(tempSelection);
            ExitModeTo(SpellGraphEditor.viewMode);
            tempSelection = null;
            return;
        }

        ConfirmationInput(@event);
    }

    public override void _Process(double delta)
    {

    }
}

public partial class ViewMode : SpellGraphEditorMode
{
    protected virtual void ConfirmationInput(InputEvent @event)
    {
        if(@event.IsActionReleased("game_act_4", false)) 
        {
            SpellGraphEditor.selectedNode = SpellGraphEditor.FindClosestNodeFrom(graphCamera.Position);
            tempSelection = SpellGraphEditor.selectedNode;
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

public class FreeMode : SpellGraphEditorMode 
{
    private const float CAMERA_SMOOTH_SPEED = 10f;
    private const float CAMERA_ZOOM = 1f;
    protected virtual void ConfirmationInput(InputEvent @event)
    {
        if(@event.IsActionPressed("game_act_1", false)) 
        {
            if(tempSelection != null) {
                ExitModeTo(SpellGraphEditor.selectionMode);
            }
            return;
        }
        else if(@event.IsActionPressed("game_act_2", false)) 
        {
            if(tempSelection != null) ExitModeTo(SpellGraphEditor.dragMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            if(tempSelection != null) ExitModeTo(SpellGraphEditor.connectMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_4", false)) 
        {
            ExitModeTo(SpellGraphEditor.viewMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_5", false)) 
        {
            ExitModeTo(SpellGraphEditor.runeSelectorMode);
            return;
        }
    }
    public override void _Input(InputEvent @event)
    {
        ConfirmationInput(@event); 
    }
    public override void _Process(double delta) { }
    public override void EnterModeFrom(SpellGraphEditorMode prevMode)
    {
        
        base.EnterModeFrom(prevMode);
        
        foreach(Control nodeView in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren()){
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
    public override void ExitModeTo(SpellGraphEditorMode nextMode)
    {
        
        foreach(Control nodeView in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren()){
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
        tempSelection = (SpellGraphVisualNode) selection;
        graphCamera.Position = tempSelection.Position;
    }
    protected virtual void UnFocusNode(Control selection)
    {
        if(selection == tempSelection) tempSelection = null;
    }

}
public class DragMode : SpellGraphEditorMode
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
            ExitModeTo(SpellGraphEditor.freeMode);
        }
    }
    public override void _Process(double delta) => tempSelection.Position = graphCamera.GetScreenCenterPosition();
    public override void EnterModeFrom(SpellGraphEditorMode prevMode)
    {
        base.EnterModeFrom(prevMode);
        if(tempSelection == null || !tempSelection.IsNodeReady()) { ExitModeTo(prevMode); return; }

        foreach(Control nodeView in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren()){
            nodeView.FocusMode = Control.FocusModeEnum.None;
        }
        
        
        tempSelection.FocusMode = Control.FocusModeEnum.All;
        tempSelection.GrabFocus();
        
    }
    public override void ExitModeTo(SpellGraphEditorMode nextMode)
    {
        base.ExitModeTo(nextMode);
    }

}
public class ConnectMode : SpellGraphEditorMode
{
    private SpellGraphVisualArc tryingArc = null;
    private Vector2 targetPosition = Vector2.Zero;
    public override void EnterModeFrom(SpellGraphEditorMode prevMode)
    {
        base.EnterModeFrom(prevMode);
        if(tempSelection == null) { ExitModeTo(prevMode); return; }
        

        targetPosition = tempSelection.Position;
        graphCamera.ZoomToFitGraph(SpellGraphEditor.Instance.graphView);

        tryingArc = new SpellGraphVisualArc{ Source = SpellGraphEditor.selectedNode };
        SpellGraphEditor.Instance.graphView.graphArcsMaster.AddChild(tryingArc);
        
        tryingArc.MoveSource();

        Action<Control> focusFirstNodeOnce = (Control node) => { 
            if( node != null && node.IsNodeReady()) node.CallDeferred(Control.MethodName.GrabFocus);
            focusFirstNodeOnce = (Control _node) => {};
        };

        foreach(Control nodeView in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren()){
            if(nodeView == tempSelection) continue;
            focusFirstNodeOnce(nodeView);
            nodeView.FocusMode = Control.FocusModeEnum.All;

            Action enter = () => { 
                tempSelection = (SpellGraphVisualNode) nodeView;
                targetPosition = tempSelection.Position;
                tryingArc.MoveTarget(targetPosition);
            };
            stashedEnterActions.Add(nodeView, enter);            
            nodeView.FocusEntered += enter;
            
        }
          
    }
    private Dictionary<Control, Action> stashedEnterActions = new Dictionary<Control, Action>();
    public override void ExitModeTo(SpellGraphEditorMode nextMode)
    {
        tryingArc.QueueFree();
        tryingArc = null;

        foreach(Control nodeView in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren()){
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
            if(tempSelection != null && tempSelection != SpellGraphEditor.selectedNode) ConnectSelection();
            ExitModeTo(SpellGraphEditor.freeMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_2", false)) 
        {
            ExitModeTo(SpellGraphEditor.dragMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            ExitModeTo(prevMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_5", false)) 
        {
            ExitModeTo(SpellGraphEditor.runeSelectorMode);
            return;
        }
    }
    
    public void ConnectSelection(){ 
        SpellGraphEditor.ConnectGraphNodes(SpellGraphEditor.selectedNode, tempSelection);
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
public class RuneSelectorMode : SpellGraphEditorMode
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
            if(prevMode is NodeFocusMode) tempSelection =  SpellGraphEditor.ReplaceGraphNode(selectedRune, tempSelection);
            else tempSelection = SpellGraphEditor.AddGraphNode(selectedRune, graphCamera.Position);
            
            ExitModeTo(SpellGraphEditor.freeMode);
            
            return;
        }
        else if(@event.IsActionPressed("game_act_2", false)) 
        {
            var selectedRune = selector.ConfirmSelection();
            if(selectedRune != null) 
                tempSelection = SpellGraphEditor.AddGraphNode(selector.ConfirmSelection(), graphCamera.Position);
            
            ExitModeTo(SpellGraphEditor.dragMode);
            return;
        }

        //To Swap the current rune for this one;
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            var selectedRune = selector.ConfirmSelection();
            if(selectedRune == null ) { ExitModeTo(prevMode); return; }
            
            tempSelection = (SpellGraphEditor.selectedNode == null) ? 
                SpellGraphEditor.AddGraphNode(selector.ConfirmSelection(), graphCamera.Position)
                : tempSelection = SpellGraphEditor.ReplaceGraphNode(selector.ConfirmSelection(), SpellGraphEditor.selectedNode);   
            
            ExitModeTo(SpellGraphEditor.freeMode);
            return;
        }

        else if(@event.IsActionPressed("game_act_4", false)) 
        {
            ExitModeTo(SpellGraphEditor.viewMode);
            return;
        }

        else if(@event.IsActionPressed("game_act_5", false)) 
        {       
            ExitModeTo(SpellGraphEditor.freeMode);
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
    public override void EnterModeFrom(SpellGraphEditorMode prevMode)
    {  
        base.EnterModeFrom(prevMode);
        tempSelection = null;
        
    }
    public override void ExitModeTo(SpellGraphEditorMode nextMode)
    {
        if(tempSelection != null && nextMode is ConnectMode)
            SpellGraphEditor.ConnectGraphNodes(tempSelection, SpellGraphEditor.selectedNode);

        base.ExitModeTo(nextMode);
    }
}

}    

/*
'
public class RuneSelectorMode : SpellGraphEditorMode
{
    public RuneSelector selector;
    public SpellGraphVisualNode selectedSlot;
    public override void _Process(double delta) {  }
    private void CheckTriggerSwap(InputEvent @event)
    {
        if(@event.IsActionPressed("game_act_1", false)) 
        {
            selectedSlot = SpellGraphEditor.AddSlot(selector.ConfirmSelection(), selector.GetViewportRect().GetCenter());
            ExitModeTo(SpellGraphEditor.dragMode);
            return;
        }

        else if(@event.IsActionPressed("game_act_2", false)) 
        {       
            if(selectedSlot == null) {
                GD.Print("lol no");
                selectedSlot = SpellGraphEditor.AddSlot(selector.ConfirmSelection(), selector.GetViewportRect().GetCenter());
            }
            else {
                selectedSlot = SpellGraphEditor.SubstituteSlot(selector.ConfirmSelection(), selectedSlot);   
            }
            
            ExitModeTo(SpellGraphEditor.dragMode);
            return;
        }

        else if(@event.IsActionPressed("game_act_5", false)) 
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
    public override void EnterModeFrom(SpellGraphEditorMode prevMode)
    {
        if(prevMode is FreeMode freeChildMode && freeChildMode.selectedSlot != null) 
            selectedSlot = freeChildMode.selectedSlot;
        
        base.EnterModeFrom(prevMode);
    }
    public override void ExitModeTo(SpellGraphEditorMode nextMode)
    {
        
        if(nextMode is FreeMode freeChildMode && selectedSlot != null) 
            freeChildMode.selectedSlot = selectedSlot;
        else if (nextMode is ConnectMode connectChildMode && selectedSlot != null) 
            connectChildMode.selectedSlot = selectedSlot;

        base.ExitModeTo(nextMode);
        selectedSlot = null;
    }
}
public class ConnectMode : SpellGraphEditorMode
{
    public SpellGraphVisualNode selectedSlot;
    public SpellGraphVisualNode startSlot = null;
    private SpellGraphVisualArc line;
    private bool mouseLocked;
    protected void MovePositionAlongAction()
    {
        Vector2 v = Input.GetVector("game_left", "game_right", "game_up", "game_down")*16;
        if(v == Vector2.Zero) { return; }
        position += v;
    }
    protected SpellGraphVisualNode FindClosestSpellGraphVisualNodeFromMouse(InputEventMouse @event)
    {
        float proximity = Mathf.Inf;
        Vector2 pos = @event.Position;
        SpellGraphVisualNode closest = null;
        foreach(Control c in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren())
        {
            float lenghtSQR = pos.DistanceSquaredTo(c.GetGlobalRect().GetCenter());
            if(lenghtSQR < proximity && c is SpellGraphVisualNode s)
            {
                proximity = lenghtSQR;
                closest = s;
            }
        }
        return closest;
    }
    protected bool SelectSlotInDirection(Vector2 direction)
    {
        if(SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildCount() == 0) return false;
        float proximity = Mathf.Inf;
        Vector2 prevPos = position;

        foreach(Control c in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren())
        {
            Vector2 moveDirection = c.GetGlobalRect().GetCenter()-position;
            float lenght =  moveDirection.Length();
            if(lenght < 1f) { continue;}
            if((moveDirection/lenght).Dot(direction) < 0.4) { continue; }
            if(lenght < proximity)
            {
                proximity = lenght;
                prevPos = c.GetGlobalRect().GetCenter();
                if(c is SpellGraphVisualNode s) { selectedSlot = s; }
            }
        }
        position = prevPos;
        return proximity != Mathf.Inf;
    }
    private void CheckTriggerSwap(InputEvent @event)
    {
        if(@event.IsActionPressed("game_act_1", false)) 
        {
            ExitModeTo(SpellGraphEditor.freeMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_2", false)) 
        {
            ExitModeTo(SpellGraphEditor.dragMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            ExitModeTo(prevMode);
            return;
        }
        //{ não tem o game_act_4 
        //}
        else if(@event.IsActionPressed("game_act_5", false)) 
        {
            ExitModeTo(SpellGraphEditor.runeSelectorMode);
            return;
        }
    }
    public void CancelLinesBetween(SpellGraphVisualNode first, SpellGraphVisualNode last)
    {
        int i = 0;
        foreach(SpellGraphVisualArc line in SpellGraphEditor.Instance.graphView.graphArcsMaster.GetChildren())
        {
            if(line.Target == first && line.Source == last) { CancelLine(line); i++; continue; }
            if(line.Source == first && line.Target == last) { CancelLine(line); i++; }
            if(i > 1) { return; }
        }
    }
    public void CancelLine(SpellGraphVisualArc line)
    {
        SpellGraphEditor.Instance.graphView.graphArcsMaster.RemoveChild(line); 
        line.DisconnectAll();
        line.QueueFree();
        return;
    }
    public void CancelLine() => CancelLine(this.line);
    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseMotion mouseMove)
        { 
            mouseLocked = true;
            line.MoveTip(mouseMove.Position);
        }
        else if(@event is InputEventMouseButton mouseButton && mouseLocked)
        { 
            selectedSlot = FindClosestSpellGraphVisualNodeFromMouse(mouseButton);
            line.Target = selectedSlot;
            line.MoveTip();
        }
        else
        {
            mouseLocked = false;
            SelectSlotInDirection(Input.GetVector("game_left", "game_right", "game_up", "game_down").Normalized());
            line.Target = selectedSlot;
            position = selectedSlot.Position;
        }

        line.MoveBot();
        
        CheckTriggerSwap(@event); 
    }
    public override void _Process(double delta) 
    {
        SpellGraphEditorCamera.Instance.Position = position; 
    }
    public override void EnterModeFrom(SpellGraphEditorMode prevMode)
    {
        base.EnterModeFrom(prevMode);
        if(prevMode is FreeMode freePrev) { 
            this.selectedSlot = freePrev.selectedSlot; 
            startSlot = selectedSlot; 
        }
        if(startSlot == null) { ExitModeTo(prevMode); }
        this.line = new SpellGraphVisualArc{ Source = startSlot };
        SpellGraphEditor.Instance.graphView.graphArcsMaster.AddChild(line);
        
    }
    public override void ExitModeTo(SpellGraphEditorMode nextMode)
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
public class FreeMode : SpellGraphEditorMode 
{   
    public SpellGraphVisualNode selectedSlot;
    protected bool mouseLocked = false;
    protected SpellGraphVisualNode FindClosestSpellGraphVisualNodeFromMouse(InputEventMouse @event)
    {
        float proximity = Mathf.Inf;
        Vector2 pos = @event.Position;
        SpellGraphVisualNode closest = null;
        foreach(Control c in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren())
        {
            float lenght = pos.DistanceTo(c.GetGlobalRect().GetCenter());
            if(lenght < proximity && c is SpellGraphVisualNode s)
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
        if(SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildCount() == 0) return false;
        float proximity = Mathf.Inf;
        Vector2 prevPos = position;
        Vector2 dir = Input.GetVector("game_left", "game_right", "game_up", "game_down").Normalized();
        foreach(Control c in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren())
        {
            Vector2 moveDirection = c.GetGlobalRect().GetCenter()-position;
            float lenght =  moveDirection.Length();
            if(lenght < 1f) { continue;}
            if((moveDirection/lenght).Dot(dir) < 0.5) { continue; }
            if(lenght < proximity)
            {
                proximity = lenght;
                prevPos = c.GetGlobalRect().GetCenter();;
                if(c is SpellGraphVisualNode s) { selectedSlot = s; }
            }
        }
        position = prevPos;
        return proximity != Mathf.Inf;
    }
    private void CheckTriggerSwap(InputEvent @event)
    {
        if(@event.IsActionPressed("game_act_1", false)) 
        {
            SpellGraphVisualNode closestSlot =  FindClosestSpellGraphVisualNodeFromMouse((InputEventMouse)@event);
            if(selectedSlot == closestSlot ) {
                ExitModeTo(SpellGraphEditor.selectionMode);
            }
            return;
        }
        else if(@event.IsActionPressed("game_act_2", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellGraphEditor.dragMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellGraphEditor.connectMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_5", false)) 
        {
            ExitModeTo(SpellGraphEditor.runeSelectorMode);
            return;
        }
    }
    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouse mouseMoveEvent) {
            if(mouseMoveEvent is InputEventMouseButton) { mouseLocked = true; }
            else if(mouseLocked) {
                selectedSlot = FindClosestSpellGraphVisualNodeFromMouse(mouseMoveEvent);
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
        if (mouseLocked == false) SpellGraphEditorCamera.Instance.Position = position;
    }
    public override void EnterModeFrom(SpellGraphEditorMode prevMode)
    {
        base.EnterModeFrom(prevMode);
    }
    public override void ExitModeTo(SpellGraphEditorMode nextMode)
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
        if(@event.IsActionPressed("game_act_1", false)) 
        {
            ExitModeTo(SpellGraphEditor.selectionMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_2", false)) 
        {
            ExitModeTo(SpellGraphEditor.freeMode);
            return;
        }
        else if(@event.IsActionPressed("game_act_3", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellGraphEditor.connectMode);
            return;
        }
        //{ não tem o game_act_4 
        //}
        else if(@event.IsActionPressed("game_act_5", false)) 
        {
            if(selectedSlot != null) ExitModeTo(SpellGraphEditor.runeSelectorMode);
            return;
        }
    }
    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouse mouseMoveEvent)
        {
            if(mouseMoveEvent is InputEventMouseButton) { mouseLocked = true; }
            else if(mouseLocked) { position = mouseMoveEvent.Position; }
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
        selectedSlot.Position = SpellGraphEditorCamera.Instance.GetScreenCenterPosition() - selectedSlot.GetGlobalRect().GetCenter() + selectedSlot.Position;
    }
    public override void EnterModeFrom(SpellGraphEditorMode prevMode)
    {
        if(prevMode is FreeMode freePrev) { 
            this.selectedSlot = freePrev.selectedSlot; 
        }
        else if(prevMode is ConnectMode connectMode) { 
            this.selectedSlot = connectMode.selectedSlot; 
        }
        base.EnterModeFrom(prevMode);
        if(selectedSlot == null) { ExitModeTo(prevMode); return; }
    }
    public override void ExitModeTo(SpellGraphEditorMode nextMode)
    {
        base.ExitModeTo(nextMode);
        selectedSlot = null;  
    }

}
public class SelectionMode : FreeMode
{
    public override void EnterModeFrom(SpellGraphEditorMode prevMode) 
    { 
        base.EnterModeFrom(prevMode);
    }
    public override void ExitModeTo(SpellGraphEditorMode nextMode) 
    {
        base.ExitModeTo(nextMode);
        selectedSlot = null;
    }
}

}



protected SpellGraphVisualNode FindClosestSpellGraphVisualNodeFromMouse(InputEventMouse @event)
    {
        float proximity = Mathf.Inf;
        Vector2 pos = @event.Position;
        SpellGraphVisualNode closest = null;
        foreach(Control c in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren())
        {
            float lenght = pos.DistanceTo(c.GetGlobalRect().GetCenter());
            if(lenght < proximity && c is SpellGraphVisualNode s)
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
        if(SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildCount() == 0) return false;
        float proximity = Mathf.Inf;
        Vector2 prevPos = position;
        Vector2 dir = Input.GetVector("game_left", "game_right", "game_up", "game_down").Normalized();
        foreach(Control c in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren())
        {
            Vector2 moveDirection = c.GetGlobalRect().GetCenter()-position;
            float lenght =  moveDirection.Length();
            if(lenght < 1f) { continue;}
            if((moveDirection/lenght).Dot(dir) < 0.5) { continue; }
            if(lenght < proximity)
            {
                proximity = lenght;
                prevPos = c.GetGlobalRect().GetCenter();;
                if(c is SpellGraphVisualNode s) { selectedSlot = s; }
            }
        }
        position = prevPos;
        return proximity != Mathf.Inf;
    }











protected SpellGraphVisualNode FindClosestSpellGraphVisualNodeFromMouse(InputEventMouse @event)
    {
        float proximity = Mathf.Inf;
        Vector2 pos = @event.Position;
        SpellGraphVisualNode closest = null;
        foreach(Control c in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren())
        {
            float lenghtSQR = pos.DistanceSquaredTo(c.GetGlobalRect().GetCenter());
            if(lenghtSQR < proximity && c is SpellGraphVisualNode s)
            {
                proximity = lenghtSQR;
                closest = s;
            }
        }
        return closest;
    }
    protected bool SelectSlotInDirection(Vector2 direction)
    {
        if(SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildCount() == 0) return false;
        float proximity = Mathf.Inf;
        Vector2 prevPos = position;

        foreach(Control c in SpellGraphEditor.Instance.graphView.graphNodeMaster.GetChildren())
        {
            Vector2 moveDirection = c.GetGlobalRect().GetCenter()-position;
            float lenght =  moveDirection.Length();
            if(lenght < 1f) { continue;}
            if((moveDirection/lenght).Dot(direction) < 0.4) { continue; }
            if(lenght < proximity)
            {
                proximity = lenght;
                prevPos = c.GetGlobalRect().GetCenter();
                if(c is SpellGraphVisualNode s) { selectedSlot = s; }
            }
        }
        position = prevPos;
        return proximity != Mathf.Inf;
    }






*/