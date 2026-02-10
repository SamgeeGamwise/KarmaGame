# 2D Engine Beginner Guide

This guide explains common game-dev terms in plain language.
It is written for both developers and non-developers.

## The Big Picture
- `Engine` = reusable tools and rules (camera, input, rendering, scenes).
- `Game` = your specific content and behavior (characters, story, maps, menus).

Think of it like this:
- Engine is the kitchen.
- Game is the meal you cook in it.

## Quick Definitions
### Actor
- A thing in the game world that can do something.
- Examples: player, NPC, tree, animal, dropped item.

### Content
- Art/audio/data files used by the game.
- Examples: sprites, tilesets, fonts, music, sound effects, dialog text, item data.

### Scene
- A 'place/context' in the game.
- Examples: main menu, farm map, house interior, pause menu.

### System
- Logic that manages a type of behavior across many actors.
- Examples: collision system, inventory system, dialogue system, save/load system.

### UI (User Interface)
- Anything the player reads or clicks.
- Examples: health bar, inventory screen, dialogue box, settings menu, tooltips.

## Where Things Usually Go
Recommended game-side folders:

- `Game/Scenes`
  - Scene roots and flow logic.
  - Example: `MainMenuScene`, `FarmScene`.
- `Game/Actors`
  - Player, NPC, items, interactables.
  - Example: `PlayerActor`, `ChickenActor`, `ChestActor`.
- `Game/Systems`
  - Shared mechanics used by many actors/scenes.
  - Example: `InteractionSystem`, `InventorySystem`, `TimeOfDaySystem`.
- `Game/UI`
  - Menu screens, HUD, dialog windows.
  - Example: `InventoryPanel`, `DialogBox`.
- `Game/Content`
  - Textures, tilemaps, audio, fonts, data.
  - Example: `Tilesets/FarmTiles.png`, `Data/Items.json`.

## Node-Based vs State Machine (Important)
You will see both patterns in this engine.

### Node-Based (Scene Tree)
- Build game objects by attaching nodes in a parent/child tree.
- Good for things that exist together at the same time.
- Example:
  - `FarmScene` (root)
  - `TileMapNode`
  - `PlayerActor`
  - `NpcActor`
  - `HudNode`

Why use it:
- Easy composition (add/remove children).
- Natural for 2D world objects.
- Great for reuse and visual organization.

### State Machine (Flow Control)
- Represents 'which mode/screen are we in right now?'
- Good for high-level flow and exclusive states.
- Example states:
  - `Boot`
  - `MainMenu`
  - `Playing`
  - `Paused`
  - `GameOver`

Why use it:
- Keeps screen flow explicit and predictable.
- Prevents 'all screens active at once' confusion.

### Simple Rule
- Use `Node` tree for world composition.
- Use `State` stack for top-level game flow.

## Common 2D Terms You'll Hear
### Sprite
- A 2D image drawn in the game.

### Tilemap
- A grid of small images (tiles) to build maps quickly.

### Tileset
- The source image containing many tiles.

### Camera
- Controls what part of the world is visible.

### Collision
- Detects when objects overlap or touch.

### Input Action
- Named player intent (like `move_left`, `interact`, `ui_accept`) instead of raw key codes.

## Example: Stardew-Style Feature Breakdown
Feature: Talking to an NPC

- Scene:
  - `FarmScene` contains player + NPCs.
- Actors:
  - `PlayerActor`, `NpcActor`.
- System:
  - `InteractionSystem` checks if player is close and facing NPC.
- UI:
  - `DialogBox` shows text and choices.
- Content:
  - NPC portrait image, dialogue text data, voice blip sound.

## Non-Developer Friendly Workflow
### Artists
- Create sprites/tilesets/UI art in `Content`.
- Use agreed names and dimensions.

### Designers
- Tune values in data files (JSON/XML) like item prices, crop growth time, NPC schedules.

### Writers
- Edit dialogue text files.
- Keep IDs stable (for quest/dialog references).

### Programmers
- Implement actor behavior, systems, and scene flow.
- Keep engine code generic; keep game rules in game project.

## 'What Should I Create?' Cheat Sheet
- 'New map area' -> new `Scene` + tilemap content.
- 'New character' -> new `Actor` + art/content.
- 'Feature used by many objects' -> `System`.
- 'New menu panel' -> `UI`.
- 'Reusable base capability' -> `Engine` code.

## Practical Tips
- Name input by intent (`interact`) not key (`EKey`).
- Keep systems focused (one clear job each).
- Keep actors thin: behavior can call into systems.
- Prefer data-driven tuning for non-dev collaboration.
- Write short docs/comments when adding new gameplay concepts.

## Final Summary
- Actors are 'who/what.'
- Systems are 'how rules run.'
- Scenes are 'where/when context.'
- UI is 'what player sees/clicks.'
- Content is 'assets/data.'
- Node tree builds the world.
- State machine controls game flow.
