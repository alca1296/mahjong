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

		var fullDeck = new Deck(TileFactory.CreateFullSet());
		var players = new List<Player>();
		players.Add(PlayerFactory.createHumanPlayer());
		players.Add(PlayerFactory.createGreedyBot());
		players.Add(PlayerFactory.createDumbBot());
		players.Add(PlayerFactory.createGreedyBot());

		gameManager.Init(players.ToArray(), fullDeck);

		GetTree().Root.AddChild(gameManager);
		GetTree().CurrentScene = gameManager;
		QueueFree();
	}

	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
