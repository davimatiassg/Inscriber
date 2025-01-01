using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract partial class SpellList : Control
{
	CircularLinkedList<SpellIcon> spellList = new CircularLinkedList<SpellIcon>();
	
	CircularLinkedListEnumerator<SpellIcon> selected;
	
	[Export] public Panel spellIconList;
	[Export] public RichTextLabel descriptionLabel;
	[Export] public RichTextLabel titleLabel;
	[Export] public RichTextLabel statsLabel;


    public override void _Ready()
    {
        base._Ready();
		spellList.AddFirst((SpellIcon)GetChild(0));
		selected = (CircularLinkedListEnumerator<SpellIcon>)spellList.GetEnumerator();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
		if(@event.IsAction("game_left", true)) { selected.MovePrev(); }
		if(@event.IsAction("game_right", true)) { selected.MoveNext(); }
    }

	public static IEnumerable<SpellIcon> LoadSpells()
	{
		List<SpellIcon> spellIcons = new List<SpellIcon>();
		foreach(var spellPath in SpellRepository.LoadSpellPaths())
		{
			
		}
	}

	public void UpdatePositions()
	{

	}

	public void UpdateLabels()
	{
	}
}



