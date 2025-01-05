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
/// Control that handles Spell Editor Inputs & UI Behavior.
/// </summary>
public partial class SpellGraphEditor : SpellGraphView, IGraph<VisualNode>
{

   
    private enum EEditorState { SPELL_SELECTOR, VIEW_MODE, FREE_MODE, DRAG_MODE, CONNECT_MODE, RUNE_SELECTOR, META_MENU }
    public static Action<VisualNode>    OnStartConnectionAtNode;
    public static Action<VisualNode>    OnEndConnectionAtNode;
    public static Action<VisualNode>    OnGrabNode;
    public static Action<VisualNode>    OnAddNode;
    public static Action<VisualNode>    OnSelectNode;
    public static Action                OnUnselectNode;
    

    private EEditorState state = EEditorState.FREE_MODE;

    EEditorState State
    {
        get; set;
    }
    [ExportCategory("Editor Objects")]
    [Export] public RuneSelector runeSelector;

    [ExportCategory("Editor Mode's Overlays")]
    
    [Export] public Control viewOverlay;
    [Export] public Control freeOverlay;
    [Export] public Control dragOverlay;
    [Export] public Control connectOverlay;
    [Export] public Control selectOverlay;

    //MODES REFERENCES
    public ViewMode viewMode;
    public FreeMode freeMode;
    public DragMode dragMode;
    public ConnectMode connectMode;
    public NodeFocusMode selectionMode;
    public RuneSelectorMode runeSelectorMode;
    

    public SpellEditorState editorMode;
    public SpellEditorState EditorState 
    {
        get { return editorMode; }
        set { editorMode = value; }
    }


    public VisualNode selectedNode;

    public SpellResource currentSpell;
    

    public override void _Ready()
    {
        base._Ready();

        viewMode = new ViewMode                 { overlay = viewOverlay, editor = this };
        freeMode = new FreeMode                 { overlay = freeOverlay, editor = this };
        dragMode = new DragMode                 { overlay = dragOverlay, editor = this };
        connectMode = new ConnectMode           { overlay = connectOverlay, editor = this };
        selectionMode = new NodeFocusMode       { overlay = selectOverlay, editor = this };
        runeSelectorMode = new RuneSelectorMode { overlay = runeSelector, selector = runeSelector, editor = this };

        EditorState = freeMode;
        

        currentSpell = SceneManager.ConsumeData<SpellResource>("SELECTED_SPELL");
        if(currentSpell != null) SpellRepository.LoadGraphFromResource<SpellGraphView, VisualNode>(currentSpell, this);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        EditorState._Input(@event);
    }

    public override void _Process(double delta)
    {
        base._Process(@delta);
        EditorState._Process(@delta);
    }

}
}