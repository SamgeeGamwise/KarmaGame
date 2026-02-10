namespace Engine.Scene;

/// <summary>
/// Defines the render pass a node participates in.
/// </summary>
public enum RenderSpace
{
    /// <summary>
    /// Node is rendered in world space and affected by the active camera.
    /// </summary>
    World = 0,

    /// <summary>
    /// Node is rendered in screen space and ignores the active camera.
    /// </summary>
    Screen = 1
}
