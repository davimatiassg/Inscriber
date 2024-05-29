using Godot;
using System;

namespace SpellEditing
{
public partial class SpellCursor : TextureRect
{
    [Signal]
    public delegate void DropPlotableEventHandler(RuneSlot slot);
    [Signal]
    public delegate void PlacePlotableEventHandler(RuneSlot slot);
    private RuneSlot slot;

    public RuneSlot Slot
    {
        get{ return slot; }
        set 
        {
            if(slot != null) { 
                slot.transition = null; 
                if(value != null) { 
                    EmitSignal(SignalName.DropPlotable, slot);
                    value.transition = (double delta) => { value.GlobalPosition = GlobalPosition; }; 
                }
            }
            slot = value;  
        }
    }
	public override void _Ready()
	{}
	public override void _Process(double delta)
	{
		Position = GetGlobalMousePosition();
	}
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if(@event.IsActionPressed("game_fire_1", false)) { EmitSignal(SignalName.PlacePlotable, slot); }
        if(@event.IsActionPressed("game_fire_2", false)) { EmitSignal(SignalName.DropPlotable, slot); }
    }


}

}