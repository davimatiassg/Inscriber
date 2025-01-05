using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[GlobalClass]
public partial class VisualNode : TextureRect, ISpellGraphNode
{
    public partial class VisualSigilSlot : TextureRect
    {
        public CastParam RepresentedParam  { get; set; }
        public bool useDefaultPortrait;
        private Line2D line;
        private Sigil currentSigil;
        public Sigil CurrentSigil 
        {   get => currentSigil;
            set
            {
                if( value == null ) { 
                    SelfModulate = new Color(SelfModulate.R, SelfModulate.G, SelfModulate.B, 0.2f);
                    Texture = useDefaultPortrait ? 
                    Sigil.DefaultSigilPortrait(RepresentedParam.paramType) :
                    Sigil.LackingSigilPortrait();                        
                }
                else {
                    SelfModulate = new Color(SelfModulate.R, SelfModulate.G, SelfModulate.B, 1);
                    Texture = CurrentSigil.Portrait;
                }

            }
        }
        

        public VisualSigilSlot()
        {
            line = new Line2D();
            line.AddPoint(Vector2.Zero);
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            line.AddPoint(Position);
        }

        public new Vector2 Position
        {
            get => base.Position;
            set {
                line.SetPointPosition(1, Position);                    
                base.Position = value;
            }
        }
    }

    public VisualNode(ICastable castable)
    {
        UpdateCurrentSigils(castable);
    }
    
    public List<VisualArc> arcs = new List<VisualArc>();
    [Export] public Label nameLabel; 

    public int Index { 
        get => GetIndex(); 
        set {}
    }

    private ICastable castable;
    public ICastable Castable
    {
        get { return castable; }
        set {
            castable = value;
            UpdateVisuals();
        }
    }
    public new Vector2 Position
    {
        get => base.Position + GetRect().Size/2;
        set => base.Position = value - GetRect().Size/2;
        
    }

    public VisualNode() {}

