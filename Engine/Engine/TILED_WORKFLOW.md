# Tiled Map Workflow (Engine Receiving Contract)

This project expects map authoring through **Tiled** and runtime loading through `TiledMapRuntime`.

## 1) Build Tilesets Correctly
1. In Tiled, create a new tileset based on your atlas image (ground/buildings textures).
2. Set correct tile size, margin, and spacing so tile slicing matches source pixels exactly.
3. Use an external tileset (`.tsx`) so multiple maps can share the same tile definitions.

References:
- Tiled tileset setup: https://docs.mapeditor.org/manual/editing-tilesets/

## 2) Recommended Layer Contract
Use these layer names unless you intentionally change `TiledMapAuthoringProfile`:
- `Ground` (tile layer)
- `GroundDetails` (tile layer)
- `Buildings` (tile layer)
- `Collision` (tile layer, non-empty tile = blocked)
- `AbovePlayer` (tile layer converted to y-sorted overhang drawables)
- `Spawns` (object layer)

Runtime defaults are defined in `Engine/Core/TiledMapAuthoringProfile.cs`.

References:
- Tiled layers and layer order: https://docs.mapeditor.org/en/latest/manual/layers/
- Tiled object layers: https://docs.mapeditor.org/de/stable/manual/objects/

## 3) Spawn Points and Gameplay Markers
In `Spawns` object layer, create point objects:
- `PlayerSpawn` (required by Sandbox fallback)
- any future NPC spawn points (`NpcSpawn_01`, etc.)

`TiledMapRuntime.TryGetObjectPosition(...)` reads these at runtime.

## 4) Bring Map Into MonoGame Content Pipeline
Place map files in `Sandbox/Sandbox/Content/Maps`:
- `StartTown.tmx`
- referenced `.tsx` files
- referenced tileset `.png` textures

Then add them to `Content.mgcb` (MGCB Editor is easiest). If editing manually, importer/processor names are:
- `.tmx`: `TiledMapImporter` + `TiledMapProcessor`
- `.tsx`: `TiledMapTilesetImporter` + `TiledMapTilesetProcessor`

MonoGame.Extended documentation: add map/tileset/texture assets to MGCB.
- https://www.monogameextended.net/docs/features/tiled/

## 5) Runtime Features Already Implemented
`Engine/Core/TiledMapRuntime.cs` now supports:
- loading and fail-safe `TryLoad`
- layered rendering (`DrawLayers`)
- collision query on tile layers (`IsWorldRectangleBlocked`)
- object lookup for spawn points (`TryGetObjectPosition`)
- camera clamp to map bounds (`ClampCameraTarget`)

Sandbox uses this receiving path in `Sandbox/Game/SandboxGame.cs`.
