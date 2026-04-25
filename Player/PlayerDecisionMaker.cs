using System;
using System.Collections.Generic;

namespace Mahjong;

public enum StealOrPass { Steal, Pass }

// decision maker can probably request illegal moves, the game manager should block them
public interface IPlayerDecisionMaker
{
    public abstract StealOrPass DecideStealOrPass(TileHandData hand, MahjongTileRecord lastDiscard);
    public abstract MahjongTileRecord DecideDiscard(TileHandData hand);
}
