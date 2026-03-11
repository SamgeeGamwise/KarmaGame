using System.Globalization;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace Engine.Core;

public sealed class TiledMapRuntime
{
    private readonly GraphicsDevice _graphicsDevice;

    private TiledMapRuntime(TiledMap map, TiledMapRenderer renderer, GraphicsDevice graphicsDevice)
    {
        Map = map;
        Renderer = renderer;
        _graphicsDevice = graphicsDevice;
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
        return new TiledMapRuntime(map, renderer, graphicsDevice);
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
        _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
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

        _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
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
        if (!TryGetNamedObject(objectLayerName, objectName, out TiledMapObject? mapObject) || mapObject is null)
            return false;

        position = mapObject.Position;
        return true;
    }

    public bool TryGetObjectAnchorPosition(string objectLayerName, string objectName, out Vector2 position)
    {
        position = Vector2.Zero;
        if (!TryGetNamedObject(objectLayerName, objectName, out TiledMapObject? mapObject) || mapObject is null)
            return false;

        Vector2 objectPosition = mapObject.Position;
        if (TryGetExplicitObjectSize(mapObject, out Vector2 explicitSize))
        {
            position = objectPosition + explicitSize * 0.5f;
            return true;
        }

        position = objectPosition;
        return true;
    }

    public bool TryGetObjectRectangle(string objectLayerName, string objectName, out Rectangle rectangle)
    {
        rectangle = Rectangle.Empty;
        if (!TryGetNamedObject(objectLayerName, objectName, out TiledMapObject? mapObject) || mapObject is null)
            return false;

        rectangle = BuildObjectRectangle(mapObject);
        return true;
    }

    public IReadOnlyList<NamedRectangle> GetObjectRectangles(string objectLayerName)
    {
        if (!TryGetObjectLayer(objectLayerName, out TiledMapObjectLayer? objectLayer) || objectLayer is null)
            return [];

        var rectangles = new List<NamedRectangle>(objectLayer.Objects.Length);
        foreach (TiledMapObject mapObject in objectLayer.Objects)
        {
            Rectangle rectangle = BuildObjectRectangle(mapObject);
            if (rectangle.Width <= 0 || rectangle.Height <= 0)
                continue;

            rectangles.Add(new NamedRectangle(mapObject.Name ?? string.Empty, rectangle));
        }

        return rectangles;
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

        // Tiled encodes flip/rotation in the high 3 bits of the raw global ID.
        // Treat a tile as blocked only when the base global ID is non-zero.
        const uint tiledFlipMask = 0x1FFF_FFFF;
        uint baseGlobalId = (uint)tile.Value.GlobalIdentifier & tiledFlipMask;
        return baseGlobalId != 0;
    }

    private bool TryGetNamedObject(string objectLayerName, string objectName, out TiledMapObject? foundObject)
    {
        foundObject = null;
        if (!TryGetObjectLayer(objectLayerName, out TiledMapObjectLayer? objectLayer) || objectLayer is null)
            return false;

        foreach (TiledMapObject mapObject in objectLayer.Objects)
        {
            if (!string.Equals(mapObject.Name, objectName, StringComparison.Ordinal))
                continue;

            foundObject = mapObject;
            return true;
        }

        return false;
    }

    private static bool TryGetExplicitObjectSize(TiledMapObject mapObject, out Vector2 size)
    {
        size = Vector2.Zero;
        Type type = mapObject.GetType();
        if (TryGetNumericProperty(type, mapObject, "Width", out float width) &&
            TryGetNumericProperty(type, mapObject, "Height", out float height))
        {
            size = new Vector2(width, height);
            return true;
        }

        object? sizeValue = GetPropertyValue(type, mapObject, "Size");
        if (TryExtractSize(sizeValue, out Vector2 extractedSize))
        {
            size = extractedSize;
            return true;
        }

        object? boundsValue = GetPropertyValue(type, mapObject, "Bounds") ??
                              GetPropertyValue(type, mapObject, "BoundingRectangle") ??
                              GetPropertyValue(type, mapObject, "Rectangle");
        if (TryExtractSize(boundsValue, out extractedSize))
        {
            size = extractedSize;
            return true;
        }

        return false;
    }

    public float ClampHorizontalMovement(string collisionLayerName, Rectangle worldRect, float deltaX)
    {
        if (deltaX == 0f || worldRect.Width <= 0 || worldRect.Height <= 0)
            return 0f;

        int minY = (int)MathF.Floor(worldRect.Top / (float)Map.TileHeight);
        int maxY = (int)MathF.Floor((worldRect.Bottom - 1) / (float)Map.TileHeight);

        if (deltaX > 0f)
        {
            float intendedRight = worldRect.Right + deltaX;
            int startTileX = (int)MathF.Floor(worldRect.Left / (float)Map.TileWidth);
            int targetTileX = (int)MathF.Floor((intendedRight - 1f) / Map.TileWidth);
            float resolvedDelta = deltaX;

            for (int tileX = startTileX; tileX <= targetTileX; tileX++)
            {
                for (int tileY = minY; tileY <= maxY; tileY++)
                {
                    if (!IsTileBlocked(collisionLayerName, tileX, tileY))
                        continue;

                    float blockingLeft = tileX * Map.TileWidth;
                    float tileRight = blockingLeft + Map.TileWidth;
                    if (blockingLeft >= intendedRight || tileRight <= worldRect.Left)
                        continue;

                    resolvedDelta = Math.Min(resolvedDelta, blockingLeft - worldRect.Right);
                }
            }

            return resolvedDelta;
        }

        float intendedLeft = worldRect.Left + deltaX;
        int startTile = (int)MathF.Floor(intendedLeft / Map.TileWidth);
        int targetTile = (int)MathF.Floor((worldRect.Right - 1) / (float)Map.TileWidth);
        float resolvedNegativeDelta = deltaX;

        for (int tileX = startTile; tileX <= targetTile; tileX++)
        {
            for (int tileY = minY; tileY <= maxY; tileY++)
            {
                if (!IsTileBlocked(collisionLayerName, tileX, tileY))
                    continue;

                float blockingLeft = tileX * Map.TileWidth;
                float blockingRight = (tileX + 1) * Map.TileWidth;
                if (blockingRight <= intendedLeft || blockingLeft >= worldRect.Right)
                    continue;

                resolvedNegativeDelta = Math.Max(resolvedNegativeDelta, blockingRight - worldRect.Left);
            }
        }

        return resolvedNegativeDelta;
    }

