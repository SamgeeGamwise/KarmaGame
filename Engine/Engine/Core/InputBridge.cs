using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Core;

public sealed class InputBridge
{
    private readonly Dictionary<string, List<Keys>> _keysByAction = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _down = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _pressed = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _released = new(StringComparer.Ordinal);
    private bool _scrollUp = false;
    private bool _scrollDown = false;

    public void BindKey(string action, Keys key)
    {
        if (!_keysByAction.TryGetValue(action, out var keys))
        {
            keys = [];
            _keysByAction[action] = keys;
        }

        if (!keys.Contains(key))
            keys.Add(key);
    }

    public bool Down(string action) => _down.TryGetValue(action, out bool value) && value;

    public bool Pressed(string action) => _pressed.TryGetValue(action, out bool value) && value;

    public bool Released(string action) => _released.TryGetValue(action, out bool value) && value;

    public bool ScrollingUp => _scrollUp;

    public bool ScrollingDown => _scrollDown;

    public Vector2 Vector(string leftAction, string rightAction, string upAction, string downAction)
    {
        float x = Axis(leftAction, rightAction);
        float y = Axis(upAction, downAction);
        Vector2 v = new(x, y);
        return v == Vector2.Zero ? v : Vector2.Normalize(v);
    }

    internal void Update(KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
    {
        foreach ((string action, List<Keys> keys) in _keysByAction)
        {
            bool isDown = false;
            bool wasDown = false;

            foreach (Keys key in keys)
            {
                isDown |= currentKeyboard.IsKeyDown(key);
                wasDown |= previousKeyboard.IsKeyDown(key);
            }

            _down[action] = isDown;
            _pressed[action] = isDown && !wasDown;
            _released[action] = !isDown && wasDown;
        }

        _scrollUp = currentMouse.ScrollWheelValue > previousMouse.ScrollWheelValue;
        _scrollDown = currentMouse.ScrollWheelValue < previousMouse.ScrollWheelValue;
    }

    private float Axis(string negativeAction, string positiveAction)
    {
        float value = 0f;
        if (Down(negativeAction))
            value -= 1f;
        if (Down(positiveAction))
            value += 1f;
        return Math.Clamp(value, -1f, 1f);
    }
}
