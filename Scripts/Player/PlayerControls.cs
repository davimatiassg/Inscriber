using Godot;
using System;

public partial class PlayerControls : SpellcastingBehavior
{
	[Export] public float Speed = 250.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	//public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready()
    {
        base._Ready();
		MotionMode = MotionModeEnum.Floating;
    }
	private Action<int> selectSpellAction;

	public override void _Process(double delta)
	{
		int spell_idx = spellcastingNode.SelectedCastableIndex;
		// Handle spell selection and casting.
		if(Input.IsActionJustPressed("game_select_spell_up")) { spell_idx += 1; }
		if(Input.IsActionJustPressed("game_select_spell_down")) { spell_idx -= 1; }
		for(int i = 0; i < 10; i++){ if(Input.IsActionJustPressed($"game_select_spell_{i}")) { spell_idx += i; return; } }
		spellcastingNode.SelectedCastableIndex = spell_idx;
	}
    public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			if(direction.LengthSquared() > 1) { direction = direction.Normalized(); }
			velocity = direction * Speed;
		}
		else
		{
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}


	
}
