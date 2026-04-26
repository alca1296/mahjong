using Mahjong;

namespace MahjongTests;

public class TileHandDataTests
{   
	private static MahjongTileRecord ArbitraryTile => new Dragon(DragonColor.Red);
	
	[Fact]
	public void AddConcealed()
	{
		var hand = new TileHandData();
		var tile = ArbitraryTile;
		
		hand.AddConcealed(tile);
		Assert.Contains(tile, hand.ConcealedTiles);
	}
	
	[Fact]
	public void AddMeld()
	{
		var hand = new TileHandData();
		var meld = new Meld(MeldType.Triplet, new[] {ArbitraryTile, ArbitraryTile, ArbitraryTile});
		
		hand.AddMeld(meld);
		Assert.Contains(meld, hand.Melds);
	}
}

public class DiscardPileTests
{
	private static MahjongTileRecord ArbitraryTile => new Dragon(DragonColor.Red);
	
	[Fact]
	public void EndReturnsNullForEmptyPile()
	{
		var pile = new DiscardPile();
		Assert.Null(pile.End);
		Assert.Null(pile.TakeEnd());
	}
	
	[Fact]
	public void EndReturnsLastAddedTile()
	{
		var pile = new DiscardPile();
		pile.Add(new Dragon(DragonColor.White));
		pile.Add(ArbitraryTile);
		Assert.Equal(ArbitraryTile, pile.End);
		var tile = pile.TakeEnd();
		Assert.Equal(ArbitraryTile, tile);
		Assert.False(pile.Contains(tile));
	}
}
