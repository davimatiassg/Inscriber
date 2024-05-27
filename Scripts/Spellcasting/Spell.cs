using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Godot;

[GlobalClass]
public partial class Spell : Resource, ICastable
{
    public bool Valid = true;
    public class Node
    {
        public ICastable castable;
        public bool marked;
        public int weight;
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
    }
    private CastingResources castReqs;
    private CastingResources castRets;
    public List<Node> nodes = new List<Node>();
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
        get{
            int cooldown = 0;
            
            SizeOfLongestPath(ref cooldown, 
            (Node currNode) => { return (int) currNode.castable.Cooldown; } );
            return (uint) cooldown;
        } 
    }
    public int Mana { 
        get{
            int mana = 0;
            foreach(Node node in nodes){ mana += node.castable.Mana; }
            return mana;
        }  
    }
    public uint CastingTime { 
        get{
            int castingtime = 0;
            
            SizeOfLongestPath(ref castingtime, 
            (Node currNode) => { return (int) currNode.castable.CastingTime; } );
            return (uint) castingtime;
        }  
    }
    public Spell(){}
    public Spell(List<Node> n)
    {

    }
    public async Task<CastingResources> Cast(CastingResources data)
    {
        //Early Returns
        if(!(data >= castReqs) || nodes.Count == 0) return data;
        TaskFactory<CastingResources> scheduller = new TaskFactory<CastingResources>();
        foreach(Node node in nodes)
        {
            node.castStatus = node.Cast();
        }
        
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(nodes[0]);
        Node currNode = nodes[0];
        while (queue.Count > 0)
        {
            currNode = queue.Dequeue();   
            foreach(Node nextNode in currNode.nexts)
            {
                if(!nextNode.marked)
                {
                    queue.Enqueue(nextNode);
                    nextNode.marked = true;
                    nextNode.castStatus.Start();
                }
            }   
        }

        foreach(Node node in nodes)
        {
            node.marked = false;
            await node.castStatus;
        }
        
        return CastingResources.Merge(nodes.Select<Node, CastingResources>(i => i.castStatus?.Result).ToArray());
    }
    private TResult BFSNodes<TResult>(ref TResult results, Func<Node, TResult> Process)
    {
        if(nodes.Count == 0) return results;

        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(nodes[0]);
        nodes[0].marked = true;

        while (queue.Count > 0)
        {   
            Node currNode = queue.Dequeue();
            foreach(Node nextNode in currNode.nexts)
            {
                if(!nextNode.marked)
                {
                    queue.Enqueue(nextNode);
                    nextNode.marked = true;
                }
            }
            Process(currNode);
        }
        foreach(Node node in nodes)
        {
            node.marked = false;
        }
        return results;
    }
    private List<Node> TopSort()
    {
        void TopologicalSortUtil(Node node, Stack<Node> stack)
        {
            node.marked = true;
            foreach(Node n in node.nexts)
            {
                if (!n.marked) TopologicalSortUtil(n, stack);
            }
            stack.Push(node);
        }

        List<Node> list = new List<Node>();
        // Stack to store the result
        Stack<Node> stack = new Stack<Node>();
        foreach (Node node in nodes) {
            if (!node.marked) TopologicalSortUtil(node, stack);
        }
        foreach(Node sp in stack)
        {
            sp.marked = false;
        }

        while(stack.Count > 0){ 
            
            list.Add(stack.Pop()); 
        }
        return list;
    }
    private List<Node> SizeOfLongestPath(ref int result, Func<Node, int> Process) 
    {
        List<Node> list = (List<Node>)TopSort();
        
        foreach(Node currNode in TopSort()){
            currNode.weight = Process(currNode);
            foreach (Node nextNode in currNode.nexts) {
            if (nextNode.weight < currNode.weight + Process(nextNode)) 
                nextNode.weight = currNode.weight + Process(nextNode); 
            }
        }
        foreach(Node node in nodes)
        {
            if(node.weight > result) { result = node.weight; }
            node.weight = 0;
        }
        
        return list;
    }
}