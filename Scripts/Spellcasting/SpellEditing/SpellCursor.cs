using Godot;
using System;

namespace SpellEditing
{
public partial class SpellCursor : TextureRect
{
    
    public Action getMoved;
    public Action cursorMoveAction;
    private SpellSlot slot;

   
    public SpellSlot Slot
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
        if(@event is InputEventMouse)  { cursorMoveAction = () => { Position = GetGlobalMousePosition() - Size/2; }; }
        else {
            cursorMoveAction = () => {
                GD.Print("k");
                Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down").Normalized();
                if(GetViewport().GetVisibleRect().HasPoint(GlobalPosition + direction*25))
                {
                    Position += direction*25;
                }
                
            };
        }

        cursorMoveAction?.Invoke();
        if(slot != null) { slot.getMoved?.Invoke(); }
        getMoved?.Invoke();
		
	}
    



}

}