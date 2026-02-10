using Engine.Core;

namespace Engine.Scene;

/// <summary>
/// Base object in the scene tree.
/// </summary>
/// <remarks>
/// <para>
/// Inspired by Godot's node model: nodes are composed in a tree, each with
/// lifecycle callbacks for enter/ready/update/draw/exit.
/// </para>
/// <para>
/// Use <see cref="AddChild(Node)"/> to compose behavior and
/// <see cref="QueueFree"/> to safely remove nodes.
/// </para>
/// </remarks>
public abstract class Node
{
    private readonly List<Node> _children = [];
    private bool _isReady;
    private bool _queuedForFree;

    /// <summary>
    /// Gets or sets a node label used for debugging and search.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets a snapshot-friendly list of child nodes.
    /// </summary>
    public IReadOnlyList<Node> Children => _children;

    /// <summary>
    /// Gets the current parent node, if any.
    /// </summary>
    public Node? Parent { get; private set; }

    /// <summary>
    /// Gets the scene tree this node belongs to, if currently attached.
    /// </summary>
    public SceneTree? Tree { get; private set; }

    /// <summary>
    /// Gets whether this node is currently attached to a <see cref="SceneTree"/>.
    /// </summary>
    public bool IsInsideTree => Tree is not null;

    /// <summary>
    /// Gets or sets whether <see cref="OnUpdate"/> should run.
    /// </summary>
    public bool ProcessEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this node participates in draw traversal.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this node renders in world or screen pass.
    /// </summary>
    public RenderSpace RenderSpace { get; set; } = RenderSpace.World;

    /// <summary>
    /// Marks this node for removal at end of frame.
    /// </summary>
    public void QueueFree()
    {
        _queuedForFree = true;
    }

    /// <summary>
    /// Adds a child to this node.
    /// </summary>
    /// <param name="child">Node to attach.</param>
    public void AddChild(Node child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (ReferenceEquals(child, this))
            throw new InvalidOperationException("A node cannot be its own child.");
        if (child.Parent is not null)
            throw new InvalidOperationException("Node already has a parent.");

        child.Parent = this;
        _children.Add(child);

        if (Tree is not null)
            Tree.AttachSubtree(child);
    }

    /// <summary>
    /// Removes a direct child from this node.
    /// </summary>
    /// <param name="child">Node to remove.</param>
    /// <returns><see langword="true"/> if removed.</returns>
    public bool RemoveChild(Node child)
    {
        ArgumentNullException.ThrowIfNull(child);

        int index = _children.IndexOf(child);
        if (index < 0)
            return false;

        _children.RemoveAt(index);

        if (Tree is not null)
            Tree.DetachSubtree(child);
        else
            child.Parent = null;

        return true;
    }

    /// <summary>
    /// Finds the first direct child with a matching <see cref="Name"/>.
    /// </summary>
    public Node? FindChild(string name)
    {
        return _children.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.Ordinal));
    }

    /// <summary>
    /// Finds the first direct child with a matching runtime type.
    /// </summary>
    public TNode? FindChild<TNode>() where TNode : Node
    {
        return _children.OfType<TNode>().FirstOrDefault();
    }

    internal void AttachToTree(SceneTree tree)
    {
        Tree = tree;
        foreach (var child in _children)
            child.AttachToTree(tree);
    }

    internal void DetachFromTree()
    {
        foreach (var child in _children)
            child.DetachFromTree();
        Tree = null;
    }

    internal void EnterTreeRecursive(EngineContext context)
    {
        OnEnterTree(context);

        foreach (var child in _children)
            child.EnterTreeRecursive(context);

        if (!_isReady)
        {
            _isReady = true;
            OnReady(context);
        }
    }

    internal void ExitTreeRecursive(EngineContext context)
    {
        foreach (var child in _children)
            child.ExitTreeRecursive(context);

        OnExitTree(context);
        _isReady = false;
        _queuedForFree = false;
        Parent = null;
    }

    internal void UpdateRecursive(EngineContext context)
    {
        if (_queuedForFree)
            return;

        if (ProcessEnabled)
            OnUpdate(context);

        // Snapshot protects against add/remove during traversal.
        var children = _children.ToArray();
        foreach (var child in children)
        {
            if (child.Parent == this)
                child.UpdateRecursive(context);
        }
    }

    internal void DrawRecursive(EngineContext context, RenderSpace pass)
    {
        if (_queuedForFree || !Visible)
            return;

        if (RenderSpace == pass)
            OnDraw(context);

        var children = _children.ToArray();
        foreach (var child in children)
        {
            if (child.Parent == this)
                child.DrawRecursive(context, pass);
        }
    }

    internal void CleanupQueuedRecursive(EngineContext context)
    {
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            var child = _children[i];
            child.CleanupQueuedRecursive(context);

            if (child._queuedForFree)
            {
                _children.RemoveAt(i);
                if (Tree is not null)
                    Tree.DetachSubtree(child);
                else
                    child.Parent = null;
            }
        }
    }

    /// <summary>
    /// Called when this node enters a tree.
    /// </summary>
    protected virtual void OnEnterTree(EngineContext context) { }

    /// <summary>
    /// Called once after first entering a tree.
    /// </summary>
    protected virtual void OnReady(EngineContext context) { }

    /// <summary>
    /// Called when this node exits a tree.
    /// </summary>
    protected virtual void OnExitTree(EngineContext context) { }

    /// <summary>
    /// Called every frame while <see cref="ProcessEnabled"/> is true.
    /// </summary>
    protected virtual void OnUpdate(EngineContext context) { }

    /// <summary>
    /// Called during the draw pass matching <see cref="RenderSpace"/>.
    /// </summary>
    protected virtual void OnDraw(EngineContext context) { }
}
