using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Simple Graph by storing it on a Adjacence Matrix 
/// </summary>
public partial class AdjacenceMatrixGraph : AdjacenceMatrixDigraph
{

    public override List<int> GetPrevNodesOf(Node node) => base.GetNextNodesOf(node);


}

