using Engine.Core;
using Engine.Scene;
using Microsoft.Xna.Framework;

namespace Engine.Tilemap;

/// <summary>
/// Node-based tile map renderer.
/// </summary>
public sealed class TileMapNode : Node2D
{
    /// <summary>
    /// Gets tile layers.
    /// </summary>
    public List<TileLayer> Layers { get; } = [];

    /// <summary>
    /// Gets or sets tile set atlas.
    /// </summary>
    public TileSet? TileSet { get; set; }

    /// <summary>
    /// Gets or sets global tint color for all layers.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets whether draw should cull tiles outside the camera view.
    /// </summary>
    public bool UseCameraCulling { get; set; } = true;

    /// <summary>
    /// Adds and returns a new tile layer.
    /// </summary>
    public TileLayer CreateLayer(string name, int width, int height, int emptyTileId = -1)
    {
        var layer = new TileLayer(name, width, height, emptyTileId);
        Layers.Add(layer);
        return layer;
    }

    /// <summary>
    /// Returns world-space rectangles for collidable, non-empty tiles
    /// intersecting a given world area.
    /// </summary>
    public IEnumerable<Rectangle> QueryCollisionTiles(Rectangle worldArea)
    {
        if (TileSet is null)
            yield break;

        int tileWidth = TileSet.TileWidth;
        int tileHeight = TileSet.TileHeight;
        Vector2 basePos = GlobalPosition;

        foreach (var layer in Layers)
        {
            if (!layer.Collidable || !layer.Visible)
                continue;

            int minX = Math.Max(0, (int)MathF.Floor((worldArea.Left - basePos.X) / tileWidth));
            int minY = Math.Max(0, (int)MathF.Floor((worldArea.Top - basePos.Y) / tileHeight));
            int maxX = Math.Min(layer.Width - 1, (int)MathF.Floor((worldArea.Right - basePos.X) / tileWidth));
            int maxY = Math.Min(layer.Height - 1, (int)MathF.Floor((worldArea.Bottom - basePos.Y) / tileHeight));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int id = layer.GetTile(x, y);
                    if (id == layer.EmptyTileId)
                        continue;

                    yield return new Rectangle(
                        (int)basePos.X + x * tileWidth,
                        (int)basePos.Y + y * tileHeight,
                        tileWidth,
                        tileHeight);
                }
            }
        }
    }

    /// <inheritdoc />
    protected override void OnDraw(EngineContext context)
    {
        if (TileSet is null)
            return;

        int tileWidth = TileSet.TileWidth;
        int tileHeight = TileSet.TileHeight;
        Vector2 basePos = GlobalPosition;

        Rectangle drawBounds = context.ActiveCamera?.GetWorldViewBounds(context.VirtualWidth, context.VirtualHeight)
            ?? new Rectangle(0, 0, context.VirtualWidth, context.VirtualHeight);

        foreach (var layer in Layers)
        {
            if (!layer.Visible)
                continue;

            int minX = 0;
            int minY = 0;
            int maxX = layer.Width - 1;
            int maxY = layer.Height - 1;

            if (UseCameraCulling)
            {
                minX = Math.Max(0, (int)MathF.Floor((drawBounds.Left - basePos.X) / tileWidth) - 1);
                minY = Math.Max(0, (int)MathF.Floor((drawBounds.Top - basePos.Y) / tileHeight) - 1);
                maxX = Math.Min(layer.Width - 1, (int)MathF.Ceiling((drawBounds.Right - basePos.X) / tileWidth) + 1);
                maxY = Math.Min(layer.Height - 1, (int)MathF.Ceiling((drawBounds.Bottom - basePos.Y) / tileHeight) + 1);
            }

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int tileId = layer.GetTile(x, y);
                    if (tileId == layer.EmptyTileId)
                        continue;

                    Rectangle src = TileSet.GetSourceRectangle(tileId);
                    Rectangle dst = new(
                        (int)MathF.Round(basePos.X) + x * tileWidth,
                        (int)MathF.Round(basePos.Y) + y * tileHeight,
                        tileWidth,
                        tileHeight);

                    context.SpriteBatch.Draw(TileSet.Texture, dst, src, Color);
                }
            }
        }
    }
}
