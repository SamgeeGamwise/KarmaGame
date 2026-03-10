using System;
using System.Collections.Generic;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sandbox.Game.Config;
using WindowDisplayMode = Sandbox.Game.Config.DisplayMode;

namespace Sandbox.Game;

public sealed class SandboxGame : ExtendedGameHost
{
    private static readonly SandboxGameSettings Settings = SandboxGameSettings.CreateDefault();

    private readonly SandboxScene _scene = new(Settings, TiledMapAuthoringProfile.Default);
    private readonly IReadOnlyDictionary<string, string> _keysByAction = BuildInputKeyMap(Settings.Input);
    private GraphicsDeviceManager _graphics = null!;
    private WindowDisplayMode _displayMode = Settings.Window.StartDisplayMode;
    private GameFlowState _state = Settings.Debug.SkipMainMenu ? GameFlowState.Playing : GameFlowState.MainMenu;
    private MainMenuView _menuView = MainMenuView.Root;
    private int _rootSelection;
    private int _settingsSelection;
    private SpriteFont _menuFont = null!;
    private Texture2D _menuPixel = null!;
    private bool _wasLeftMouseDown;

    public SandboxGame() : base(Settings.Window.VirtualWidth, Settings.Window.VirtualHeight)
    {
    }

    protected override Color ClearColor => Settings.Render.ClearColor.ToColor();

    protected override bool AutoBeginWorldSpriteBatch => false;

    protected override void ConfigureWindow(GraphicsDeviceManager graphics)
    {
        base.ConfigureWindow(graphics);
        _graphics = graphics;
        ApplyDisplayMode(_displayMode, applyChanges: false);
        graphics.ApplyChanges();
    }

    protected override void ConfigureInput(InputBridge input)
    {
        foreach (InputBindingSettings binding in Settings.Input.Bindings)
        {
            if (!Enum.TryParse(binding.Key, ignoreCase: true, out Keys key))
            {
                Console.WriteLine($"[Sandbox] unknown key '{binding.Key}' for action '{binding.Action}' in default settings.");
                continue;
            }

            input.BindKey(binding.Action, key);
        }
    }

    protected override void OnLoadContent()
    {
        _scene.LoadContent(Content, GraphicsDevice);
        _menuFont = Content.Load<SpriteFont>("UIFont");
        _menuPixel = new Texture2D(GraphicsDevice, 1, 1);
        _menuPixel.SetData([Color.White]);
    }

    protected override void OnUpdateGame(EngineFrameContext context)
    {
        if (_state == GameFlowState.Playing)
        {
            _scene.Update(context, Exit);
            return;
        }

        UpdateMainMenu(context);
    }

    protected override void DrawWorld(EngineFrameContext context)
    {
        if (_state == GameFlowState.Playing)
            _scene.Draw(context);
    }

    protected override void DrawScreen(EngineFrameContext context)
    {
        if (_state == GameFlowState.Playing)
        {
            _scene.DrawScreen(context);
            return;
        }

        DrawMainMenu(context);
    }

    private void UpdateMainMenu(EngineFrameContext context)
    {
        MouseState mouse = Mouse.GetState();
        bool leftDown = mouse.LeftButton == ButtonState.Pressed;
        bool leftPressed = leftDown && !_wasLeftMouseDown;
        _wasLeftMouseDown = leftDown;

        if (_menuView == MainMenuView.Root)
        {
            UpdateRootMenu(context, leftPressed, mouse.Position);
            return;
        }

        UpdateSettingsMenu(context, leftPressed, mouse.Position);
    }

    private void UpdateRootMenu(EngineFrameContext context, bool leftPressed, Point mousePosition)
    {
        if (context.Input.Pressed(Settings.Scene.ExitInputActionName) ||
            context.Input.Pressed(Settings.Menu.BackInputActionName))
        {
            Exit();
            return;
        }

        if (context.Input.Pressed("move_down") ||
            context.Input.Pressed("move_up"))
        {
            _rootSelection = 1 - _rootSelection;
        }

        MainMenuLayout layout = BuildMainMenuLayout(GraphicsDevice.Viewport);
        if (layout.StartButtonRect.Contains(mousePosition))
            _rootSelection = 0;
        else if (layout.SettingsButtonRect.Contains(mousePosition))
            _rootSelection = 1;

        if (leftPressed)
        {
            if (layout.StartButtonRect.Contains(mousePosition))
            {
                StartGame();
                return;
            }

            if (layout.SettingsButtonRect.Contains(mousePosition))
            {
                _menuView = MainMenuView.Settings;
                _settingsSelection = 0;
                return;
            }
        }

        if (context.Input.Pressed(Settings.Menu.ConfirmInputActionName) ||
            context.Input.Pressed(Settings.Scene.ActionInputActionName))
        {
            if (_rootSelection == 0)
            {
                StartGame();
                return;
            }

            _menuView = MainMenuView.Settings;
            _settingsSelection = 0;
        }
    }

