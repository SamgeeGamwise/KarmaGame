using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox.Game.Scene;

internal sealed class MapNode(string mapAssetName, TiledMapAuthoringProfile mapProfile)
{
    private Texture2D _fallbackTileset = null!;
    private TiledMapRuntime? _runtime;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _fallbackTileset = content.Load<Texture2D>("Tileset2");
        if (TiledMapRuntime.TryLoad(content, graphicsDevice, mapAssetName, out var mapRuntime))
            _runtime = mapRuntime;
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
}
