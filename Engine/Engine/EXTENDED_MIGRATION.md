# MonoGame.Extended Migration Review

## Overlap Review

| Previous custom module | Extended equivalent | Decision |
|---|---|---|
| `Graphics/Camera2D` | `OrthographicCamera` | Removed from active compile |
| `Graphics/VirtualResolutionScaler` | `BoxingViewportAdapter` | Removed from active compile |
| `Tilemap/TileSet, TileLayer, TileMapNode` | `TiledMap`, `TiledMapRenderer` | Removed from active compile |
| `Scene/*` tree lifecycle | `Screens`, ECS, scene patterns in Extended ecosystem | Removed from active compile |
| `Input/*` action wrappers | MonoGame + Extended input utilities | Removed from active compile |
| `UI/*` custom widgets | Extended UI/options or game-level UI choices | Removed from active compile |
| `State/*` stack | Extended screens/state patterns | Removed from active compile |
| `Assets/AssetSet` | MonoGame content + Extended runtime integration | Removed from active compile |

## New Engine baseline
- `Core/ExtendedGameHost.cs`
- `Core/TiledMapRuntime.cs`

## Notes
- Old source files remain in repo history/worktree, but are no longer part of the Engine build.
- This keeps migration reversible while eliminating runtime overlap immediately.
