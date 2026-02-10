using Engine.Core;
using Engine.Graphics;

namespace Engine.Scene;

/// <summary>
/// Manages the current root node and scene traversal.
/// </summary>
public sealed class SceneTree
{
    private EngineContext? _lastContext;

    /// <summary>
    /// Gets the current root node.
    /// </summary>
    public Node? Root { get; private set; }

    /// <summary>
    /// Gets the active world camera, if one is set.
    /// </summary>
    public Camera2D? ActiveCamera { get; private set; }

    /// <summary>
    /// Replaces the root node with a new scene root.
    /// </summary>
    public void SetRoot(Node? root, EngineContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _lastContext = context;

        if (Root is not null)
        {
            Root.ExitTreeRecursive(context);
            Root.DetachFromTree();
        }

        ActiveCamera = null;
        Root = root;

        if (Root is null)
            return;

        Root.AttachToTree(this);
        Root.EnterTreeRecursive(context);
    }

    /// <summary>
    /// Updates all nodes in root-to-leaf order and processes queued removals.
    /// </summary>
    public void Update(EngineContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _lastContext = context;

        if (Root is null)
            return;

        Root.UpdateRecursive(context);
        Root.CleanupQueuedRecursive(context);
    }

    /// <summary>
    /// Draws nodes for the requested render pass.
    /// </summary>
    public void Draw(EngineContext context, RenderSpace pass)
    {
        ArgumentNullException.ThrowIfNull(context);
        _lastContext = context;
        Root?.DrawRecursive(context, pass);
    }

    internal void AttachSubtree(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        node.AttachToTree(this);

        if (_lastContext is not null)
            node.EnterTreeRecursive(_lastContext);
    }

    internal void DetachSubtree(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (_lastContext is not null)
            node.ExitTreeRecursive(_lastContext);

        node.DetachFromTree();
    }

    internal void SetActiveCamera(Camera2D? camera)
    {
        ActiveCamera = camera;
    }
}
