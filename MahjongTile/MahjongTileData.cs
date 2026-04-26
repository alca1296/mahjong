using System.Collections.Generic;
using System;

namespace Mahjong;

public abstract record MahjongTileRecord
{
	//sealed because otherwise the children override with their default ToString method
	public sealed override string ToString()
	{
		return this switch
		{
			Suited s => $"{s.Number} of {s.Suit}",
			Wind w => $"{w.Direction} Wind",
			Dragon d => $"{d.Color} Dragon",
			_ => "Unknown"
		};
	}
}

public record Suited(Suit Suit, int Number) : MahjongTileRecord;
public record Wind(WindType Direction) : MahjongTileRecord;
public record Dragon(DragonColor Color) : MahjongTileRecord;

public enum Suit { Dot, Bamboo, Character }
public enum WindType { East, South, West, North }
public enum DragonColor { Red, Green, White }
//No flower tiles yet. Those only matter for scoring so if we add scoring we can add them later.

public static class TileFactory
{
	public static List<MahjongTileRecord> CreateFullSet()
	{
		var tiles = new List<MahjongTileRecord>();

		foreach (var suit in Enum.GetValues<Suit>())
		{
			for (int num = 1; num <= 9; num++)
			{
				for (int i = 0; i < 4; i++)
					tiles.Add(new Suited(suit, num));
			}
		}

		foreach (var direction in Enum.GetValues<WindType>())
		{
			for (int i = 0; i < 4; i++)
				tiles.Add(new Wind(direction));
		}

		foreach (var color in Enum.GetValues<DragonColor>())
		{
			for (int i = 0; i < 4; i++)
				tiles.Add(new Dragon(color));
		}

		// If we end up doing points, also add flowers to the deck

		return tiles;
	}
}
