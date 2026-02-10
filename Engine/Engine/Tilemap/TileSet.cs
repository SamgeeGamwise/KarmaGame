using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Tilemap;

/// <summary>
/// Defines how tile IDs map into a source atlas texture.
/// </summary>
public sealed class TileSet
{
    /// <summary>
    /// Creates a new tile set from an atlas texture.
    /// </summary>
    public TileSet(Texture2D texture, int tileWidth, int tileHeight, int margin = 0, int spacing = 0)
    {
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        TileWidth = tileWidth > 0 ? tileWidth : throw new ArgumentOutOfRangeException(nameof(tileWidth));
        TileHeight = tileHeight > 0 ? tileHeight : throw new ArgumentOutOfRangeException(nameof(tileHeight));
        Margin = Math.Max(0, margin);
        Spacing = Math.Max(0, spacing);

        int availableWidth = Math.Max(0, texture.Width - Margin * 2);
        int availableHeight = Math.Max(0, texture.Height - Margin * 2);
        int tileStrideX = TileWidth + Spacing;
        int tileStrideY = TileHeight + Spacing;

        Columns = Math.Max(1, (availableWidth + Spacing) / tileStrideX);
        Rows = Math.Max(1, (availableHeight + Spacing) / tileStrideY);
        TileCount = Columns * Rows;
    }

    /// <summary>
    /// Gets atlas texture.
    /// </summary>
    public Texture2D Texture { get; }

    /// <summary>
    /// Gets source tile width in pixels.
    /// </summary>
    public int TileWidth { get; }

    /// <summary>
    /// Gets source tile height in pixels.
    /// </summary>
    public int TileHeight { get; }

    /// <summary>
    /// Gets atlas border margin in pixels.
    /// </summary>
    public int Margin { get; }

    /// <summary>
    /// Gets spacing between tile cells in pixels.
    /// </summary>
    public int Spacing { get; }

    /// <summary>
    /// Gets number of tile columns.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// Gets number of tile rows.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Gets total tile count in atlas.
    /// </summary>
    public int TileCount { get; }

    /// <summary>
    /// Returns source rectangle for a tile ID.
    /// </summary>
    /// <param name="tileId">Zero-based tile index.</param>
    public Rectangle GetSourceRectangle(int tileId)
    {
        if (tileId < 0 || tileId >= TileCount)
            throw new ArgumentOutOfRangeException(nameof(tileId), $"Valid range is [0, {TileCount - 1}].");

        int col = tileId % Columns;
        int row = tileId / Columns;

        int x = Margin + col * (TileWidth + Spacing);
        int y = Margin + row * (TileHeight + Spacing);
        return new Rectangle(x, y, TileWidth, TileHeight);
    }
}
