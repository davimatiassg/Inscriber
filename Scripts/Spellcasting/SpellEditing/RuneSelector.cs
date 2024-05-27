using Godot;
using System;
using System.Collections.Generic;
namespace SpellEditing
{
public partial class RuneSelector : VBoxContainer
{
    [Export] int slotSize = 0;
    [Export] int selectIdentation = 2;
    [Export] int transitionSpeedMilli = 1000;
    public List<IPlotable> plotables = new List<IPlotable>();
    public List<TextureRect> slots = new List<TextureRect>();

    private Action<double> transition;
    private float initialY;
    private TextureRect selected;
    public TextureRect Selected
    {
        get { return selected; }
        set
        {
            if(value == selected) { return; }
            float differenceY = initialY - value.GlobalPosition.Y;
            selected = value;
            foreach(RuneSlot slot in slots)
            {
                float finalY = slot.GlobalPosition.Y + differenceY;
                float deviationY = (initialY-finalY)/50;
                GD.Print("deviationY: " + deviationY);
                deviationY *= deviationY;
                float finalX = GlobalPosition.X-(selectIdentation*(deviationY==0?1:Mathf.Abs(Mathf.Atan(deviationY)/deviationY)));
                
                GD.Print("finalX: " + finalX);
                Vector2 final = new Vector2(finalX, finalY);
                slot.transition = (double delta) =>
                {
                    slot.GlobalPosition = slot.GlobalPosition.MoveToward(final, (float)delta*transitionSpeedMilli);
                    if(slot.GlobalPosition.Y == finalY) { slot.transition = null; }
                };    
            }
        }
    }

    
    public override void _Ready()
    {
        // STUB
        plotables.Add( new RuneCreate() );
        plotables.Add( new RuneCreate() );
        plotables.Add( new RuneCreate() );
        plotables.Add( new RuneCreate() );

        


    
        // END STUB

        initialY = (GetViewport().GetVisibleRect().Position.Y + Mathf.Abs(GetViewport().GetVisibleRect().Size.Y) - 135)/2;

        foreach(IPlotable p in plotables)
        {   
            TextureRect runePortrait = (TextureRect)ResourceLoader.Load<PackedScene>("res://Scenes/Menus/SpellEditor/RuneSlot.tscn").Instantiate();
            runePortrait.Texture = p.Portrait;
            this.AddChild(runePortrait);
            slots.Add(runePortrait);
            runePortrait.Connect("gui_input", Callable.From(
                (InputEvent @event) => {
                    if(@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed) { Selected = runePortrait; }
                })
            );
        }
        
        
        
    }

    public async override void _Process(double delta)
    {
        transition?.Invoke(delta);
        //GlobalPosition = GlobalPosition.MoveToward(GlobalPosition + Vector2.Down*500f, (float)delta*500);
    }
    
}

}