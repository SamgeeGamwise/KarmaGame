# Roadmap (Extended-first)

## Next Best Steps
1. Tiled map workflow production pass
   - Finalize `MapScene` bridge in Engine to load TMX + resolve collision layers + spawn points + map bounds from Tiled properties.
   - Add support for map transitions (door/warp objects) through named object-layer markers.
   - Deliverable: Sandbox boots directly into `Maps/StartTown.tmx` and handles collision/spawn/foreground layers from map data only.

2. Gameplay framework bridge
   - Add `EntityBridge` + `ComponentBridge` wrappers that map engine-level concepts onto Extended ECS progressively.
   - Keep game code dependent on Engine interfaces (`EngineFrameContext`, `InputBridge`, `TiledMapRuntime`) instead of direct library calls.
   - Deliverable: Player/NPC movement + interaction systems run through bridge interfaces and are reusable across maps.

## What You Need To Do (Authoring Side)
1. Tilesets and map layout in Tiled
   - Create external tilesets (`.tsx`) for ground/building atlases.
   - Build map tile layers with the expected naming contract (`Ground`, `GroundDetails`, `Buildings`, `Collision`, `AbovePlayer`).

2. Gameplay marker placement
   - Add `Spawns` object layer with at least one `PlayerSpawn` point.
   - Add future map metadata as object/layer properties (warp IDs, interaction IDs).

3. Content import updates
   - Add `*.tmx`, `*.tsx`, and referenced textures to `Content.mgcb`.
   - Keep map asset names stable (example runtime expects `Maps/StartTown`).

## Follow-up
3. Asset validation and diagnostics
   - Add startup validation that reports missing layers/properties/spawn points before entering gameplay.
