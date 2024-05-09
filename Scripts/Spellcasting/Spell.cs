using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Godot;

[GlobalClass]
public partial class Spell : Resource, ICastable
{
    public class SpellNode
    {
        public ICastable castable;
        public bool marked;
        public int weight;
        public List<SpellNode> prevs = new List<SpellNode>();
        public List<SpellNode> nexts = new List<SpellNode>();
        public Task<CastingResources> castStatus;
        public CastingResources CastRequirements { get {return castable.CastRequirements; } }
        public CastingResources CastReturns { get { return castable.CastReturns; } }
        public async Task<CastingResources> Cast(CastingResources data)
        {
            return await castable.Cast(data);
        }
        public async Task<CastingResources> Cast()
        {
            foreach(SpellNode dep in prevs)
            {
                await dep.castStatus;
            }
            return await castable.Cast(CastingResources.Merge(prevs.Select<SpellNode, CastingResources>(i => i.castStatus?.Result).ToArray()));
        }
    }
    private CastingResources castReqs;
    private CastingResources castRets;
    public List<SpellNode> nodes = new List<SpellNode>();
    public CastingResources CastRequirements 
    {   get
        {
            castReqs = new CastingResources();
            castReqs = BFSNodes<CastingResources>(ref castReqs, 
            (SpellNode currNode) => { 
                    castRets.Merge(currNode.CastRequirements); 
                    return castReqs;
                });
            return castReqs;
        }
    }
    public CastingResources CastReturns 
    {   
        get{
            castRets = new CastingResources();
            castRets = BFSNodes<CastingResources>(ref castRets, 
            (SpellNode currNode) => { 
                    castRets.Merge(currNode.CastReturns); 
                    return castReqs;
                });
            return castRets;
        }
    }
    public uint Cooldown { 
        get{
            int cooldown = 0;
            
            SizeOfLongestPath(ref cooldown, 
            (SpellNode currNode) => { return (int) currNode.castable.Cooldown; } );
            return (uint) cooldown;
        } 
    }
    public int Mana { 
        get{
            int mana = 0;
            foreach(SpellNode node in nodes){mana+= node.castable.Mana; }
            return mana;
        }  
    }
    public uint CastingTime { 
        get{
            int castingtime = 0;
            
            SizeOfLongestPath(ref castingtime, 
            (SpellNode currNode) => { return (int) currNode.castable.CastingTime; } );
            return (uint) castingtime;
        }  
    }
    public Spell(){}
    public Spell(List<SpellNode> n)
    {

    }
    public async Task<CastingResources> Cast(CastingResources data)
    {
        //Early Returns
        if(!(data >= castReqs) || nodes.Count == 0) return data;
        TaskFactory<CastingResources> scheduller = new TaskFactory<CastingResources>();
        foreach(SpellNode node in nodes)
        {
            node.castStatus = node.Cast();
        }
        
        Queue<SpellNode> queue = new Queue<SpellNode>();
        queue.Enqueue(nodes[0]);
        SpellNode currNode = nodes[0];
        while (queue.Count > 0)
        {
            currNode = queue.Dequeue();   
            foreach(SpellNode nextNode in currNode.nexts)
            {
                if(!nextNode.marked)
                {
                    queue.Enqueue(nextNode);
                    nextNode.marked = true;
                    nextNode.castStatus.Start();
                }
            }   
        }

        foreach(SpellNode node in nodes)
        {
            node.marked = false;
            await node.castStatus;
        }
        
        return CastingResources.Merge(nodes.Select<SpellNode, CastingResources>(i => i.castStatus?.Result).ToArray());
    }
    private TResult BFSNodes<TResult>(ref TResult results, Func<SpellNode, TResult> Process)
    {
        if(nodes.Count == 0) return results;

        Queue<SpellNode> queue = new Queue<SpellNode>();
        queue.Enqueue(nodes[0]);
        nodes[0].marked = true;

        while (queue.Count > 0)
        {   
            SpellNode currNode = queue.Dequeue();
            foreach(SpellNode nextNode in currNode.nexts)
            {
                if(!nextNode.marked)
                {
                    queue.Enqueue(nextNode);
                    nextNode.marked = true;
                }
            }
            Process(currNode);
        }
        foreach(SpellNode node in nodes)
        {
            node.marked = false;
        }
        return results;
    }
    private List<SpellNode> TopSort()
    {
        void TopologicalSortUtil(SpellNode node, Stack<SpellNode> stack)
        {
            node.marked = true;
            foreach(SpellNode n in node.nexts)
            {
                if (!n.marked) TopologicalSortUtil(n, stack);
            }
            stack.Push(node);
        }

        List<SpellNode> list = new List<SpellNode>();
        // Stack to store the result
        Stack<SpellNode> stack = new Stack<SpellNode>();
        foreach (SpellNode node in nodes) {
            if (!node.marked) TopologicalSortUtil(node, stack);
        }
        foreach(SpellNode sp in stack)
        {
            sp.marked = false;
        }

        while(stack.Count > 0){ 
            
            list.Add(stack.Pop()); 
        }
        return list;
    }
    private List<SpellNode> SizeOfLongestPath(ref int result, Func<SpellNode, int> Process) 
    {
        List<SpellNode> list = (List<SpellNode>)TopSort();
        
        foreach(SpellNode currNode in TopSort()){
            currNode.weight = Process(currNode);
            foreach (SpellNode nextNode in currNode.nexts) {
            if (nextNode.weight < currNode.weight + Process(nextNode)) 
                nextNode.weight = currNode.weight + Process(nextNode); 
            }
        }
        foreach(SpellNode node in nodes)
        {
            if(node.weight > result) { result = node.weight; }
            node.weight = 0;
        }
        
        return list;
    }
}