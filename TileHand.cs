using Godot;
using System;

namespace Mahjong;

public partial class TileHand : VBoxContainer
{
	private TileHandData _hand;
	public TileHandData Hand
	{
		get => _hand;
		set
		{
			_hand = value;
			UpdateUI();
		}
	}
	
	private HBoxContainer _concealedTilesContainer;
	private HBoxContainer _meldsContainer;
	
	private MahjongTile CreateTileView(MahjongTileRecord tile)
	{
		var scene = GD.Load<PackedScene>("res://MahjongTile.tscn");
		var view = scene.Instantiate<MahjongTile>();
		view.Tile = tile;
		return view;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_concealedTilesContainer = GetNode<HBoxContainer>("ConcealedTiles");
		_meldsContainer = GetNode<HBoxContainer>("Melds");
		
		var testHand = new TileHandData();
		testHand.AddConcealed(new Suited(Suit.Bamboo, 2));
		testHand.AddConcealed(new Suited(Suit.Bamboo, 3));
		testHand.AddConcealed(new Suited(Suit.Bamboo, 4));
		
		Hand = testHand;
		
		UpdateUI();
	}
	
	private void DrawConcealed()
	{
		foreach (Node child in _concealedTilesContainer.GetChildren())
			child.QueueFree();
			
		if (_hand == null) return;
		
		foreach (var tile in _hand.ConcealedTiles)
		{
			var tileView = CreateTileView(tile);
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
			
			foreach (var tile in meld.Tiles)
			{
				var tileView = CreateTileView(tile);
				meldBox.AddChild(tileView);
			}
			
			_meldsContainer.AddChild(meldBox);
		}
	}

	private void UpdateUI()
	{
		DrawConcealed();
		DrawMelds();
	}
}
