using System.Collections.Generic;
using System.Linq;

namespace Mahjong;

public enum MeldType 
{
    Sequence,
    Triplet,
    Pair
}

public record Meld(MeldType Type, IReadOnlyList<MahjongTileRecord> Tiles);

public class TileCollection
{
    private readonly List<MahjongTileRecord> _tiles = new();
    public IReadOnlyList<MahjongTileRecord> Tiles => _tiles;
    
    public void Add(MahjongTileRecord tile) => _tiles.Add(tile);
    public bool Remove(MahjongTileRecord tile) => _tiles.Remove(tile);
    
    public int Count(MahjongTileRecord tile) => _tiles.Count(t => t == tile);
    public bool Contains(MahjongTileRecord tile) => _tiles.Contains(tile);
    
    // can maybe extend to check for possible triplets/sequences/etc.
}

public class TileHandData
{
    private readonly TileCollection _concealedHand = new();
    public IReadOnlyList<MahjongTileRecord> ConcealedTiles => _concealedHand.Tiles;
    
    private readonly List<Meld> _melds = new();
    public IReadOnlyList<Meld> Melds => _melds;
    
    public void AddConcealed(MahjongTileRecord tile) => _concealedHand.Add(tile);
    public void AddMeld(Meld meld) => _melds.Add(meld);
}

public class DiscardPile : TileCollection
{
    public MahjongTileRecord? End => Tiles.LastOrDefault();
    public MahjongTileRecord? TakeEnd()
    {
        if (!Tiles.Any()) return null;
        
        var top = Tiles.Last();
        Remove(top);
        return top;
    }
}
