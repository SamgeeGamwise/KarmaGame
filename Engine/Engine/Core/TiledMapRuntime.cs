using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace Engine.Core;

public sealed class TiledMapRuntime
{
    private TiledMapRuntime(TiledMap map, TiledMapRenderer renderer)
    {
        Map = map;
        Renderer = renderer;
    }

    public TiledMap Map { get; }

    public TiledMapRenderer Renderer { get; }

    public int WidthInPixels => Map.Width * Map.TileWidth;

    public int HeightInPixels => Map.Height * Map.TileHeight;

    public Rectangle WorldBounds => new(0, 0, WidthInPixels, HeightInPixels);

    public static TiledMapRuntime Load(ContentManager content, GraphicsDevice graphicsDevice, string assetName)
    {
        TiledMap map = content.Load<TiledMap>(assetName);
        var renderer = new TiledMapRenderer(graphicsDevice, map);
        return new TiledMapRuntime(map, renderer);
    }

    public static bool TryLoad(ContentManager content, GraphicsDevice graphicsDevice, string assetName, out TiledMapRuntime? runtime)
    {
        try
        {
            runtime = Load(content, graphicsDevice, assetName);
            return true;
        }
        catch (ContentLoadException)
        {
            runtime = null;
            return false;
        }
    }

    public void Update(GameTime gameTime)
    {
        Renderer.Update(gameTime);
    }

    public void Draw(Matrix? cameraMatrix = null)
    {
        Renderer.Draw(cameraMatrix);
    }

    public void DrawLayer(string layerName, Matrix? cameraMatrix = null)
    {
        DrawLayers([layerName], cameraMatrix);
    }

    public void DrawLayers(IEnumerable<string> layerNames, Matrix? cameraMatrix = null)
    {
        var selected = new HashSet<string>(layerNames, StringComparer.Ordinal);
        if (selected.Count == 0)
            return;

        var previousVisibility = new List<(TiledMapLayer Layer, bool IsVisible)>(Map.Layers.Count);
        foreach (TiledMapLayer layer in Map.Layers)
        {
            previousVisibility.Add((layer, layer.IsVisible));
            layer.IsVisible = selected.Contains(layer.Name);
        }

        Renderer.Draw(cameraMatrix);

        foreach ((TiledMapLayer layer, bool isVisible) in previousVisibility)
            layer.IsVisible = isVisible;
    }

    public bool TryGetTileLayer(string layerName, out TiledMapTileLayer? layer)
    {
        layer = Map.GetLayer<TiledMapTileLayer>(layerName);
        return layer is not null;
    }

    public bool TryGetObjectLayer(string layerName, out TiledMapObjectLayer? layer)
    {
        layer = Map.GetLayer<TiledMapObjectLayer>(layerName);
        return layer is not null;
    }

    public bool TryGetMapProperty(string key, out string value)
    {
        value = string.Empty;
        if (!Map.Properties.TryGetValue(key, out string? found) || string.IsNullOrWhiteSpace(found))
            return false;

        value = found;
        return true;
    }

    public bool TryGetMapPropertyInt(string key, out int value)
    {
        value = default;
        if (!TryGetMapProperty(key, out string raw))
            return false;

        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    public bool TryGetObjectPosition(string objectLayerName, string objectName, out Vector2 position)
    {
        position = Vector2.Zero;
        if (!TryGetObjectLayer(objectLayerName, out TiledMapObjectLayer? objectLayer) || objectLayer is null)
            return false;

        foreach (TiledMapObject mapObject in objectLayer.Objects)
        {
            if (!string.Equals(mapObject.Name, objectName, StringComparison.Ordinal))
                continue;

            position = mapObject.Position;
            return true;
        }

        return false;
    }

    public bool IsWorldPointBlocked(string collisionLayerName, Vector2 worldPosition)
    {
        int tileX = (int)MathF.Floor(worldPosition.X / Map.TileWidth);
        int tileY = (int)MathF.Floor(worldPosition.Y / Map.TileHeight);
        return IsTileBlocked(collisionLayerName, tileX, tileY);
    }

    public bool IsWorldRectangleBlocked(string collisionLayerName, Rectangle worldRect)
    {
        if (worldRect.Width <= 0 || worldRect.Height <= 0)
            return false;

        int minX = (int)MathF.Floor(worldRect.Left / (float)Map.TileWidth);
        int minY = (int)MathF.Floor(worldRect.Top / (float)Map.TileHeight);
        int maxX = (int)MathF.Floor((worldRect.Right - 1) / (float)Map.TileWidth);
        int maxY = (int)MathF.Floor((worldRect.Bottom - 1) / (float)Map.TileHeight);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (IsTileBlocked(collisionLayerName, x, y))
                    return true;
            }
        }

        return false;
    }

    public Vector2 ClampCameraTarget(Vector2 desiredTarget, int viewportWidth, int viewportHeight)
    {
        float halfViewportWidth = viewportWidth * 0.5f;
        float halfViewportHeight = viewportHeight * 0.5f;

        float minX = halfViewportWidth;
        float maxX = Math.Max(minX, WidthInPixels - halfViewportWidth);
        float minY = halfViewportHeight;
        float maxY = Math.Max(minY, HeightInPixels - halfViewportHeight);

        return new Vector2(
            Math.Clamp(desiredTarget.X, minX, maxX),
            Math.Clamp(desiredTarget.Y, minY, maxY));
    }

    private bool IsTileBlocked(string layerName, int tileX, int tileY)
    {
        if (tileX < 0 || tileY < 0 || tileX >= Map.Width || tileY >= Map.Height)
            return true;

        if (!TryGetTileLayer(layerName, out TiledMapTileLayer? tileLayer) || tileLayer is null)
            return false;

        if (!tileLayer.TryGetTile((ushort)tileX, (ushort)tileY, out TiledMapTile? tile) || tile is null)
            return false;

        return !tile.Value.IsBlank;
    }
}
