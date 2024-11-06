using System;
using System.Threading.Tasks;
using System.Linq;
using Godot;


using Node = ISpellGraph.Node;

/// <summary>
/// Represents a Spell during Runtime.
/// Is agnostic to graph's storage strategy.
/// </summary>
[GlobalClass]
public partial class Spell : Resource, ICastable
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

	public ISpellGraph graphData;
	protected CastingResources castReqs;
	public CastingResources CastRequirements 
	{   get
		{
			castReqs = new CastingResources();
			GraphUtil.ForEachNodeByBFSIn(graphData, graphData[0], (Node currNode) => castRets.Merge(currNode.castable.CastRequirements));
			return castReqs;
		}
	}
	protected CastingResources castRets;
	public CastingResources CastReturns 
	{   
		get{
			castRets = new CastingResources();
			GraphUtil.ForEachNodeByBFSIn(graphData, graphData[0], (Node currNode) => castRets.Merge(currNode.castable.CastReturns));
			return castRets;
		}
	}
	public uint Cooldown { 
		///STUB
		get  
		{ 
			uint cd = 0;
			GraphUtil.ForEachNodeIn(graphData, (Node node) => cd += node.castable.Cooldown );
			return cd;
		}
	}
	public int Mana { 
		get{
			int mana = 0;
			GraphUtil.ForEachNodeIn(graphData, (Node node) => mana += node.castable.Mana );
			return mana;
		}  
	}
	public uint CastingTime {
		///STUB
		get  
		{ 
			uint ct = 0;
			GraphUtil.ForEachNodeIn(graphData, (Node node) => ct += node.castable.CastingTime );
			return ct;
		}
	}
#endregion SPELL_DATA
	

	public Spell()
	{
		graphData = new IncidenceMatrixGraph();
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
				graphData.Nodes.Where((Node n) => graphData.GetNextNodesOf(n).Count == 0).
				Select(async (Node n, int idx) => await PreviousCastings(data, n))
			)
		).Aggregate((CastingResources totalRes, CastingResources newRes) => totalRes+newRes);
	
	private async Task<CastingResources> PreviousCastings(CastingResources data, Node current)
	{
		/*data += current.GetSigilResources();
		data += (
				await Task.WhenAll(
					graphData.GetPrevNodesOf(current).
					Select(async (int nodeIndex, int idx) => await PreviousCastings(data, graphData[nodeIndex]))
				)
			).Aggregate((CastingResources totalRes, CastingResources newRes) => totalRes+newRes);
		
		data += await current.castable.Cast(data);*/
		return data;
	}


}
