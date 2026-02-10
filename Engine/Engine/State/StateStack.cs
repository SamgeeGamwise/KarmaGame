using Microsoft.Xna.Framework;

namespace Engine.State;

/// <summary>
/// Generic pushdown stack for layered game states.
/// </summary>
public sealed class StateStack<TStateId>(StateFactory<TStateId> factory) where TStateId : notnull
{
    private readonly Stack<IState> _stack = new();

    /// <summary>
    /// Gets current state count.
    /// </summary>
    public int Count => _stack.Count;

    /// <summary>
    /// Pushes a new state on top.
    /// </summary>
    public void Push(TStateId id, object? data = null)
    {
        var state = factory.Create(id);
        state.OnEnter(data);
        _stack.Push(state);
    }

    /// <summary>
    /// Pops top-most state.
    /// </summary>
    public void Pop()
    {
        if (_stack.Count == 0)
            return;

        var state = _stack.Pop();
        state.OnExit();
    }

    /// <summary>
    /// Replaces top state.
    /// </summary>
    public void Replace(TStateId id, object? data = null)
    {
        Pop();
        Push(id, data);
    }

    /// <summary>
    /// Clears stack and pushes a single state.
    /// </summary>
    public void ClearAndPush(TStateId id, object? data = null)
    {
        while (_stack.Count > 0)
            Pop();

        Push(id, data);
    }

    /// <summary>
    /// Updates states from top to bottom until blocked.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        foreach (var state in _stack)
        {
            state.Update(gameTime);
            if (state.BlocksUpdateBelow)
                break;
        }
    }

    /// <summary>
    /// Draws states from top to bottom until blocked.
    /// </summary>
    public void Draw()
    {
        foreach (var state in _stack)
        {
            state.Draw();
            if (state.BlocksDrawBelow)
                break;
        }
    }
}
