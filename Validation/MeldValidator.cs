using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong;

public enum PartialMeldResult { NoMeld, PartialSequence, PartialTriplet };

public partial class MeldValidator
{   
    // is this a valid triplet or sequence?
    public static bool IsValidMeld(Meld meld) => meld.Type switch
    {
        MeldType.Triplet => IsValidTriplet(meld.Tiles),
        MeldType.Sequence => IsValidSequence(meld.Tiles),
        MeldType.Pair => IsValidPair(meld.Tiles),
        _ => false
    };
    
    private static bool IsValidTriplet(IReadOnlyList<MahjongTileRecord> tiles)
    {
        if (tiles.Count != 3) return false;
        return tiles.All(t => t == tiles[0]);
    }
    
    private static bool IsValidSequence(IReadOnlyList<MahjongTileRecord> tiles)
    {
        if (tiles.Count != 3) return false;
        
        // all must be suited
        if (tiles.Any(t => t is not Suited)) return false;
        
        var suited = tiles.Cast<Suited>().ToList();
        
        // same suit
        if (suited.Select(t => t.Suit).Distinct().Count() != 1) return false;
        
        // consecutive numbers
        var numbers = suited.Select(t => t.Number).OrderBy(n => n).ToList();
        return numbers[1] == numbers[0]+1 && numbers[2] == numbers[1]+1;
    }
    
    // is this a valid pair (mahjong is 4 sets of 3 and one set of 2)
    private static bool IsValidPair(IReadOnlyList<MahjongTileRecord> tiles)
    {
        if (tiles.Count != 2) return false;
        return tiles[0] == tiles[1];
    }
    
    // part of sequence of 2 or set of 2?
    public static PartialMeldResult CanFormPartialMeld(MahjongTileRecord a, MahjongTileRecord b)
    {
        if (a == b)
            return PartialMeldResult.PartialTriplet;
            
        if (a is Suited sa && b is Suited sb && sa.Suit == sb.Suit)
        {
            var diff = Math.Abs(sa.Number - sb.Number);
            if (diff == 1 || diff == 2)
                return PartialMeldResult.PartialSequence;
        }
            
        return PartialMeldResult.NoMeld;
    }
    
    // can the player steal this / can the player form a meld from this + stuff in their hand?
    public static bool CanSteal(TileHandData hand, MahjongTileRecord discard, bool isNextPlayer)
    {
        throw new NotImplementedException("not implemented yet");
    }
}
