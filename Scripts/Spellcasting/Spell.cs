using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Godot;
using System.Runtime.CompilerServices;
using System.Collections;

[GlobalClass]
public partial class Spell : Resource, ICastable, ICollection<Spell.Node>
{
    public bool Valid = true;

    // Class for the nodes of the Spellcasting graph
    public class Node
    {
        public ICastable castable;
        public List<Node> prevs = new List<Node>();
        public List<Node> nexts = new List<Node>();
        public Task<CastingResources> castStatus;
        public CastingResources CastRequirements { get { return castable.CastRequirements; } }
        public CastingResources CastReturns { get { return castable.CastReturns; } }
        public async Task<CastingResources> Cast(CastingResources data)
        {
            return await castable.Cast(data);
        }
        public async Task<CastingResources> Cast()
        {
            foreach(Node dep in prevs)
            {
                await dep.castStatus;
            }
            return await castable.Cast(CastingResources.Merge(prevs.Select<Node, CastingResources>(i => i.castStatus?.Result).ToArray()));
        }
        public bool ConnectNext(Node next)
        { 
            if(nexts.Contains(next)) return false;
            nexts.Add(next);
            next.prevs.Add(this);
            return true;
        }
        public bool ConnectPrev(Node prev)
        { 
            if(prevs.Contains(prev)) return false;
            prevs.Add(prev);
            prev.nexts.Add(this);
            return true;
        }

        public bool DisconnectNext(Node next)
        { 
            if(!nexts.Contains(next)) return false;
            nexts.Remove(next);
            next.prevs.Remove(this);
            return true;
        }
        public bool DisconnectPrev(Node prev)
        { 
            if(!prevs.Contains(prev)) return false;
            prevs.Remove(prev);
            prev.nexts.Remove(this);
            return true;
        }

        public bool Disconnect(Node node)
        { 
            return nexts.Remove(node) || prevs.Remove(node);
        }

        public override string ToString()
        {
            return "Spellnode from " + castable.ToString();
        }
    }
    public List<Node> nodes;
    public List<Node> inactiveNodes;
    private CastingResources castReqs;
    public CastingResources CastRequirements 
    {   get
        {
            castReqs = new CastingResources();
            castReqs = BFSNodes<CastingResources>(ref castReqs, 
            (Node currNode) => { 
                    castRets.Merge(currNode.CastRequirements); 
                    return castReqs;
                });
            return castReqs;
        }
    }
    private CastingResources castRets;
    public CastingResources CastReturns 
    {   
        get{
            castRets = new CastingResources();
            castRets = BFSNodes<CastingResources>(ref castRets, 
            (Node currNode) => { 
                    castRets.Merge(currNode.CastReturns); 
                    return castReqs;
                });
            return castRets;
        }
    }
    public uint Cooldown { 
        get => (uint) WalkFullPath( (Node currNode) => { return (int) currNode.castable.Cooldown; } );

    }
    public int Mana { 
        get{
            int mana = 0;
            foreach(Node node in nodes){ mana += node.castable.Mana; }
            return mana;
        }  
    }
    public uint CastingTime { 
        get => (uint) WalkFullPath( (Node currNode) => { return (int) currNode.castable.CastingTime; } );
    }


    public Spell()
    { 
        nodes = new List<Node>();
        inactiveNodes = new List<Node>();
    }
    public Spell(List<Node> active, List<Node> inactive) { 
        nodes = active; 
        inactiveNodes = inactive;
    }
    public async Task<CastingResources> Cast(CastingResources data)
    {
        if(!(data >= castReqs) || nodes.Count == 0) throw new InvalidOperationException("Insuficient Data to cast this spell");
        foreach(Node node in nodes)
        {
            node.castStatus = node.Cast(); 
            node.castStatus.Start();
        }
        foreach(Node node in nodes)
        {
            await node.castStatus;
        }
        return CastingResources.Merge(nodes.Select<Node, CastingResources>(i => i.castStatus?.Result).ToArray());
    }



#region GRAPH_METHODS
    public static Dictionary<Node, T> InitializePairType<T>(List<Node> nodes, T defValue)
    {
        Dictionary<Node, T> dict = new Dictionary<Node,T>();
        foreach(Node n in nodes)
        {
            dict.Add(n, defValue);
        }
        return dict;
    }
    public static Dictionary<Node, T> InitializePairType<T>(List<Node> nodes) where T : new() => InitializePairType<T>(nodes, new T());
   

