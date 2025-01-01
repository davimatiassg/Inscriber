using Godot;
using System;

public abstract partial class SpellIcon : TextureRect
{
	public abstract void OnSelectionConfirm();
}



public partial class InscribedSpellIcon : TextureRect
{
	public string spellPath;
	public Spell spell;
	public void OnSelectionConfirm()
	{
		spell = SpellRepository.LoadNew
	}
}
