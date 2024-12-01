using System;
using System.Threading.Tasks;
using System.Linq;
using Godot;

/// <summary>
/// Represents a Spell during Runtime.
/// Is agnostic to graph's storage strategy.
/// </summary>
using GraphType = AdjacenceListGraph<DefaultSpellGraphNode>;
using SpellEditing;

[GlobalClass]
public partial class Spell : Resource, ICastable
{
	public bool isValid = true;
	
	
#region SPELL_DATA

	public ISpellGraph<ISpellGraphNode> graphData;
	protected CastingResources castReqs;
	public CastingResources CastRequirements 
	{   get
		{
			castReqs = new CastingResources();
			GraphUtil.ForEachNodeByBFSIn(graphData, graphData[0], (currNode) => castRets.Merge(currNode.Castable.CastRequirements));
			return castReqs;
		}
	}
	protected CastingResources castRets;
	public CastingResources CastReturns 
	{   
		get{
			castRets = new CastingResources();
			GraphUtil.ForEachNodeByBFSIn(graphData, graphData[0], (currNode) => castRets.Merge(currNode.Castable.CastReturns));
			return castRets;
		}
	}

	protected CastingResources castDefs;
	public CastingResources CastDefaults
	{   
		get{
			castDefs = new CastingResources();
			GraphUtil.ForEachNodeByBFSIn(graphData, graphData[0], (currNode) => castRets.Merge(currNode.Castable.CastDefaults));
			return castDefs;
		}
	}
	public uint Cooldown { 
		///STUB
		get  
		{ 
			uint cd = 0;
			GraphUtil.ForEachNodeIn(graphData, (node) => cd += node.Castable.Cooldown );
			return cd;
		}
	}
	public int Mana { 
		get{
			int mana = 0;
			GraphUtil.ForEachNodeIn(graphData, (node) => mana += node.Castable.Mana );
			return mana;
		}  
	}
	public uint CastingTime {
		///STUB
		get  
		{ 
			uint ct = 0;
			GraphUtil.ForEachNodeIn(graphData, (node) => ct += node.Castable.CastingTime );
			return ct;
		}
	}

    public Texture2D Portrait => throw new NotImplementedException();

    public string Category => throw new NotImplementedException();

    public string Description => throw new NotImplementedException();

    public Color Color => throw new NotImplementedException();

    string IMagicSymbol.Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


    #endregion SPELL_DATA


    public Spell()
	{
		graphData = (ISpellGraph<ISpellGraphNode>) new GraphType();
	}
	
	/// <summary>
	/// Casts this Spell using the avaliable Casting Resources.
	/// Each of the spell's nodes will only be cast once its predecessor all finish their own casting.
	/// </summary>
	/// <param name="data">The resources usable by this spell's casting</param>
	/// <returns>A task that is completed only once the Spell's casting finishes</returns>
	/// <exception cref="InvalidOperationException"></exception>
	public async Task<CastingResources> Cast(CastingResources data) 
	=>  (
			await Task.WhenAll
			(
				graphData.Nodes.Where((n) => graphData.GetNextNodesOf(n).Count == 0).
				Select(async (n, idx) => await PreviousCastings(data, n))
			)
		).Aggregate((CastingResources totalRes, CastingResources newRes) => totalRes+newRes);
	
	private async Task<CastingResources> PreviousCastings(CastingResources data, ISpellGraphNode current)
	{
		return data;
	}


	

}
