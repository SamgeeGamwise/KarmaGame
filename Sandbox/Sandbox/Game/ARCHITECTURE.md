# Sandbox Architecture

This sandbox is organized to support a full game loop while features are still placeholder-quality.

## Runtime anchors
- `Game/Scene/SandboxScene.cs`: composition root for world update/draw.
- `Game/Scene/MapNode.cs`: map runtime integration and y-sorted overhang tile drawables.
- `Game/Scene/Npc/*`: NPC definitions, spawning, and interaction targets.
- `Game/Scene/UI/*`: HUD, menu overlay, dialogue box, and notifications.
- `Game/Scene/Progression/*`: money, level, inventory, skills, lore, day progression.

## Planned expansion folders
- `Game/Systems/*` folders are placeholders for dedicated services as features mature.