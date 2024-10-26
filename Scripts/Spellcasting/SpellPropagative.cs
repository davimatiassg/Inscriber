using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Godot;
using System.Runtime.CompilerServices;
using System.Collections;

using Node = GraphData.Node;

/// <summary>
/// Represents a Spell during Runtime.
/// Is agnostic to graph's storage method.
/// Propagates casting from a starter node to all subsequent nodes.
/// </summary>
[GlobalClass]
public partial class SpellPropagative : Spell
{

    public SpellPropagative()
    {
        graphData =  new IncidenceMatrixDigraph();
    }
    protected Node startingNode;
    public override async Task<CastingResources> Cast(CastingResources data) => data + await CastRecursive(data, startingNode);
    protected async Task<CastingResources> CastRecursive(CastingResources data, Node current)
    {
        data += current.GetSigilResources();
        data += await current.castable.Cast(data);
        List<int> nexts = graphData.GetNextNodesOf(current);

        Task<CastingResources>[] castTasks = new Task<CastingResources>[nexts.Count];

        foreach(int node in nexts)
        {
            castTasks.Append(new Task(async () => {
                    await graphData[node].castable.Cast(data);
                }
            ));
        }

        return data + CastingResources.Merge(await Task.WhenAll(castTasks));
    }



}