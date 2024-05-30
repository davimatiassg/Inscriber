using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpellEditing
{
    using SpellNode = Spell.Node;
public partial class SpellManager
{

    private static Spell currentSpell;
    public Spell CurrentSpell { 
        get { return currentSpell; } 
        set { currentSpell = value; } 
    }
    public SpellManager(Spell spell)
    {
        currentSpell = spell;
    }

    public SpellManager()
    {
        //STUB
        currentSpell = new Spell();  
        //END STUB    
    }
    public static SpellNode CreateCastable(ICastable castable)
    {
        SpellNode node = new SpellNode();
        node.castable = castable;
        return node;
    }

    public static void AddNode(SpellNode node)
    {
        currentSpell.nodes.Add(node);
    }

    public static void RemoveNode(SpellNode node)
    {
        currentSpell.nodes.Remove(node);
        foreach(SpellNode n in node.nexts){ n.prevs.Remove(node); }
        foreach(SpellNode n in node.prevs){ n.nexts.Remove(node); }
    }

    private enum EEntangleStatus { FIRST_EQUALS_LAST, HAS_CYCLE, ENTANGLED };

    private EEntangleStatus Entagle(SpellNode first, SpellNode last)
    {
        //Blocking conditions
        if(first == last) { return EEntangleStatus.FIRST_EQUALS_LAST; }
        if(first.nexts.Contains(last)) { return EEntangleStatus.HAS_CYCLE; }
        
        //TODO: Tarjan algorithm to detect cycles longer than size 1 and 2

        //Entangle Succeful
        first.nexts.Add(last);
        last.prevs.Add(first);
        return EEntangleStatus.ENTANGLED;
    }

    public async Task<bool> TryEntangleNodes(SpellNode first, SpellNode last)
    {
        if(!(currentSpell.nodes.Contains(first) && currentSpell.nodes.Contains(last))){ return false; }
        var task = new Task<bool>( () => { return Entagle(first, last) == EEntangleStatus.ENTANGLED; } );
        task.Start();
        return await task;
    }

}

}

