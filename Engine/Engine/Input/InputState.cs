using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Input;

/// <summary>
/// Tracks raw keyboard and mouse states across frames.
/// </summary>
public sealed class InputState
{
    private KeyboardState _prevKeyboard;
    private KeyboardState _curKeyboard;

    private MouseState _prevMouse;
    private MouseState _curMouse;

    /// <summary>
    /// Captures keyboard and mouse for the current frame.
    /// </summary>
    public void Update()
    {
        _prevKeyboard = _curKeyboard;
        _curKeyboard = Keyboard.GetState();

        _prevMouse = _curMouse;
        _curMouse = Mouse.GetState();
    }

    /// <summary>
    /// Returns whether a key is currently held.
    /// </summary>
    public bool Down(Keys key) => _curKeyboard.IsKeyDown(key);

    /// <summary>
    /// Returns whether a key transitioned from up to down this frame.
    /// </summary>
    public bool Pressed(Keys key)
        => _curKeyboard.IsKeyDown(key) && !_prevKeyboard.IsKeyDown(key);

    /// <summary>
    /// Returns whether a key transitioned from down to up this frame.
    /// </summary>
    public bool Released(Keys key)
        => !_curKeyboard.IsKeyDown(key) && _prevKeyboard.IsKeyDown(key);

    /// <summary>
    /// Gets mouse position in backbuffer/screen coordinates.
    /// </summary>
    public Point MouseScreenPos => _curMouse.Position;

    /// <summary>
    /// Returns whether left mouse button is held.
    /// </summary>
    public bool LeftDown => _curMouse.LeftButton == ButtonState.Pressed;

    /// <summary>
    /// Returns whether left mouse button was clicked this frame.
    /// </summary>
    public bool LeftClicked
        => _curMouse.LeftButton == ButtonState.Pressed
        && _prevMouse.LeftButton == ButtonState.Released;

    /// <summary>
    /// Returns whether right mouse button is held.
    /// </summary>
    public bool RightDown => _curMouse.RightButton == ButtonState.Pressed;

    /// <summary>
    /// Returns whether middle mouse button is held.
    /// </summary>
    public bool MiddleDown => _curMouse.MiddleButton == ButtonState.Pressed;

    /// <summary>
    /// Returns whether the specified mouse button is currently held.
    /// </summary>
    public bool MouseDown(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => _curMouse.LeftButton == ButtonState.Pressed,
            MouseButton.Right => _curMouse.RightButton == ButtonState.Pressed,
            MouseButton.Middle => _curMouse.MiddleButton == ButtonState.Pressed,
            MouseButton.XButton1 => _curMouse.XButton1 == ButtonState.Pressed,
            MouseButton.XButton2 => _curMouse.XButton2 == ButtonState.Pressed,
            _ => false
        };
    }

    /// <summary>
    /// Returns whether the specified mouse button transitioned to pressed this frame.
    /// </summary>
    public bool MousePressed(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => _curMouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released,
            MouseButton.Right => _curMouse.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released,
            MouseButton.Middle => _curMouse.MiddleButton == ButtonState.Pressed && _prevMouse.MiddleButton == ButtonState.Released,
            MouseButton.XButton1 => _curMouse.XButton1 == ButtonState.Pressed && _prevMouse.XButton1 == ButtonState.Released,
            MouseButton.XButton2 => _curMouse.XButton2 == ButtonState.Pressed && _prevMouse.XButton2 == ButtonState.Released,
            _ => false
        };
    }

    /// <summary>
    /// Converts current mouse screen position into virtual-canvas coordinates.
    /// </summary>
    public Point? GetMouseVirtualPos(Rectangle destRect, int virtualWidth, int virtualHeight)
    {
        var p = MouseScreenPos;

        if (!destRect.Contains(p))
            return null;

        float nx = (p.X - destRect.X) / (float)destRect.Width;
        float ny = (p.Y - destRect.Y) / (float)destRect.Height;

        int vx = (int)(nx * virtualWidth);
        int vy = (int)(ny * virtualHeight);

        vx = Math.Clamp(vx, 0, virtualWidth - 1);
        vy = Math.Clamp(vy, 0, virtualHeight - 1);

        return new Point(vx, vy);
    }
}
