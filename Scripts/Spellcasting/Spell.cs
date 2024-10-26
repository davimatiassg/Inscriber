using System;
using System.Threading.Tasks;
using System.Linq;
using Godot;


using Node = GraphData.Node;

/// <summary>
/// Represents a Spell during Runtime.
/// Is agnostic to graph's storage strategy and casting method.
/// </summary>
[GlobalClass]
public abstract partial class Spell : Resource, ICastable
{

    
    public bool isValid = true;

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

    public GraphData graphData;
    protected CastingResources castReqs;
    public CastingResources CastRequirements 
    {   get
        {
            castReqs = new CastingResources();
            GraphData.ForEachNodeByBFSIn(graphData, (Node currNode) => castReqs.Merge(currNode.castable.CastRequirements) );
            return castReqs;
        }
    }
    protected CastingResources castRets;
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
#endregion SPELL_DATA
    
    
    /// <summary>
    /// Casts this Spell using the avaliable Casting Resources.
    /// </summary>
    /// <param name="data">The resources usable by this spell's casting</param>
    /// <returns>A task that is completed only once the Spell's casting finishes</returns>
    /// <exception cref="InvalidOperationException"></exception>
    
    public abstract Task<CastingResources> Cast(CastingResources data);


}