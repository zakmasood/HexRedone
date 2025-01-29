## [Latest Changes]
# Changelog

## Added
- **Floating Text Functionality**: 
  - Introduced `FloatingTextManager` for displaying contextual information above buildings (e.g., resource type, storage status).
  - Integrated floating text display in `Drill`, `Sawmill`, and `Storage` classes upon mouse hover.

## Fixed
- **Resource Initialization in ResourceManager**: 
  - Changed initial resource count from 100 to 0 to ensure correct resource tracking.
  
## Updated
- **Drill Class Enhancements**: 
  - Added serialized field for floating text to improve UI interaction.
  - Improved collection logic to handle resource management more efficiently.
- **Sawmill Class Enhancements**: 
  - Added hover text display indicating output resource and its status.
- **Storage Class Enhancements**: 
  - Added hover text display for showing current resource type and storage status.
- **BuildingManager Improvements**: 
  - Refactored building placement logic into separate initializer methods for `Drill`, `Storage`, and `Sawmill` to enhance code readability and maintainability.
- **TileClickHandler Improvements**: 
  - Refined mouse click handling to differentiate between building placement, deletion, and resource collection, improving user experience.

## Removed
- **UI Counter Text in Storage**: 
  - Removed the counter text setup in `Storage` as it was deemed unnecessary for the current UI design.
