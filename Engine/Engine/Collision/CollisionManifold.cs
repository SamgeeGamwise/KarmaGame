using Microsoft.Xna.Framework;

namespace Engine.Collision;

/// <summary>
/// Collision contact result for two intersecting shapes.
/// </summary>
public readonly struct CollisionManifold
{
    /// <summary>
    /// Gets whether an overlap was detected.
    /// </summary>
    public bool Colliding { get; }

    /// <summary>
    /// Gets collision normal (A to B).
    /// </summary>
    public Vector2 Normal { get; }

    /// <summary>
    /// Gets penetration depth.
    /// </summary>
    public float Penetration { get; }

    /// <summary>
    /// Gets representative contact point.
    /// </summary>
    public Vector2 ContactPoint { get; }

    /// <summary>
    /// Creates a new manifold value.
    /// </summary>
    public CollisionManifold(bool colliding, Vector2 normal, float penetration, Vector2 contactPoint)
    {
        Colliding = colliding;
        Normal = normal;
        Penetration = penetration;
        ContactPoint = contactPoint;
    }

    /// <summary>
    /// Gets a non-colliding manifold.
    /// </summary>
    public static CollisionManifold None => new(false, Vector2.Zero, 0f, Vector2.Zero);
}
