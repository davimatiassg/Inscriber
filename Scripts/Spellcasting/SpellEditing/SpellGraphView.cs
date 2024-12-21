using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpellEditing
{
/// <summary>
/// Control-inherited class that displays a Rune Graph of a Spell.
/// </summary>
    
public partial class SpellGraphView : Control, ISpellDigraph<SpellGraphView.VisualNode>, IWeighted<SpellGraphView.VisualNode>
{
#region SPELLGRAPH_INTERFACE

    public VisualNode this[int index] { 
        get => (VisualNode)graphNodeMaster.GetChild(index);
        set => this[index].Castable = value.Castable;
    }
    public List<VisualNode> Nodes {
        get => graphNodeMaster.GetChildren().Cast<VisualNode>().ToList(); 
        set { Clear(); foreach(var node in value) graphNodeMaster.AddChild(node); }
    }
    

    Dictionary<(VisualNode, VisualNode), int> edges;
    public List<(VisualNode, VisualNode)> Edges 
    {
        get => WeightedEdges.Keys.ToList();

        set
        {
            var nodes = Nodes;
            foreach(var node in nodes){ node.DisconnectFromAll(); }
            foreach(var edge in value){ edge.Item1.ConnectTo(edge.Item2); }
        }
    }

    public Dictionary<(VisualNode, VisualNode), int> WeightedEdges 
    {
        get{
            if(edges != null) return edges;
            edges = new Dictionary<(VisualNode, VisualNode), int>();
            foreach(VisualArc arc in graphArcsMaster.GetChildren()) edges.Add((arc.Source, arc.Target), arc.weight);
            return edges;
        }
        set
        {
            var nodes = Nodes;
            foreach(var node in nodes){ node.DisconnectFromAll(); }
            foreach(KeyValuePair<(VisualNode, VisualNode), int> weightedArc in value)
            {
                weightedArc.Key.Item1.ConnectTo(weightedArc.Key.Item2).weight = weightedArc.Value; 
            }
        }
    }

    public int Count =>  graphNodeMaster.GetChildCount();

    public bool IsReadOnly => false;

    public VisualNode CreateNode(ICastable castable)
    {
        VisualNode node = new VisualNode(){ 
            Castable = castable, 
        };
        return node;
    }

    public List<VisualNode> GetNextNodesOf(VisualNode node)
    {
        return node.arcs.Where(arc => arc.Source == node).Select(arc => arc.Target).ToList();
    }

    public void SetNextNodesOf(VisualNode node, List<VisualNode> nodes)
    {
        node.DisconnectFromAll();
        nodes.ForEach(otherNode => Connect(node, otherNode));
    }

    public List<VisualNode> GetPrevNodesOf(VisualNode node)
    {
        return node.arcs.Where(arc => arc.Source == node).Select(arc => arc.Source).ToList();
    }

    public void SetPrevNodesOf(VisualNode node, List<VisualNode> nodes)
    {
        node.DisconnectFromAll();
        nodes.ForEach(otherNode => Connect(otherNode, node));
    }

    public int EdgeAmmount() => graphArcsMaster.GetChildCount();
    public bool AdjacenceBetween(VisualNode n1, VisualNode n2) => n1.arcs.Select(arc => arc.Target).Contains(n2);

    public int InwardsDegree(VisualNode n) => n.arcs.Where(arc => arc.Source==n).Count();
    public int OutwardsDegree(VisualNode n) => n.arcs.Where(arc => arc.Target==n).Count();
    public int Degree(VisualNode n) => InwardsDegree(n) - OutwardsDegree(n);

    public void Add(VisualNode node)
    {
        graphNodeMaster.AddChild(node);
    }

    public virtual void Clear()
    {
        foreach(Node n in graphNodeMaster.GetChildren()){ n.QueueFree(); }
        foreach(Node n in graphArcsMaster.GetChildren()){ n.QueueFree(); }
    }

    public bool Contains(VisualNode item)
    {
        throw new System.NotImplementedException();
    }

    public void CopyTo(VisualNode[] array, int arrayIndex)
    => graphNodeMaster.GetChildren().CopyTo(array, arrayIndex);
    

    public virtual bool Remove(VisualNode node)
    {
        graphNodeMaster.RemoveChild(node);
        node.DisconnectFromAll();
        node.QueueFree();
        return true;
    }

    public IEnumerator<VisualNode> GetEnumerator() 
    => graphNodeMaster.GetChildren().Cast<VisualNode>().GetEnumerator();
    

    IEnumerator IEnumerable.GetEnumerator()
    => graphNodeMaster.GetChildren().Cast<VisualNode>().GetEnumerator();

    public virtual VisualNode Add(ICastable castable, Vector2 position)
    {
        VisualNode node = CreateNode(castable);
        node.Position = position;
        Add(node);
        return node;
    }
    public void Add(ICastable castable) => Add(castable, Vector2.Zero);

    public bool Connect(VisualNode sourceNode, VisualNode targetNode) => Connect(sourceNode, targetNode, 1);

    public bool Connect(VisualNode sourceNode, VisualNode targetNode, int weight)
    {
        var arc = sourceNode.CreateArcTowards(targetNode);
        arc.weight = weight;
        if(GraphUtil.HasCycle(this, sourceNode)) {
            sourceNode.DestroyArc(arc);
            return false;
        }
        sourceNode.AssembleConnetion(arc);        
        return true;
    }

    public bool Disconnect(VisualNode sourceNode, VisualNode targetNode)
    {
        sourceNode.DisconnectFrom(targetNode);
        return true;
    }

    public bool ReplaceNode(VisualNode node, ICastable castable)
    {
        node.Castable = castable;
        return false;
    }

    

#endregion SPELLGRAPH_INTERFACE


    [Export] public Control graphNodeMaster;
    [Export] public Control graphArcsMaster;
    [Export] public SpellGraphCamera spellGraphCamera;
    

    /// SpellGraph's Camera Methods
    public void SetCameraGlobalPosition(Vector2 position) => spellGraphCamera.GlobalPosition = position;

    public void SetCameraZoomPosition(Vector2 zoom) => spellGraphCamera.Zoom = zoom;

    public void SetCameraFocus(List<Vector2> positionList)
    {
        Vector2 pos = new Vector2();
        foreach (Vector2 position in positionList) pos+=position;
        pos /= positionList.Count;
        SetCameraGlobalPosition(pos);
    }

    public VisualNode FindClosestNodeFrom(Vector2 position)
    {
        Control node = null;
        float distSQR = float.MaxValue;
        foreach(Control n in graphNodeMaster.GetChildren())
        {
            float d = n.Position.DistanceSquaredTo(position);
            if(d <distSQR)
            {
                node = n;
                distSQR = d;
            }
        }

        return (VisualNode)node;
    }


    public virtual void LoadSpellGraph<TGraph, TNode>(TGraph graph)
        where TGraph : ISpellGraph<TNode>, new()
        where TNode :  ISpellGraphNode, new()
    {
        Clear();
        if(graph.Count == 0) return;

        foreach(var node in graph.Nodes)
        {
            Add(node.Castable, node.Position);
        }

        foreach((var src, var trg) in graph.Edges)
        {
            Connect((VisualNode)graphNodeMaster.GetChild(src.Index), (VisualNode)graphNodeMaster.GetChild(trg.Index));
        }
    }

        

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
    }

    public partial class VisualArc : Line2D
    {
        private VisualNode[] vertices = { null, null };

        public int weight;
        public VisualNode Source
        {
            get => vertices[0];
            set => vertices[0] = value;        
        }

        public VisualNode Target
        {
            get => vertices[1];
            set => vertices[1] = value;
            
        }
        
        
        public void MoveSource(Vector2 vect) => SetPointPosition(0, vect); 
        public void MoveSource(VisualNode obj) => MoveSource(obj.GetRect().GetCenter());
        public void MoveSource() => MoveSource(Source);
        public void MoveTarget(Vector2 vect) => SetPointPosition(1, vect); 
        public void MoveTarget(VisualNode obj) => MoveTarget(obj.GetRect().GetCenter());
        public void MoveTarget() => MoveTarget(Target);

        public void UpdatePosition() { MoveSource(); MoveTarget(); }

        public VisualArc()
        {
            Points = new Vector2[2];
            this.WidthCurve = new Curve();
            this.WidthCurve.AddPoint(Vector2.Down);
            this.WidthCurve.AddPoint(Vector2.Right);
        }

        public void DisconnectAll()
        {
            Target = null;
            Source = null;
        }
    }

}
}