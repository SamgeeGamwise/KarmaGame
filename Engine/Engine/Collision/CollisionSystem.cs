using Microsoft.Xna.Framework;

namespace Engine.Collision;

/// <summary>
/// Narrow-phase AABB helpers and basic resolution utilities.
/// </summary>
public sealed class CollisionSystem
{
    /// <summary>
    /// Tests AABB vs AABB intersection and returns a manifold if colliding.
    /// </summary>
    public CollisionManifold IntersectAabb(Rectangle a, Rectangle b)
    {
        if (!a.Intersects(b))
            return CollisionManifold.None;

        float aCenterX = a.X + a.Width / 2f;
        float aCenterY = a.Y + a.Height / 2f;
        float bCenterX = b.X + b.Width / 2f;
        float bCenterY = b.Y + b.Height / 2f;

        float dx = bCenterX - aCenterX;
        float px = (b.Width / 2f + a.Width / 2f) - MathF.Abs(dx);

        float dy = bCenterY - aCenterY;
        float py = (b.Height / 2f + a.Height / 2f) - MathF.Abs(dy);

        if (px < py)
        {
            Vector2 normal = dx < 0 ? new Vector2(-1, 0) : new Vector2(1, 0);
            float penetration = px;
            Vector2 contact = new(aCenterX + normal.X * (a.Width / 2f), bCenterY);
            return new CollisionManifold(true, normal, penetration, contact);
        }

        {
            Vector2 normal = dy < 0 ? new Vector2(0, -1) : new Vector2(0, 1);
            float penetration = py;
            Vector2 contact = new(bCenterX, aCenterY + normal.Y * (a.Height / 2f));
            return new CollisionManifold(true, normal, penetration, contact);
        }
    }

    /// <summary>
    /// Resolves overlap by moving rectangle A opposite the collision normal.
    /// </summary>
    public Rectangle ResolveAabb(Rectangle a, in CollisionManifold manifold)
    {
        if (!manifold.Colliding)
            return a;

        int moveX = (int)MathF.Round(-manifold.Normal.X * manifold.Penetration);
        int moveY = (int)MathF.Round(-manifold.Normal.Y * manifold.Penetration);
        a.Offset(moveX, moveY);
        return a;
    }

    /// <summary>
    /// Reflects velocity against an axis-aligned collision normal.
    /// </summary>
    public Vector2 ReflectAxisAlignedVelocity(Vector2 velocity, in CollisionManifold manifold)
    {
        if (!manifold.Colliding)
            return velocity;

        if (manifold.Normal.X != 0) velocity.X = -velocity.X;
        if (manifold.Normal.Y != 0) velocity.Y = -velocity.Y;
        return velocity;
    }
}
