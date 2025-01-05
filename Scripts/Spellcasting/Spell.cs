using System;
using System.Threading.Tasks;
using System.Linq;
using Godot;

/// <summary>
/// Represents a Spell during Runtime.
/// Is agnostic to graph's storage strategy.
/// </summary>

using SpellEditing;
public class Spell : SpellGraph<DefaultSpellGraphNode>, ICastable
{
	public bool isValid = true;
	
	
#region SPELL_DATA

	protected CastingResources castReqs;
	public CastingResources CastRequirements 
	{   get
		{
			castReqs = new CastingResources();
			GraphUtil.ForEachNodeByBFSIn((IGraph<ISpellGraphNode>)this, this[0], (currNode) => castRets.Merge(currNode.Castable.CastRequirements));
			return castReqs;
		}
	}
	protected CastingResources castRets;
	public CastingResources CastReturns 
	{   
		get{
			castRets = new CastingResources();
			GraphUtil.ForEachNodeByBFSIn((IGraph<ISpellGraphNode>)this, this[0], (currNode) => castRets.Merge(currNode.Castable.CastReturns));
			return castRets;
		}
	}

	protected CastingResources castDefs;
	public CastingResources CastDefaults
	{   
		get{
			castDefs = new CastingResources();
			GraphUtil.ForEachNodeByBFSIn((IGraph<ISpellGraphNode>)this, this[0], (currNode) => castRets.Merge(currNode.Castable.CastDefaults));
			return castDefs;
		}
	}
	public uint Cooldown { 
		///STUB
		get  
		{ 
			uint cd = 0;
			GraphUtil.ForEachNodeIn((IGraph<ISpellGraphNode>)this, (node) => cd += node.Castable.Cooldown );
			return cd;
		}
	}
	public int Mana { 
		get{
			int mana = 0;
			GraphUtil.ForEachNodeIn((IGraph<ISpellGraphNode>)this, (node) => mana += node.Castable.Mana );
			return mana;
		}  
	}
	public uint CastingTime {
		///STUB
		get  
		{ 
			uint ct = 0;
			GraphUtil.ForEachNodeIn((IGraph<ISpellGraphNode>)this, (node) => ct += node.Castable.CastingTime );
			return ct;
		}
	}

	Texture2D portrait;
    public Texture2D Portrait { get => portrait; set => portrait = value; }
    
	string description;
    public string Description { get => description; set => description = value; }

	Color color;
    public Color Color { get => color; set => color = value; }

	string name;
    public string Name { get => name; set => name = value; }

	public string Category => "Spell";

    #endregion SPELL_DATA

	
	private enum CastingSituation { READY, WAITING, DONE };
	/// <summary>
	/// Casts this Spell using the avaliable Casting Resources.
	/// Each of the spell's nodes will only be cast once its predecessor all finish their own casting.
	/// </summary>
	/// <param name="data">The resources usable by this spell's casting</param>
	/// <returns>A task that is completed only once the Spell's casting finishes</returns>
	/// <exception cref="InvalidOperationException"></exception>
	public async Task<CastingResources> Cast(CastingResources data) 
	{
		return data;
		/*
		var castStatus = GraphUtil.InitializePairType(this.Nodes, CastingSituation.READY);
		Action<ISpellGraphNode> VisitationProcess = async (ISpellGraphNode n) => 
		{
			if(castStatus[n] == CastingSituation.READY) {
				
			}
		}; 
		Action<ISpellGraphNode, ISpellGraphNode>  UnmarkedVisitProcess = null,
		Action<ISpellGraphNode, ISpellGraphNode>  MarkedVisitProcess = null;
		
		GraphUtil.ForEachNodeByBFSIn(this)
		return await Task.WhenAll((
			this.Nodes.Where((n) => this.GetNextNodesOf(n).Count == 0).
			Select(async (n, idx) => await PreviousCastings(data, n))
		)).Aggregate((CastingResources totalRes, CastingResources newRes) => totalRes+newRes);*/
	}
}
