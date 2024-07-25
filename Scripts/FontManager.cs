using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class FontManager : Node
{
    public static FontManager Instance;

    public static Dictionary<string, Font> fontLib = new Dictionary<string, Font>();

    public override void _Ready()
    {
        base._Ready();
        if(Instance == null) { Instance = this; }
        else if (Instance != this) { QueueFree(); }
    }
}
