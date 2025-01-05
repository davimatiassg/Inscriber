using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Godot;



public static class SpellRepository
{
#region DECLARATIONS
    public const string SPELL_DIRECTORY_PATH = "res://Data/Spells/";

#endregion


#region LOADING

    public static IEnumerable<SpellResource> LoadSpellResources()
    {
        IEnumerable<SpellResource> spellResources = new List<SpellResource>();

        using var spellDir = DirAccess.Open(SPELL_DIRECTORY_PATH);
        spellDir.ListDirBegin();
        string fileName = spellDir.GetNext();
        while (fileName != "")
        {
            if (spellDir.CurrentIsDir()) continue;
            spellResources.Append(ResourceLoader.Load(SPELL_DIRECTORY_PATH + fileName));
            fileName = spellDir.GetNext();
        }
        return spellResources;
    }
    //REVIEW: REPLACE THIS FUNCTION WITH ONE THAT LOADS INSIDE THE SPELL MANAGER FOR THE PLAYER
    //
    public static Spell LoadSpell(SpellResource resource)
    {
        Spell spell = new Spell()
        {
            Name = resource.Name,
            Description = resource.Description,
            Portrait = resource.Portrait,
        };
        LoadGraphFromResource<Spell, DefaultSpellGraphNode>(resource, spell);
        return spell;
    }
    public static TGraph LoadGraphFromResource<TGraph, TNode>(SpellResource resource, TGraph graph)          
        where TGraph : IGraph<TNode>, new()
        where TNode :  ISpellGraphNode, new()
    {

        if(graph == null) graph = new TGraph();

        XElement graphData = XDocument.Parse(resource.XMLGraphData).Root.Elements("Graph").Single();

        LoadNodes<TGraph,TNode>(graphData, graph);

        LoadArcs<TGraph,TNode>(graphData, graph);

        return graph;
    }

    public static void LoadNodes<TGraph,TNode>(XElement root, TGraph graph)
        where TGraph : IGraph<TNode>, new()
        where TNode :  ISpellGraphNode, new()

    {
        if(root == null) throw new ArgumentNullException();
        
        foreach (XElement nodeData in root.Elements("Node"))
        {
            var node = new TNode();

            node.Position = new Vector2{
                X = float.Parse(nodeData.Attribute("posX").Value),
                Y = float.Parse(nodeData.Attribute("posY").Value)
            };

            LoadRuneOnNode(nodeData.Elements("Rune").Single(), node);

            foreach (var paramElement in nodeData.Elements("Sigil"))
                node.AddSigil(LoadSigil(paramElement));

            graph.Add(node);
        }
    }

    public static void LoadArcs<TGraph,TNode>(XElement root, TGraph graph)
        where TGraph : IGraph<TNode>, new()
        where TNode :  ISpellGraphNode, new()

    {
        if(root == null) throw new ArgumentNullException();
        
        foreach (XElement arcData in root.Elements("Node"))
        {
            int sourceIndex = int.Parse(arcData.Attributes("source").Single().Value);
            int targetIndex = int.Parse(arcData.Attributes("target").Single().Value);
            if(graph.Flags.Contains(GraphFlag.WEIGHTED))
            {
                var w = arcData.Attributes("weight").Single().Value;
                int weight = w == null ? 1 : int.Parse(arcData.Attributes("weight").Single().Value);
                graph.Connect(graph[sourceIndex], graph[targetIndex], weight);
                continue;
            }
            graph.Connect(graph[sourceIndex], graph[targetIndex]);
        }
    }

    public static void LoadRuneOnNode<TNode>(XElement runeElement, TNode node) 
        where TNode :  ISpellGraphNode, new() 
    {
        ICastable castable;
        String runeType = runeElement.Attributes().Single().Value;
        var rarity = (Rune.ERuneRarity) Enum.Parse(typeof(Rune.ERuneRarity), runeElement.Attribute("rarity").Value.AsSpan());
        if(runeType == typeof(RuneCreate).ToString())
        {
            castable = new RuneCreate{rarity = rarity};
        }
        /*else if(false)
        {
            //TODO!
        }*/
        else 
        {
            castable = new RandomTestRune();
        }

        node.Castable = castable;
    }

    public static Sigil LoadSigil(XElement sigilElement)
    {
        var type = (CastParam.ECastParamType) Enum.Parse(typeof(CastParam.ECastParamType), sigilElement.Attribute("type").Value.AsSpan());
        var rarity = (Rune.ERuneRarity) Enum.Parse(typeof(Rune.ERuneRarity), sigilElement.Attribute("rarity").Value.AsSpan());
        var name = sigilElement.Attributes("name").Single().Value;
        var desc = sigilElement.Attributes("description").Single().Value;
        
        Sigil sigil = new Sigil(name, type, desc);

        Object val = null;

        var valueData = sigilElement.Attributes("value").Single().Value;
        switch (type)
        {
            case CastParam.ECastParamType.BOOL:
                val = bool.Parse(valueData);
                break;
            case CastParam.ECastParamType.INT:
                val = int.Parse(valueData);
                break;
            case CastParam.ECastParamType.FLOAT:
                val = float.Parse(valueData);
                break;
            case CastParam.ECastParamType.VECTOR2:
                {
                    var components = valueData.Split('-');
                    val = new Vector2(float.Parse(components[0]), float.Parse(components[1]));
                }
                break;
            case CastParam.ECastParamType.VECTOR3:
                {
                    var components = valueData.Split('-');
                    val = new Vector3(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]));
                }
                break;
            case CastParam.ECastParamType.COLOR:
                val = Color.FromHtml(valueData);
                break;
            case CastParam.ECastParamType.STRING:
                val = valueData;
                break;
            case CastParam.ECastParamType.NODE2D:
                val = GD.Load<PackedScene>(valueData);
                break;
            case CastParam.ECastParamType.PHYSICAL_BEHAVIOR:
                val = GD.Load<PackedScene>(valueData);
                break;
            default:
            break;
        }

        sigil.val = val;

        return sigil;
    }

