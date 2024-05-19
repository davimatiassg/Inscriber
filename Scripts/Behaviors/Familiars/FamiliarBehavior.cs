using Godot;
using System;

[GlobalClass]
public partial class FamiliarBehavior : PhysicalBehavior
{
	
	private delegate void MoveAction(float delta);
	public enum EMoveStyle{ Follow, Orbit, Protect, Shoot }

	[Export] public float speed;
	[Export] public float floatSpeed;
	[Export] public float distanceToTarget;
	private EMoveStyle moveStyle;
	private bool isAloft;
	[Export] public string targetPath = "../Player";
	[Export] private Node2D target;
	private MoveAction moveBehaviour;
	private MoveAction floatBehaviour;
	[Export] public bool IsAloft 
	{
		get { return isAloft; } set { 
			isAloft = value;
			if(value)
			{
				
				floatBehaviour = (float delta) => {};
				return;
			}
			floatBehaviour = (float delta) => 
			{ Position += Vector2.Up*(float)Mathf.Sin(floatSpeed*Time.GetTicksMsec()/1000f);};
		}
	}

	[Export] public String Target
	{
		get { return target.GetPath(); }
		set { target = GetNode<PhysicsBody2D>(value); }
	}
	
	[Export] public EMoveStyle MoveStyle
	{
		get { return moveStyle; }
		set { 
			moveStyle = value; 
			switch (value)
			{
				case EMoveStyle.Orbit:
					moveBehaviour = (float delta) => {
						//Vector2 pos = (this.Position - target.Position).Normalized().Rotated();
					};
				break;
				default:
					moveBehaviour = (float delta) => {
						
						Vector2 pos = (this.Position - target.Position).Normalized()*distanceToTarget;

						Position = Position.MoveToward(target.Position + pos, (float)delta*speed);
					};
				break;
			}
		}
	}
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MoveStyle = moveStyle;
		IsAloft = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		floatBehaviour( (float) delta );
		moveBehaviour( (float) delta );	
	}
}