    private void UpdateSettingsMenu(EngineFrameContext context, bool leftPressed, Point mousePosition)
    {
        if (context.Input.Pressed(Settings.Menu.BackInputActionName) ||
            context.Input.Pressed(Settings.Scene.ExitInputActionName))
        {
            _menuView = MainMenuView.Root;
            return;
        }

        if (context.Input.Pressed("move_down") ||
            context.Input.Pressed("move_up"))
        {
            _settingsSelection = 1 - _settingsSelection;
        }

        SettingsMenuLayout layout = BuildSettingsMenuLayout(GraphicsDevice.Viewport);
        if (layout.WindowedButtonRect.Contains(mousePosition) ||
            layout.FullscreenButtonRect.Contains(mousePosition) ||
            layout.BorderlessButtonRect.Contains(mousePosition))
        {
            _settingsSelection = 0;
        }
        else if (layout.BackButtonRect.Contains(mousePosition))
        {
            _settingsSelection = 1;
        }

        if (_settingsSelection == 0 &&
            (context.Input.Pressed("move_left") || context.Input.Pressed("move_right") ||
             context.Input.Pressed(Settings.Menu.ConfirmInputActionName) ||
             context.Input.Pressed(Settings.Scene.ActionInputActionName)))
        {
            bool cycleBackward = context.Input.Pressed("move_left");
            ApplyDisplayMode(CycleDisplayMode(_displayMode, cycleBackward));
        }
        else if (_settingsSelection == 1 &&
                 (context.Input.Pressed(Settings.Menu.ConfirmInputActionName) ||
                  context.Input.Pressed(Settings.Scene.ActionInputActionName)))
        {
            _menuView = MainMenuView.Root;
            return;
        }

        if (!leftPressed)
            return;

        if (layout.WindowedButtonRect.Contains(mousePosition))
            ApplyDisplayMode(WindowDisplayMode.Windowed);
        else if (layout.FullscreenButtonRect.Contains(mousePosition))
            ApplyDisplayMode(WindowDisplayMode.Fullscreen);
        else if (layout.BorderlessButtonRect.Contains(mousePosition))
            ApplyDisplayMode(WindowDisplayMode.Borderless);
        else if (layout.BackButtonRect.Contains(mousePosition))
            _menuView = MainMenuView.Root;
    }

    private void DrawMainMenu(EngineFrameContext context)
    {
        if (_menuView == MainMenuView.Root)
        {
            DrawRootMenu(context);
            return;
        }

        DrawSettingsMenu(context);
    }

