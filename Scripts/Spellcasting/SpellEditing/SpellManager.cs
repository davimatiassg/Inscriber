using Godot;
using System;
using System.Linq;

namespace SpellEditing
{
using SpellNode = Spell.Node;
public partial class SpellManager : Control
{
    [Export] private RuneSelector selector;
    [Export] private SpellCursor cursor;
    private Spell currentSpell;
    public Spell CurrentSpell { 
        get { return currentSpell; } 
        set { currentSpell = value; } 
    }
    public SpellNode AddCastable(ICastable castable)
    {
        SpellNode node = new SpellNode();
        node.castable = castable;
        currentSpell.nodes.Add(node);
        return node;
    }

    public void RemoveNode(SpellNode node)
    {
        currentSpell.nodes.Remove(node);
        foreach(SpellNode n in node.nexts){ n.prevs.Remove(node); }
        foreach(SpellNode n in node.prevs){ n.nexts.Remove(node); }
    }

    public enum EEntangleStatus { FIRST_EQUALS_LAST, SIZE_TWO_CYCLE, ENTANGLED };

    public EEntangleStatus Entagle(SpellNode first, SpellNode last)
    {
        //Blocking conditions
        if(first == last) { return EEntangleStatus.FIRST_EQUALS_LAST; }
        if(first.nexts.Contains(last)) { return EEntangleStatus.SIZE_TWO_CYCLE; }

        first.nexts.Add(last);
        last.prevs.Add(first);

        return EEntangleStatus.ENTANGLED;
    }


    public void OnPickPlotable(RuneSlot slot)
    {
        selector.RemovePlotable(slot);
        AddChild(slot);
        cursor.Slot = slot;
    }
    public void OnDropPlotable(RuneSlot slot)
    {
        RemoveChild(slot);
        selector.AddPlotable(slot);
        cursor.Slot = null;
    }

    public void OnPlacePlotable(RuneSlot slot)
    {
        if(slot.Plotable is not ICastable){ OnDropPlotable(slot); return; }
        AddCastable((ICastable)slot.Plotable);
        cursor.Slot = null;
        
    }
}
}