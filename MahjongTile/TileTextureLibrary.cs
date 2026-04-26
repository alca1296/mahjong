using Godot;
using System;
using System.Collections.Generic;

namespace Mahjong;

public class TileTextureLibrary
{
    private static TileTextureLibrary? _instance;
    public static TileTextureLibrary Instance => _instance ??= new TileTextureLibrary();

    private readonly Dictionary<MahjongTileRecord, Texture2D> _textures = new();

    private TileTextureLibrary()
    {
        LoadAllTextures();
    }

    private void LoadAllTextures()
    {
        // Suited tiles
        for (int n = 1; n <= 9; n++)
        {
            _textures[new Suited(Suit.Character, n)] =
                Load($"res://Images/Man{n}.png");

            _textures[new Suited(Suit.Dot, n)] =
                Load($"res://Images/Pin{n}.png");

            _textures[new Suited(Suit.Bamboo, n)] =
                Load($"res://Images/Sou{n}.png");
        }

        _textures[new Wind(WindType.East)]  = Load("res://Images/Ton.png");
        _textures[new Wind(WindType.South)] = Load("res://Images/Nan.png");
        _textures[new Wind(WindType.West)]  = Load("res://Images/Shaa.png");
        _textures[new Wind(WindType.North)] = Load("res://Images/Pei.png");

        _textures[new Dragon(DragonColor.White)] = Load("res://Images/Haku.png");
        _textures[new Dragon(DragonColor.Green)] = Load("res://Images/Hatsu.png");
        _textures[new Dragon(DragonColor.Red)]   = Load("res://Images/Chun.png");
    }

    private Texture2D Load(string path)
    {
        var tex = GD.Load<Texture2D>(path);
        if (tex == null)
            GD.PrintErr($"Failed to load: {path}");
        return tex;
    }

    public Texture2D Get(MahjongTileRecord tile)
    {
        return _textures[tile];
    }
}