    private void InitNameLabel()
    {	
        nameLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            LabelSettings = new LabelSettings{ FontSize = 10 }
        };
        AddChild(nameLabel);
        nameLabel.Position = new Vector2(-135/3, 50);
        nameLabel.Size = new Vector2I(135, 40);
    }

    
    /// <summary>
    /// Updates the position of all connected arcs.
    /// </summary>
    public void UpdateArcPosition() { foreach(VisualArc arc in arcs) arc.UpdatePosition(); }

    /// <summary>
    /// Updates the color and portrait of this node to match it's Castable object.
    /// </summary>
    public void UpdateVisuals()
    {
        Modulate = Rune.ColorByRarity(((Rune)castable).rarity);
        Texture = castable.Portrait;
        if(castable is CharacterTextRune) { 
            if(nameLabel == null) InitNameLabel();
            nameLabel.Text = ((Rune)castable).Name; 
        }
    }


    /// <summary>
    /// Adjusts connected sigils positions
    /// </summary>
    public void UpdateSigilPosition()
    {
        List<VisualSigilSlot> slots = GetChildren().Cast<VisualSigilSlot>().ToList();
        float startAngle = 1/(slots.Count()+1);
        float angle = startAngle;
        Vector2 position = Vector2.Down;

        foreach(var slot in slots)
        {
            slot.Position = position.Rotated(angle*Mathf.Pi);
            angle = angle > 0? -angle : startAngle - angle;
        }
    }
    /// <summary>
    ///  Removes current sigils and initializes new sigils based on the requirements of a given ICastable
    /// </summary>
    /// <param name="c">The castable to initalize sigils from</param>        
    public void UpdateCurrentSigils(ICastable c)
    {
        foreach(var child in GetChildren()) { child.QueueFree(); } 
        
        List<VisualSigilSlot> sigilSlots = 
        c.CastRequirements.Keys.Select(
            param => new VisualSigilSlot {
                RepresentedParam = param,
                CurrentSigil = null
            }
        ).ToList();

        var defaults = c.CastDefaults.Keys.Select(param => param.name);

        foreach(var defaultSlot in sigilSlots.Where(slot => defaults.Contains(slot.RepresentedParam.name)))
        {   
            defaultSlot.useDefaultPortrait = true;
        }

        UpdateSigilPosition();
    }


    /// <summary>
    /// Find an Arc between this node towards another. Direction Sensitive.
    /// </summary>
    /// <param name="node"> The node to check connection </param>
    /// <returns>An arc between this and node, if it exists. Null otherwise. </returns>
    private VisualArc SearchConnectionArcFor(VisualNode node)
    {

        foreach(VisualArc arc in arcs)
        {
            if(arc.Target == node) return arc;
        }
        return null;
    }
    /// <summary>
    /// Creates an Arc between two nodes, if it does not already exists. Does not assemble spell data.
    /// </summary>
    /// <param name="node">The node to create the arc towards.</param>
    /// <returns>The arc between the two nodes.</returns>
    public VisualArc CreateArcTowards(VisualNode node)
    {
        VisualArc arc = SearchConnectionArcFor(node);
        if(arc == null) { 
            arc = new VisualArc();
            arc.Source = this;
            arc.Target = node;
            this.arcs.Add(arc);
            node.arcs.Add(arc);
            arc.UpdatePosition();
        }
        return arc;
    }
    
    /// <summary>
    /// Stablishes the node-relevant spell data changes from a connection. 
    /// </summary>
    /// <param name="arc">The arc representing the connection</param>
    public void AssembleConnetion(VisualArc arc)
    {
        System.Diagnostics.Debug.Assert(arc != null);
        // TODO
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public VisualArc ConnectTo(VisualNode node)
    {
        VisualArc arc = CreateArcTowards(node);
        AssembleConnetion(arc);
        return arc;
    }


    /// <summary>
    /// Destroys an given Arc. Does not assemble spell data.
    /// </summary>
    /// <param name="node">The target arc</param>
    /// <returns></returns>
    public bool DestroyArc(VisualArc arc)
    {
        System.Diagnostics.Debug.Assert(arc != null);   

        arc.Source = null;
        arc.Target = null;
        arc.Source.arcs.Remove(arc);
        arc.Target.arcs.Remove(arc);
        arc.QueueFree();

        return true;

    }

    /// <summary>
    /// Stablishes the node-relevant spell data changes from the removal of a connection. 
    /// </summary>
    /// <param name="arc">The target node of the connection</param>
    public void AssembleDisconnetion(VisualArc arc)
    {
        System.Diagnostics.Debug.Assert(arc != null);
        // TODO
    }
    
    public void DisconnectFrom(VisualNode node)
    {
        VisualArc arc = SearchConnectionArcFor(node);
        if(arc == null) { return; }
        AssembleDisconnetion(arc);
        DestroyArc(arc);       
    }

    /// <summary>
    /// Disconnect this node from all other nodes it is a source or a target of.
    /// </summary>
    public void DisconnectFromAll()
    {
        foreach(VisualArc arc in arcs)
        {
            AssembleDisconnetion(arc);
            DestroyArc(arc);
        }
        arcs.Clear();
    }

    public void AddSigil(Sigil sigil)
    =>
        GetChildren().Cast<VisualSigilSlot>().
        Where((slot) => slot.RepresentedParam == sigil).Single().
        CurrentSigil = sigil;

    public Sigil GetSigil(int index) 
    => GetChild<VisualSigilSlot>(index).CurrentSigil;

    public IEnumerable<Sigil> GetSigils() 
    =>
        GetChildren().Cast<VisualSigilSlot>().
        Where((slot) => slot.CurrentSigil != null).
        Select((slot) => slot.CurrentSigil);

    public int GetSigilCount() => 
        GetChildren().Cast<VisualSigilSlot>().
        Where((slot) => slot.CurrentSigil != null).
        Count();
}