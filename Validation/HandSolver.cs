using System.Collections.Generic;
using System.Linq;

namespace Mahjong;

public partial class HandSolver
{
    private static Dictionary<MahjongTileRecord, int> TileListToCounts(IReadOnlyList<MahjongTileRecord> tiles)
    {
        var dict = new Dictionary<MahjongTileRecord, int>();
        foreach (var t in tiles)
        {
            if (!dict.ContainsKey(t)) dict[t] = 0;
            dict[t]++;
        }
        return dict;
    }

    private static Dictionary<MahjongTileRecord, int> CloneCounts(
        Dictionary<MahjongTileRecord, int> counts)
    {
        return counts.ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private static MahjongTileRecord? GetFirstAvailableTile(
        Dictionary<MahjongTileRecord, int> counts)
    {
        return counts
            .Where(kv => kv.Value > 0)
            .Select(kv => kv.Key)
            .OrderBy(tile => tile switch {
                Suited s => (0, (int) s.Suit * 10 + s.Number),
                Wind w => (1, (int) w.Direction),
                Dragon d => (2, (int) d.Color),
                _ => (3, 0) // catch all
            })
            .FirstOrDefault();
    }

    // see if the current hand wins, either before or after a draw
    // return a solution if the hand wins
    public static List<Meld>? FindWinningHand(TileHandData hand) 
    {
        return FindWinningHand(hand.ConcealedTiles, hand.Melds);
    }

    public static List<Meld>? FindWinningHand(
        IReadOnlyList<MahjongTileRecord> concealed,
        IReadOnlyList<Meld> existingMelds)
    {
        int meldsNeeded = 4 - existingMelds.Count(m => m.Type != MeldType.Pair);
        var counts = TileListToCounts(concealed);

        foreach (var (tile, count) in counts.ToList())
        {
            if (count < 2) continue;
            var countsCopy = CloneCounts(counts);
            countsCopy[tile] -= 2;
            var solution = new List<Meld>(existingMelds)
            {
                new Meld(MeldType.Pair, new List<MahjongTileRecord> { tile, tile })
            };
            var result = TryFormMelds(countsCopy, meldsNeeded, solution);
            if (result != null) return result;
        }
        return null;
    }

    private static List<Meld>? TryFormMelds(
        Dictionary<MahjongTileRecord, int> counts,
        int meldsRemaining,
        List<Meld> solution)
    {
        if (meldsRemaining == 0) return solution;

        var tile = GetFirstAvailableTile(counts);
        if (tile == null) return null;

        // try triplet
        if (counts[tile] >= 3)
        {
            var nextCounts = CloneCounts(counts);
            nextCounts[tile] -= 3;
            var nextSolution = new List<Meld>(solution)
            {
                new Meld(MeldType.Triplet, new List<MahjongTileRecord> { tile, tile, tile })
            };
            var result = TryFormMelds(nextCounts, meldsRemaining - 1, nextSolution);
            if (result != null) return result;
        }

        // try sequence
        if (tile is Suited s && s.Number <= 7)
        {
            var t2 = new Suited(s.Suit, s.Number + 1);
            var t3 = new Suited(s.Suit, s.Number + 2);
            if (counts.GetValueOrDefault(t2) > 0 && counts.GetValueOrDefault(t3) > 0)
            {
                var nextCounts = CloneCounts(counts);
                nextCounts[tile]--;
                nextCounts[t2]--;
                nextCounts[t3]--;
                var nextSolution = new List<Meld>(solution)
                {
                    new Meld(MeldType.Sequence, new List<MahjongTileRecord> { tile, t2, t3 })
                };
                var result = TryFormMelds(nextCounts, meldsRemaining - 1, nextSolution);
                if (result != null) return result;
            }
        }

        return null;
    }
    
    // heuristic partial solver
    public static readonly int CompleteMeldScore = 4;
    public static readonly int PairScore = 2;
    public static readonly int PartialMeldScore = 1;

    public static int ScoreConcealedTiles(IReadOnlyList<MahjongTileRecord> tiles, int meldsNeeded)
    {
        /*
        same idea as the hand validator. do a DFS over all decompositions 
        of this hand into complete melds, partial melds, and pairs.
        hand is scored by: 2 * complete meld count + 1 * partial meld count + pair bonus.
        an AI player can enumerate over all its discard decisions and pick the tile that maximizes this score (places it closer to melds).
        an AI player can also see if a potential steal increases this score or not, and steal if it does.
        */
        var counts = TileListToCounts(tiles);

        // no pair
        int bestScore = BestPartialScore(counts, meldsNeeded);

        foreach (var (tile, count) in counts.ToList())
        {
            if (count < 2) continue;
            counts[tile] -= 2;
            // +1 for 
            int candidate = BestPartialScore(counts, meldsNeeded) + PairScore;
            bestScore = System.Math.Max(bestScore, candidate);
            counts[tile] += 2;
        }

        return bestScore;
    }

    private static int BestPartialScore(
        Dictionary<MahjongTileRecord, int> counts,
        int meldsNeeded)
    {
        int bestCompleteCount = 0; // complete melds
        int bestPartialCount = 0; // partial melds
        BestPartialScoreSearch(counts, meldsNeeded, 0, 0, ref bestCompleteCount, ref bestPartialCount);
        return CompleteMeldScore * bestCompleteCount + PartialMeldScore * bestPartialCount;
    }

    private static void BestPartialScoreSearch(
        Dictionary<MahjongTileRecord, int> counts,
        int meldsNeeded,
        int completeCount,
        int partialCount,
        ref int bestCompleteCount,
        ref int bestPartialCount)
    {
        // update best
        if (CompleteMeldScore * completeCount + PartialMeldScore * partialCount > 
            CompleteMeldScore * bestCompleteCount + PartialMeldScore * bestPartialCount)
        {
            bestCompleteCount = completeCount;
            bestPartialCount = partialCount;
        }

        // early out
        if (completeCount >= meldsNeeded) return;

        var candidates = counts
            .Where(p => p.Value > 0)
            .OrderBy(p => p.Key switch {
                Suited s => (0, (int) s.Suit * 10 + s.Number),
                Wind w => (1, (int) w.Direction),
                Dragon d => (2, (int) d.Color),
                _ => (3, 0) // catch all
            })
            .ToList();

        if (!candidates.Any()) return;
        var tile = candidates.First().Key;

        // complete a triplet?
        if (counts[tile] >= 3)
        {
            counts[tile] -= 3;
            BestPartialScoreSearch(counts, meldsNeeded, completeCount+1, partialCount, ref bestCompleteCount, ref bestPartialCount);
            counts[tile] += 3; // backtrack
        }

        // complete a sequence?
        if (tile is Suited s1 && s1.Number <= 7)
        {
            var t2 = new Suited(s1.Suit, s1.Number + 1);
            var t3 = new Suited(s1.Suit, s1.Number + 2);
            if (counts.GetValueOrDefault(t2) > 0 && counts.GetValueOrDefault(t3) > 0)
            {
                counts[tile]--; 
                counts[t2]--; 
                counts[t3]--;
                
                BestPartialScoreSearch(counts, meldsNeeded, completeCount+ 1, partialCount, ref bestCompleteCount, ref bestPartialCount);

                counts[tile]++; 
                counts[t2]++; 
                counts[t3]++; // backtrack
            }
        }

        // triplet in progress (pair)
        if (counts[tile] >= 2)
        {
            counts[tile] -= 2;
            BestPartialScoreSearch(counts, meldsNeeded, completeCount, partialCount + 1, ref bestCompleteCount, ref bestPartialCount);
            counts[tile] += 2;
        }

        // partial sequence
        if (tile is Suited s2)
        {
            // 2 sided sequence members
            TryPartialSequence(counts, meldsNeeded, completeCount, partialCount, tile, new Suited(s2.Suit, s2.Number + 1), ref bestCompleteCount, ref bestPartialCount);

            // middle sequence member missing
            TryPartialSequence(counts, meldsNeeded, completeCount, partialCount, tile, new Suited(s2.Suit, s2.Number + 2), ref bestCompleteCount, ref bestPartialCount);
        }

        // ignore tile
        counts[tile]--;
        BestPartialScoreSearch(counts, meldsNeeded, completeCount, partialCount, ref bestCompleteCount, ref bestPartialCount);
        counts[tile]++; // backtrack
    }

    private static void TryPartialSequence(
        Dictionary<MahjongTileRecord, int> counts,
        int meldsNeeded,
        int completeCount,
        int partialCount,
        MahjongTileRecord a,
        MahjongTileRecord b,
        ref int bestCompleteCount,
        ref int bestPartialCount)
    {
        if (counts.GetValueOrDefault(b) <= 0) return; // no partial sequence

        counts[a]--;
        counts[b]--;

        BestPartialScoreSearch(counts, meldsNeeded, completeCount, partialCount + 1, ref bestCompleteCount, ref bestPartialCount);

        counts[a]++;
        counts[b]++; // backtrack
    }
}
