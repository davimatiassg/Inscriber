using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Xml;
using System.Xml.Linq;
using Godot;
using SpellEditing;


public static class GraphRepository
{

    public static bool IsGraphValid(ISpellDigraph<ISpellGraphNode> graph, ref IEnumerable<ISpellGraphNode> faultyNodes)
    {
        foreach(var node in graph.Nodes)
        {
            CastingResources res = ((Rune)node.Castable).SigilResources;
            res.Merge( CastingResources.Merge(graph.GetPrevNodesOf(node).Select(n => n.Castable.CastReturns)) );

            if(!(node.Castable.CastRequirements <= res)) 
            {
                faultyNodes.Append(node);
            }
        }
        return faultyNodes.Count() == 0;
    }

    private static bool IsGraphValid(ISpellDigraph<ISpellGraphNode> graph)
    {
        foreach(var node in graph.Nodes)
        {
            CastingResources res = ((Rune)node.Castable).SigilResources;
            res.Merge( CastingResources.Merge(graph.GetPrevNodesOf(node).Select(n => n.Castable.CastReturns)) );

            if(!(node.Castable.CastRequirements <= res)) 
            {
                return false;
            }
        }
        return true;
    }

    public static string ConvertSigilParamToXML(Sigil sigil)
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
    public static void SaveGraphToXml(ISpellDigraph<ISpellGraphNode> graph, string filePath)
    {
        if(!IsGraphValid(graph)) return;
        
        var xmlDoc = new XDocument(
            new XElement("GraphData",
                new XElement("Specs",
                    new XElement("NoLoops", true),
                    new XElement("NoCycles", false),
                    new XElement("Directed", graph is ISpellDigraph<ISpellGraphNode>),
                    new XElement("IsWeighted", new XAttribute("defaultWeight", 1), graph is IWeighted<ISpellGraphNode>)
                ),
                new XElement("Graph",
                    new XElement("Nodes",
                        graph.Nodes.ConvertAll(nodeData =>
                            new XElement("Node",
                                new XAttribute("posX", nodeData.Position.X),
                                new XAttribute("posX", nodeData.Position.Y),
                                new XElement("Rune",
                                    new XAttribute("type", nodeData.Castable.GetType().ToString()),
                                    new XAttribute("rarity", ((Rune)nodeData.Castable).rarity.ToString())
                                ),
                                nodeData.GetSigilCount() > 0 ?
                                    nodeData.GetSigils().ToList().ConvertAll(sigil =>
                                    new XElement("Sigil",
                                        new XAttribute("name", sigil.Name),
                                        new XAttribute("description", sigil.Description),
                                        new XAttribute("rarity", sigil.rarity.ToString()),
                                        new XAttribute("value", ConvertSigilParamToXML(sigil)),
                                        new XAttribute("type", sigil.paramType.ToString())))
                                : null
                            )
                        )
                    ),
                    new XElement("Arcs",
                        graph is IWeighted<ISpellGraphNode> ?
                        ((IWeighted<ISpellGraphNode>)graph).WeightedEdges.Select(arc => {
                            return new XElement("Arc",
                                new XAttribute("source", arc.Key.Item1),
                                new XAttribute("target", arc.Key.Item2),
                                new XAttribute("target", arc.Value)
                            ); }
                        )
                        :
                        graph.Edges.ConvertAll(arc => {
                            return new XElement("Arc",
                                new XAttribute("source", arc.Item1),
                                new XAttribute("target", arc.Item2)
                            ); }
                        )

                    )
                )
            )
        );

        xmlDoc.Save(filePath);
    }



    public struct GraphStats
    {
        public bool noLoops;
        public bool isTree;
        public bool isDirected;
        public bool isWeighted;
        public int defaultWeight;
        public bool isValid;
    }
    public static TGraph LoadGraphFromXml<TGraph, TNode>(string filePath)          
        where TGraph : ISpellDigraph<TNode>, new()
        where TNode :  ISpellGraphNode, new()
    {
        var xmlDoc = XDocument.Load(filePath);
        TGraph graph = new TGraph();

        GraphStats stats = new GraphStats{
            noLoops = xmlDoc.Root.Element("Specs")?.Element("NoLoops")?.Value != null,
            isTree = xmlDoc.Root.Element("Specs")?.Element("NoCycles")?.Value != null,
            isDirected = xmlDoc.Root.Element("Specs")?.Element("Directed") != null,
            isWeighted = bool.Parse(xmlDoc.Root.Element("Specs")?.Element("IsWeighted")?.Value ?? "false")
        };
        

        if(stats.isDirected)
        {
            if(graph is not ISpellDigraph<TNode>) 
                throw new InvalidOperationException("The loaded graph is directed but you tried to load it on a undirected graph type.");
        }
        
        if(stats.isWeighted) 
        {
            stats.defaultWeight = int.Parse(xmlDoc.Root.Element("Specs")?.Element("IsWeighted")?.Attribute("defaultWeight")?.Value);
            if(graph is not IWeighted<TNode>) 
                throw new InvalidOperationException("The loaded graph is weighted, but tried to load it on a unweighted graph type.");
            
        }  

        XElement graphData = xmlDoc.Root.Elements("Graph").Single();

        LoadNodes<TGraph,TNode>(graphData, ref graph);

        LoadArcs<TGraph,TNode>(graphData, ref graph);


        if(!IsGraphValid((ISpellDigraph<ISpellGraphNode>)graph))
        {
            throw new FileLoadException("The loaded Graph is Invalid");
        }
        return graph;
    }

    public static void LoadNodes<TGraph,TNode>(XElement root, ref TGraph graph)
        where TGraph : ISpellGraph<TNode>, new()
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

            LoadRuneOnNode(nodeData.Elements("Rune").Single(), ref node);

            foreach (var paramElement in nodeData.Elements("Sigil"))
                node.AddSigil(LoadSigil(paramElement));

            graph.Add(node);
        }
    }

    public static void LoadArcs<TGraph,TNode>(XElement root, ref TGraph graph)
        where TGraph : ISpellGraph<TNode>, new()
        where TNode :  ISpellGraphNode, new()

    {
        if(root == null) throw new ArgumentNullException();
        
        foreach (XElement arcData in root.Elements("Node"))
        {
            int sourceIndex = int.Parse(arcData.Attributes("source").Single().Value);
            int targetIndex = int.Parse(arcData.Attributes("target").Single().Value);
            if(graph is IWeighted<TNode>)
            {
                var w = arcData.Attributes("weight").Single().Value;
                int weight = w == null ? 1 : int.Parse(arcData.Attributes("weight").Single().Value);
                ((IWeighted<TNode>)graph).Connect(graph[sourceIndex], graph[targetIndex], weight);
                continue;
            }
            graph.Connect(graph[sourceIndex], graph[targetIndex]);
        }
    }

    public static void LoadRuneOnNode<TNode>(XElement runeElement, ref TNode node) 
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
    
}