    public void UpdateNodeTopSorting() => this.nodes = TopSortNodes(this.nodes);

    public static List<Node> TopSortNodes(List<Node> nodes)
    {
        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(nodes);
        void TopologicalSortUtil(Node node, ref Stack<Node> stack)
        {
            markedNodes[node] = true;
            foreach(Node n in node.nexts)
            {
                if (!markedNodes[n]) TopologicalSortUtil(n, ref stack);
            }
            stack.Push(node);
        }
        
        Stack<Node> stack = new Stack<Node>();
        foreach (Node node in nodes) {
            if (!markedNodes[node] && (node.nexts.Count + node.prevs.Count > 0)) TopologicalSortUtil(node, ref stack);
        }

        List<Node> list = new List<Node>();
        while(stack.Count > 0){ 
            list.Insert(0, stack.Pop());
        }

        return list;
    }
    
    private TResult BFSNodes<TResult>(ref TResult results, Func<Node, TResult> Process)
    {
        if(nodes.Count == 0) return results;
        Dictionary<Node, bool> markedNodes = InitializePairType<bool>(nodes);
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(nodes[0]);
        markedNodes[nodes[0]] = true;

        while (queue.Count > 0)
        {   
            Node currNode = queue.Dequeue();
            foreach(Node nextNode in currNode.nexts)
            {
                if(!markedNodes[nextNode])
                {
                    queue.Enqueue(nextNode);
                    markedNodes[nextNode] = true;
                }
            }
            Process(currNode);
        }

        return results;
    }
    public int WalkFullPath( Func<Node, int> Process) 
    {
        Dictionary<Node, int> weightedDict = InitializePairType<int>(nodes, -1);
        int result = 0;
        foreach(Node node in nodes)
        {
            int p = 0;
            foreach(Node prevNode in node.prevs)
            {
                p = Mathf.Max(p, weightedDict[prevNode]);
                result = Mathf.Max(result, p);
            }
            weightedDict[node] = Process(node) + p;
        }
        return result;
    }
#endregion GRAPH_METHODS

#region INTERFACE_METHODS
    public int Count => nodes.Count;
    public bool IsReadOnly => false;
    public void Add(Node item) 
    {
        void RecursiveAdd(Node item) 
        {
            if(nodes.Contains((item))) return;
            nodes.Add(item);
            foreach(Spell.Node n in item.nexts)
            {
                RecursiveAdd(n);
            }
        }  
        RecursiveAdd(item);
        UpdateNodeTopSorting();
    }
    public void Clear() => nodes.Clear();
    public bool Contains(Node item) => nodes.Contains(item);
    public void CopyTo(Node[] array, int arrayIndex) => nodes.CopyTo(array, arrayIndex);
    public bool Remove(Node item) => nodes.Remove(item);    
    public IEnumerator<Node> GetEnumerator() => nodes.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => nodes.GetEnumerator();

    public Node this[int i]{ get => nodes[i]; set => nodes[i] = value; }
    public void SwapNodeAtIndexBy(int index, Node value){

        if(value == nodes[index]) { return; }
        foreach(Node n in nodes[index].nexts) { value.ConnectNext(n); }
        foreach(Node n in nodes[index].prevs) { value.ConnectNext(n); }
        while(nodes[index].nexts.Count > 0)
        {
            nodes[index].nexts[nodes[index].nexts.Count-1].prevs.Remove(nodes[index]);
            nodes[index].nexts.RemoveAt(nodes[index].nexts.Count-1);
        }
        while(nodes[index].prevs.Count > 0)
        {
            nodes[index].prevs[nodes[index].prevs.Count-1].nexts.Remove(nodes[index]);
            nodes[index].prevs.RemoveAt(nodes[index].prevs.Count-1);
        }
        inactiveNodes.Add(nodes[index]);
        nodes[index] = value;
    }

#endregion INTERFACE_METHODS
}