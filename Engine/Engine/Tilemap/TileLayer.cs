namespace Engine.Tilemap;

/// <summary>
/// A 2D tile grid layer.
/// </summary>
public sealed class TileLayer
{
    private readonly int[] _tiles;

    /// <summary>
    /// Creates a tile layer.
    /// </summary>
    /// <param name="name">Layer name.</param>
    /// <param name="width">Layer width in tiles.</param>
    /// <param name="height">Layer height in tiles.</param>
    /// <param name="emptyTileId">Tile ID used for empty cells.</param>
    public TileLayer(string name, int width, int height, int emptyTileId = -1)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Layer" : name;
        Width = width > 0 ? width : throw new ArgumentOutOfRangeException(nameof(width));
        Height = height > 0 ? height : throw new ArgumentOutOfRangeException(nameof(height));
        EmptyTileId = emptyTileId;

        _tiles = new int[Width * Height];
        Array.Fill(_tiles, emptyTileId);
    }

    /// <summary>
    /// Gets layer name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets layer width in tiles.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets layer height in tiles.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets whether layer is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this layer should be considered for collision queries.
    /// </summary>
    public bool Collidable { get; set; }

    /// <summary>
    /// Gets tile ID used for empty cells.
    /// </summary>
    public int EmptyTileId { get; }

    /// <summary>
    /// Reads a tile ID at x/y.
    /// </summary>
    public int this[int x, int y]
    {
        get => GetTile(x, y);
        set => SetTile(x, y, value);
    }

    /// <summary>
    /// Returns tile ID at x/y.
    /// </summary>
    public int GetTile(int x, int y)
    {
        if (!InBounds(x, y))
            return EmptyTileId;
        return _tiles[y * Width + x];
    }

    /// <summary>
    /// Sets tile ID at x/y.
    /// </summary>
    public void SetTile(int x, int y, int tileId)
    {
        if (!InBounds(x, y))
            throw new ArgumentOutOfRangeException($"Tile position ({x}, {y}) is outside layer bounds.");

        _tiles[y * Width + x] = tileId;
    }

    /// <summary>
    /// Fills entire layer with the same tile ID.
    /// </summary>
    public void Fill(int tileId)
    {
        Array.Fill(_tiles, tileId);
    }

    /// <summary>
    /// Returns whether x/y is inside the layer.
    /// </summary>
    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}
