using Microsoft.Xna.Framework;

namespace Engine.Graphics;

/// <summary>
/// Maintains virtual resolution scaling and coordinate conversion.
/// </summary>
public sealed class VirtualResolutionScaler(int virtualWidth, int virtualHeight)
{
    /// <summary>
    /// Gets virtual width in pixels.
    /// </summary>
    public int VirtualWidth { get; } = virtualWidth;

    /// <summary>
    /// Gets virtual height in pixels.
    /// </summary>
    public int VirtualHeight { get; } = virtualHeight;

    /// <summary>
    /// Gets destination rectangle in backbuffer coordinates.
    /// </summary>
    public Rectangle DestinationRect { get; private set; }

    /// <summary>
    /// Recalculates destination rectangle with preserved aspect ratio.
    /// </summary>
    public Rectangle Recalculate(int backBufferWidth, int backBufferHeight)
    {
        float scale = MathF.Min(
            backBufferWidth / (float)VirtualWidth,
            backBufferHeight / (float)VirtualHeight);

        int width = (int)MathF.Round(VirtualWidth * scale);
        int height = (int)MathF.Round(VirtualHeight * scale);
        int x = (backBufferWidth - width) / 2;
        int y = (backBufferHeight - height) / 2;

        DestinationRect = new Rectangle(x, y, width, height);
        return DestinationRect;
    }

    /// <summary>
    /// Converts screen/backbuffer coordinates to virtual coordinates.
    /// Returns null when outside destination rectangle.
    /// </summary>
    public Point? ScreenToVirtual(Point screenPoint)
    {
        if (!DestinationRect.Contains(screenPoint))
            return null;

        float nx = (screenPoint.X - DestinationRect.X) / (float)DestinationRect.Width;
        float ny = (screenPoint.Y - DestinationRect.Y) / (float)DestinationRect.Height;

        int vx = Math.Clamp((int)(nx * VirtualWidth), 0, VirtualWidth - 1);
        int vy = Math.Clamp((int)(ny * VirtualHeight), 0, VirtualHeight - 1);

        return new Point(vx, vy);
    }
}
