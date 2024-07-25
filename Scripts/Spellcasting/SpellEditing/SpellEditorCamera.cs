using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellEditing
{
    using SpellNode = Spell.Node;
public partial class SpellEditorCamera : Camera2D 
{
    public static SpellEditorCamera Instance;
    private enum ECameraState {FOLLOW, FOCUS, STATIC, FREE}
    private readonly float[] ZOOM_MIN_MAX = { 0.5f, 2.5f };
    public const float CAMERA_SPEED = 1.0f; 
    private Action<double> currentProcess; 
    
    private ECameraState state = ECameraState.STATIC;
    public void SetStatic()
    {
        state = ECameraState.STATIC;
        currentProcess = null;
    }
    public void SetFollow(Control cntrl)
    {
        state = ECameraState.FOLLOW;
        currentProcess = (double delta) => FollowProcess(cntrl, delta);
    }
    public void SetFocus(Control cntrl){
        state = ECameraState.FOCUS;
        currentProcess = (double delta) => FocusProcess(cntrl, delta);
    }
    public void SetFree(){
        state = ECameraState.FREE;
        currentProcess = FreeProcess;

    }
    
    
    private void FollowProcess(Control cntrl, double delta)
    {
        this.Position = cntrl.GlobalPosition;
    }

    private void FocusProcess(Control cntrl, double delta)
    {

    }

    private void FreeProcess(double delta)
    {
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Position+= direction*CAMERA_SPEED*(float)delta;
        int i = (Input.IsActionPressed("game_select_spell_up") ? 1 : 0) + (Input.IsActionPressed("game_select_spell_down") ? -1 : 0 );
        Zoom = Vector2.One*Mathf.MoveToward(Zoom.X, ZOOM_MIN_MAX[i], CAMERA_SPEED*(float)delta);
        GD.Print("lol :" + i);
    }

    public override void _Ready()
    {
        base._Ready();
        if(Instance == null) { Instance = this; }
        else if( Instance != this) { QueueFree(); }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        currentProcess?.Invoke(delta);
    }


}

}
