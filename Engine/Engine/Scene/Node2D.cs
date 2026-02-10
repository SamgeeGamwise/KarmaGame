using Microsoft.Xna.Framework;

namespace Engine.Scene;

/// <summary>
/// A <see cref="Node"/> with 2D transform properties.
/// </summary>
public abstract class Node2D : Node
{
    /// <summary>
    /// Gets or sets local position relative to parent.
    /// </summary>
    public Vector2 Position { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets or sets local rotation in radians.
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// Gets or sets local scale.
    /// </summary>
    public Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or sets origin/pivot used for local transform.
    /// </summary>
    public Vector2 Origin { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets local transform matrix for this node.
    /// </summary>
    public Matrix LocalTransform
        => Matrix.CreateTranslation(new Vector3(-Origin, 0f))
        * Matrix.CreateScale(new Vector3(Scale, 1f))
        * Matrix.CreateRotationZ(Rotation)
        * Matrix.CreateTranslation(new Vector3(Position, 0f));

    /// <summary>
    /// Gets world transform matrix resolved through parent chain.
    /// </summary>
    public Matrix GlobalTransform
    {
        get
        {
            if (Parent is Node2D p)
                return LocalTransform * p.GlobalTransform;
            return LocalTransform;
        }
    }

    /// <summary>
    /// Gets global world position.
    /// </summary>
    public Vector2 GlobalPosition
    {
        get
        {
            var t = GlobalTransform.Translation;
            return new Vector2(t.X, t.Y);
        }
    }

    /// <summary>
    /// Gets global world rotation in radians.
    /// </summary>
    public float GlobalRotation
    {
        get
        {
            float parentRotation = Parent is Node2D p ? p.GlobalRotation : 0f;
            return parentRotation + Rotation;
        }
    }

    /// <summary>
    /// Gets global scale.
    /// </summary>
    public Vector2 GlobalScale
    {
        get
        {
            if (Parent is Node2D p)
                return new Vector2(p.GlobalScale.X * Scale.X, p.GlobalScale.Y * Scale.Y);
            return Scale;
        }
    }

    /// <summary>
    /// Converts local coordinates into global coordinates.
    /// </summary>
    public Vector2 ToGlobal(Vector2 localPoint)
    {
        return Vector2.Transform(localPoint, GlobalTransform);
    }

    /// <summary>
    /// Converts global coordinates into this node's local coordinates.
    /// </summary>
    public Vector2 ToLocal(Vector2 globalPoint)
    {
        Matrix global = GlobalTransform;
        Matrix.Invert(ref global, out var inv);
        return Vector2.Transform(globalPoint, inv);
    }
}
