using Microsoft.Xna.Framework;

namespace Engine.State;

/// <summary>
/// Interface for pushdown state-stack entries.
/// </summary>
public interface IState
{
    /// <summary>
    /// Gets whether update traversal stops after this state updates.
    /// </summary>
    bool BlocksUpdateBelow { get; }

    /// <summary>
    /// Gets whether draw traversal stops after this state draws.
    /// </summary>
    bool BlocksDrawBelow { get; }

    /// <summary>
    /// Called when the state is pushed.
    /// </summary>
    void OnEnter(object? data);

    /// <summary>
    /// Called when the state is popped.
    /// </summary>
    void OnExit();

    /// <summary>
    /// Per-frame update.
    /// </summary>
    void Update(GameTime gameTime);

    /// <summary>
    /// Draw callback.
    /// </summary>
    void Draw();
}
