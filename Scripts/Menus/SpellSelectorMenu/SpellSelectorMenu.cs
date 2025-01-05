using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class SpellSelectorMenu : Control
{
	public int selected = 0;

	[Export]
	public PackedScene spellIconScene;
	public PackedScene spellEditorScene;
	
	[Export] public HBoxContainer spellIconList;
	[Export] public RichTextLabel descriptionLabel;
	[Export] public RichTextLabel titleLabel;
	[Export] public RichTextLabel statsLabel;


	[Export] public Button inscribeButton;
	[Export] public Button deleteButton;
	[Export] public Button exitButton;


	float portraitSize;
	int maxPortraitsOnScreen;
    public override void _Ready()
    {
        base._Ready();
		selected = spellIconList.GetChildCount()/2;
		portraitSize = spellIconList.GetChild<TextureRect>(0).Size.X;
		
		maxPortraitsOnScreen = (int)(Size.X/portraitSize*1.2f);
		LoadSpells();
		
		UpdateSpellPreview();
	
		MoveRight();

		inscribeButton.Pressed += LaunchSpellEditor;
		
    }

	public override void _Input(InputEvent @event)
    {
        base._Input(@event);
		if(@event.IsActionPressed("game_left", true)) 
		{ 
			
			MoveLeft();
			UpdateSpellPreview();
		}
		else if(@event.IsActionPressed("game_right", true)) 
		{ 
			
			MoveRight();
			UpdateSpellPreview();
		}
		
		if(@event.IsAction("ui_accept", true)) { LaunchSpellEditor(); }
    }

	public override void _Process(double delta)
    {
        base._Process(delta);
		//translateProcess?.Invoke(delta);
		//STUB:
		
		
    }

	public void LoadSpells()
	{
		foreach(var resource in SpellRepository.LoadSpellResources())
		{
			SpellIcon icon = spellIconScene.Instantiate<SpellIcon>();
			icon.resource = resource;
			icon.Texture = resource.Portrait;
			InsertIcon(icon);
		}
	}

	private void InsertIcon(SpellIcon icon)
	{
		Debug.Assert(icon != null);
		spellIconList.AddChild(icon);
		icon.Position = spellIconList.GetChild<SpellIcon>(spellIconList.GetChildCount()-2).Position 
		+ Vector2.Right*portraitSize*1.2f;
		if(spellIconList.GetChildCount() % 2 == 0) MoveLeft();
	}


#region MOVING_LIST

	private Action<double> translateProcess = null;
	[Export] float transitionSpeed = 6.0f;

	public void MoveLeft()
	{
		if(spellIconList.GetChildCount() < 2) return;
		selected++;
		if(selected >= spellIconList.GetChildCount()) selected = 0;

		SetupTranslation();
	}

	public void MoveRight()
	{
		if(spellIconList.GetChildCount() < 2) return;
		selected--;
		if(selected < 0) selected = spellIconList.GetChildCount()-1;
		
		SetupTranslation();
	}
	public void SetupTranslation()
	{
		Vector2 originalPos = spellIconList.Position;

		Vector2 finalPos = originalPos 
		- Vector2.Right * ((spellIconList.GetChild<SpellIcon>(selected).GlobalPosition.X 
			- spellIconList.GetParent<Control>().GetGlobalRect().GetCenter().X)
		+ spellIconList.GetChild<SpellIcon>(selected).Size.X/2);

		//STUB:
			spellIconList.Position = finalPos;

	/*
		translateProcess = (double delta) => 
		{
			
			translateProcess = null;
			/*
			
			bool endThisFrame = false;
			for(int i = 0; i < icons.Count(); i++)
			{
				var currentPosition = icons[i].Position.MoveToward(newPositions[i], transitionSpeed*(float)delta);
				icons[i].Position = currentPosition;
				if(currentPosition.IsEqualApprox(newPositions[i])) endThisFrame = true;
			}
			if(endThisFrame) translateProcess = null;
		};*/
	}
	

#endregion

#region SPELL_SELECTION

	public void UpdateSpellPreview()
	{
		SpellResource resource = ((SpellIcon)spellIconList.GetChild(selected)).resource;
		if(resource == null) resource = new SpellResource{Name = "New Spell", Description = "Create a new spell"};

		titleLabel.Text = resource.Name;
		descriptionLabel.Text = resource.Description;
		//TODO: statsLabel.Text = ??????
		
	}

	public void LaunchSpellEditor()
	{
		SpellResource resource = ((SpellIcon)spellIconList.GetChild(selected)).resource;
		SceneManager.SaveData("SELECTED_SPELL", resource);
		SceneManager.LoadScene("SpellEditor");
	}

#endregion

}



