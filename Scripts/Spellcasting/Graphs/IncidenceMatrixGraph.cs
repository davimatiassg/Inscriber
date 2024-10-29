using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Implements a Spell's Simple Graph by storing it on a Incidence Matrix 
/// </summary>
public partial class IncidenceMatrixGraph : IncidenceMatrixDigraph
{
    public override List<int> GetPrevNodesOf(Node node) => base.GetNextNodesOf(node);


}