#endregion LOADING


#region SAVING
    public static bool IsSpellValid(Spell spell, IEnumerable<ISpellGraphNode>? faultyNodes)
    {
        foreach(var node in spell)
        {
            CastingResources res = ((Rune)node.Castable).SigilResources;
            spell.ForeachSourceOf(node, (src, weight) => res.Merge(src.Castable.CastReturns) );
            if(!(node.Castable.CastRequirements <= res)) 
            {
                if(faultyNodes == null) return false;
                faultyNodes.Append(node);
            }
        }
        return faultyNodes.Count() == 0;
    }
    public static IEnumerable<ISpellGraphNode>? SaveSpell(Spell spell)
    {
        IEnumerable<ISpellGraphNode> faultyNodes = new List<ISpellGraphNode>();
        
        if(!IsSpellValid(spell, faultyNodes)) return faultyNodes;

        SpellResource spellResource = new SpellResource
        {
            Name = spell.Name,
            Description = spell.Description,
            Portrait = spell.Portrait,
            XMLGraphData =  new XDocument(GraphToXML((IGraph<ISpellGraphNode>)spell)).ToString()
        };

        ResourceSaver.Save(spellResource, SPELL_DIRECTORY_PATH + spellResource.GetRid().ToString());

        return null;
    }

    public static XElement GraphToXML(IGraph<ISpellGraphNode> graph)
    {
        return new XElement("Graph",
            NodesToXML(graph),
            ArcsToXML(graph)
        );
    }

    public static XElement ArcsToXML(IGraph<ISpellGraphNode> graph)
    {   
        XElement arcsElement = new XElement("Arcs");
        if( graph.Flags.Contains(GraphFlag.WEIGHTED))
        {
            graph.ForeachEdge(
                (src, trg, weight) => {
                    arcsElement.Add(
                        new XElement("Arc",
                            new XAttribute("source", src.Index),
                            new XAttribute("target", trg.Index),
                            new XAttribute("weight", weight)
                        )
                    ); 
                }
            );
        }
        else
        {
            graph.ForeachEdge(
                (src, trg, weight) => {
                    arcsElement.Add(
                        new XElement("Arc",
                            new XAttribute("source", src.Index),
                            new XAttribute("target", trg.Index)
                        )
                    ); 
                }
            );
        }
        return arcsElement;
    }

    public static XElement NodesToXML(IGraph<ISpellGraphNode> graph)
    {
        return new XElement("Nodes",
            graph.ToList().ConvertAll(nodeData =>
                new XElement("Node",
                    new XAttribute("index", nodeData.Index),
                    new XAttribute("posX", nodeData.Position.X),
                    new XAttribute("posX", nodeData.Position.Y),
                    new XElement("Rune",
                        new XAttribute("type", nodeData.Castable.GetType().ToString()),
                        new XAttribute("rarity", ((Rune)nodeData.Castable).rarity.ToString())
                    ),
                    nodeData.GetSigilCount() > 0 ?
                        nodeData.GetSigils().Select(sigil =>
                        new XElement("Sigil",
                            new XAttribute("name", sigil.Name),
                            new XAttribute("description", sigil.Description),
                            new XAttribute("rarity", sigil.rarity.ToString()),
                            new XAttribute("value", SigilToXML(sigil)),
                            new XAttribute("type", sigil.paramType.ToString())))
                    : null
                )
            )
        );
    }

    public static string SigilToXML(Sigil sigil)
    {
        string xmlStr = "";
        switch (sigil.paramType)
        {
            case CastParam.ECastParamType.BOOL:
                xmlStr = ((bool)sigil.val).ToString();
                break;
            case CastParam.ECastParamType.INT:
                xmlStr = ((int)sigil.val).ToString();
                break;
            case CastParam.ECastParamType.FLOAT:
                xmlStr = ((float)sigil.val).ToString();
                break;
            case CastParam.ECastParamType.VECTOR2: 
                xmlStr = $"{((Vector2)sigil.val).X}-{((Vector2)sigil.val).Y}";
                break;
            case CastParam.ECastParamType.VECTOR3:
                xmlStr = $"{((Vector3)sigil.val).X}-{((Vector3)sigil.val).Y}-{((Vector3)sigil.val).Z}";
                break;
            case CastParam.ECastParamType.COLOR:
                xmlStr = ((Color)sigil.val).ToHtml();
                break;
            case CastParam.ECastParamType.STRING:
                xmlStr = (string)sigil.val;
                break;
            case CastParam.ECastParamType.NODE2D:
                xmlStr = ((PackedScene)sigil.val).ResourcePath;
                break;
            case CastParam.ECastParamType.PHYSICAL_BEHAVIOR:
                xmlStr = ((PackedScene)sigil.val).ResourcePath;
                break;
            default:
            break;
        }
        return xmlStr;
    }

#endregion SAVING
}
