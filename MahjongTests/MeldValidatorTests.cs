using Mahjong;

namespace MahjongTests;

public class MeldValidatorTests
{
    [Fact]
    public void ValidTriplet()
    {
        var tile = new Wind(WindType.East);
        var meld = new Meld(MeldType.Triplet, new[] { tile, tile, tile });
        Assert.True(MeldValidator.IsValidMeld(meld));
    }
    
    [Fact]
    public void InvalidTripletDifferentTiles()
    {
        var meld = new Meld(MeldType.Triplet, new MahjongTileRecord[]
        {
            new Wind(WindType.East),
            new Wind(WindType.East),
            new Wind(WindType.South)
        });
        Assert.False(MeldValidator.IsValidMeld(meld));
    }

    [Fact]
    public void InvalidTripletWrongCount()
    {
        var tile = new Wind(WindType.East);
        var meld = new Meld(MeldType.Triplet, new[] { tile, tile });
        Assert.False(MeldValidator.IsValidMeld(meld));
    }

    [Fact]
    public void ValidSequence()
    {
        var meld = new Meld(MeldType.Sequence, new MahjongTileRecord[]
        {
            new Suited(Suit.Dot, 1),
            new Suited(Suit.Dot, 2),
            new Suited(Suit.Dot, 3)
        });
        Assert.True(MeldValidator.IsValidMeld(meld));
    }

    [Fact]
    public void ValidSequenceOutOfOrder()
    {
        // should still pass since we sort internally
        var meld = new Meld(MeldType.Sequence, new MahjongTileRecord[]
        {
            new Suited(Suit.Bamboo, 3),
            new Suited(Suit.Bamboo, 1),
            new Suited(Suit.Bamboo, 2)
        });
        Assert.True(MeldValidator.IsValidMeld(meld));
    }

    [Fact]
    public void InvalidSequenceMixedSuits()
    {
        var meld = new Meld(MeldType.Sequence, new MahjongTileRecord[]
        {
            new Suited(Suit.Dot, 1),
            new Suited(Suit.Bamboo, 2),
            new Suited(Suit.Dot, 3)
        });
        Assert.False(MeldValidator.IsValidMeld(meld));
    }

    [Fact]
    public void InvalidSequenceNotConsecutive()
    {
        var meld = new Meld(MeldType.Sequence, new MahjongTileRecord[]
        {
            new Suited(Suit.Dot, 1),
            new Suited(Suit.Dot, 2),
            new Suited(Suit.Dot, 4)
        });
        Assert.False(MeldValidator.IsValidMeld(meld));
    }

    [Fact]
    public void InvalidSequenceContainsHonorTile()
    {
        var meld = new Meld(MeldType.Sequence, new MahjongTileRecord[]
        {
            new Suited(Suit.Dot, 1),
            new Suited(Suit.Dot, 2),
            new Wind(WindType.East)
        });
        Assert.False(MeldValidator.IsValidMeld(meld));
    }

    [Fact]
    public void ValidPair()
    {
        var tile = new Dragon(DragonColor.Red);
        var meld = new Meld(MeldType.Pair, new[] { tile, tile });
        Assert.True(MeldValidator.IsValidMeld(meld));
    }

    [Fact]
    public void InvalidPairDifferentTiles()
    {
        var meld = new Meld(MeldType.Pair, new MahjongTileRecord[]
        {
            new Dragon(DragonColor.Red),
            new Dragon(DragonColor.Green)
        });
        Assert.False(MeldValidator.IsValidMeld(meld));
    }

    [Fact]
    public void InvalidPairWrongCount()
    {
        var tile = new Dragon(DragonColor.Red);
        var meld = new Meld(MeldType.Pair, new[] { tile, tile, tile });
        Assert.False(MeldValidator.IsValidMeld(meld));
    }

    [Fact]
    public void ValidPartialTriplet()
    {
        var tile = new Wind(WindType.North);
        Assert.Equal(PartialMeldResult.PartialTriplet, MeldValidator.CanFormPartialMeld(tile, tile));
    }

    [Fact]
    public void ValidPartialSequenceConsecutive()
    {
        Assert.Equal(PartialMeldResult.PartialSequence,
            MeldValidator.CanFormPartialMeld(new Suited(Suit.Dot, 3), new Suited(Suit.Dot, 4)));
    }

    [Fact]
    public void ValidPartialSequenceGap()
    {
        Assert.Equal(PartialMeldResult.PartialSequence,
            MeldValidator.CanFormPartialMeld(new Suited(Suit.Bamboo, 3), new Suited(Suit.Bamboo, 5)));
    }

    [Fact]
    public void InvalidPartialSequenceNotConsecutive()
    {
        Assert.Equal(PartialMeldResult.NoMeld,
            MeldValidator.CanFormPartialMeld(new Suited(Suit.Dot, 1), new Suited(Suit.Dot, 4)));
    }

    [Fact]
    public void InvalidPartialMeldDifferentSuits()
    {
        Assert.Equal(PartialMeldResult.NoMeld,
            MeldValidator.CanFormPartialMeld(new Suited(Suit.Dot, 1), new Suited(Suit.Bamboo, 2)));
    }

    [Fact]
    public void InvalidPartialTripletDifferentHonorTiles()
    {
        Assert.Equal(PartialMeldResult.NoMeld,
            MeldValidator.CanFormPartialMeld(new Wind(WindType.East), new Wind(WindType.South)));
    }

    [Fact]
    public void InvalidPartialMeldHonorTile()
    {
        Assert.Equal(PartialMeldResult.NoMeld,
            MeldValidator.CanFormPartialMeld(new Suited(Suit.Dot, 1), new Wind(WindType.East)));
    }
}
