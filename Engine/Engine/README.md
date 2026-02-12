# Engine (MonoGame.Extended-first)

This project now treats **MonoGame.Extended** as the primary runtime foundation.

## Active Runtime Surface
- `Engine.Core.ExtendedGameHost`
  - Game bootstrap with virtual resolution via `BoxingViewportAdapter`
  - `OrthographicCamera`
  - World draw pass with camera transform
- `Engine.Core.EngineFrameContext`
  - Engine-facing per-frame context that bridges game code away from raw Extended APIs
- `Engine.Core.InputBridge`
  - Action-based input mapping over MonoGame keyboard state
- `Engine.Core.TiledMapRuntime`
  - Loads `TiledMap`
  - Wraps `TiledMapRenderer`

## Why this changed
The old custom systems overlapped heavily with MonoGame.Extended features (camera, tilemap runtime, scene/state/input wrappers, etc.).

This migration reduces duplicated engine maintenance and keeps the project aligned with the Extended ecosystem.

## Packages in use
- `MonoGame.Extended`

## Content pipeline note
If a game project uses Extended content processors, ensure its `.mgcb` has:

```txt
/reference:../pipeline-references/MonoGame.Extended.Content.Pipeline.dll
```

See:
- `Engine/Engine/TILED_WORKFLOW.md` for tileset/map authoring and import contract.
- `Engine/Engine/EXTENDED_MIGRATION.md` for overlap details.
