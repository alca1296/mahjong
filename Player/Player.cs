using System.Collections.Generic;

namespace Mahjong;

public class Player
{
    // The "Brain" of the player (can be AI or Human)
    private readonly IPlayerDecisionMaker _decisionMaker;

    // The state of the player
    private readonly List<MahjongTileRecord> _hand = new();

    // Read-only access to the hand so external classes can't bypass our logic
    public IReadOnlyList<MahjongTileRecord> Hand => _hand.AsReadOnly();

    public Player(IPlayerDecisionMaker decisionMaker)
    {
        _decisionMaker = decisionMaker;
    }

    public void ReceiveTile(MahjongTileRecord tile)
    {
        _hand.Add(tile);
    }

    public void YourTurn(MahjongTileRecord lastPlayedTile)
    {
        var decision = _decisionMaker.YourTurnDecision(lastPlayedTile);

        //based on decision either draw card or pick up the discard. need to write gamemanager class to hook in there

        //now discard
        var tileToDiscard = _decisionMaker.YourTurnDiscardDecision(_hand);

        _hand.Remove(tileToDiscard);

        //add it to discard pile
    }

    public void OtherTurn(MahjongTileRecord lastPlayedTile)
    {
        var decision = _decisionMaker.OtherTurnDecision(lastPlayedTile);

        if (decision == StealOrPass.Steal)
        {
            YourTurn(lastPlayedTile);
        }
    }
}
