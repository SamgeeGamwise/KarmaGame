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

    private readonly List<OcclusionAnchor> _occlusionAnchors = [];
    private readonly List<IYSortDrawable> _ySortForegroundDrawables = [];
    private Texture2D _fallbackFloorTexture = null!;
    private TiledMapRuntime? _runtime;

    public string MapAssetName => mapAssetName;

    public IReadOnlyList<IYSortDrawable> YSortForegroundDrawables => _ySortForegroundDrawables;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _fallbackFloorTexture ??= CreateFallbackFloorTexture(graphicsDevice);
        
        if (!TiledMapRuntime.TryLoad(content, graphicsDevice, mapAssetName, out var mapRuntime)) return;
        
        _runtime = mapRuntime;
        BuildYSortForegroundDrawables();
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

    public float ClampHorizontalMovement(Rectangle worldRect, float deltaX)
    {
        if (_runtime is null)
            return deltaX;

        return _runtime.ClampHorizontalMovement(mapProfile.CollisionLayerName, worldRect, deltaX);
    }

    public float ClampVerticalMovement(Rectangle worldRect, float deltaY)
    {
        if (_runtime is null)
            return deltaY;

        return _runtime.ClampVerticalMovement(mapProfile.CollisionLayerName, worldRect, deltaY);
    }

    public bool TryResolveOcclusionSort(Rectangle worldRect, out float sortY)
    {
        sortY = default;
        if (_runtime is null)
            return false;

        Point center = new(worldRect.Center.X, worldRect.Center.Y);
        foreach (OcclusionAnchor anchor in _occlusionAnchors)
        {
            if (!anchor.Bounds.Intersects(worldRect) &&
                !anchor.Bounds.Contains(center))
            {
                continue;
            }

            sortY = anchor.SortY - 1f;
            return true;
        }

        return false;
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

    public void DrawCollisionDebug(SpriteBatch spriteBatch, Texture2D pixel, Color fillColor, Color outlineColor)
    {
        if (_runtime is null)
            return;

        if (!_runtime.TryGetTileLayer(mapProfile.CollisionLayerName, out TiledMapTileLayer? layer) || layer is null)
            return;

        int offsetX = (int)MathF.Round(layer.Offset.X);
        int offsetY = (int)MathF.Round(layer.Offset.Y);
        for (int y = 0; y < layer.Height; y++)
        {
            for (int x = 0; x < layer.Width; x++)
            {
                if (!layer.TryGetTile((ushort)x, (ushort)y, out TiledMapTile? tileValue) ||
                    tileValue is null ||
                    tileValue.Value.IsBlank)
                {
                    continue;
                }

                uint rawGlobalId = unchecked((uint)tileValue.Value.GlobalIdentifier);
                uint baseGlobalId = rawGlobalId & TiledGlobalIdentifierMask;
                if (baseGlobalId == 0)
                    continue;

                Rectangle tileRect = new(
                    x * layer.TileWidth + offsetX,
                    y * layer.TileHeight + offsetY,
                    layer.TileWidth,
                    layer.TileHeight);
                spriteBatch.Draw(pixel, tileRect, fillColor);
                DrawRectangleOutline(spriteBatch, pixel, tileRect, outlineColor);
            }
        }
    }

    private void BuildYSortForegroundDrawables()
    {
        _occlusionAnchors.Clear();
        _ySortForegroundDrawables.Clear();
        if (_runtime is null)
            return;

        BuildOcclusionAnchors();

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
                Rectangle worldBounds = new(
                    (int)MathF.Round(worldPosition.X),
                    (int)MathF.Round(worldPosition.Y),
                    layer.TileWidth,
                    layer.TileHeight);

                TileTransform transform = ResolveTileTransform(tile);
                _ySortForegroundDrawables.Add(new TileLayerDrawable(
                    texture,
                    sourceRect,
                    worldPosition,
                    ResolveTileYSort(worldBounds),
                    layer.TileWidth,
                    layer.TileHeight,
                    transform));
            }
        }
    }

    private void BuildOcclusionAnchors()
    {
        if (_runtime is null)
            return;

        IReadOnlyList<TiledMapRuntime.NamedRectangle> rectangles =
            _runtime.GetObjectRectangles(mapProfile.OcclusionObjectLayerName);
        foreach (TiledMapRuntime.NamedRectangle rectangle in rectangles)
            _occlusionAnchors.Add(new OcclusionAnchor(rectangle.Rectangle));

        _occlusionAnchors.Sort(static (a, b) =>
        {
            int areaOrder = a.Area.CompareTo(b.Area);
            if (areaOrder != 0)
                return areaOrder;

            return a.Bounds.Top.CompareTo(b.Bounds.Top);
        });
    }

    private float ResolveTileYSort(Rectangle tileWorldBounds)
    {
        Point tileCenter = new(tileWorldBounds.Center.X, tileWorldBounds.Center.Y);
        foreach (OcclusionAnchor anchor in _occlusionAnchors)
        {
            if (!anchor.Bounds.Intersects(tileWorldBounds) &&
                !anchor.Bounds.Contains(tileCenter))
            {
                continue;
            }

            return anchor.SortY;
        }

        return tileWorldBounds.Bottom;
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

    private static void DrawRectangleOutline(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, Color color)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), color);
    }

    private void DrawFallbackFloor(SpriteBatch spriteBatch, int virtualWidth, int virtualHeight)
    {
        const int tileSize = 32;
        Rectangle srcA = new(0, 0, 1, 1);
        Rectangle srcB = new(1, 0, 1, 1);

        int tilesX = (virtualWidth + tileSize - 1) / tileSize;
        int tilesY = (virtualHeight + tileSize - 1) / tileSize;
        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                Rectangle src = ((x + y) & 1) == 0 ? srcA : srcB;
                Rectangle dst = new(x * tileSize, y * tileSize, tileSize, tileSize);
                spriteBatch.Draw(_fallbackFloorTexture, dst, src, Color.White);
            }
        }
    }

    private static Texture2D CreateFallbackFloorTexture(GraphicsDevice graphicsDevice)
    {
        var texture = new Texture2D(graphicsDevice, 2, 1);
        texture.SetData(
        [
            new Color(54, 62, 72),
            new Color(69, 78, 90)
        ]);
        return texture;
    }

    private readonly record struct TileTransform(float Rotation, SpriteEffects Effects);

    private readonly record struct OcclusionAnchor(Rectangle Bounds)
    {
        public int Area => Bounds.Width * Bounds.Height;

        public float SortY => Bounds.Bottom;
    }

    private sealed class TileLayerDrawable(
        Texture2D texture,
        Rectangle sourceRect,
        Vector2 worldPosition,
        float ySort,
        int worldTileWidth,
        int worldTileHeight,
MapNode.TileTransform transform) : IYSortDrawable
    {
        private readonly Texture2D _texture = texture;
        private readonly Rectangle _sourceRect = sourceRect;
        private readonly Vector2 _position = worldPosition + new Vector2(worldTileWidth * 0.5f, worldTileHeight * 0.5f);
        private readonly Vector2 _origin = new Vector2(sourceRect.Width * 0.5f, sourceRect.Height * 0.5f);
        private readonly Vector2 _scale = new Vector2(
                worldTileWidth / (float)sourceRect.Width,
                worldTileHeight / (float)sourceRect.Height);
        private readonly TileTransform _transform = transform;

        public float YSort { get; } = ySort;

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
