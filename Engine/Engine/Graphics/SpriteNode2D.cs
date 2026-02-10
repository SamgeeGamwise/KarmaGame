using Engine.Core;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics;

/// <summary>
/// Drawable node for a single sprite.
/// </summary>
public sealed class SpriteNode2D : Node2D
{
    /// <summary>
    /// Gets or sets direct texture reference.
    /// </summary>
    public Texture2D? Texture { get; set; }

    /// <summary>
    /// Gets or sets optional asset key used when <see cref="Texture"/> is null.
    /// </summary>
    public string? TextureKey { get; set; }

    /// <summary>
    /// Gets or sets source rectangle inside texture.
    /// </summary>
    public Rectangle? SourceRect { get; set; }

    /// <summary>
    /// Gets or sets sprite tint color.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets draw depth for <see cref="SpriteSortMode.BackToFront"/> pipelines.
    /// </summary>
    public float LayerDepth { get; set; }

    /// <summary>
    /// Gets or sets sprite effects.
    /// </summary>
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;

    /// <inheritdoc />
    protected override void OnDraw(EngineContext context)
    {
        Texture2D? texture = Texture;
        if (texture is null && !string.IsNullOrWhiteSpace(TextureKey))
            texture = context.Assets.Texture(TextureKey);

        if (texture is null)
            return;

        context.SpriteBatch.Draw(
            texture,
            GlobalPosition,
            SourceRect,
            Color,
            GlobalRotation,
            Origin,
            GlobalScale,
            Effects,
            LayerDepth);
    }
}
