# Mahjong
## Mid-Project Review

### 3 Design Patterns
The Player class uses the strategy pattern. The class controls the players hand and is responsible for executing the players turns.
It hands off the actual decision making to an internal PlayerDecisionMaker. The decision maker is responsible only for choices. What to discard, to steal or to pass, etc.
That way AI and interactive players can be created without having to know internal details about how the player, hand, or game is structured.

The Player class will use the push observer pattern. We will have a GameManager class or something similar which controls the state of the whole game.
Whenever something happens (new turn, a player discards, a player forms a meld) the GameManager will push the information to the players via one of their relevant methods. "YourTurn", "OtherTurn", etc.

The MahjongTileRecord class uses the factory pattern. We have a factory method which is responsible for creating the full deck of mahjong tiles. That way, if we add or remove tiles no clients have to be updated. Only the factory does. It also means we don't have any duplicate instantiation code anywhere.

### Foundational Classes

We have a number of foundational classes for this project done. We have a Player and PlayerDecisionMaker, responsible for managing a players hand and making gameplay decisions. A basic AI bot is complete.

We also have classes representing player hands, and tiles, as well as some important meld validation code.

We still need to write a GameManager that's responsible for directing the players and managing the base level game state.

### Core OO Principles
* Polymorphism

    Right now, we use polymorphism in the `PlayerDecisionMaker.cs`. This file has an interface  `IPlayerDecisionMaker` which represents a strategy for the game, with corresponding methods for what to do on the player's current turn and what to do on another player's turn (e.g., steal a discarded tile from someone else on their turn). There is also a method to make a discard decision, i.e. choose which tile to discard. Right now, we have some "AI" players that implement autonomous decisions for their turns. There is a smart AI and a bad AI, and their respective implementations for their turn, other turns, and discard reflects our use of polymorphism in making different strategies.

* Dependency Injection

    Right now, we use DI for some of our UI components. For instance, `MahjongTile` and `TileHand` are classes that extend UI classes from Godot. We currently use setter injection to bind them to classes implementing game logic such as `MahjongTileRecord` and `TileHandData`, and updates to the visualization are made accordingly.


### Meaningful Test Cases
We are implementing unit tests using the `xUnit.net` framework for C#. 
All unit tests are implemented under [MahjongTests](MahjongTests/) in corresponding `*Tests.cs` files.
Classes with unit tests include:
* `MeldValidator` (in `MeldValidatorTests.cs`): Utility class for determining if given melds (triplet of identical tiles or sequence of suited tiles) are actually valid or not according to the rules of Mahjong. The class is tested against many combinations of 3 or 2 tiles to determine if they are valid or invalid combos.
* `TileHandData` (in `TileHandDataTests.cs`): Class for storing a player's concealed tiles and their revealed melds. The tests just check that the hand correctly adds tiles and melds.
* `DiscardPile` (in `TileHandDataTests.cs`): Class for storing a player's discard pile. The tests just check that the pile correctly gets or removes the player's most recently discarded tile.