    public float ClampVerticalMovement(string collisionLayerName, Rectangle worldRect, float deltaY)
    {
        if (deltaY == 0f || worldRect.Width <= 0 || worldRect.Height <= 0)
            return 0f;

        int minX = (int)MathF.Floor(worldRect.Left / (float)Map.TileWidth);
        int maxX = (int)MathF.Floor((worldRect.Right - 1) / (float)Map.TileWidth);

        if (deltaY > 0f)
        {
            float intendedBottom = worldRect.Bottom + deltaY;
            int startTileY = (int)MathF.Floor(worldRect.Top / (float)Map.TileHeight);
            int targetTileY = (int)MathF.Floor((intendedBottom - 1f) / Map.TileHeight);
            float resolvedDelta = deltaY;

            for (int tileY = startTileY; tileY <= targetTileY; tileY++)
            {
                for (int tileX = minX; tileX <= maxX; tileX++)
                {
                    if (!IsTileBlocked(collisionLayerName, tileX, tileY))
                        continue;

                    float blockingTop = tileY * Map.TileHeight;
                    float blockingBottom = blockingTop + Map.TileHeight;
                    if (blockingTop >= intendedBottom || blockingBottom <= worldRect.Top)
                        continue;

                    resolvedDelta = Math.Min(resolvedDelta, blockingTop - worldRect.Bottom);
                }
            }

            return resolvedDelta;
        }

        float intendedTop = worldRect.Top + deltaY;
        int startTile = (int)MathF.Floor(intendedTop / Map.TileHeight);
        int targetTile = (int)MathF.Floor((worldRect.Bottom - 1) / (float)Map.TileHeight);
        float resolvedNegativeDelta = deltaY;

        for (int tileY = startTile; tileY <= targetTile; tileY++)
        {
            for (int tileX = minX; tileX <= maxX; tileX++)
            {
                if (!IsTileBlocked(collisionLayerName, tileX, tileY))
                    continue;

                float blockingTop = tileY * Map.TileHeight;
                float blockingBottom = (tileY + 1) * Map.TileHeight;
                if (blockingBottom <= intendedTop || blockingTop >= worldRect.Bottom)
                    continue;

                resolvedNegativeDelta = Math.Max(resolvedNegativeDelta, blockingBottom - worldRect.Top);
            }
        }

        return resolvedNegativeDelta;
    }

    private Rectangle BuildObjectRectangle(TiledMapObject mapObject)
    {
        Vector2 position = mapObject.Position;
        Vector2 size = TryGetExplicitObjectSize(mapObject, out Vector2 explicitSize)
            ? explicitSize
            : new Vector2(Map.TileWidth, Map.TileHeight);
        return new Rectangle(
            (int)MathF.Floor(position.X),
            (int)MathF.Floor(position.Y),
            Math.Max(1, (int)MathF.Ceiling(size.X)),
            Math.Max(1, (int)MathF.Ceiling(size.Y)));
    }

    private static bool TryGetNumericProperty(Type type, object instance, string propertyName, out float value)
    {
        value = default;
        PropertyInfo? property = type.GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property is not null)
        {
            object? raw = property.GetValue(instance);
            return TryConvertToFloat(raw, out value);
        }

        return TryGetNumericField(type, instance, propertyName, out value);
    }

    private static object? GetPropertyValue(Type type, object instance, string propertyName)
    {
        PropertyInfo? property = type.GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property is not null)
            return property.GetValue(instance);

        FieldInfo? field = type.GetField(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        return field?.GetValue(instance);
    }

    private static bool TryExtractSize(object? value, out Vector2 size)
    {
        size = Vector2.Zero;
        if (value is null)
            return false;

        if (value is Vector2 vector)
        {
            size = vector;
            return true;
        }

        Type valueType = value.GetType();
        if (TryGetNumericProperty(valueType, value, "Width", out float width) &&
            TryGetNumericProperty(valueType, value, "Height", out float height))
        {
            size = new Vector2(width, height);
            return true;
        }

        if (TryGetNumericProperty(valueType, value, "X", out float x) &&
            TryGetNumericProperty(valueType, value, "Y", out float y))
        {
            size = new Vector2(x, y);
            return true;
        }

        return false;
    }

    private static bool TryConvertToFloat(object? raw, out float value)
    {
        value = default;
        switch (raw)
        {
            case float f:
                value = f;
                return true;
            case double d:
                value = (float)d;
                return true;
            case int i:
                value = i;
                return true;
            default:
                return false;
        }
    }

    private static bool TryGetNumericField(Type type, object instance, string fieldName, out float value)
    {
        value = default;
        FieldInfo? field = type.GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (field is null)
            return false;

        object? raw = field.GetValue(instance);
        return TryConvertToFloat(raw, out value);
    }

    public readonly record struct NamedRectangle(string Name, Rectangle Rectangle);
}


