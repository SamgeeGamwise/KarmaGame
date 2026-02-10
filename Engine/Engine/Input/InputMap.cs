using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Input;

/// <summary>
/// Action-based input map similar to engines that use named input actions.
/// </summary>
/// <remarks>
/// <para>
/// Typical usage:
/// </para>
/// <code>
/// input.BindKey("move_left", Keys.A);
/// input.BindKey("move_right", Keys.D);
/// input.BindKey("confirm", Keys.Enter);
/// </code>
/// <para>
/// Then query by action name:
/// </para>
/// <code>
/// if (context.Input.Pressed("confirm")) { ... }
/// </code>
/// </remarks>
public sealed class InputMap
{
    private readonly Dictionary<string, List<InputBinding>> _bindings = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _down = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _pressed = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _released = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets all currently defined action names.
    /// </summary>
    public IReadOnlyCollection<string> Actions => _bindings.Keys;

    /// <summary>
    /// Ensures an action exists.
    /// </summary>
    public void AddAction(string action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        if (!_bindings.ContainsKey(action))
            _bindings[action] = [];
    }

    /// <summary>
    /// Removes an action and all bindings.
    /// </summary>
    public bool RemoveAction(string action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        _down.Remove(action);
        _pressed.Remove(action);
        _released.Remove(action);
        return _bindings.Remove(action);
    }

    /// <summary>
    /// Binds a key to an action.
    /// </summary>
    public void BindKey(string action, Keys key) => Bind(action, new KeyBinding(key));

    /// <summary>
    /// Binds a mouse button to an action.
    /// </summary>
    public void BindMouse(string action, MouseButton button) => Bind(action, new MouseBinding(button));

    /// <summary>
    /// Binds an arbitrary input binding to an action.
    /// </summary>
    public void Bind(string action, InputBinding binding)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentNullException.ThrowIfNull(binding);

        if (!_bindings.TryGetValue(action, out var list))
        {
            list = [];
            _bindings[action] = list;
        }

        if (!list.Contains(binding))
            list.Add(binding);
    }

    /// <summary>
    /// Clears bindings for a specific action.
    /// </summary>
    public bool ClearBindings(string action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        return _bindings.Remove(action);
    }

    /// <summary>
    /// Updates action states from current raw input.
    /// Call once per frame after <see cref="InputState.Update"/>.
    /// </summary>
    public void Update(InputState rawInput)
    {
        ArgumentNullException.ThrowIfNull(rawInput);

        foreach (var (action, list) in _bindings)
        {
            bool wasDown = _down.TryGetValue(action, out var oldValue) && oldValue;
            bool isDown = false;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsDown(rawInput))
                {
                    isDown = true;
                    break;
                }
            }

            _down[action] = isDown;
            _pressed[action] = isDown && !wasDown;
            _released[action] = !isDown && wasDown;
        }
    }

    /// <summary>
    /// Returns whether any binding for the action is currently held.
    /// </summary>
    public bool Down(string action)
        => _down.TryGetValue(action, out var value) && value;

    /// <summary>
    /// Returns whether the action transitioned from up to down this frame.
    /// </summary>
    public bool Pressed(string action)
        => _pressed.TryGetValue(action, out var value) && value;

    /// <summary>
    /// Returns whether the action transitioned from down to up this frame.
    /// </summary>
    public bool Released(string action)
        => _released.TryGetValue(action, out var value) && value;

    /// <summary>
    /// Builds an axis value from two actions.
    /// </summary>
    public float Axis(string negativeAction, string positiveAction)
    {
        float value = 0f;
        if (Down(negativeAction)) value -= 1f;
        if (Down(positiveAction)) value += 1f;
        return Math.Clamp(value, -1f, 1f);
    }

    /// <summary>
    /// Builds a normalized movement vector from four actions.
    /// </summary>
    public Vector2 Vector(string leftAction, string rightAction, string upAction, string downAction)
    {
        var v = new Vector2(
            Axis(leftAction, rightAction),
            Axis(upAction, downAction));

        return v == Vector2.Zero ? v : Vector2.Normalize(v);
    }
}
