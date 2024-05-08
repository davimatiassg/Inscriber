using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Security;

public class Spell : ICastable
{
    public class SpellNode
    {
    
        ICastable castable;
        public bool marked;
        public List<SpellNode> dependencies = new List<SpellNode>();
        public Task<CastingResources> castStatus;
        public CastingResources CastRequirements { get {return castable.CastRequirements; } }
        public CastingResources CastReturns { get { return castable.CastReturns; } }

        public async Task<CastingResources> Cast(CastingResources data)
        {
            return await castable.Cast(data);
        }
        public async Task<CastingResources> Cast()
        {
            foreach(SpellNode dep in dependencies)
            {
                await dep.castStatus;
            }
            return await castable.Cast(CastingResources.Merge(dependencies.Select<SpellNode, CastingResources>(i => i.castStatus?.Result).ToArray()));
        }
    }
    public List<SpellNode> nodes = new List<SpellNode>();
    public List<int[]> arcs = new List<int[]>();
    private CastingResources castReqs;
    public CastingResources CastRequirements 
    {   get
        {
            CastingResources castReqs = new CastingResources();
            if(nodes.Count == 0) return castReqs;

            bool mark = !nodes[0].marked;
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(0);
            nodes[0].marked = mark;

            while (queue.Count > 0)
            {   
                int currNode = queue.Dequeue();
                foreach(int i in arcs[currNode])
                {
                    if(nodes[i].marked != mark)
                    {
                        queue.Enqueue(i);
                        nodes[i].marked = mark;
                    }
                }
                castReqs.Merge(nodes[currNode].CastRequirements);
            }
            return castReqs;
        }
    }
    private CastingResources castRets;
    public CastingResources CastReturns 
    {   
        get{
            CastingResources castRets = new CastingResources();
            if(nodes.Count == 0) return castRets;

            bool mark = !nodes[0].marked;
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(0);
            nodes[0].marked = mark;

            while (queue.Count > 0)
            {   
                int currNode = queue.Dequeue();
                foreach(int i in arcs[currNode])
                {
                    if(nodes[i].marked != mark)
                    {
                        queue.Enqueue(i);
                        nodes[i].marked = mark;
                    }
                }
                castRets.Merge(nodes[currNode].CastReturns);
            }
            return castRets;
        }
    }

    CastingResources ICastable.CastRequirements => throw new NotImplementedException();

    CastingResources ICastable.CastReturns => throw new NotImplementedException();

    float ICastable.Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    int ICastable.Mana { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    float ICastable.CastingTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    private SpellNode end;
    public async Task<CastingResources> Cast(CastingResources data)
    {
        //Early Returns
        if(!(data >= castReqs) || nodes.Count == 0) return data;
        TaskFactory<CastingResources> scheduller = new TaskFactory<CastingResources>();
        foreach(SpellNode node in nodes)
        {
            node.castStatus = node.Cast();
        }
        
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(0);
        int currNode = 0;
        while (queue.Count > 0)
        {   
            
            foreach(int idx in arcs[currNode])
            {
                SpellNode node = nodes[idx];
                if(!node.marked)
                {
                    queue.Enqueue(idx);
                    nodes[idx].marked = true;
                    nodes[idx].castStatus.Start();
                }
                
            }
            currNode = queue.Dequeue();
        }

        foreach(SpellNode node in nodes)
        {
            node.marked = false;
            await node.castStatus;
        }
        
        return CastingResources.Merge(nodes.Select<SpellNode, CastingResources>(i => i.castStatus?.Result).ToArray());
    }
}
