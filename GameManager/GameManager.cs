using System;
using System.Collections.Generic;

namespace Mahjong;

public partial class GameManager
{
    private readonly Player[] _players;
    private readonly Deck _deck;
    private readonly DiscardPile _discardPile = new(); // shared discard pile for simplicity

    // game loop state
    private int _currentPlayerIndex = 0;
    private int _lastDiscardPlayerIndex = -1;
    private bool _skipDrawThisTurn = false;

    private GameManager(Player[] players, Deck deck) 
    {
        _players = players;
        _deck = deck;
    }

    // getters that will probably be useful for the GUI
    public IReadOnlyList<Player> Players => _players;
    public MahjongTileRecord? LastDiscard => _discardPile.End;
    public int? LastDiscardPlayerIndex => _lastDiscardPlayerIndex >= 0 ? _lastDiscardPlayerIndex : null;

    public void PlayTurn()
    {
        var player = _players[_currentPlayerIndex];
        var lastDiscard = LastDiscard;

        if (!_skipDrawThisTurn) 
        {
            // TODO: deck runs out
            MahjongTileRecord acquiredTile = _deck.Draw();
            player.ReceiveTile(acquiredTile);
        }
        else
        {
            // turn right after a steal
            _skipDrawThisTurn = false;
        }

        var winningHand = HandSolver.FindWinningHand(player.Hand);
        if (winningHand != null)
        {
            // HANDLE WIN
            return;
        }

        var discard = player.DecideDiscard();
        player.Discard(discard); // TODO: check return value

        _discardPile.Add(discard);
        _lastDiscardPlayerIndex = _currentPlayerIndex;

        if (ResolveSteals(discard))
            return; // turn already reassigned

        AdvanceTurn();
    }

    private bool IsNextPlayer()
    {
        return LastDiscardPlayerIndex.HasValue && 
            (_lastDiscardPlayerIndex + 1) % _players.Length == _currentPlayerIndex;
    }

    private void AdvanceTurn()
    {
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Length;
    }

    private bool ResolveSteals(MahjongTileRecord lastDiscard)
    {
        for (int i = 1; i < _players.Length; i++)
        {
            int idx = (_currentPlayerIndex + i) % _players.Length;
            var player = _players[idx];

            bool isNext = (i == 1);

            if (player.DecideStealOrPass(lastDiscard) == StealOrPass.Steal && 
                MeldValidator.CanSteal(player.Hand, lastDiscard, isNext))
            {
                // give tile
                player.ReceiveTile(_discardPile.TakeEnd());
                
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

            return new GameManager(_players.ToArray(), _deck);
        }
    }
}
