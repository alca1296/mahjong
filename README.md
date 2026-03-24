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

    Right now, we use polymorphism 

* Dependency Injection

    s


### Meaningful Test Cases
We are implementing unit tests using the `xUnit.net` framework for C#. 
All unit tests are implemented under [MahjongTests](MahjongTests/) in corresponding `*Tests.cs` files.
Classes with unit tests include:
* `MeldValidator`: utility class for determining if