    private void DrawRootMenu(EngineFrameContext context)
    {
        Viewport viewport = GraphicsDevice.Viewport;
        MainMenuLayout layout = BuildMainMenuLayout(viewport);
        Point mousePosition = Mouse.GetState().Position;
        bool startHovered = layout.StartButtonRect.Contains(mousePosition);
        bool settingsHovered = layout.SettingsButtonRect.Contains(mousePosition);

        context.SpriteBatch.Begin();
        context.SpriteBatch.Draw(_menuPixel, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(10, 14, 22));

        Rectangle shadow = new(layout.PanelRect.X + 4, layout.PanelRect.Y + 4, layout.PanelRect.Width, layout.PanelRect.Height);
        context.SpriteBatch.Draw(_menuPixel, shadow, new Color(0, 0, 0, 92));
        DrawPanel(context.SpriteBatch, layout.PanelRect, new Color(20, 24, 33, 244), new Color(86, 103, 132));

        string title = "Karma";
        Vector2 titleSize = _menuFont.MeasureString(title);
        Vector2 titlePos = new(layout.PanelRect.Center.X - titleSize.X / 2f, layout.PanelRect.Y + 32);
        context.SpriteBatch.DrawString(_menuFont, title, titlePos, new Color(243, 248, 255));

        string subtitle = "Build your settlement";
        Vector2 subtitleSize = _menuFont.MeasureString(subtitle);
        Vector2 subtitlePos = new(layout.PanelRect.Center.X - subtitleSize.X / 2f, titlePos.Y + 30);
        context.SpriteBatch.DrawString(_menuFont, subtitle, subtitlePos, new Color(178, 193, 223));

        bool startSelected = _rootSelection == 0 || startHovered;
        bool settingsSelected = _rootSelection == 1 || settingsHovered;
        Color startFill = startSelected ? new Color(91, 112, 150, 235) : new Color(63, 81, 112, 230);
        Color settingsFill = settingsSelected ? new Color(91, 112, 150, 235) : new Color(63, 81, 112, 230);

        DrawPanel(context.SpriteBatch, layout.StartButtonRect, startFill, new Color(129, 147, 181));
        string startText = "Start";
        Vector2 startSize = _menuFont.MeasureString(startText);
        Vector2 startPos = new(
            layout.StartButtonRect.Center.X - startSize.X / 2f,
            layout.StartButtonRect.Center.Y - startSize.Y / 2f);
        context.SpriteBatch.DrawString(_menuFont, startText, startPos, new Color(245, 249, 255));

        DrawPanel(context.SpriteBatch, layout.SettingsButtonRect, settingsFill, new Color(129, 147, 181));
        string settingsText = "Settings";
        Vector2 settingsSize = _menuFont.MeasureString(settingsText);
        Vector2 settingsPos = new(
            layout.SettingsButtonRect.Center.X - settingsSize.X / 2f,
            layout.SettingsButtonRect.Center.Y - settingsSize.Y / 2f);
        context.SpriteBatch.DrawString(_menuFont, settingsText, settingsPos, new Color(245, 249, 255));

        string upKey = ResolveKey("move_up");
        string downKey = ResolveKey("move_down");
        string confirmKey = ResolveKey(Settings.Menu.ConfirmInputActionName);
        string quitKey = ResolveKey(Settings.Scene.ExitInputActionName);
        string hints = $"[{upKey}/{downKey}] Select  [{confirmKey}] Confirm  [{quitKey}] Quit";
        Vector2 hintsSize = _menuFont.MeasureString(hints);
        Vector2 hintsPos = new(layout.PanelRect.Center.X - hintsSize.X / 2f, layout.PanelRect.Bottom - 32);
        context.SpriteBatch.DrawString(_menuFont, hints, hintsPos, new Color(168, 182, 210));
        context.SpriteBatch.End();
    }

    private void DrawSettingsMenu(EngineFrameContext context)
    {
        Viewport viewport = GraphicsDevice.Viewport;
        SettingsMenuLayout layout = BuildSettingsMenuLayout(viewport);
        Point mousePosition = Mouse.GetState().Position;

        context.SpriteBatch.Begin();
        context.SpriteBatch.Draw(_menuPixel, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(10, 14, 22));

        Rectangle shadow = new(layout.PanelRect.X + 4, layout.PanelRect.Y + 4, layout.PanelRect.Width, layout.PanelRect.Height);
        context.SpriteBatch.Draw(_menuPixel, shadow, new Color(0, 0, 0, 92));
        DrawPanel(context.SpriteBatch, layout.PanelRect, new Color(20, 24, 33, 244), new Color(86, 103, 132));

        string title = "Settings";
        Vector2 titleSize = _menuFont.MeasureString(title);
        Vector2 titlePos = new(layout.PanelRect.Center.X - titleSize.X / 2f, layout.PanelRect.Y + 26);
        context.SpriteBatch.DrawString(_menuFont, title, titlePos, new Color(243, 248, 255));

        string rowLabel = "Display Mode";
        Vector2 rowLabelSize = _menuFont.MeasureString(rowLabel);
        Vector2 rowLabelPos = new(layout.PanelRect.Center.X - rowLabelSize.X / 2f, layout.WindowedButtonRect.Y - 30);
        context.SpriteBatch.DrawString(_menuFont, rowLabel, rowLabelPos, new Color(194, 206, 231));

        DrawDisplayModeButton(context.SpriteBatch, layout.WindowedButtonRect, "Windowed", WindowDisplayMode.Windowed, mousePosition);
        DrawDisplayModeButton(context.SpriteBatch, layout.FullscreenButtonRect, "Fullscreen", WindowDisplayMode.Fullscreen, mousePosition);
        DrawDisplayModeButton(context.SpriteBatch, layout.BorderlessButtonRect, "Borderless", WindowDisplayMode.Borderless, mousePosition);

        bool backSelected = _settingsSelection == 1 || layout.BackButtonRect.Contains(mousePosition);
        Color backFill = backSelected ? new Color(91, 112, 150, 235) : new Color(63, 81, 112, 230);
        DrawPanel(context.SpriteBatch, layout.BackButtonRect, backFill, new Color(129, 147, 181));
        string backText = "Back";
        Vector2 backSize = _menuFont.MeasureString(backText);
        Vector2 backPos = new(
            layout.BackButtonRect.Center.X - backSize.X / 2f,
            layout.BackButtonRect.Center.Y - backSize.Y / 2f);
        context.SpriteBatch.DrawString(_menuFont, backText, backPos, new Color(245, 249, 255));

        string leftKey = ResolveKey("move_left");
        string rightKey = ResolveKey("move_right");
        string upKey = ResolveKey("move_up");
        string downKey = ResolveKey("move_down");
        string confirmKey = ResolveKey(Settings.Menu.ConfirmInputActionName);
        string backKey = ResolveKey(Settings.Menu.BackInputActionName);
        string hints = $"[{upKey}/{downKey}] Focus  [{leftKey}/{rightKey}] Change  [{confirmKey}] Select  [{backKey}] Back";
        Vector2 hintsSize = _menuFont.MeasureString(hints);
        Vector2 hintsPos = new(layout.PanelRect.Center.X - hintsSize.X / 2f, layout.PanelRect.Bottom - 32);
        context.SpriteBatch.DrawString(_menuFont, hints, hintsPos, new Color(168, 182, 210));
        context.SpriteBatch.End();
    }

