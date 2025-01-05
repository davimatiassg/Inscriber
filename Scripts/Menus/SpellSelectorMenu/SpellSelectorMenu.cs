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
	
	[Export] public Panel spellIconList;
	[Export] public RichTextLabel descriptionLabel;
	[Export] public RichTextLabel titleLabel;
	[Export] public RichTextLabel statsLabel;


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
    }

	public override void _Input(InputEvent @event)
    {
        base._Input(@event);
		if(@event.IsAction("game_left", true)) 
		{ 
			UpdateSpellPreview();
			MoveLeft(); 
		}
		else if(@event.IsAction("game_right", true)) 
		{ 
			UpdateSpellPreview();
			MoveRight(); 
		}
		

		if(@event.IsAction("ui_accept", true)) { LaunchSpellEditor(); }
    }

	public override void _Process(double delta)
    {
        base._Process(delta);
		translateProcess?.Invoke(delta);

    }

	public void LoadSpells()
	{
		foreach(var resource in SpellRepository.LoadSpellResources())
		{
			SpellIcon icon = spellIconScene.Instantiate<SpellIcon>();
			icon.resource = resource;
			InsertIcon(icon);
		}
	}

	private void InsertIcon(SpellIcon icon)
	{
		AddChild(icon);
		icon.Position = icon.GetChild<SpellIcon>(spellIconList.GetChildCount()-2).Position + Vector2.Right*portraitSize*1.2f;
		if(spellIconList.GetChildCount() % 2 == 0) MoveRight();

	}


#region MOVING_LIST

	private Action<double> translateProcess = null;
	[Export] float transitionSpeed = 6.0f;

	public void MoveRight()
	{
		if(spellIconList.GetChildCount() < 2) return;
		bool looped = false;
		selected++;
		if(selected >= spellIconList.GetChildCount()) {selected = 0; looped = true;}
		
		List<SpellIcon> icons =  spellIconList.GetChildren().Cast<SpellIcon>().ToList();

		List<Vector2> newPositions = spellIconList.GetChildren().Select
		(
			childNode => ((Control)childNode).Position + 
			(
				looped ?
				Vector2.Left*portraitSize*1.2f*icons.Count() :
				Vector2.Right*portraitSize*1.2f	
			)
		).ToList();

		translateProcess = (double delta) => 
		{
			bool endThisFrame = false;
			for(int i = 0; i < icons.Count(); i++)
			{
				var currentPosition = icons[i].Position.MoveToward(newPositions[i], transitionSpeed*(float)delta);
				icons[i].Position = currentPosition;
				if(currentPosition.IsEqualApprox(newPositions[i])) endThisFrame = true;
			}
			if(endThisFrame) translateProcess = null;
		};
	}
	public void MoveLeft()
	{
		if(spellIconList.GetChildCount() < 2) return;
		bool looped = false;
		selected--;
		if(selected < 0 ) {selected = spellIconList.GetChildCount()-1; looped = true;}
		
		List<SpellIcon> icons =  spellIconList.GetChildren().Cast<SpellIcon>().ToList();

		List<Vector2> newPositions = spellIconList.GetChildren().Select
		(
			childNode =>	((Control)childNode).Position - 
			(
				looped ?
				Vector2.Left*portraitSize*1.2f*icons.Count() :
				Vector2.Right*portraitSize*1.2f	
			)
		).ToList();

		translateProcess = (double delta) => 
		{
			bool endThisFrame = false;
			for(int i = 0; i < icons.Count(); i++)
			{
				var currentPosition = icons[i].Position.MoveToward(newPositions[i], transitionSpeed*(float)delta);
				icons[i].Position = currentPosition;
				if(currentPosition.IsEqualApprox(newPositions[i])) endThisFrame = true;
			}
			if(endThisFrame) translateProcess = null;
		};

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



