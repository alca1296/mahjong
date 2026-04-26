using Godot;
using System;

namespace Mahjong;

[Tool]
public partial class MahjongTile : PanelContainer
{
	private MahjongTileRecord _tile = null;

	public MahjongTileRecord Tile
	{
		get => _tile;
		set
		{
			_tile = value;
			UpdateUI();
		}
	}

	public override void _Ready()
	{
		UpdateUI();
	}

	private void UpdateUI()
	{
		var label = GetNodeOrNull<Label>("MarginContainer/Label");

		if (label == null) return;

		if (_tile == null)
		{
			label.Text = "No tile value set.";
		}
		else
		{
			label.Text = _tile.ToString(); ;
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				HandleTileClicked();
				AcceptEvent();
			}
		}
	}

	private void HandleTileClicked()
	{
		var gameScene = GetTree().CurrentScene as GameManager;

		if (gameScene != null)
		{
			gameScene.NotifyTileClicked(_tile);
			GD.Print($"Clicked: {_tile}");
		}
	}
}
