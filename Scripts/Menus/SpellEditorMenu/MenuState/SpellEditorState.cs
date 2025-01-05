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
    public VisualNode tempSelection;
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

}    
