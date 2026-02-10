using Microsoft.Xna.Framework;

namespace Engine.Collision;

/// <summary>
/// Collision body contract used by filtering and broad/narrow-phase systems.
/// </summary>
public interface ICollider
{
    /// <summary>
    /// Gets or sets current world bounds.
    /// </summary>
    Rectangle Bounds { get; set; }

    /// <summary>
    /// Gets whether collider is static (non-moving).
    /// </summary>
    bool IsStatic { get; }

    /// <summary>
    /// Gets this collider's collision layer.
    /// </summary>
    CollisionLayer Layer { get; }

    /// <summary>
    /// Gets layer mask this collider should collide with.
    /// </summary>
    CollisionLayer Mask { get; }
}
