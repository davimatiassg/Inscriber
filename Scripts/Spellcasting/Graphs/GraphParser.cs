using Godot;
using SpellEditing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class GraphParser
{

    public static Node parent;
    private static FileDialog fileDialog;

    public static void InitiateDialog(Node parent)
    {
        fileDialog = new FileDialog
        {
            Access = FileDialog.AccessEnum.Filesystem,
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Mode = Window.ModeEnum.Windowed,
            Filters = new string[] { "*.txt ; Text Files"}
        };
        parent.AddChild(fileDialog);
        
    }
    public static void OpenGraph<T>() where T : ISpellGraph, new()
    {
        if(fileDialog == null) { InitiateDialog(parent); }
        fileDialog.FileSelected += (string path) => SpellGraphEditor.LoadSpellGraph(ParseGraphFile<T>(path));
        fileDialog.PopupCentered();
    }

    
    /// <summary>
    /// Reads a file and creates a Graph based on its inputs.
    /// </summary>
    /// <typeparam name="TGraph">The representation of the Graph.</typeparam>
    /// <param name="filePath">The path of the file to be read.</param>
    /// <returns>The graph contained in the specified file;</returns>
    public static ISpellGraph ParseGraphFile<TGraph>(string filePath) where TGraph : ISpellGraph, new()
    {

        TGraph graph = new TGraph();
        string[] lines = File.ReadAllLines(filePath);
        
        if(lines.Length < 2) throw new FileLoadException("The chosen file can not contain a Graph");

        Dictionary<string, int> nodes = new Dictionary<string, int>();

        void TryAddName(string name)
        {
            if(nodes.Keys.Contains(name)) return;
            graph.Add(new CharacterTextRune(name));
            nodes.Add(name, graph.Count-1);
        }

        for(int i = 1; i < lines.Length-1; i++)
        {
            string[] values = lines[i].Split(',');
            if(values.Length < 2) continue; 
            TryAddName(values[0]);
            TryAddName(values[1]);
            graph.Connect(graph[nodes[values[0]]], graph[nodes[values[1]]]);
        }

        string s = "[b]Nós do novo Grafo: { [/b]";
        foreach(var n in graph.Nodes) { s+= GraphUtil.NodeStringName(n) + " ";}
        s += " [b]}[/b]";
        GD.PrintRich(s);

        return graph;
    }

}