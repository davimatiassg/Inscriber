using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[GlobalClass]
public partial class SceneManager : Node
{
    [Export] 
    public Godot.Collections.Dictionary<string, PackedScene> scenes = new Godot.Collections.Dictionary<string, PackedScene>();

    public static Dictionary<string, object> persistentData = new Dictionary<string, object>();
    
    public static SceneTree CurrentTree;
    public static SceneManager Instance;

    public override void _Ready()
    {
        base._Ready();
        if(Instance == null) Instance = this; 
        CurrentTree = GetTree();
    }
 
    public static void LoadScene(string sceneName)
    {
        
        CurrentTree.ChangeSceneToPacked(Instance.scenes[sceneName]);

    }

    public static void SaveData(string name, Object data)
    {
        if(persistentData.ContainsKey(name))
        {
            persistentData[name] = data;
            return;
        }
        persistentData.Add(name, data);
    }

    public static T ConsumeData<T>(string name)
    {
        object data;
        persistentData.TryGetValue(name, out data);
        persistentData.Remove(name);
        return (T)data;
    }
}
