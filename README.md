# Mahjong
We implemented Mahjong in Godot in C#. Players can play with bots. 

### 4 Design Patterns
The Player class uses the strategy pattern. The class controls the players hand and is responsible for executing the players turns.
It hands off the actual decision making to an internal PlayerDecisionMaker. The decision maker is responsible only for choices. What to discard, to steal or to pass, etc.
That way AI and interactive players can be created without having to know internal details about how the player, hand, or game is structured. There is a greedy AI that implements a "lookahead" over decisions and picks the choice that maximizes a score heuristic. There is also a "dumb" AI that just discards tiles randomly and never tries to steal tiles.

Many UI classes we implemented use the push observer pattern. We have a GameManager class which controls the state of the whole game.
Whenever something happens (new turn, a player discards, a player forms a meld) the GameManager will push the information to the views of the players and board via one of their relevant methods.

The MahjongTileRecord class uses the factory pattern. We have a factory method which is responsible for creating the full deck of mahjong tiles. That way, if we add or remove tiles no clients have to be updated. Only the factory does. It also means we don't have any duplicate instantiation code anywhere. A factory pattern is also used for all player strategy creation and all 

We use the singleton pattern in `TileTextureLibrary`. This class loads all textures for all tiles on creation of its single instance, to ensure that all images are loaded just once. Then, all other classes that need those images can just query the singleton instance.

We also use the command pattern to an extent in `IPlayerDecisionMaker`. There is a method to decide whether to steal a tile or pass, and this option is implemented a records deriving from a single record. The `GameManager` then interpets these commands from the players to either pass their steal opportunity or attempt to steal the last discarded tile.

### Foundational Classes

We have a Player and PlayerDecisionMaker, responsible for managing a players hand and making gameplay decisions. A basic and complex bot strategy is done. 

We also have classes representing player hands, and tiles, as well as some important meld validation code. There are some other classes for determining if the game has been won by any player after a tile draw. This requires a recursive search over the player's tiles to see if they form a valid combination of melds and a pair. There is a class which implement similar methods but calculate a score heuristic for the bot strategies to try and maximize when evaluating their options. This similarly searches over possible combinations of tiles and picks moves that create melds or partial melds. 

There is a GameManager that is responsible for controlling and handling all game state, as well as pushing status updates to UI elements. This stores players, the tile deck, and iterates through players to run their turns. 

### Core OO Principles
* Polymorphism

    Right now, we use polymorphism in the `PlayerDecisionMaker.cs`. This file has an interface  `IPlayerDecisionMaker` which represents a strategy for the game, with corresponding methods for what to do on the player's current turn and what to do on another player's turn (e.g., steal a discarded tile from someone else on their turn). There is also a method to make a discard decision, i.e. choose which tile to discard. Right now, we have some "AI" players that implement autonomous decisions for their turns. There is a smart AI and a bad AI, and their respective implementations for their turn, other turns, and discard reflects our use of polymorphism in making different strategies.

* Dependency Injection

    Right now, we use DI for some of our UI components. For instance, `MahjongTile` and `TileHand` are classes that extend UI classes from Godot. We currently use setter injection to bind them to classes implementing game logic such as `MahjongTileRecord` and `TileHandData`, and updates to the visualization are made accordingly.


### Meaningful Test Cases
We are implementing unit tests using the `xUnit.net` framework for C#. 
All unit tests are implemented under [MahjongTests](MahjongTests/) in corresponding `*Tests.cs` files. Many of the core game logic classes are verified with test cases: game logic like validating melds is checked, the player strategies are verified under example turns to make logically consistent decisions, data structures like the tile hand are tested to check consistency, and the central win checking algorithm (a complex, recursive DFS) is checked, among other features/classes. To test, you have to cd into the tests directory and run `dotnet test`. 

### AI Disclosure
We used AI to help debug errors and help with development of UI elements.

### Attribution
We got our tile images from [this repository](https://github.com/FluffyStuff/riichi-mahjong-tiles).
