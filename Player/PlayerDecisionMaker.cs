using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong;

public abstract record StealDecision;
public record Pass() : StealDecision;
public record Steal(Meld Meld) : StealDecision;

// decision maker can probably request illegal moves, the game manager should block them
public interface IPlayerDecisionMaker
{
    public abstract StealDecision DecideStealOrPass(TileHandData hand, MahjongTileRecord discard, bool isNextPlayer);
    public abstract MahjongTileRecord DecideDiscard(TileHandData hand);
}

public partial class GreedyBot : IPlayerDecisionMaker
{
    private static List<Meld> GetPossibleMelds(
        TileHandData hand,
        MahjongTileRecord discard,
        bool isNextPlayer
    )
    {
        var results = new List<Meld>();

        var tiles = hand.ConcealedTiles;

        // try triplet
        if (tiles.Count(t => t == discard) >= 2)
        {
            results.Add(new Meld(
                MeldType.Triplet,
                new List<MahjongTileRecord> { discard, discard, discard }
            ));
        }

        // try sequence (only next player)
        if (isNextPlayer && discard is Suited s)
        {
            var candidates = new[]
            {
                (s.Number-2, s.Number-1), // behind
                (s.Number-1, s.Number+1), // middle
                (s.Number+1, s.Number+2), // in front
            };

            foreach (var (a, b) in candidates)
            {
                if (a < 1 || b > 9) continue;

                var t1 = new Suited(s.Suit, a);
                var t2 = new Suited(s.Suit, b);

                if (tiles.Contains(t1) && tiles.Contains(t2))
                {
                    results.Add(new Meld(
                        MeldType.Sequence,
                        new List<MahjongTileRecord> { t1, discard, t2 }
                    ));
                }
            }
        }

        return results;
    }

    public StealDecision DecideStealOrPass(TileHandData hand, MahjongTileRecord discard, bool isNextPlayer)
    {
        // check if this steal improves score heuristic, and submit a steal request if so. 
        // the manager will validate the steal and block it if the tile would only contribute to a partial meld
        int meldsNeeded = 4 - hand.Melds.Count(m => m.Type != MeldType.Pair);
        int baseScore = HandSolver.ScoreConcealedTiles(hand.ConcealedTiles, meldsNeeded);

        var melds = GetPossibleMelds(hand, discard, isNextPlayer);
        Meld? bestMeld = null;
        int bestScore = baseScore;

        foreach (var meld in melds)
        {
            var simulated = hand.ConcealedTiles.ToList();

            // remove tiles used
            bool valid = true;
            switch (meld.Type)
            {
                case MeldType.Triplet:
                    for (int i = 0; i < 2; i++)
                        if (!simulated.Remove(discard))
                            valid = false;
                    break;
                case MeldType.Sequence:
                    foreach (var t in meld.Tiles)
                    {
                        if (!t.Equals(discard))
                        {
                            if (!simulated.Remove(t))
                                valid = false;
                        }
                    }
                    break;
            }
            if (!valid) continue; // skip this meld if a problem was found

            // after meld, discard one
            for (int i = 0; i < simulated.Count; i++)
            {
                var test = new List<MahjongTileRecord>(simulated);
                test.RemoveAt(i);
                int score = HandSolver.CompleteMeldScore + HandSolver.ScoreConcealedTiles(test, meldsNeeded-1);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMeld = meld;
                }
            }

            
        }

        return bestMeld != null ? new Steal(bestMeld) : new Pass();
    }

    public MahjongTileRecord DecideDiscard(TileHandData hand)
    {
        var concealed = hand.ConcealedTiles;
        int meldsNeeded = 4 - hand.Melds.Count(m => m.Type != MeldType.Pair);

        MahjongTileRecord bestDiscard = concealed[0];
        int bestScore = int.MinValue;

        for (int i = 0; i < concealed.Count; i++)
        {
            var test = concealed.ToList();
            var removed = test[i];
            test.RemoveAt(i);
            int score = HandSolver.ScoreConcealedTiles(test, meldsNeeded);
            if (score > bestScore)
            {
                bestScore = score;
                bestDiscard = removed;
            }
        }

        return bestDiscard;
    }
}

public partial class DumbBot : IPlayerDecisionMaker
{
    public StealDecision DecideStealOrPass(TileHandData hand, MahjongTileRecord discard, bool isNextPlayer)
    {
        return new Pass();
    }

    public MahjongTileRecord DecideDiscard(TileHandData hand)
    {
        int randomIndex = Random.Shared.Next(hand.ConcealedTiles.Count);
        return hand.ConcealedTiles[randomIndex];
    }
}