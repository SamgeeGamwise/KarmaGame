# TODO

## HospitalInterior rendering issue

- `HospitalInterior.tmx` loads, but its placed tiles render black instead of showing the interior art.
- Empty spaces still show the usual gray background/fallback, so the scene itself is loading.
- The active map file is `Sandbox/Sandbox/Content/Maps/HospitalInterior.tmx`.
- That map uses `Sandbox/Sandbox/Content/Maps/Modern Interior Master Tileset.tsx`.
- The tileset points at `Sandbox/Sandbox/Content/Maps/Modern Interior Master Tileset 32x32.png`.
- That image is `512x34048`.
- `34048` pixels tall is beyond common GPU texture limits, so the map can compile/load while the tileset texture still fails to render correctly at runtime.

## Recommended fixes

- Split the interior atlas into multiple smaller tilesets and update `HospitalInterior.tmx` to use them.
- Or export a reduced interior atlas that only contains the tiles actually used by the hospital map.
- Rebuild after the tileset change and verify `Maps/HospitalInterior` renders normally in-game.

## Follow-up risk

- `Modern Exterior Master Tileset 32x32.png` is also very large at `5632x16448`.
- It is worth reviewing that atlas too, since it may hit hardware/runtime limits on some systems even if it currently appears to work.
