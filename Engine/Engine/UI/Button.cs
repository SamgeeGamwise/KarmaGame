using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI;

/// <summary>
/// Standalone clickable button helper.
/// </summary>
public sealed class Button(Rectangle bounds, string text, Action onClick)
{
    /// <summary>
    /// Gets or sets button bounds.
    /// </summary>
    public Rectangle Bounds = bounds;

    /// <summary>
    /// Gets or sets displayed text.
    /// </summary>
    public string Text = text;

    /// <summary>
    /// Gets whether button is hovered this frame.
    /// </summary>
    public bool IsHovered { get; private set; }

    /// <summary>
    /// Forces hovered state (useful for keyboard navigation).
    /// </summary>
    public void ForceHovered(bool hovered) => IsHovered = hovered;

    /// <summary>
    /// Updates hover and click state.
    /// </summary>
    public void Update(Point? mouseVirtualPos, bool leftClicked)
    {
        IsHovered = mouseVirtualPos.HasValue && Bounds.Contains(mouseVirtualPos.Value);

        if (IsHovered && leftClicked)
            onClick();
    }

    /// <summary>
    /// Draws button rectangle, border, and centered text.
    /// </summary>
    public void Draw(
        SpriteBatch spriteBatch,
        SpriteFont font,
        Texture2D pixel,
        Color normalColor,
        Color hoverColor,
        Color borderColor,
        Color textColor)
    {
        spriteBatch.Draw(pixel, Bounds, IsHovered ? hoverColor : normalColor);

        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, 1), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Bottom - 1, Bounds.Width, 1), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, 1, Bounds.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.Right - 1, Bounds.Y, 1, Bounds.Height), borderColor);

        var size = font.MeasureString(Text);
        var pos = new Vector2(
            Bounds.X + (Bounds.Width - size.X) / 2f,
            Bounds.Y + (Bounds.Height - size.Y) / 2f);

        spriteBatch.DrawString(font, Text, pos, textColor);
    }
}
