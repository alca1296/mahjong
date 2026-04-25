using System.Collections.Generic;

namespace Mahjong;

public class Player
{
    // The "Brain" of the player (can be AI or Human)
    private readonly IPlayerDecisionMaker _decisionMaker;

    // The state of the player
    private readonly TileHandData _hand = new();
    public TileHandData Hand => _hand;

    public Player(IPlayerDecisionMaker decisionMaker)
    {
        _decisionMaker = decisionMaker;
    }

    public void ReceiveTile(MahjongTileRecord tile) => _hand.AddConcealed(tile);
    public bool Discard(MahjongTileRecord tile) => _hand.Discard(tile);

    public StealOrPass DecideStealOrPass(MahjongTileRecord lastDiscard) => _decisionMaker.DecideStealOrPass(Hand, lastDiscard);

    public MahjongTileRecord DecideDiscard() => _decisionMaker.DecideDiscard(Hand);
}
