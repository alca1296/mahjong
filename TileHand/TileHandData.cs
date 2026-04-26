using System;
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
    protected readonly List<MahjongTileRecord> _tiles = new();
    public IReadOnlyList<MahjongTileRecord> Tiles => _tiles;

    public void Add(MahjongTileRecord tile) => _tiles.Add(tile);
    public bool Remove(MahjongTileRecord tile) => _tiles.Remove(tile);

    public int Count(MahjongTileRecord tile) => _tiles.Count(t => t == tile);
    public bool Contains(MahjongTileRecord tile) => _tiles.Contains(tile);

    public bool Empty() => _tiles.Count() == 0;

    // can maybe extend to check for possible triplets/sequences/etc.
}

public class TileHandData
{
    private readonly TileCollection _concealedHand = new();
    public IReadOnlyList<MahjongTileRecord> ConcealedTiles => _concealedHand.Tiles;

    private readonly List<Meld> _melds = new();
    public IReadOnlyList<Meld> Melds => _melds;

    public void AddConcealed(MahjongTileRecord tile) => _concealedHand.Add(tile);
    public bool Discard(MahjongTileRecord tile) => _concealedHand.Remove(tile);
    public void AddMeld(Meld meld) => _melds.Add(meld);

    public override string ToString()
    {
        string concealed = ConcealedTiles.Count > 0
            ? string.Join(" ", ConcealedTiles)
            : "No Tiles";

        string melds = _melds.Count > 0
            ? string.Join(" ", _melds.Select(m => $"({string.Join(" ", m.Tiles)})"))
            : "No Melds";

        return $"Hand: {concealed} | Melds: {melds}";
    }
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

public class Deck : TileCollection
{
    private int _drawIndex;

    public Deck(IEnumerable<MahjongTileRecord> tiles)
    {
        _tiles.AddRange(tiles);
        Shuffle();
        _drawIndex = 0;
    }

    public void Shuffle()
    {
        var rng = new Random();
        for (int i = _tiles.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (_tiles[i], _tiles[j]) = (_tiles[j], _tiles[i]);
        }
    }

    public MahjongTileRecord? Draw()
    {
        if (_drawIndex >= _tiles.Count)
            return null;

        return _tiles[_drawIndex++];
    }
}
