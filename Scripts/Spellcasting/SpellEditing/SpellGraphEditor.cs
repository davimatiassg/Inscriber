using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SpellEditing
{

/// <summary>
/// Class to handle Spell Editor Inputs & UI Behavior
/// </summary>


using GraphType = AdjacenceListGraph;
public partial class SpellGraphEditor : Control
{

   
    private enum EEditorState { SPELL_SELECTOR, VIEW_MODE, FREE_MODE, DRAG_MODE, CONNECT_MODE, RUNE_SELECTOR, META_MENU }
    public static Action<SpellGraphVisualNode>    OnStartConnectionAtNode;
    public static Action<SpellGraphVisualNode>    OnEndConnectionAtNode;
    public static Action<SpellGraphVisualNode>    OnGrabNode;
    public static Action<SpellGraphVisualNode>    OnAddNode;
    public static Action<SpellGraphVisualNode>    OnSelectNode;
    public static Action                          OnUnselectNode;
    

    private EEditorState state = EEditorState.FREE_MODE;

    EEditorState State
    {
        get; set;
    }
    [ExportCategory("Editor Objects")]
    [Export] public RuneSelector runeSelector;

    
    [Export] public SpellGraphViewer graphView;

    [ExportCategory("Editor Mode's Overlays")]
    
    [Export] public Control viewOverlay;
    [Export] public Control freeOverlay;
    [Export] public Control dragOverlay;
    [Export] public Control connectOverlay;
    [Export] public Control selectOverlay;

    //STATIC MODES REFERENCES
    public static SpellGraphEditor Instance;
    public static ViewMode viewMode;
    public static FreeMode freeMode;
    public static DragMode dragMode;
    public static ConnectMode connectMode;
    public static NodeFocusMode selectionMode;
    public static RuneSelectorMode runeSelectorMode;
    

    public static SpellGraphEditorMode _editorMode;

    public static SpellGraphEditorMode editorMode 
    {
        get { return _editorMode; }
        set { _editorMode = value;
            //GD.PrintErr("Set editor mode: " + value.GetType()); 
            }
    }


    public static SpellGraphVisualNode selectedNode;
    

    public override void _Ready()
    {
        base._Ready();
        if(Instance == null) { 
            Instance = this;
            viewMode = new ViewMode { overlay = viewOverlay };
            freeMode = new FreeMode { overlay = freeOverlay };
            dragMode = new DragMode { overlay = dragOverlay };
            connectMode = new ConnectMode { overlay = connectOverlay };
            selectionMode = new NodeFocusMode { overlay = selectOverlay };
            runeSelectorMode = new RuneSelectorMode{ overlay = runeSelector, selector = runeSelector };

            editorMode = freeMode;
        }
        else if( Instance != this) { QueueFree(); }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        editorMode._Input(@event);

        if (@event is InputEventKey eventKey && eventKey.Pressed){
            
            if(eventKey.Keycode == Key.O)
            {
                GraphUtil.PrintPruffer();
            }

            if(eventKey.Keycode == Key.P){
        
                var pruffer = GraphUtil.TreeToPruffer((Graph)SpellManager.currentSpell.graphData);
                string s = "[b]Pruffer code: { ";
                foreach(int i in pruffer) 
                    s += GraphUtil.TestNodeString(SpellManager.currentSpell.graphData[i]) + " " ;
                s += "}[/b]" ;
                

                GraphType g = GraphUtil.ConvertGraphTo<GraphType>((Graph)SpellManager.currentSpell.graphData);
                
                g.Edges = new List<(ISpellGraph.Node, ISpellGraph.Node)>();

                LoadSpellGraph(GraphUtil.PrufferToTree<GraphType>(g, pruffer));
            }

            if (eventKey.Keycode == Key.Enter)
            {
                GraphParser.parent = this;
                GraphParser.OpenGraph<GraphType>();
                editorMode.ExitModeTo(viewMode);
            }

            if (eventKey.Keycode == Key.R && editorMode is NodeFocusMode)
            {
                string s = "[b]Articulações: {[/b] ";
                foreach(var node in GraphUtil.FindArticulations((Graph)SpellManager.currentSpell.graphData, graphView.viewPairsReverse[selectedNode]))
                {
                    s += ((Rune)node.castable).Name + " ";
                }
                s += "[b]}[/b]";
                GD.Print(s);
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(@delta);
        editorMode._Process(@delta);
    }


    public static SpellGraphVisualNode AddGraphNode<T>(T deployable, Vector2 position) where T : IGraphDeployable
    {
        var nodeView = Instance.graphView.DeployNewNode(deployable, position);
        if(deployable is ICastable castable)
        {
            Instance.graphView.AddNodeViewPair(SpellManager.AddNode(castable), nodeView);
        }
        OnAddNode?.Invoke(nodeView);
        return nodeView;
    }

    public static SpellGraphVisualNode ReplaceGraphNode<T>(T deployable, SpellGraphVisualNode nodeView) where T : IGraphDeployable
    {
        if(deployable is ICastable != nodeView.Deployable is ICastable) return nodeView;
        nodeView.Deployable = deployable;
        if(deployable is ICastable castable) 
        {
            SpellManager.ReplaceNode(Instance.graphView.GetPairNodeFrom(nodeView), castable);
        }
        
        nodeView.Deployable = deployable;
        OnAddNode?.Invoke(nodeView);
        return nodeView;
    }

    public static void RemoveGraphNode(SpellGraphVisualNode nodeView)
    {
        Instance.graphView.RemoveGraphNode(nodeView);
        SpellManager.RemoveNode(Instance.graphView.GetPairNodeFrom(nodeView));
    }

    public static void ConnectGraphNodes(SpellGraphVisualNode source, SpellGraphVisualNode target)
    {
        bool connectionSuccess = SpellManager.AddConnectionToSpellGraph(
            Instance.graphView.GetPairNodeFrom(source), 
            Instance.graphView.GetPairNodeFrom(target));
        if(!connectionSuccess) 
        { 
            ///Play Unconection Animation;
            return;
        }
        Instance.graphView.graphArcsMaster.AddChild(source.ConnectTo(target));
    }

    public static SpellGraphVisualNode FindClosestNodeFrom(Vector2 position) => Instance.graphView.FindClosestNodeFrom(position);



    //STUB!!!!!
    public static void LoadSpellGraph(ISpellGraph graph)
    {
        editorMode.ExitModeTo(viewMode);
        Instance.graphView.ClearView();

        SpellManager.currentSpell.graphData = graph;

        if(graph.Count == 0) return;

        float angleSpread = 2*Mathf.Pi/graph.Count;
        Vector2 position = new Vector2(25*graph.Count, 0);

        foreach(ISpellGraph.Node graphNode in graph.Nodes)
        {
            if(graphNode.castable is IGraphDeployable deployable){
            var nodeView = Instance.graphView.DeployNewNode(deployable, position);
            Instance.graphView.AddNodeViewPair(graphNode, nodeView);
            }
            position = position.Rotated(angleSpread);
        }

        foreach((ISpellGraph.Node src, ISpellGraph.Node trg) in graph.Edges)
        {
            var source = Instance.graphView.GetPairNodeFrom(src);
            var target = Instance.graphView.GetPairNodeFrom(trg);

            Instance.graphView.graphArcsMaster.AddChild(source.ConnectTo(target));
        }

        selectedNode = (SpellGraphVisualNode)Instance.graphView.graphNodeMaster.GetChild(0);

        editorMode.ExitModeTo(freeMode);

    }



}
}

/*
protected abstract class InputHandler 
    {  
        protected SpellLine line;
        protected Action<InputEvent> currentProcess;
        protected Action aditionalTasks;
        public abstract void Process(InputEvent @event);
        public abstract void Select(InputEvent @event);
    }


protected class MouseInputHandler : InputHandler 
        {

            private bool confirmedFocus = true;
            public override void Process(InputEvent @event)
            {
                switch(Instance.State)
                {
                    case EEditorState.ON_DESKTOP:
                        currentProcess = DesktopProcess;
                        break;
                    case EEditorState.ON_RUNE_SELECTOR:
                        currentProcess = RuneSelectorProcess;
                        if(Instance.CursorState == ECursorState.CONNECTING) EndConnection(); 
                        break;
                }
                currentProcess?.Invoke(@event);
                aditionalTasks?.Invoke();

            }
            public void DesktopProcess(InputEvent @event)
            {
                if(@event is InputEventMouseMotion) { Select(@event); }
                if(@event is InputEventMouseButton btn && btn.Pressed && btn.IsEcho())
                {
                    if(btn.ButtonIndex == MouseButton.Left){
                        if(Instance.CursorState == ECursorState.CONNECTING) { EndConnection(); }
                        if(Instance.CursorState == ECursorState.HOLDING) { EndDragging(); }
                        else { StartDragging(); }
                    }
                    if(btn.ButtonIndex == MouseButton.Right){
                        if(Instance.CursorState == ECursorState.HOLDING) { EndDragging(); }
                        if(Instance.CursorState == ECursorState.CONNECTING) { EndConnection(); }
                        else { StartConnection(); }
                    }
                }

                //if(!confirmedFocus) return;

                if(RuneSelector.Instance.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                { 
                    confirmedFocus = false; 
                    Instance.State = EEditorState.ON_RUNE_SELECTOR;

                }
                else if(RuneSelector.Instance.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                {
                    confirmedFocus = false; 
                }
                else {
                    confirmedFocus = true;
                    Instance.State = EEditorState.ON_DESKTOP;
                }
            }
            public void RuneSelectorProcess(InputEvent @event)
            {
                if(@event is InputEventMouseMotion) 
                { 
                    SelectFromMouseHeight();
                }
                if(@event is InputEventMouseButton btn && btn.Pressed && btn.IsEcho())
                {
                    if(btn.ButtonIndex == MouseButton.Left){
                        switch (Instance.CursorState){                    
                            case ECursorState.CONNECTING: 
                                EndConnection(); 
                                break;
                            case ECursorState.HOLDING: 
                                EndDragging();
                                break; 
                            default:
                                SelectFromMouseHeight();
                                StartDragging();
                                break;
                        }
                    }
                }
            }
            public override void Select(InputEvent @event) => Instance.SelectedControl = ChaseMouseSelection<Control>() ;
            public void SelectFromMouseHeight()
            {
                float percentage = Instance.GetGlobalMousePosition().Y/Instance.GetViewportRect().Size.Y;
                //Instance.SelectedControl = RuneSelector.Instance[percentage];
            }
            Action updateConnectionGraphs;
            private void StartConnection()
            {
                if( Instance.SelectedControl == null ) return;
                Instance.CursorState = ECursorState.CONNECTING;
                line = Instance.SpawnLine((SpellSlot)Instance.SelectedControl);
                updateConnectionGraphs = () => 
                { 
                    if(line == null) return;
                    line.MoveTip(GetGlobalMousePosition());
                };
                aditionalTasks+=updateConnectionGraphs;
            }
            private void EndConnection()
            {
                if(line == null) return;

                line.MoveTip(ChaseMouseSelection<SpellSlot>());

                if(!Instance.TrySpellNodeConnection(line.Bot, line.Tip)) Instance.DispawnLine(line);

                Instance.CursorState = ECursorState.CLEAR;
                aditionalTasks -= updateConnectionGraphs;
                updateConnectionGraphs = null;

            }
            private void StartDragging()
            {
                if( Instance.SelectedControl == null ) return;
                Instance.CursorState = ECursorState.HOLDING;
                Control sel = Instance.SelectedControl;
                updateConnectionGraphs = () => 
                { 
                    if(sel == null) return;
                    Vector2 mousePos = GetGlobalMousePosition();
                    sel.Position = mousePos + sel.GetGlobalRect().Size/2;
                    if(sel is SpellSlot selectedSlot)
                    {
                        foreach(SpellLine line in selectedSlot.spellLines)
                        {
                            if(line.Tip == selectedSlot){ line.MoveTip(mousePos); }
                            else { line.MoveBot(mousePos); }
                        }
                    }
                };
                aditionalTasks+=updateConnectionGraphs;
            }
            private void EndDragging()
            {
                Instance.CursorState = ECursorState.CLEAR;
                aditionalTasks -= updateConnectionGraphs;
                updateConnectionGraphs = null; 
            }

            private T ChaseMouseSelection<T>() where T : Control => 
            CheckPointSelection<T>(GetGlobalMousePosition(), Instance.selectables?.Cast<T>().ToList());
            private T CheckPointSelection<T>(Vector2 point, List<T> selectables) where T : Control
            {
                if(selectables == null) return null;
                List<T> intersections = new List<T>();
                foreach(Control selectable in selectables)
                {
                    if(((T)selectable).GetGlobalRect().HasPoint(point)){
                        intersections.Add((T)selectable);
                    }
                }
                if(intersections.Count == 0) { return null; }
                T best = intersections[0];
                for(int i = 1; i < intersections.Count; i++)
                {
                    if(             best.GetGlobalRect().GetCenter().DistanceSquaredTo(point) >
                        intersections[i].GetGlobalRect().GetCenter().DistanceSquaredTo(point))
                    { best = intersections[i]; }
                }
                return best;
            }      
        }
        protected class ActionInputHandler : InputHandler
        {
            public override void Process(InputEvent @event)
            {
                GD.Print(nameof(NotImplementedException));
            }
            public  void _Pressed(InputEvent @event)
            {
               GD.Print(nameof(NotImplementedException));
            }
            public  void _Dragging(InputEvent @event)
            {
                GD.Print(nameof(NotImplementedException));
            }
            public override void Select(InputEvent @event)
            {
                Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
                if(direction == Vector2.Zero) return;
                SpellEditor.Instance.SelectedControl =
                ChaseSelection<Control>(SpellEditor.Instance.SelectedControl.GlobalPosition, direction);
            }

            private T ChaseSelection<T>(Vector2 position, Vector2 direction) where T : Control => 
            CheckClosestControlAtDirection<T>(position, direction, SpellEditor.Instance.selectables.Cast<T>().ToList());
            private T CheckClosestControlAtDirection<T>(Vector2 position, Vector2 direction, List<T> selectables) where T : Control 
            {
                T best = (T)SpellEditor.Instance.selectedControl;
                List<T> matches = new List<T>();
                Comparison<Vector2> comp = (Vector2 v1, Vector2 v2) => {
                    Vector2 v = v2 - v1;
                    int result = 0;
                    if(Mathf.Sign(direction.X) != 0)
                    {
                        result += Mathf.Sign(direction.X) == Mathf.Sign(v.X) ? 1: -1;
                    }

                    if(Mathf.Sign(direction.Y) != 0)
                    {
                        result += Mathf.Sign(direction.Y) == Mathf.Sign(v.Y) ? 1: -1;
                    }
                    return result;
                };
                foreach(Control selectable in selectables)
                {
                    if(comp.Invoke(position, selectable.Position) > 1){
                        matches.Add((T)selectable);
                    }
                }
                if(matches.Count == 0) { return best; }
                if(best == null) { best = matches[0]; }
                for(int i = 1; i < matches.Count; i++)
                {
                    if(       best.GetGlobalRect().GetCenter().DistanceSquaredTo(position) >
                        matches[i].GetGlobalRect().GetCenter().DistanceSquaredTo(position))
                    { best = matches[i]; }
                }
                return best;
            }
        }

        [Export] private RuneSelector runeSelector;
    //[Export] private RuneSelector sigilSelector;
    [Export] private Container metaMenu;
    [Export] private Container arrowMaster;
    [Export] public Container plotableMaster;
    private List<Container> selectables;

    public static SpellEditor Instance;
    private SpellManager manager;
    private InputHandler handler;
    //private MouseInputHandler mouseInputHandler = new MouseInputHandler();
    //private ActionInputHandler actionInputHandler = new ActionInputHandler();


    private EEditorState state;
    private EEditorState State
    {
        get => state;
        set
        {           
            state = value;
        }
    }
    


    
    //private ECursorState cursorState = ECursorState.CLEAR;
    /*private ECursorState CursorState {
        get => cursorState;
        set{ switch(value){
            case ECursorState.CLEAR:
                if(cursorState != ECursorState.CLEAR) { UnselectControl?.Invoke(); }
                break;

            case ECursorState.SELECTED:
                if(cursorState == ECursorState.HOLDING || cursorState == ECursorState.SELECTED) { UnselectControl?.Invoke(); }
                if(cursorState == ECursorState.CONNECTING) { EndSlotConnectionAt?.Invoke((SpellSlot)selectedControl);  }
                SelectControl?.Invoke((SpellSlot)selectedControl);
                break;

            case ECursorState.HOLDING:
                if(cursorState == ECursorState.CLEAR) { SelectControl?.Invoke((SpellSlot)selectedControl); }
                if(cursorState == ECursorState.CONNECTING) { UnselectControl?.Invoke(); }
                GrabSpellSlot?.Invoke((SpellSlot)selectedControl);
                break;

            case ECursorState.CONNECTING:
                if(cursorState == ECursorState.CLEAR || cursorState == ECursorState.SELECTED) { GrabSpellSlot?.Invoke((SpellSlot)selectedControl); } 
                if(cursorState != ECursorState.CONNECTING ) { StartSlotConnectionAt?.Invoke((SpellSlot)selectedControl); }
                else { EndSlotConnectionAt?.Invoke((SpellSlot)selectedControl); }
                break;
        }
        cursorState = value;
        }
    }
    



    
    private Control selectedControl;
    public Control SelectedControl {
        get => selectedControl; 
        set 
        {
            if(value == null) { CursorState = ECursorState.CLEAR; }
            if (selectedControl == value) 
            {   
                if(CursorState == ECursorState.SELECTED) CursorState = ECursorState.HOLDING;
                return;
            }
            if(CursorState == ECursorState.CONNECTING) 
            {
                CursorState = ECursorState.SELECTED;
                selectedControl = value;
                return;
            }
            if(value == null) { CursorState = ECursorState.CLEAR; }
            if(CursorState == ECursorState.HOLDING) { CursorState = ECursorState.SELECTED; }
            selectedControl = value;
        }
    }
    




    public override void _Ready()
    {
        if(SpellEditor.Instance == null) { SpellEditor.Instance = this; }
        else if (SpellEditor.Instance != this) {QueueFree();}
        State = EEditorState.ON_DESKTOP;
        manager = new SpellManager();
    }
    
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        handler = @event is InputEventMouse ? mouseInputHandler : actionInputHandler;
        ToggleCursor(@event is InputEventMouse);
        handler.Process(@event);
    }

    private void ToggleCursor(bool state)
    {
        //
    }

    public SpellLine SpawnLine()
    {
        SpellLine line = new SpellLine();
        arrowMaster.AddChild(line);
        return line;
    }

    public SpellLine SpawnLine(SpellSlot from)
    {
        SpellLine line = SpawnLine();
        line.MoveBot(from);
        return line;
    }

    public void DispawnLine(SpellLine line)
    {
        line.DisconnectAll();
        arrowMaster.RemoveChild(line);
        line.QueueFree();
    }

    public bool TrySpellNodeConnection(SpellSlot r1, SpellSlot r2)
    {
        if(r1 == null || r2 == null)  { return false; }
        if(r1.node == null || r2.node == null)  { return false; }
        return manager.AddToSpellGraph(r1.node, r2.node);
    }
    /*

    private SpellSlot EmptyCursorPick()
    {
        SpellSlot slot = CheckCursorIntersection();
        if(slot == null) { return null; }
        if(slot.GetParent() == runeSelector)
        {
            runeSelector.Selected = runeSelector.slots.IndexOf(slot);
            PickPlotable(slot);
        }
        cursor.Slot = slot;
        return slot;
    }
    private SpellLine EmptyCursorTrail()
    {
        SpellSlot slot = CheckCursorIntersection();
        if(slot == null) { return null; }
        SpellLine line = new SpellLine();

        arrowMaster.AddChild(line);
        
        line.ConnectBot(slot);
        line.ConnectCursor(cursor);
        
        slot.getMoved?.Invoke();
        return line;
    }

    private void EmptyCursorUnTrail()
    {
        SpellSlot slot = CheckCursorIntersection();
        if(slot == null) 
        { 
            tracingLine.Disconnect(); 
            arrowMaster.RemoveChild(tracingLine);
        }
        
        if(manager.AddToSpellGraph(tracingLine.bot.node, slot.node))
        {
            tracingLine.DisconnectCursor();
            tracingLine.ConnectTip(slot);
            return;
        }

        tracingLine.Disconnect(); 
        arrowMaster.RemoveChild(tracingLine);
    }

    private void CarriedCursorDrop()
    {
        if(((Control)runeSelector.GetParent()).GetGlobalRect().HasPoint(cursor.GetGlobalRect().GetCenter())){ DropPlotable(cursor.Slot); return; }
        SpellSlot slot = cursor.Slot;
        PlacePlotable(cursor.Slot);
        SpellSlot bumper = CheckRectIntersection(slot.GetGlobalRect());
        slot.getMoved?.Invoke();        

    }

    public void AddSlotToDesktop(SpellSlot slot)
    {
        SpellManager.AddNode(slot.node);
        plotableMaster.AddChild(slot);
    }

    public void RemoveSlotToDesktop(SpellSlot slot)
    {
        SpellManager.RemoveNode(slot.node);
        plotableMaster.RemoveChild(slot);
    }

}
}
*/