using System.Collections.Generic;
using System.Threading.Tasks;

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

	public async Task<StealDecision> DecideStealOrPass(MahjongTileRecord lastDiscard, bool isNextPlayer) => await _decisionMaker.DecideStealOrPass(Hand, lastDiscard, isNextPlayer);

	public Task<MahjongTileRecord> DecideDiscard() => _decisionMaker.DecideDiscard(Hand);

	public void notifyTileClicked(MahjongTileRecord tile)
	{
		_decisionMaker.NotifyTileClicked(tile);
	}
}

public class PlayerFactory
{
	public static Player createGreedyBot()
	{
		return new Player(DecisionMakerFactory.newGreedyBotStrategy());
	}

	public static Player createDumbBot()
	{
		return new Player(DecisionMakerFactory.newDumbBotStrategy());
	}

	public static Player createHumanPlayer()
	{
		return new Player(DecisionMakerFactory.newHumanPlayerStrategy());
	}
}
