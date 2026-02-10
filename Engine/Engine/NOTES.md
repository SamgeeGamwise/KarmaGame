# Engine Notes

## Why Two Render Passes?
- `World` pass uses active `Camera2D`.
- `Screen` pass ignores camera (menus, HUD, dialog boxes).

This maps to common 2D game needs and avoids ad-hoc matrix hacks.

## Input Philosophy
- `InputState` is raw polling.
- `InputMap` translates raw input into semantic actions.

Use actions everywhere in gameplay/UI code. It makes rebinding and control variants easier.

## Scene Tree vs State Stack
- Scene tree (`Node`) is the primary model.
- State stack is optional for game flow overlays or full-screen screens.

You can use either or both:
- Scene tree for world and entities.
- State stack for global flow (boot/menu/in-game/pause).

## Tilemaps
- `TileMapNode` focuses on drawing and simple collision tile queries.
- `TileLayer.Collidable` marks gameplay collision layers.

Start with manually-built layers. Later, add loaders for external authoring tools.

## Style Rules for Expanding the Engine
- Keep new modules independent and composable.
- Prefer small classes with explicit responsibilities.
- Document all public APIs with XML comments.
- Add focused examples into docs whenever adding non-obvious behavior.
