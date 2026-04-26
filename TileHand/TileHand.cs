using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong;

public partial class TileHand : VBoxContainer
{
	// Static reference so we only load the scene once
	private static readonly PackedScene TileScene = GD.Load<PackedScene>("res://MahjongTile/MahjongTile.tscn");

	// Event for the GameManager to subscribe to
	public event Action<MahjongTileRecord> OnTileDiscardRequested;

	private TileHandData _hand;

	public TileHandData Hand
	{
		get => _hand;
		set
		{
			GD.Print("got new value " + value.ToString());
			_hand = value;
			UpdateUI();
		}
	}

	private bool _hidden = false;

	public bool Hidden
	{
		get => _hidden;
		set
		{
			_hidden = value;
		}
	}

	private HBoxContainer _concealedTilesContainer;
	private HBoxContainer _meldsContainer;

	public override void _Ready()
	{
		_concealedTilesContainer = GetNode<HBoxContainer>("ConcealedTiles");
		_meldsContainer = GetNode<HBoxContainer>("Melds");
	}

	private MahjongTile CreateTileView(MahjongTileRecord tile, bool isInteractive)
	{
		var view = TileScene.Instantiate<MahjongTile>();
		view.Tile = tile;

		if (isInteractive)
		{
			//view.Pressed += () => HandleTileClick(tile);
			view.MouseDefaultCursorShape = CursorShape.PointingHand;
		}

		return view;
	}

	private void HandleTileClick(MahjongTileRecord tile)
	{
		//OnTileDiscardRequested?.Invoke(tile);
	}

	private void DrawConcealed()
	{
		foreach (Node child in _concealedTilesContainer.GetChildren())
			child.QueueFree();

		if (_hand == null) return;

		// GD.Print($"hidden: {Hidden}");

		foreach (var tile in SortTiles(_hand.ConcealedTiles))
		{

			var tileView = CreateTileView(tile, true);
			tileView.Hidden = Hidden;
			_concealedTilesContainer.AddChild(tileView);
		}
	}

	private void DrawMelds()
	{
		foreach (Node child in _meldsContainer.GetChildren())
			child.QueueFree();

		if (_hand == null) return;

		foreach (var meld in _hand.Melds)
		{
			var meldBox = new HBoxContainer();

			foreach (var tile in SortTiles(meld.Tiles))
			{
				var tileView = CreateTileView(tile, false);
				meldBox.AddChild(tileView);
			}
			_meldsContainer.AddChild(meldBox);
		}
	}

	public void UpdateUI()
	{
		DrawConcealed();
		DrawMelds();
	}

	private IReadOnlyList<MahjongTileRecord> SortTiles(IReadOnlyList<MahjongTileRecord> tiles)
	{
		return tiles.OrderBy(tile => tile switch {
			Suited s => (0, (int) s.Suit * 10 + s.Number),
			Wind w => (1, (int) w.Direction),
			Dragon d => (2, (int) d.Color),
			_ => (3, 0) // catch all
		}).ToList();
	}
}