    private void DrawDisplayModeButton(
        SpriteBatch spriteBatch,
        Rectangle rect,
        string label,
        WindowDisplayMode mode,
        Point mousePosition)
    {
        bool hovered = rect.Contains(mousePosition);
        bool selected = _displayMode == mode;
        bool focused = _settingsSelection == 0;
        Color fill = selected
            ? new Color(91, 112, 150, 235)
            : hovered || focused
                ? new Color(72, 90, 122, 220)
                : new Color(50, 64, 90, 210);

        DrawPanel(spriteBatch, rect, fill, new Color(129, 147, 181));
        Vector2 size = _menuFont.MeasureString(label);
        Vector2 pos = new(rect.Center.X - size.X / 2f, rect.Center.Y - size.Y / 2f);
        spriteBatch.DrawString(_menuFont, label, pos, new Color(245, 249, 255));
    }

    private static MainMenuLayout BuildMainMenuLayout(Viewport viewport)
    {
        int panelWidth = Math.Clamp((int)(viewport.Width * 0.42f), 340, 560);
        int panelHeight = Math.Clamp((int)(viewport.Height * 0.56f), 280, 400);
        Rectangle panelRect = new(
            (viewport.Width - panelWidth) / 2,
            (viewport.Height - panelHeight) / 2,
            panelWidth,
            panelHeight);

        int buttonWidth = Math.Clamp(panelRect.Width - 130, 180, 280);
        Rectangle startButtonRect = new(
            panelRect.Center.X - buttonWidth / 2,
            panelRect.Bottom - 144,
            buttonWidth,
            48);

        Rectangle settingsButtonRect = new(
            panelRect.Center.X - buttonWidth / 2,
            startButtonRect.Bottom + 14,
            buttonWidth,
            48);

        return new MainMenuLayout(panelRect, startButtonRect, settingsButtonRect);
    }

    private static SettingsMenuLayout BuildSettingsMenuLayout(Viewport viewport)
    {
        int panelWidth = Math.Clamp((int)(viewport.Width * 0.52f), 460, 700);
        int panelHeight = Math.Clamp((int)(viewport.Height * 0.62f), 320, 460);
        Rectangle panelRect = new(
            (viewport.Width - panelWidth) / 2,
            (viewport.Height - panelHeight) / 2,
            panelWidth,
            panelHeight);

        int groupX = panelRect.X + 40;
        int groupWidth = panelRect.Width - 80;
        const int gap = 10;
        int buttonWidth = (groupWidth - gap * 2) / 3;
        int buttonY = panelRect.Y + 132;
        Rectangle windowedButtonRect = new(groupX, buttonY, buttonWidth, 48);
        Rectangle fullscreenButtonRect = new(windowedButtonRect.Right + gap, buttonY, buttonWidth, 48);
        Rectangle borderlessButtonRect = new(fullscreenButtonRect.Right + gap, buttonY, buttonWidth, 48);

        int backWidth = Math.Clamp(panelRect.Width - 220, 180, 280);
        Rectangle backButtonRect = new(
            panelRect.Center.X - backWidth / 2,
            panelRect.Bottom - 92,
            backWidth,
            46);

        return new SettingsMenuLayout(
            panelRect,
            windowedButtonRect,
            fullscreenButtonRect,
            borderlessButtonRect,
            backButtonRect);
    }

