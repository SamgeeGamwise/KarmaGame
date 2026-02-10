# Engine (MonoGame 2D Framework)

GDD: https://docs.google.com/document/d/153FYGJzrGRPoTR3Qp7OdYFJxWTK2ZeKOiLsObwKkqoo/edit?usp=sharing

## New to these terms?
- Start here: `Engine/Beginner Guide.md`


## Goals
- Reduce game-loop boilerplate.
- Work with scene composition (`Node`, `Node2D`) instead of monolithic game classes.
- Use action-based input mappings.
- Support tilemap-heavy 2D games (farming/life-sim RPG style).
- Keep code transparent with XML docs and small focused systems.

## Current Modules
- `Core`
  - `EngineGame`: high-level host with virtual resolution, update pipeline, and world/screen render passes.
  - `EngineContext`: per-frame services passed into nodes.
- `Scene`
  - `Node`, `Node2D`, `SceneTree`, `RenderSpace`.
- `Input`
  - `InputState` for raw keyboard/mouse.
  - `InputMap` with named actions (`Pressed`, `Released`, `Down`, vector helpers).
- `Graphics`
  - `VirtualResolutionScaler`.
  - `Camera2D`.
  - `SpriteNode2D`.
- `Tilemap`
  - `TileSet`, `TileLayer`, `TileMapNode`.
- `Collision`
  - Layer-filtered AABB primitives and manifold helpers.
- `UI`
  - `Button`, `MenuEntry`, `MenuListNode`.
- `State`
  - Optional stack-based state machine (`StateStack<TStateId>`).

## Mental Model
- `EngineGame` runs the frame lifecycle.
- `SceneTree` owns a root `Node`.
- Nodes use callbacks:
  - `OnEnterTree` once attached.
  - `OnReady` first time initialized.
  - `OnUpdate` each frame.
  - `OnDraw` in either world or screen pass.
  - `OnExitTree` when removed.
- `RenderSpace.World` is camera-aware.
- `RenderSpace.Screen` is UI/HUD space.

## Quick Start
```csharp
using Engine.Core;
using Engine.Input;
using Engine.Scene;
using Microsoft.Xna.Framework.Input;

public sealed class MyGame : EngineGame
{
    public MyGame() : base(640, 360, useVirtualResolution: true) { }

    protected override void ConfigureInput(InputMap input)
    {
        input.BindKey("move_left", Keys.A);
        input.BindKey("move_right", Keys.D);
        input.BindKey("move_up", Keys.W);
        input.BindKey("move_down", Keys.S);
        input.BindKey("ui_accept", Keys.Enter);
    }

    protected override Node CreateInitialScene() => new MyRootScene();
}
```

## Suggested Project Layout
- `Game/Scenes`
- `Game/Actors`
- `Game/Systems`
- `Game/UI`
- `Game/Content`

Keep engine code generic and game-specific behavior in your game project.

## What Is Not Done Yet
- Animation/state machines for sprites.
- Built-in save/load system.
- Rich UI layout containers.
- Tiled map importers (.tmx/.json).
- ECS bridge (optional alternative to node-first design).

These are good candidates for part 2.
