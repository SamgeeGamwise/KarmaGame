using Microsoft.Xna.Framework.Input;

namespace Engine.Input;

/// <summary>
/// Base input binding used by <see cref="InputMap"/>.
/// </summary>
public abstract record InputBinding
{
    /// <summary>
    /// Returns whether the binding is currently down.
    /// </summary>
    public abstract bool IsDown(InputState input);
}

/// <summary>
/// Keyboard binding.
/// </summary>
public sealed record KeyBinding(Keys Key) : InputBinding
{
    /// <inheritdoc />
    public override bool IsDown(InputState input) => input.Down(Key);
}

/// <summary>
/// Mouse binding.
/// </summary>
public sealed record MouseBinding(MouseButton Button) : InputBinding
{
    /// <inheritdoc />
    public override bool IsDown(InputState input) => input.MouseDown(Button);
}
