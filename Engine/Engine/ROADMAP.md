# Roadmap (Extended-first)

## Next Best Steps
1. Tiled map workflow end-to-end
   - Add TMX map import through MonoGame.Extended content pipeline.
   - Introduce `MapScene` bridge in Engine that loads map + collision layers + spawn points from Tiled properties.
   - Deliverable: Sandbox loads `Maps/StartTown.tmx` with camera bounds and blocked layers.

2. Gameplay framework bridge
   - Add `EntityBridge` + `ComponentBridge` wrappers that map your engine-level concepts onto Extended ECS progressively.
   - Keep game code dependent on Engine interfaces (`EngineFrameContext`, `InputBridge`) so Extended remains replaceable behind adapters.
   - Deliverable: Player/NPC movement and interaction systems run through bridge interfaces, not direct library calls.

## Follow-up
3. Asset conventions + validation
   - Define sheet/map naming conventions and a startup validator that checks required textures/layers/properties before gameplay starts.
