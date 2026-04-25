using System;
using System.Collections.Generic;

namespace Mahjong;

public enum DrawOrSteal { Draw, Steal }
public enum StealOrPass { Steal, Pass }

public interface IPlayerDecisionMaker
{
    public abstract DrawOrSteal YourTurnDecision(MahjongTileRecord lastPlayedTile);
    public abstract StealOrPass OtherTurnDecision(MahjongTileRecord lastPlayedTile);

    public abstract MahjongTileRecord YourTurnDiscardDecision(List<MahjongTileRecord> yourHand);
}

//This AI player tries to make smart decisions about what to draw or discard
// After implementing all of the commented algorithm improvements, we can make a hard AI player that also inspects the discard piles and
// factors the "extinction" of certain tiles into its decision making.
// An even better player would also use that to predict enemy players likely hands and then use that to predict what they will discard, and try to "wait" for those tiles
// But that's really overkill and out of the scope of this assignment
public class AIPlayer : IPlayerDecisionMaker
{
    public DrawOrSteal YourTurnDecision(MahjongTileRecord lastPlayedTile)
    {
        // Always steal because we want to be making as many melds as we can
        // TODO improve this by checking if completing a meld with the new tile will break an existing meld
        // In that case we don't want to steal because that would be needlessly revealing information
        return DrawOrSteal.Steal;
    }

    public StealOrPass OtherTurnDecision(MahjongTileRecord lastPlayedTile)
    {
        // TODO update with same logic as YourTurnDecision needs
        return StealOrPass.Steal;
    }

    public MahjongTileRecord YourTurnDiscardDecision(List<MahjongTileRecord> yourHand)
    {
        //Discard a random tile for now. This needs to be updated to prioritize tiles in the following order
        // 1. tiles not part of any sets or sequences
        // 2. tiles part of sequences of 2
        // 3. tiles part of sets of 2
        // It shouldn't get any farther than this as the absense of 1,2, or 3 implies a winning hand
        int randomIndex = Random.Shared.Next(yourHand.Count);
        return yourHand[randomIndex];
    }
}

//The bad AI player never steals and discards at random.
public class BadAIPlayer : IPlayerDecisionMaker
{
    public DrawOrSteal YourTurnDecision(MahjongTileRecord lastPlayedTile)
    {
        return DrawOrSteal.Draw;
    }

    public StealOrPass OtherTurnDecision(MahjongTileRecord lastPlayedTile)
    {
        return StealOrPass.Pass;
    }

    public MahjongTileRecord YourTurnDiscardDecision(List<MahjongTileRecord> yourHand)
    {
        int randomIndex = Random.Shared.Next(yourHand.Count);
        return yourHand[randomIndex];
    }
}
