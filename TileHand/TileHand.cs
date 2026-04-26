using Godot;
using System;

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

        foreach (var tile in _hand.ConcealedTiles)
        {

            var tileView = CreateTileView(tile, true);
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
}
