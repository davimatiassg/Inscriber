using Godot;
using System;

public partial class MainScene : Node2D
{
    private static MainScene instance;
    public override void _Ready()
    {
        base._Ready();
        if(instance == null) { instance = this; }
        else if (instance != this) { QueueFree(); }
        
    }
    public static Node2D Node { get { return instance; } }
}
