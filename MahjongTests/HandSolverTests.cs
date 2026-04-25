using System;
using System.Collections.Generic;
using System.Linq;

using Mahjong;

namespace MahjongTests;

public class HandSolverTests
{
    // helpers
    private static void AssertValidSolution(IReadOnlyList<MahjongTileRecord> input, List<Meld> solution) {
        var pairs = solution.Where(m => m.Type == MeldType.Pair).ToList();
        var nonPairs = solution.Where(m => m.Type != MeldType.Pair).ToList();

        // only 1 pair, 4 melds
        Assert.Single(pairs);
        Assert.Equal(4, nonPairs.Count);

        foreach (var meld in solution)
            Assert.True(MeldValidator.IsValidMeld(meld),   
                $"invalid meld in solution: {meld.Type} [{string.Join(", ", meld.Tiles)}]");

        // solution tile counts should match input counts
        var inputCounts = Counts(input);
        var solutionTiles = solution.SelectMany(m => m.Tiles).ToList();
        var solutionCounts = Counts(solutionTiles);

        foreach (var (tile, count) in inputCounts) {
            Assert.True(solutionCounts.TryGetValue(tile, out var sc), $"tile {tile} present in input but mnissing from solution");
            Assert.Equal(count, sc);
        }
    }

    private static Dictionary<MahjongTileRecord, int> Counts(IEnumerable<MahjongTileRecord> tiles) {
        var dict = new Dictionary<MahjongTileRecord, int>();
        foreach (var t in tiles) {
            if (!dict.ContainsKey(t)) dict[t] = 0;
            dict[t]++;
        }
        return dict;
    }

    // shorter aliases for hand building
    private static Suited D(int n) => new(Suit.Dot, n);
    private static Suited B(int n) => new(Suit.Bamboo, n);
    private static Suited C(int n) => new(Suit.Character, n);
    private static Wind W(WindType t) => new(t);
    private static Dragon D(DragonColor c) => new(c);
    
    // ACTUAL TESTS
    [Fact]
    public void AllTripletsSimpleHonors()
    {
        // 4 triplets of winds/dragons + pair of red dragons
        var tiles = new List<MahjongTileRecord>
        {
            W(WindType.East),  W(WindType.East),  W(WindType.East),
            W(WindType.South), W(WindType.South), W(WindType.South),
            W(WindType.West),  W(WindType.West),  W(WindType.West),
            W(WindType.North), W(WindType.North), W(WindType.North),
            D(DragonColor.Red), D(DragonColor.Red),
        };
        var result = HandSolver.FindWinningHand(tiles, new List<Meld>());
        Assert.NotNull(result);
        AssertValidSolution(tiles, result!);
    }

    [Fact]
    public void MixedSuitsSequencesAndTriplet()
    {
        var tiles = new List<MahjongTileRecord>
        {
            D(1), D(2), D(3),
            B(4), B(5), B(6),
            C(7), C(8), C(9),
            D(9), D(9), D(9),
            B(1), B(1),
        };
        var result = HandSolver.FindWinningHand(tiles, new List<Meld>());
        Assert.NotNull(result);
        AssertValidSolution(tiles, result!);
    }

    [Fact]
    public void HonorTilesInTripletsWithSuitedPair()
    {
        var tiles = new List<MahjongTileRecord>
        {
            D(DragonColor.Green),  D(DragonColor.Green),  D(DragonColor.Green),
            D(DragonColor.White),  D(DragonColor.White),  D(DragonColor.White),
            W(WindType.East),       W(WindType.East),        W(WindType.East),
            W(WindType.North),      W(WindType.North),       W(WindType.North),
            C(5), C(5),
        };
        var result = HandSolver.FindWinningHand(tiles, new List<Meld>());
        Assert.NotNull(result);
        AssertValidSolution(tiles, result!);
    }

    [Fact]
    public void SequencesEndingAt9FirstTilePickedMightBe8or9()
    {
        // constructed so the counts dict might naturally surface 8 or 9 first.
        var tiles = new List<MahjongTileRecord>
        {
            B(7), B(8), B(9),
            B(7), B(8), B(9),
            B(7), B(8), B(9),
            D(1), D(1), D(1),
            B(5), B(5),
        };
        var result = HandSolver.FindWinningHand(tiles, new List<Meld>());
        Assert.NotNull(result);
        AssertValidSolution(tiles, result!);
    }

    [Fact]
    public void OnlyHighNumberSuitedTilesTripletPath()
    {
        var tiles = new List<MahjongTileRecord>
        {
            D(8), D(8), D(8),
            D(9), D(9), D(9),
            B(8), B(8), B(8),
            B(9), B(9), B(9),
            C(8), C(8),
        };
        var result = HandSolver.FindWinningHand(tiles, new List<Meld>());
        Assert.NotNull(result);
        AssertValidSolution(tiles, result!);
    }

    [Fact]
    public void AmbiguousHandTripletOrSequenceReturnsASolution()
    {
        // triplets or sequences work
        var tiles = new List<MahjongTileRecord>
        {
            D(1), D(1), D(1),
            D(2), D(2), D(2),
            D(3), D(3), D(3),
            B(5), B(6), B(7),
            C(9), C(9),
        };
        var result = HandSolver.FindWinningHand(tiles, new List<Meld>());
        Assert.NotNull(result);
        AssertValidSolution(tiles, result!);
    }

    [Fact]
    public void AmbiguousHandPairChoiceBothValid()
    {
        // both D(1) and D(2) could serve as the pair
        // let the solver decide
        var tiles = new List<MahjongTileRecord>
        {
            D(1), D(1), D(1), D(1),
            D(2), D(2), D(2), D(2),
            D(3), D(3), D(3),
            B(5), B(6), B(7),
        };
        var result = HandSolver.FindWinningHand(tiles, new List<Meld>());
        Assert.NotNull(result);
        AssertValidSolution(tiles, result!);
    }

    [Fact]
    public void EmptyHandReturnsNull()
    {
        Assert.Null(HandSolver.FindWinningHand(new List<MahjongTileRecord>(), new List<Meld>()));
    }

    [Fact]
    public void NoPairReturnsNull()
    {
        // 14 distinct tiles, no pair possible
        var tiles = new List<MahjongTileRecord>
        {
            D(1), D(2), D(3),
            B(1), B(2), B(3),
            C(1), C(2), C(3),
            W(WindType.East), W(WindType.South), W(WindType.West),
            D(DragonColor.Red), D(DragonColor.Green),
        };
        Assert.Null(HandSolver.FindWinningHand(tiles, new List<Meld>()));
    }
}