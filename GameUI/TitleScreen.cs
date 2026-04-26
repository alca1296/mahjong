using Godot;
using System;

using System.Collections.Generic;

namespace Mahjong;

public partial class TitleScreen : Control
{

    [Export] public PackedScene GameScene;

    private void OnPlayButtonPressed()
    {
        var gameManager = GameScene.Instantiate<GameManager>();

        // 2. Prepare your Mahjong data
        var fullDeck = new Deck(TileFactory.CreateFullSet());
        var players = new List<Player>();
        players.Add(PlayerFactory.createDumbBot());
        players.Add(PlayerFactory.createDumbBot());
        players.Add(PlayerFactory.createGreedyBot());
        players.Add(PlayerFactory.createGreedyBot());

        // 3. Inject the data BEFORE adding to the tree
        gameManager.Init(players.ToArray(), fullDeck);

        // 4. Switch scenes
        GetTree().Root.AddChild(gameManager);
        GetTree().CurrentScene = gameManager;

        // 5. Remove the menu
        QueueFree();
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
