using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mahjong;

public abstract record GameState;
public record GameOngoing() : GameState;
public record DeckEmpty() : GameState;
public record Winner(int id, Player player, List<Meld> winningHand) : GameState;

public partial class GameManager : Control
{
	[Export] public PackedScene TileHandScene;
	[Export] public PackedScene MahjongTileScene;

	private Overlay _overlay;

	private Player[] _players;
	private Deck _deck;
	private readonly DiscardPile _discardPile = new(); // shared discard pile for simplicity

	// game loop state
	private int _currentPlayerIndex = 0;
	private int _lastDiscardPlayerIndex = -1;
	private bool _skipDrawThisTurn = false;

	private List<TileHand> _handVisuals = new();
	private MahjongTile _discardVisual = new();

	public void Init(Player[] players, Deck deck)
	{
		_players = players;
		_deck = deck;

		foreach (var player in _players)
		{
			for (int i = 0; i < 13; i++)
			{
				var tile = _deck.Draw();

				if (tile != null)
					player.ReceiveTile(tile);
			}
		}
	}

	public override void _Ready()
	{
		if (_deck == null)
		{
			GD.PrintErr("GameManager was not initialized with a Deck!");
			return;
		}

		for (int i = 0; i < _players.Length; i++)
		{
			// 1. Find the anchor node (e.g., "PlayerAnchors/Anchor0")
			var anchor = GetNode<Control>($"Anchor{i}");

			// 2. Create the Visual Hand
			var handUI = TileHandScene.Instantiate<TileHand>();

			if (i != 0) handUI.Hidden = true;
			GD.Print($"{i}: ${handUI.Hidden}");

			anchor.AddChild(handUI);

			// 3. Link them
			_handVisuals.Add(handUI);
		}

		var discardAnchor = GetNode<Control>("AnchorDiscard");
		_discardVisual = MahjongTileScene.Instantiate<MahjongTile>();
		discardAnchor.AddChild(_discardVisual);

		_overlay = GetNode<Overlay>("Overlay");

		RunGameLoop();
		RefreshVisuals();
	}

	public void RefreshVisuals()
	{
		for (int i = 0; i < _players.Length; i++)
		{
			// Pass the internal data from the Player to the TileHand script
			_handVisuals[i].Hand = _players[i].Hand;
		}

		_discardVisual.Tile = _discardPile.End;
	}

	private async void RunGameLoop()
	{
		while (true)
		{
			var state = await PlayTurn();
			RefreshVisuals();

			// Pause here for 3 seconds without freezing the game
			await Task.Delay(100);

			GD.Print("3 seconds passed, next turn!");

			if (state is Winner winner)
			{
				GD.Print("Winner!");
				foreach (var meld in winner.winningHand)
				{
					GD.Print($"meld: {string.Join(" ", meld.Tiles)}");
				}

				_overlay.ShowWin(winner.id);
				break;
			}
			else if (state is DeckEmpty)
			{
				GD.Print("Deck empty!");

				_overlay.ShowWin(-1);
				break;
			}
		}

		foreach (var hand in _handVisuals)
		{
			hand.Hidden = false;
		}

		RefreshVisuals();
	}

	public GameManager()
	{
	}

	public void NotifyTileClicked(MahjongTileRecord tile)
	{
		var human = _players[0];
		human.notifyTileClicked(tile);
	}

	// getters that will probably be useful for the GUI
	public IReadOnlyList<Player> Players => _players;
	public MahjongTileRecord? LastDiscard => _discardPile.End;
	public int? LastDiscardPlayerIndex => _lastDiscardPlayerIndex >= 0 ? _lastDiscardPlayerIndex : null;

	public async Task<GameState> PlayTurn()
	{
		if (_deck.Empty()) return new DeckEmpty();

		var player = _players[_currentPlayerIndex];
		var lastDiscard = LastDiscard;

		if (!_skipDrawThisTurn)
		{
			MahjongTileRecord acquiredTile = _deck.Draw();
			if (acquiredTile == null)
			{
				return new DeckEmpty();
			}

			player.ReceiveTile(acquiredTile);
			GD.Print("got a tile");
		}
		else
		{
			// turn right after a steal
			_skipDrawThisTurn = false;
		}

		var winningHand = HandSolver.FindWinningHand(player.Hand);
		if (winningHand != null)
		{
			return new Winner(_currentPlayerIndex, player, winningHand);
		}

		var discard = player.DecideDiscard();
		player.Discard(await discard); // TODO: check return value

		_discardPile.Add(await discard);
		_lastDiscardPlayerIndex = _currentPlayerIndex;

		if (await ResolveSteals(await discard))
			// turn already reassigned
			return new GameOngoing();

		AdvanceTurn();
		return new GameOngoing();
	}

	private void AdvanceTurn()
	{
		_currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Length;
	}

	// atomic meld commit, should either succeed or fail
	private bool TryCommitMeld(TileHandData hand, Meld meld, MahjongTileRecord discard, bool isNextPlayer)
	{
		if (!MeldValidator.CanSteal(hand, discard, isNextPlayer))
		{
			return false;
		}

		// will take first matching instance
		var tiles = meld.Tiles.ToList();
		if (!tiles.Remove(discard))
		{
			return false; // discarded tile missing from the meld for some reason
		}

		// remove other 2 from hand
		var rollback = new List<MahjongTileRecord>();
		bool needsRollback = false;

		foreach (var tile in tiles)
		{
			if (!hand.Discard(tile))
			{
				needsRollback = true;
				break;
			}
			else
			{
				rollback.Add(tile);
			}
		}

		if (needsRollback)
		{
			foreach (var tile in rollback)
			{
				hand.AddConcealed(tile);
			}
			return false;
		}

		hand.AddMeld(meld);
		return true;
	}

	private async Task<bool> ResolveSteals(MahjongTileRecord lastDiscard)
	{
		for (int i = 1; i < _players.Length; i++)
		{
			int idx = (_currentPlayerIndex + i) % _players.Length;
			var player = _players[idx];

			bool isNext = (i == 1);

			if (await player.DecideStealOrPass(lastDiscard, isNext) is Steal steal)
			{
				// give tile
				if (!TryCommitMeld(player.Hand, steal.Meld, lastDiscard, isNext)) continue;
				_discardPile.TakeEnd();

				// next turn starts with stealing player
				_currentPlayerIndex = idx;
				_lastDiscardPlayerIndex = -1;
				_skipDrawThisTurn = true;

				return true;
			}
		}

		return false;
	}

	// nested builder for adding players (probably through some config)
	public class Builder
	{
		private static readonly int MinPlayers = 2;
		private static readonly int MaxPlayers = 4;

		private readonly List<Player> _players = new();
		private Deck _deck;

		public Builder AddPlayer(Player player)
		{
			_players.Add(player);
			return this;
		}

		public Builder AddPlayers(IEnumerable<Player> players)
		{
			_players.AddRange(players);
			return this;
		}

		public Builder SetDeck(IEnumerable<MahjongTileRecord> tiles)
		{
			_deck = new Deck(tiles);
			return this;
		}

		public GameManager Build()
		{
			if (_players.Count < MinPlayers || _players.Count > MaxPlayers)
			{
				throw new InvalidOperationException("Mahjong needs 2-4 players");
			}

			if (_deck == null)
			{
				throw new InvalidOperationException("Deck must be set");
			}
			GameManager gameManager = new GameManager();
			gameManager.Init(_players.ToArray(), _deck);
			return gameManager;
		}
	}
}
