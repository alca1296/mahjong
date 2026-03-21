using Godot;
using System;

[Tool] 
public partial class MahjongTile : PanelContainer
{
	// TODO: Winds and Dragons, they don't have numbers
	public enum TileSuit { Dots, Bamboo, Characters }

	[Export] public TileSuit Suit = TileSuit.Dots;
	[Export] public int Number = 1;

	public override void _Process(double delta)
	{
		UpdateUI();
	}

	private void UpdateUI()
	{
		var label = GetNodeOrNull<Label>("MarginContainer/Label");
		
		if (label != null)
		{
			label.Text = $"{Number}\n{Suit}";
		}
	}
}
