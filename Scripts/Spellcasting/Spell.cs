using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Godot;
using System.Runtime.CompilerServices;
using System.Collections;

using Node = GraphData.Node;
[GlobalClass]
public partial class Spell : Resource, ICastable
{

    
    public bool Valid = true;

    // Class for the nodes of the Spellcasting graph

    /*public struct Node : ICastable
    {
        uint index;
        public ICastable castable;
        public Task<CastingResources> castStatus;
        public CastingResources CastRequirements { get { return castable.CastRequirements; } }
        public CastingResources CastReturns { get { return castable.CastReturns; } }

        public uint CastingTime => castable.CastingTime;

        public uint Cooldown => castable.Cooldown;

        public int Mana => castable.Mana;

        public async Task<CastingResources> Cast(CastingResources data)
        {
            return await castable.Cast(data);
        }

    }*/
    
    
#region SPELL_DATA

    private GraphData graphData;

    private Node startingNode;
    private CastingResources castReqs;
    public CastingResources CastRequirements 
    {   get
        {
            castReqs = new CastingResources();
            GraphData.ForEachNodeByBFSIn(graphData, (Node currNode) => castReqs.Merge(currNode.castable.CastRequirements) );
            return castReqs;
        }
    }
    private CastingResources castRets;
    public CastingResources CastReturns 
    {   
        get{
            castRets = new CastingResources();
            GraphData.ForEachNodeByBFSIn(graphData, (Node currNode) => castRets.Merge(currNode.castable.CastReturns) );
            return castRets;
        }
    }
    public uint Cooldown { 
        get => (uint) graphData.WalkFullPath( (Node currNode) => { return (int) currNode.castable.Cooldown; } );

    }
    public int Mana { 
        get{
            int mana = 0;
            GraphData.ForEachNodeIn(graphData, (Node node) => mana += node.castable.Mana );
            return mana;
        }  
    }
    public uint CastingTime { 
        get => (uint) graphData.WalkFullPath( (Node currNode) => { return (int) currNode.castable.CastingTime; } );
    }

    /// <summary>
    /// Casts this Spell using the avaliable Casting Resources, starting by the Spell's entryNode.
    /// </summary>
    /// <param name="data">The resources usable by this spell's casting</param>
    /// <returns>A task that is completed only once the Spell's casting finishes</returns>
    /// <exception cref="InvalidOperationException"></exception>
    
    public async Task<CastingResources> Cast(CastingResources data)
    {
        if(!(data >= castReqs)) throw new InvalidOperationException("Insuficient Data to cast this spell");
        return data + await CastRecursive(data, startingNode);
    }

    private async Task<CastingResources> CastRecursive(CastingResources data, Node current)
    {
        data += await current.castable.Cast(data);
        uint[] nexts = graphData.GetNextNodesOf(current);

        Task<CastingResources>[] castTasks = new Task<CastingResources>[nexts.Length];

        foreach(uint node in nexts)
        {
            castTasks.Append(new Task(async () => {
                    await graphData[node].castable.Cast(data);
                }
            ));
        }

        return data + CastingResources.Merge(await Task.WhenAll(castTasks));
    }


#endregion SPELL_DATA
}