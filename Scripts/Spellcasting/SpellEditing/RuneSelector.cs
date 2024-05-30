using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace SpellEditing
{
public partial class RuneSelector : VBoxContainer
{
    [Export] int slotSize = 135;
    [Export] int selectIdentation = 2;
    [Export] int transitionSpeed = 20;
    public List<IPlotable> plotables = new List<IPlotable>();
    public List<RuneSlot> slots = new List<RuneSlot>();
    private Action<double> transition;
    private float initialY;

    private bool initialized = false;
    private int selected;

    public RuneSlot SelectedSlot {
        get {
            var value = selected;
            while(value < 0) { value += slots.Count; }
            while(value >= slots.Count) { value -= slots.Count; }
            return slots[value];
        }
    }
    public int Selected
    {
        get { return selected; }
        set {
            if(!initialized) { return; }
            if(slots.Count == 0) { return; }
            while(value < 0) { value += slots.Count; }
            while(value >= slots.Count) { value -= slots.Count; }
            if(value == selected) { return; }
            selected = value;
            initialY = (GetViewport().GetVisibleRect().Position.Y + Mathf.Abs(GetViewport().GetVisibleRect().Size.Y) - slotSize)/2.5f;
            float differenceY = initialY - slots[value%slots.Count].GlobalPosition.Y;  
            foreach(RuneSlot slot in slots)
            {
                float finalY = slot.GlobalPosition.Y + differenceY;
                float deviationY = (initialY-finalY)/150;
                float selectGradValue = deviationY==0?1:Mathf.Abs(Mathf.Atan(deviationY)/deviationY);
                float finalX = GlobalPosition.X-(selectIdentation*selectGradValue);
                slot.finalPosition = new Vector2(finalX, finalY);
                slot.gradValue = selectGradValue;
                    
            }
            transition = (double delta) =>
            {
                float interpolation = Mathf.Min((float)delta*transitionSpeed/Mathf.Abs(SelectedSlot.finalPosition.Y-SelectedSlot.GlobalPosition.Y), 1);
                foreach(RuneSlot slot in slots)
                {
                    if(slot.processTransition(interpolation))
                    {foreach(RuneSlot s in slots) {s.processTransition(1); } transition = null;  break;}
                }
                
            };
        }
    }

    public async override void _Ready()
    {
        // STUB
        plotables.Add( new RuneCreate() );
        plotables.Add( new RuneCreate() );
        plotables.Add( new RuneCreate() );
        plotables.Add( new RuneCreate() ); 
        plotables.Add( new RuneCreate() );
        plotables.Add( new RuneCreate() );
        plotables.Add( new RuneCreate() );
        plotables.Add( new RuneCreate() );
        plotables.Add( new RuneCreate() );

        
        ((Rune)plotables[0]).rarity = Rune.ERuneRarity.Dull;
        ((Rune)plotables[1]).rarity = Rune.ERuneRarity.Common;
        ((Rune)plotables[2]).rarity = Rune.ERuneRarity.Uncommon;
        ((Rune)plotables[3]).rarity = Rune.ERuneRarity.Rare;
        ((Rune)plotables[4]).rarity = Rune.ERuneRarity.Mithic;
        ((Rune)plotables[5]).rarity = Rune.ERuneRarity.Arcane;
        ((Rune)plotables[6]).rarity = Rune.ERuneRarity.Primal;
        ((Rune)plotables[7]).rarity = Rune.ERuneRarity.Profane;
        ((Rune)plotables[8]).rarity = Rune.ERuneRarity.Divine;
    
        // END STUB
        foreach(IPlotable p in plotables)
        {   
            RuneSlot runePortrait = (RuneSlot)(TextureRect)ResourceLoader.Load<PackedScene>("res://Scenes/Menus/SpellEditor/RuneSlot.tscn").Instantiate();
            runePortrait.Plotable = p;
            this.AddChild(runePortrait);
            slots.Add(runePortrait);
            runePortrait.Connect("gui_input", Callable.From(
                (InputEvent @event) => {
                    if(@event is InputEventMouseButton mouseEvent && mouseEvent.IsReleased()) { Selected = slots.IndexOf(runePortrait); }
                })
            );
        }
        
        initialized = true;
        await Task.Delay(20);
        if(slots.Count > 0) { Selected = slots.Count/2; }
    }
    
    

    public async override void _Process(double delta)
    {
        transition?.Invoke(delta);

        
    }
    public void AddPlotable(RuneSlot slot)
    {
        AddChild(slot);
        if(slots.Count == 0) { 
            slots.Add(slot); 
            Selected = 0; 
            return;
        }
        else if(slots.Count == 1)
        {   
            slots.Add(slot);
            slot.GlobalPosition = slots[0].GlobalPosition + Vector2.Up*slots[0].Size.Y;
            Selected = 1;
            return;
        }
        else
        {
            float distance = Mathf.Abs(slots[0].GlobalPosition.Y - slots[1].GlobalPosition.Y);
            slot.GlobalPosition = slots[Selected].GlobalPosition + Vector2.Down*distance;
            for(int i = selected+1; i < slots.Count-1; i++)
            {
                //GD.PrintRich("[b]Slot da pos [color=" + slots[i+1].Modulate.ToHtml() + "]" + slots[i+1].GlobalPosition + "[/color] aplicado ao da pos [color=" + slots[i].Modulate.ToHtml() + "]" + slots[i].GlobalPosition + "[/color][/b]");
                slots[i].GlobalPosition = slots[i+1].GlobalPosition;
            }
            if(selected != slots.Count-1) { slots[slots.Count-1].GlobalPosition += Vector2.Down*distance; }
            slots.Insert(selected+1, slot);
            Selected++;
        }
    }
    
    public void RemovePlotable(RuneSlot slot)
    {
        
        int idx = slots.IndexOf(slot);
        if(idx < 0) { return; }
        for(int i = slots.Count-1; i >= idx+1 ; i--)
        {
            
            slots[i].GlobalPosition = slots[i-1].GlobalPosition;
        }
        slots.RemoveAt(idx);
        RemoveChild(slot);
        Selected--;
        Selected++;
    }

    Task inputDelay = Task.CompletedTask;
    public override void _Input(InputEvent @event)
    {
        if(!inputDelay.IsCompleted) { return; }
        if(@event.IsActionPressed("ui_up", true)) { Selected--; checkDelay(@event); }
        if(@event.IsActionPressed("ui_down", true)) { Selected++; checkDelay(@event); }
    }
    
    public void checkDelay(InputEvent @event)
    { if(@event.IsEcho()){ inputDelay = Task.Delay(100); inputDelay.Start(); } }
    
}

}