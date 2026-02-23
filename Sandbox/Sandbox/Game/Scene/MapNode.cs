using System;
using System.Collections.Generic;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;

namespace Sandbox.Game.Scene;

internal sealed class MapNode(string mapAssetName, TiledMapAuthoringProfile mapProfile)
{
    private const uint TiledGlobalIdentifierMask = 0x1FFF_FFFF;

    private readonly List<IYSortDrawable> _ySortForegroundDrawables = [];
    private Texture2D _fallbackTileset = null!;
    private TiledMapRuntime? _runtime;

    public string MapAssetName => mapAssetName;

    public IReadOnlyList<IYSortDrawable> YSortForegroundDrawables => _ySortForegroundDrawables;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _fallbackTileset = content.Load<Texture2D>("Tileset2");
        if (TiledMapRuntime.TryLoad(content, graphicsDevice, mapAssetName, out var mapRuntime))
        {
            _runtime = mapRuntime;
            BuildYSortForegroundDrawables();
        }
    }

    public void Update(GameTime gameTime)
    {
        _runtime?.Update(gameTime);
    }

    public bool TryGetPlayerSpawn(out Vector2 spawn)
    {
        return TryGetObjectAnchorPosition(mapProfile.PlayerSpawnObjectName, out spawn);
    }

    public bool TryGetObjectPosition(string objectName, out Vector2 position)
    {
        position = default;
        return _runtime is not null &&
               _runtime.TryGetObjectPosition(mapProfile.SpawnObjectLayerName, objectName, out position);
    }

    public bool TryGetObjectAnchorPosition(string objectName, out Vector2 position)
    {
        position = default;
        return _runtime is not null &&
               _runtime.TryGetObjectAnchorPosition(mapProfile.SpawnObjectLayerName, objectName, out position);
    }

    public bool TryGetObjectRectangle(string objectName, out Rectangle rectangle)
    {
        rectangle = Rectangle.Empty;
        return _runtime is not null &&
               _runtime.TryGetObjectRectangle(mapProfile.SpawnObjectLayerName, objectName, out rectangle);
    }

    public bool IsWorldRectangleBlocked(Rectangle worldRect)
    {
        if (_runtime is null)
            return false;

        return _runtime.IsWorldRectangleBlocked(mapProfile.CollisionLayerName, worldRect);
    }

    public Vector2 ClampPlayerPosition(Vector2 position, int frameWidth, int frameHeight, int viewportWidth, int viewportHeight)
    {
        if (_runtime is not null)
        {
            return new Vector2(
                Math.Clamp(position.X, 0f, _runtime.WidthInPixels - frameWidth),
                Math.Clamp(position.Y, 0f, _runtime.HeightInPixels - frameHeight));
        }

        return new Vector2(
            Math.Clamp(position.X, 0f, viewportWidth - frameWidth),
            Math.Clamp(position.Y, 0f, viewportHeight - frameHeight));
    }

    public Vector2 ClampCameraTarget(Vector2 desiredTarget, int viewportWidth, int viewportHeight)
    {
        if (_runtime is null)
            return desiredTarget;

        return _runtime.ClampCameraTarget(desiredTarget, viewportWidth, viewportHeight);
    }

    public void DrawBackground(SpriteBatch spriteBatch, Matrix view, int virtualWidth, int virtualHeight)
    {
        if (_runtime is not null)
        {
            _runtime.DrawLayers(mapProfile.BackgroundLayerNames, view);
            return;
        }

        DrawFallbackFloor(spriteBatch, virtualWidth, virtualHeight);
    }

    public void DrawForeground(Matrix view)
    {
        _runtime?.DrawLayers(mapProfile.ForegroundLayerNames, view);
    }

    private void BuildYSortForegroundDrawables()
    {
        _ySortForegroundDrawables.Clear();
        if (_runtime is null)
            return;

        foreach (string layerName in mapProfile.YSortForegroundLayerNames)
        {
            if (!_runtime.TryGetTileLayer(layerName, out TiledMapTileLayer? layer) || layer is null)
                continue;

            BuildTileLayerDrawables(_runtime.Map, layer);
        }
    }

    private void BuildTileLayerDrawables(TiledMap map, TiledMapTileLayer layer)
    {
        int layerWidth = layer.Width;
        int layerHeight = layer.Height;
        for (int y = 0; y < layerHeight; y++)
        {
            for (int x = 0; x < layerWidth; x++)
            {
                if (!layer.TryGetTile((ushort)x, (ushort)y, out TiledMapTile? tileValue) ||
                    tileValue is null ||
                    tileValue.Value.IsBlank)
                {
                    continue;
                }

                TiledMapTile tile = tileValue.Value;
                uint rawGlobalId = unchecked((uint)tile.GlobalIdentifier);
                int baseGlobalId = (int)(rawGlobalId & TiledGlobalIdentifierMask);
                if (baseGlobalId == 0)
                    continue;

                if (!TryResolveTileVisual(map, baseGlobalId, out Texture2D? texture, out Rectangle sourceRect) ||
                    texture is null)
                {
                    continue;
                }

                Vector2 worldPosition = new(
                    x * layer.TileWidth + layer.Offset.X,
                    y * layer.TileHeight + layer.Offset.Y);

                TileTransform transform = ResolveTileTransform(tile);
                _ySortForegroundDrawables.Add(new TileLayerDrawable(
                    texture,
                    sourceRect,
                    worldPosition,
                    layer.TileWidth,
                    layer.TileHeight,
                    transform));
            }
        }
    }

    private static bool TryResolveTileVisual(TiledMap map, int baseGlobalId, out Texture2D? texture, out Rectangle sourceRect)
    {
        texture = null;
        sourceRect = Rectangle.Empty;

        try
        {
            TiledMapTileset tileset = map.GetTilesetByTileGlobalIdentifier(baseGlobalId);
            if (tileset is null || tileset.Texture is null)
                return false;

            int firstGlobalId = map.GetTilesetFirstGlobalIdentifier(tileset);
            int localTileIdentifier = baseGlobalId - firstGlobalId;
            if (localTileIdentifier < 0)
                return false;

            sourceRect = tileset.GetTileRegion(localTileIdentifier);
            texture = tileset.Texture;
            return sourceRect.Width > 0 && sourceRect.Height > 0;
        }
        catch
        {
            return false;
        }
    }

    private static TileTransform ResolveTileTransform(TiledMapTile tile)
    {
        bool flipHorizontally = tile.IsFlippedHorizontally;
        bool flipVertically = tile.IsFlippedVertically;

        if (!tile.IsFlippedDiagonally)
            return new TileTransform(0f, BuildSpriteEffects(flipHorizontally, flipVertically));

        // Tiled's diagonal flag can be represented as a quarter-turn with optional flips.
        // This is enough for occluder tiles authored on orthogonal maps.
        if (flipHorizontally && flipVertically)
            return new TileTransform(MathHelper.PiOver2, SpriteEffects.FlipHorizontally);
        if (flipHorizontally)
            return new TileTransform(MathHelper.PiOver2, SpriteEffects.None);
        if (flipVertically)
            return new TileTransform(-MathHelper.PiOver2, SpriteEffects.None);

        return new TileTransform(-MathHelper.PiOver2, SpriteEffects.FlipHorizontally);
    }

    private static SpriteEffects BuildSpriteEffects(bool flipHorizontally, bool flipVertically)
    {
        SpriteEffects effects = SpriteEffects.None;
        if (flipHorizontally)
            effects |= SpriteEffects.FlipHorizontally;
        if (flipVertically)
            effects |= SpriteEffects.FlipVertically;
        return effects;
    }

    private void DrawFallbackFloor(SpriteBatch spriteBatch, int virtualWidth, int virtualHeight)
    {
        const int tileSize = 32;
        Rectangle srcA = new(0, 0, tileSize, tileSize);
        Rectangle srcB = new(tileSize, 0, tileSize, tileSize);

        int tilesX = (virtualWidth + tileSize - 1) / tileSize;
        int tilesY = (virtualHeight + tileSize - 1) / tileSize;
        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                Rectangle src = ((x + y) & 1) == 0 ? srcA : srcB;
                Rectangle dst = new(x * tileSize, y * tileSize, tileSize, tileSize);
                spriteBatch.Draw(_fallbackTileset, dst, src, Color.White);
            }
        }
    }

    private readonly record struct TileTransform(float Rotation, SpriteEffects Effects);

    private sealed class TileLayerDrawable : IYSortDrawable
    {
        private readonly Texture2D _texture;
        private readonly Rectangle _sourceRect;
        private readonly Vector2 _position;
        private readonly Vector2 _origin;
        private readonly Vector2 _scale;
        private readonly TileTransform _transform;

        public TileLayerDrawable(
            Texture2D texture,
            Rectangle sourceRect,
            Vector2 worldPosition,
            int worldTileWidth,
            int worldTileHeight,
            TileTransform transform)
        {
            _texture = texture;
            _sourceRect = sourceRect;
            _transform = transform;
            _scale = new Vector2(
                worldTileWidth / (float)sourceRect.Width,
                worldTileHeight / (float)sourceRect.Height);
            _origin = new Vector2(sourceRect.Width * 0.5f, sourceRect.Height * 0.5f);
            _position = worldPosition + new Vector2(worldTileWidth * 0.5f, worldTileHeight * 0.5f);
            YSort = worldPosition.Y + worldTileHeight;
        }

        public float YSort { get; }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _texture,
                _position,
                _sourceRect,
                Color.White,
                _transform.Rotation,
                _origin,
                _scale,
                _transform.Effects,
                0f);
        }
    }
}
