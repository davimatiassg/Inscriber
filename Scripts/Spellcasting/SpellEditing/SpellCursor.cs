using Godot;
using System;

namespace SpellEditing
{
public partial class SpellCursor : TextureRect
{
    public Action getMoved;
    private RuneSlot slot;

    public RuneSlot Slot
    {
        get{ return slot; }
        set 
        {
            if(slot != null) { 
                slot.transition = null; 
                if(value != null) { 
                    //
                }
            }
            if(value != null)
            {
                value.transition = (double delta) => { value.GlobalPosition = this.GlobalPosition + this.Size/2 - value.Size/2; }; 
            } 
            slot = value;       
        }
    }
	public override void _Input(InputEvent @event)
	{
        if(@event is InputEventMouseMotion)
        {
            Position = GetGlobalMousePosition() - Size/2;
            if(slot != null) { slot.getMoved?.Invoke(); }
            getMoved?.Invoke();
        }
		
	}
    



}

}