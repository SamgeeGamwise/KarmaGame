namespace Engine.Collision;

/// <summary>
/// Collision layer-mask helper functions.
/// </summary>
public static class CollisionFiltering
{
    /// <summary>
    /// Returns true when both colliders opt-in to each other's layer.
    /// </summary>
    public static bool ShouldTest(ICollider a, ICollider b)
        => (a.Mask & b.Layer) != 0 && (b.Mask & a.Layer) != 0;
}
