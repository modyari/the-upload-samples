Code samples from The Upload

Includes a few extracts of the code for demonstration

# Where To Start?
Please read the code philosophy document if you want to understand the philosophy of the code better: [Click Here](https://docs.google.com/document/d/1fct1Ug9rISiyJreD6xXWBfIUV2DGquHOvk1N3vM2MH8/edit?usp=sharing)

## Note
The code in the CodeSamples is not in its natural directory location as placed in the original project for convenience.
The architecture here was an experiment in software design using Unity. The choices were made to fit the specific challenges of the project. 

## [GameSystem](https://github.com/modyari/the-upload-samples/blob/main/CodeSamples/GameSystem.cs)
Base class for a GameSystem. A GameSystem exists in a scene on its own that gets loaded additively from the launch scene/GameController. Appropriate arguments are passed on initialization and then from then to the subsystems.
They represent the different segments of the game (gameplay, hacking mini-game, cutscenes, main menu, etc) and contains the generic setups and logic this system will needs.

## [Subsystem](https://github.com/modyari/the-upload-samples/blob/main/CodeSamples/Subsystem.cs)
Base class for a subsystem. A GameSystem contains a stack of subsystems that represent the state of the GameSystem (default, dialogue, pause, etc) and updates only the one on top. That way the logic of the current state is the only one updated.

## [HackingGameSystem](https://github.com/modyari/the-upload-samples/blob/main/CodeSamples/HackingGameSystem.cs)
Example of a game system that represents the hacking mini-game. Beside getting passed the Services in the argumens (input, sound, and data services), it also takes a HackRoom address and loads it via addressables, which contains the specific level data this hacking segment contains.
The level of the HackRoom are then processed by HackFlowSubsystem.  

## [HackFlowSubsystem](https://github.com/modyari/the-upload-samples/blob/main/CodeSamples/HackFlowSubsystem.cs)
The subsystem the processes the hacking mini-game levels in the HackRoom.
A HackRoom contains several levels, when a level ends successfully, the next level is loaded, which may or may not play a piece of dialogue
When all levels are done, the GameSystem (HackingGameSystem) is informed and finishes. The game then loads the next appropriate GameSystem. 

## [GameController](https://github.com/modyari/the-upload-samples/blob/main/CodeSamples/GameController.cs)
A game controller exists in the launch scene and loads the appropriate GameSystem scenes additively / initializes them with arguments.
The arguments are gathered from the launch scene (all services and data) which allows for testing by changing them in the editor. 
It was planned to undergo further refactoring to create different types of debug controllers and achieve cleaner code.
 
## [UIFadable](https://github.com/modyari/the-upload-samples/blob/main/CodeSamples/UIFadable.cs)
A parent class for any fadable UI that you can show/hide with animation

## [GameConfig](https://github.com/modyari/the-upload-samples/blob/main/CodeSamples/GameConfig.cs)
Scriptable object containing configurable internal game configs like scene names and UI/animation settings

## [GameCoroutine](https://github.com/modyari/the-upload-samples/blob/main/CodeSamples/GameCoroutine.cs)
A custom coroutine processor that allows pausing, 
Simply extends MonoBehaviour so you can use it by calling this.StartGameCoroutine(..) from any MonoBehaviour
When the game is paused, ShouldPauseAll is set to true and processing is paused

## [GroupAssetLoader](https://github.com/modyari/the-upload-samples/blob/main/CodeSamples/GroupAssetLoader.cs)
Class for loading then reporting progress and result for a group of addressables 
