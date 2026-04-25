using System;
using System.Collections.Generic;
using System.Linq;

using Mahjong;

namespace MahjongTests;

public class DecisionMakerTests
{
    // helpers
    private static TileHandData MakeHand(params MahjongTileRecord[] tiles)
    {
        var hand = new TileHandData();
        foreach (var t in tiles)
            hand.AddConcealed(t);
        return hand;
    }   

     // shorter aliases for hand building
    private static Suited D(int n) => new(Suit.Dot, n);
    private static Suited B(int n) => new(Suit.Bamboo, n);
    private static Suited C(int n) => new(Suit.Character, n);
    private static Wind W(WindType t) => new(t);
    private static Dragon D(DragonColor c) => new(c);
    
    // ACTUAL TESTS

    [Fact]
    public void GreedyBotDiscardIsolated()
    {
        var hand = MakeHand(
            D(1), D(2), D(3), // complete
            D(4), D(5), // partial
            B(9) // <- junk
        );

        var bot = new GreedyBot();
        var discard = bot.DecideDiscard(hand);

        Assert.Equal(B(9), discard);
    }

    [Fact]
    public void GreedyBotBreaksPartial()
    {
        var hand = MakeHand(
            D(1), D(2), D(3), // complete
            B(4), B(5), // partial
            C(1), C(2)  // partial
        );

        var bot = new GreedyBot();
        var discard = bot.DecideDiscard(hand);

        Assert.Contains(discard, new[] { B(4), B(5), C(1), C(2) });
    }

    [Fact]
    public void GreedyBotStealsToCompleteSequence()
    {
        var hand = MakeHand(
            D(1), D(2), 
            B(1) // junk to discard
        );

        var bot = new GreedyBot();
        var decision = bot.DecideStealOrPass(hand, D(3), true);

        Assert.IsType<Steal>(decision);
        
        if (decision is Steal s) {
            foreach (var tile in new[] { D(1), D(2), D(3) }) {
                Assert.Contains(tile, s.Meld.Tiles);
            }
        }
    }

    [Fact]
    public void GreedyBotStealsToCompleteTriplet()
    {
        var hand = MakeHand(
            D(1), D(1),
            B(1) // junk to discard
        );

        var bot = new GreedyBot();
        var decision = bot.DecideStealOrPass(hand, D(1), false);

        Assert.IsType<Steal>(decision);
        if (decision is Steal s) {
            foreach (var tile in new[] { D(1), D(1), D(1) }) {
                Assert.Contains(tile, s.Meld.Tiles);
            }
        }
    }

    [Fact]
    public void GreedyBotDoesNotStealUselessTile()
    {
        var hand = MakeHand(
            D(1), D(2), D(3)
        );

        var bot = new GreedyBot();
        var decision = bot.DecideStealOrPass(hand, B(1), true);
        
        Assert.IsType<Pass>(decision);
    }

    
}