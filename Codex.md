# Codex Session Notes

## Project
- Repo: `KarmaGame`
- Active runtime target: `Sandbox`

## Current Architecture
- `SandboxGame` is a host/composition root.
- Scene orchestration lives in `Sandbox/Sandbox/Game/Scene/SandboxScene.cs`.
- Gameplay nodes are split into `MapNode`, `PlayerNode`, and `CameraNode`.
- Engine-level Y-sort utilities exist in:
  - `Engine/Engine/Core/IYSortDrawable.cs`
  - `Engine/Engine/Core/YSortRenderer.cs`

## Rendering/Collision Notes
- Tiled collision checks use `Collision` layer with masked raw GID logic in `TiledMapRuntime`.
- `Collision` is currently included in background rendered layers through `TiledMapAuthoringProfile`.
- Y-sort currently queues player draw through `YSortRenderer`; map layers are still rendered as tiled layers.

## Next Workstream
- Rudimentary day/night cycle:
  - Clock increments by 15 in-game minutes every 5 real seconds.
  - Full-screen tint for day/night transitions.
  - Start at 6:00 AM, with day ending at 2:00 AM.
