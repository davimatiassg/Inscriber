using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Godot;

[GlobalClass]
public partial class SpellResource : Resource
{
    [Export] public string Name;
    [Export] public string Description;
    [Export] public Texture2D Portrait;
    [Export] public string XMLGraphData;
}