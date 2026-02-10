using Microsoft.Xna.Framework;

namespace Engine.Collision;

/// <summary>
/// Collider with mutable velocity.
/// </summary>
public interface IDynamicBody : ICollider
{
    /// <summary>
    /// Gets or sets current velocity.
    /// </summary>
    Vector2 Velocity { get; set; }
}
