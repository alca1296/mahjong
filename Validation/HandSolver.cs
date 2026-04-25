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
    
    private static bool CanFormMelds(
        Dictionary<MahjongTileRecord, int> counts, 
        List<Meld> result,
        int meldsNeeded)
    {
        /*
        recursive meld solver. pick first tile you can find with nonzero count in the 
        counts dictionary. for each meld type, try forming a meld with the chosen tile. 
        if a meld can be formed, remove that triplet from the counts and recursively 
        find more melds in whatever remains. so effectively does a full search over meld types
        to see what works.
        */
        
        // base case
        int complete = result.Count(m => m.Type != MeldType.Pair);
        if (complete == meldsNeeded) return true; // don't need more

        // order tiles so we don't pick 8 or 9 for a sequence check
        // tuples will compare with first member taking priority
        var tile = counts
            .Where(pair => pair.Value > 0)
            .OrderBy(pair => pair.Key switch {
                Suited s => (0, (int) s.Suit * 10 + s.Number),
                Wind w => (1, (int) w.Direction),
                Dragon d => (2, (int) d.Color),
                _ => (3, 0) // catch all
            })
            .First().Key;

        // try triplet
        if (counts[tile] >= 3)
        {
            counts[tile] -= 3;

            result.Add(new Meld(
                MeldType.Triplet,
                new List<MahjongTileRecord> { tile, tile, tile }
            ));

            if (CanFormMelds(counts, result, meldsNeeded))
                return true;

            result.RemoveAt(result.Count - 1);
            counts[tile] += 3;
        }

        // try sequence
        if (tile is Suited s && s.Number <= 7)
        {
            var t2 = new Suited(s.Suit, s.Number + 1);
            var t3 = new Suited(s.Suit, s.Number + 2);

            if (counts.TryGetValue(t2, out var c2) && c2 > 0 &&
                counts.TryGetValue(t3, out var c3) && c3 > 0)
            {
                counts[tile]--;
                counts[t2]--;
                counts[t3]--;

                result.Add(new Meld(
                    MeldType.Sequence,
                    new List<MahjongTileRecord> { tile, t2, t3 }
                ));

                if (CanFormMelds(counts, result, meldsNeeded))
                    return true;

                result.RemoveAt(result.Count - 1);

                counts[tile]++;
                counts[t2]++;
                counts[t3]++;
            }
        }

        return false;
    }
    
    // see if the current hand wins, either before or after a draw
    // return a solution if the hand wins
    public static List<Meld>? FindWinningHand(TileHandData hand) 
    {
        return FindWinningHand(hand.ConcealedTiles, hand.Melds);
    }

    public static List<Meld>? FindWinningHand(IReadOnlyList<MahjongTileRecord> concealed, IReadOnlyList<Meld> melds)
    {
        int meldsNeeded = 4 - melds.Count(m => m.Type != MeldType.Pair);

        // concealed should be 14 - 3 * revealed, if we just drew a tile
        // or 13 - 3 * revealed, if waiting for turn
        // winning requires needed*3 + 2 concealed tiles
        if (concealed.Count != meldsNeeded*3 + 2) return null;
        
        // convert to dictionary of counts of all tile types
        var counts = TileListToCounts(concealed);
        
        foreach (var (tile, count) in counts.ToList())
        {
            if (count < 2) continue;

            // try using this as the pair
            counts[tile] -= 2; // remove from the list
            
            var solution = melds.ToList();
            solution.Add(new Meld(
                MeldType.Pair, 
                new List<MahjongTileRecord>{ tile, tile }
            ));
            
            if (CanFormMelds(counts, solution, meldsNeeded))
                return solution;
            
            counts[tile] += 2; // backtrack if the pair doesn't work
        }
        
        return null;
    }

    // heuristic partial solver
    private static readonly int CompleteMeldScore = 2;
    private static readonly int PartialMeldScore = 1;
    private static readonly int PairScore = 1;

    public static int ScoreConcealedTiles(IReadOnlyList<MahjongTileRecord> tiles)
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
            bestScore = System.Math.Max(best, candidate);
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

    private static int BestPartialScoreSearch(
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
        if (tile is Suited s && s.Number <= 7)
        {
            var t2 = new Suited(s.Suit, s.Number + 1);
            var t3 = new Suited(s.Suit, s.Number + 2);
            if (counts.GetValueOrDefault(t2) > 0 && counts.GetValueOrDefault(t3) > 0)
            {
                counts[tile]--; 
                counts[t2]--; 
                counts[t3]--;
                
                BestPartialScoreSearch(counts, meldsNeeded, complete Count+ 1, partialCount, ref bestCompleteCount, ref bestPartialCount);

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
        if (tile is Suited s)
        {
            // 2 sided sequence members
            TryPartialSequence(counts, meldsNeeded, completeCount, partialCount, tile, new Suited(s.Suit, s.Number + 1), ref bestCompleteCount, ref bestPartialCount);

            // middle sequence member missing
            TryPartialSequence(counts, meldsNeeded, completeCount, partialCount, tile, new Suited(s.Suit, s.Number + 2), ref bestCompleteCount, ref bestPartialCount);
        }
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
