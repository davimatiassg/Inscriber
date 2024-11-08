using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace SpellEditing
{
public partial class RuneTextureRect : TextureRect
{
    public RuneSelector controller;

    public Vector2 finalPosition;
    public Vector2 finalScale;
    public Color finalColor;

    public void SlerpTowardsFinals(float weight)
    {
        Position = Position.Lerp(finalPosition, weight);
        Scale = Scale.Slerp(finalScale, weight);
        Modulate = finalColor + (Modulate - finalColor ) * weight;
    }
}
public partial class RuneSelector : Control
{
    
    public const float SLOT_SIZE = 108;
    const double SLIDE_TIME = 0.35;
    public static RuneSelector Instance;


    public List<List<IGraphDeployable>> plotables = new List<List<IGraphDeployable>>();
    private int selIndex;
    private List<int> rarityIndex = new List<int>();
    private List<RuneTextureRect> visibleSlots = new List<RuneTextureRect>();

    [Export] private Container box;
    [Export] private RichTextLabel nameLabel;

    

    public int Selected 
    {
        get => selIndex;
        set
        {
            
            if(value >= visibleSlots.Count) { 
                do value -= visibleSlots.Count; while (value >= visibleSlots.Count); 
            }
            else if(value < 0) {  do value += visibleSlots.Count; while (value < 0); }
            selIndex = value;
            StartTransition();
            UpdateTextBox();
        }
    }

    public int SelectedRarity 
    {
        get { 
            rarityIndex[Selected] %= plotables[Selected].Count;
            return rarityIndex[Selected];
        }
        set
        {
            if(value >= plotables[Selected].Count) { do value -= plotables[Selected].Count; while (value >= plotables[Selected].Count); }
            else if(value < 0) {  do value += plotables[Selected].Count; while (value < 0); }
            rarityIndex[Selected] = value;
            StartTransition();
            UpdateTextBox();
        }
    }
    


    public override void _Ready()
    {
        base._Ready();
        // STUB
        for(int i = 0; i < 9; i++) { AddPlotable(new RuneCreate{ rarity = (Rune.ERuneRarity)(i%9)} ); }
        for(int i = 0; i < 12; i++){ AddPlotable(new RandomTestRune()); }
        // END STUB

        if(plotables.Count == 0) {
            nameLabel.Text = "[center][color=white][tornado radius=1.0 freq=20.0 connected=0]No Runes Here ;-;[/tornado][/color][center]";
            return;
        }

        Selected = 0;
    }

    public override void _Process(double delta) 
    {
        base._Process(delta);
        selectTransition?.Invoke(delta);
    }


    private Action<double> selectTransition;
    private void StartTransition()
    {
        float xHalf = GetViewportRect().Size.X/2;
        float xSep = 15;
        float yDelta = box.Size.Y;
        float yPos = GetViewportRect().Size.Y/2 - SLOT_SIZE/3;
        for(int i = 0; i < visibleSlots.Count; i++) xSep += 15f;
        for(int i = 0; i < visibleSlots.Count; i++)
        {
            float sortingFactor = CalculateSorting(i);
            visibleSlots[i].ZIndex = box.ZIndex + visibleSlots.Count - (int)(sortingFactor*visibleSlots.Count);
            visibleSlots[i].finalColor = plotables[i][rarityIndex[i]].Color*(1-Mathf.Abs(sortingFactor));
            visibleSlots[i].finalScale = Vector2.One*ScalingFactor(sortingFactor);
            visibleSlots[i].finalPosition = new Vector2(
                XPositionFactor(sortingFactor)*xSep + xHalf - visibleSlots[i].finalScale.X*SLOT_SIZE + SLOT_SIZE/2, 
                (-YPositionFactor(sortingFactor)*yDelta*1.8f) + yPos*1.8f
            ) ;
            
        }
        selectTransition = Transitionate;
    }
    private void Transitionate(double delta) => Transitionate(0, delta);
    private void Transitionate(double time, double delta)
    {
        if(time >= SLIDE_TIME) { selectTransition = null; return; }
        var nextTime = delta + time;

        foreach(RuneTextureRect slot in visibleSlots)
        {
            slot.SlerpTowardsFinals((float)((float)nextTime/SLIDE_TIME));
        }
         
        selectTransition = (double delta) => Transitionate(nextTime, delta);
    }

    private float CalculateSorting(int idx)
    {
        float factor = ((float)(idx - Selected))/(float)visibleSlots.Count;
        if(factor > 0.5) { factor -= 1f; }
        else if( factor < -0.5) {factor += 1f; }
        return factor;
    }

    const float SCALE_FACTOR = 1.5f; 
    private float ScalingFactor(float weight)
    {
        weight = Mathf.Clamp(Mathf.Abs(weight), 0, 1f);
        float s = Mathf.Sin(SCALE_FACTOR*weight) + SCALE_FACTOR;
        return s/(2*Mathf.Abs(SCALE_FACTOR*weight)+1) - 0.5f;
    }

    const float Y_FACTOR = Mathf.Pi;
    private float YPositionFactor(float weight) 
    {
        weight = Mathf.Clamp(Mathf.Abs(weight), 0, 1f);
        float k = Mathf.Pow(Mathf.Cos(Y_FACTOR*weight), 3) + 1 - Mathf.Sin(Mathf.Pow(weight/Y_FACTOR, 3*Y_FACTOR));
        return k/2;
    }
    private float XPositionFactor(float weight) => Mathf.Sin(Mathf.Pi*weight*2);
    
    public void UpdateTextBox()
    {
        nameLabel.Clear();
        
        nameLabel.ParseBbcode("[center][tornado radius=1.0 freq=40.0 connected=1]");
        nameLabel.PushColor(plotables[Selected][SelectedRarity].Color);
        nameLabel.AddText(plotables[Selected][SelectedRarity].Name);
        nameLabel.PopAll();
    }


    public IGraphDeployable ConfirmSelection() => plotables[Selected][SelectedRarity];
    public Rect2 GetSelectedRect() => visibleSlots[Selected].GetGlobalRect();
    public void SelectLeft(InputEvent @event) { if(inputDelay.IsCompleted && @event.IsPressed()) { Selected--; checkDelay(@event.IsEcho()); }}
    public void SelectRight(InputEvent @event) { if(inputDelay.IsCompleted && @event.IsPressed()) {  Selected++; checkDelay(@event.IsEcho()); }}
    public void SelectRarityUp() => SelectedRarity++;
    public void SelectBackwards(InputEvent @event) { if(inputDelay.IsCompleted) { Selected -= plotables.Count/2; checkDelay(@event.IsEcho()); }}

    Task inputDelay = Task.CompletedTask;
    const float TRANS_ACEL = 1f;
    const float TRANS_TOP_SPEED = 1;
    float transSpeed = 100;
    
    public void checkDelay(bool isEcho)
    { if(isEcho){ 
        inputDelay = Task.Delay((int)(transSpeed*SLIDE_TIME));
        transSpeed = transSpeed > TRANS_TOP_SPEED? transSpeed - TRANS_ACEL : TRANS_TOP_SPEED;
        inputDelay = Task.CompletedTask;
    } else { transSpeed = 200; }}

    public bool AddPlotable(IGraphDeployable plotable)
    {
        foreach(List<IGraphDeployable> plist in plotables)
        {
            foreach(IGraphDeployable p in plist) if(p.Name == plotable.Name) return false;
            else if(plotable.Category == plist[0].Category)
            {
                plist.Add(plotable);
                return true;
            }
        }
        plotables.Add(new List<IGraphDeployable>{plotable} );
        rarityIndex.Add(0);
        AddVisibleSlot(plotable);
        return true;

    }

    private void AddVisibleSlot(IGraphDeployable plotable) 
    {
        RuneTextureRect newSlot = new RuneTextureRect
        {
            CustomMinimumSize = Vector2.One * SLOT_SIZE,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            Texture = plotable.Portrait,
            Modulate = plotable.Color,
            PivotOffset = Vector2.Left*SLOT_SIZE/2
        };
        visibleSlots.Add(newSlot);
        box.AddChild(newSlot);
    }

    



}
}