    private void DrawPanel(SpriteBatch spriteBatch, Rectangle panel, Color fillColor, Color borderColor)
    {
        spriteBatch.Draw(_menuPixel, panel, fillColor);
        spriteBatch.Draw(_menuPixel, new Rectangle(panel.X, panel.Y, panel.Width, 2), new Color(211, 177, 103));
        spriteBatch.Draw(_menuPixel, new Rectangle(panel.X, panel.Bottom - 1, panel.Width, 1), borderColor * 0.7f);
        spriteBatch.Draw(_menuPixel, new Rectangle(panel.X, panel.Y, 1, panel.Height), borderColor * 0.65f);
        spriteBatch.Draw(_menuPixel, new Rectangle(panel.Right - 1, panel.Y, 1, panel.Height), borderColor * 0.65f);
    }

    private void StartGame()
    {
        _state = GameFlowState.Playing;
    }

    private void ApplyDisplayMode(WindowDisplayMode mode, bool applyChanges = true)
    {
        _displayMode = mode;
        var currentDisplay = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

        switch (mode)
        {
            case WindowDisplayMode.Windowed:
                _graphics.IsFullScreen = false;
                _graphics.HardwareModeSwitch = true;
                _graphics.PreferredBackBufferWidth = Settings.Window.BackBufferWidth;
                _graphics.PreferredBackBufferHeight = Settings.Window.BackBufferHeight;
                break;

            case WindowDisplayMode.Fullscreen:
                _graphics.HardwareModeSwitch = true;
                _graphics.IsFullScreen = true;
                _graphics.PreferredBackBufferWidth = currentDisplay.Width;
                _graphics.PreferredBackBufferHeight = currentDisplay.Height;
                break;

            case WindowDisplayMode.Borderless:
                _graphics.HardwareModeSwitch = false;
                _graphics.IsFullScreen = true;
                _graphics.PreferredBackBufferWidth = currentDisplay.Width;
                _graphics.PreferredBackBufferHeight = currentDisplay.Height;
                break;
        }

        if (applyChanges)
            _graphics.ApplyChanges();

        Window.AllowUserResizing = mode == WindowDisplayMode.Windowed && Settings.Window.AllowUserResizing;
    }

    private static WindowDisplayMode CycleDisplayMode(WindowDisplayMode current, bool backward)
    {
        WindowDisplayMode[] modes = [WindowDisplayMode.Windowed, WindowDisplayMode.Fullscreen, WindowDisplayMode.Borderless];
        int index = Array.IndexOf(modes, current);
        if (index < 0)
            index = 0;

        if (backward)
            index = (index - 1 + modes.Length) % modes.Length;
        else
            index = (index + 1) % modes.Length;

        return modes[index];
    }

    private string ResolveKey(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return "?";

        return _keysByAction.TryGetValue(actionName, out string? keyLabel) ? keyLabel : actionName;
    }

    private static IReadOnlyDictionary<string, string> BuildInputKeyMap(InputSettings inputSettings)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (InputBindingSettings binding in inputSettings.Bindings)
        {
            if (string.IsNullOrWhiteSpace(binding.Action) || string.IsNullOrWhiteSpace(binding.Key))
                continue;

            string keyLabel = FormatKeyLabel(binding.Key);
            if (!result.TryGetValue(binding.Action, out string? existing))
            {
                result[binding.Action] = keyLabel;
                continue;
            }

            if (existing.IndexOf(keyLabel, StringComparison.OrdinalIgnoreCase) >= 0)
                continue;

            result[binding.Action] = $"{existing}/{keyLabel}";
        }

        return result;
    }

    private static string FormatKeyLabel(string rawKey)
    {
        return rawKey.Trim() switch
        {
            "LeftShift" => "Shift",
            "RightShift" => "Shift",
            "LeftControl" => "Ctrl",
            "RightControl" => "Ctrl",
            "LeftAlt" => "Alt",
            "RightAlt" => "Alt",
            "Back" => "Backspace",
            "Return" => "Enter",
            "OemQuestion" => "?",
            "OemComma" => ",",
            "OemPeriod" => ".",
            _ => rawKey.Trim()
        };
    }

    private readonly record struct MainMenuLayout(Rectangle PanelRect, Rectangle StartButtonRect, Rectangle SettingsButtonRect);
    private readonly record struct SettingsMenuLayout(
        Rectangle PanelRect,
        Rectangle WindowedButtonRect,
        Rectangle FullscreenButtonRect,
        Rectangle BorderlessButtonRect,
        Rectangle BackButtonRect);

    private enum GameFlowState
    {
        MainMenu,
        Playing
    }

    private enum MainMenuView
    {
        Root,
        Settings
    }
}
