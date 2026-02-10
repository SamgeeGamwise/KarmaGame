using Engine.Core;
using Engine.Scene;
using Microsoft.Xna.Framework;

namespace Engine.Graphics;

/// <summary>
/// Scene node that defines a 2D world camera.
/// </summary>
public sealed class Camera2D : Node2D
{
    private float _zoom = 1f;

    /// <summary>
    /// Gets or sets whether this camera should become active.
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    /// <summary>
    /// Gets or sets zoom multiplier (1 = default).
    /// </summary>
    public float Zoom
    {
        get => _zoom;
        set => _zoom = Math.Max(0.001f, value);
    }

    /// <summary>
    /// Builds a world-to-screen transform matrix.
    /// </summary>
    public Matrix GetViewMatrix(int viewportWidth, int viewportHeight)
    {
        Vector2 target = GlobalPosition;
        return Matrix.CreateTranslation(new Vector3(-target, 0f))
            * Matrix.CreateScale(_zoom, _zoom, 1f)
            * Matrix.CreateTranslation(new Vector3(viewportWidth / 2f, viewportHeight / 2f, 0f));
    }

    /// <summary>
    /// Gets world bounds currently visible by this camera.
    /// </summary>
    public Rectangle GetWorldViewBounds(int viewportWidth, int viewportHeight)
    {
        Matrix view = GetViewMatrix(viewportWidth, viewportHeight);
        Matrix.Invert(ref view, out var inv);

        Vector2 topLeft = Vector2.Transform(Vector2.Zero, inv);
        Vector2 bottomRight = Vector2.Transform(new Vector2(viewportWidth, viewportHeight), inv);

        int x = (int)MathF.Floor(MathF.Min(topLeft.X, bottomRight.X));
        int y = (int)MathF.Floor(MathF.Min(topLeft.Y, bottomRight.Y));
        int w = (int)MathF.Ceiling(MathF.Abs(bottomRight.X - topLeft.X));
        int h = (int)MathF.Ceiling(MathF.Abs(bottomRight.Y - topLeft.Y));

        return new Rectangle(x, y, Math.Max(1, w), Math.Max(1, h));
    }

    /// <inheritdoc />
    protected override void OnEnterTree(EngineContext context)
    {
        if (IsCurrent)
            Tree?.SetActiveCamera(this);
    }

    /// <inheritdoc />
    protected override void OnUpdate(EngineContext context)
    {
        if (IsCurrent && !ReferenceEquals(Tree?.ActiveCamera, this))
            Tree?.SetActiveCamera(this);
    }

    /// <inheritdoc />
    protected override void OnExitTree(EngineContext context)
    {
        if (ReferenceEquals(Tree?.ActiveCamera, this))
            Tree?.SetActiveCamera(null);
    }
}
