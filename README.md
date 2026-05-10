This project involves converting a base Match 3 Unity project into a Tile Match mechanic, along with adding several new features and UI enhancements. Below are the key areas and components that have been worked o Key Features & Implementations

 1. Tile Match Mechanic
- Gameplay Conversion: Finalized the conversion from traditional Match 3 to a Tile Match mechanic.

2. Game Modes & Custom Buttons
- Autoplay Mode: Implemented logic to ensure the Autoplay feature initiates correctly upon request.
- Auto-Lose Mode: Integrated custom logic for forcefully ending the game in a lose state.
- Time Attack / Timer Mode: Connected the Timer mode selection buttons to the `GameManager` and `LevelTime` components, setting up countdown logic that triggers a Game Over when time runs out.

 3. UI & State Management
- Win / Lose Panels: Fixed logic to ensure UI components (Win and Game Over panels) are triggered and displayed correctly based on the active level conditions.
- Menu Integration: Hooked up `UIMainManager` and `UIPanelMain` to properly handle different game mode initializations.
