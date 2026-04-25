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
    
    private static bool CanFormMelds(Dictionary<MahjongTileRecord, int> counts, List<Meld> result)
    {
        /*
        recursive meld solver. pick first tile you can find with nonzero count in the 
        counts dictionary. for each meld type, try forming a meld with the chosen tile. 
        if a meld can be formed, remove that triplet from the counts and recursively 
        find more melds in whatever remains. so effectively does a full search over meld types
        to see what works.
        */
        
        // base case
        if (counts.All(pair => pair.Value == 0))
            return true;

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

            if (CanFormMelds(counts, result))
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

                if (CanFormMelds(counts, result))
                    return true;

                result.RemoveAt(result.Count - 1);

                counts[tile]++;
                counts[t2]++;
                counts[t3]++;
            }
        }

        return false;
    }
    
    public static List<Meld>? FindWinningHand(IReadOnlyList<MahjongTileRecord> tiles) 
    {
        if (tiles.Count != 14) return null;
        
        // convert to dictionary of counts of all tile types
        var counts = TileListToCounts(tiles);
        
        foreach (var pair in counts.ToList())
        {
            // check if can form a pair (more than 1)
            if (pair.Value >= 2) {
                // try using this as the pair
                counts[pair.Key] -= 2; // remove from the list
                
                var solution = new List<Meld>{
                    new Meld(MeldType.Pair, new List<MahjongTileRecord>{
                        pair.Key, pair.Key      
                    })
                };
                
                if (CanFormMelds(counts, solution))
                    return solution;
                    
                counts[pair.Key] += 2; // backtrack if the pair doesn't work
            }
        }
        
        return null;
    }
}
