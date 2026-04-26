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
        var icon = GetNodeOrNull<TextureRect>("MarginContainer/Icon");

        if (icon == null) return;
        if (label == null) return;

        if (_tile == null)
        {
            label.Text = "No tile value set.";
        }
        else
        {
            label.Text = "";
            if (_tile is Suited s && s.Suit == Suit.Character) {
                label.Text = $"{s.Number}";
            }

            icon.Texture = TileTextureLibrary.Instance.Get(_tile);
        }
    }
}
