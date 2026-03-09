using System;
using System.Collections.Generic;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sandbox.Game.Config;

namespace Sandbox.Game;

public sealed class SandboxGame : ExtendedGameHost
{
    private static readonly SandboxGameSettings Settings = SandboxGameSettings.CreateDefault();

    private readonly SandboxScene _scene = new(Settings, TiledMapAuthoringProfile.Default);
    private readonly IReadOnlyDictionary<string, string> _keysByAction = BuildInputKeyMap(Settings.Input);
    private GameFlowState _state = Settings.Debug.SkipMainMenu ? GameFlowState.Playing : GameFlowState.MainMenu;
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
        graphics.PreferredBackBufferWidth = Settings.Window.BackBufferWidth;
        graphics.PreferredBackBufferHeight = Settings.Window.BackBufferHeight;
        graphics.ApplyChanges();
        Window.AllowUserResizing = Settings.Window.AllowUserResizing;
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
        if (context.Input.Pressed(Settings.Scene.ExitInputActionName))
        {
            Exit();
            return;
        }

        if (context.Input.Pressed(Settings.Menu.ConfirmInputActionName) ||
            context.Input.Pressed(Settings.Scene.ActionInputActionName))
        {
            StartGame();
            return;
        }

        MouseState mouse = Mouse.GetState();
        bool leftDown = mouse.LeftButton == ButtonState.Pressed;
        bool leftPressed = leftDown && !_wasLeftMouseDown;
        _wasLeftMouseDown = leftDown;

        MainMenuLayout layout = BuildMainMenuLayout(GraphicsDevice.Viewport);
        if (leftPressed && layout.StartButtonRect.Contains(mouse.Position))
            StartGame();
    }

    private void DrawMainMenu(EngineFrameContext context)
    {
        Viewport viewport = GraphicsDevice.Viewport;
        MainMenuLayout layout = BuildMainMenuLayout(viewport);
        bool startHovered = layout.StartButtonRect.Contains(Mouse.GetState().Position);

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

        Color buttonFill = startHovered ? new Color(91, 112, 150, 235) : new Color(63, 81, 112, 230);
        DrawPanel(context.SpriteBatch, layout.StartButtonRect, buttonFill, new Color(129, 147, 181));
        string startText = "Start";
        Vector2 startSize = _menuFont.MeasureString(startText);
        Vector2 startPos = new(
            layout.StartButtonRect.Center.X - startSize.X / 2f,
            layout.StartButtonRect.Center.Y - startSize.Y / 2f);
        context.SpriteBatch.DrawString(_menuFont, startText, startPos, new Color(245, 249, 255));

        string startKey = ResolveKey(Settings.Menu.ConfirmInputActionName);
        string quitKey = ResolveKey(Settings.Scene.ExitInputActionName);
        string hints = $"[{startKey}] Start  [{quitKey}] Quit";
        Vector2 hintsSize = _menuFont.MeasureString(hints);
        Vector2 hintsPos = new(layout.PanelRect.Center.X - hintsSize.X / 2f, layout.PanelRect.Bottom - 32);
        context.SpriteBatch.DrawString(_menuFont, hints, hintsPos, new Color(168, 182, 210));
        context.SpriteBatch.End();
    }

    private static MainMenuLayout BuildMainMenuLayout(Viewport viewport)
    {
        int panelWidth = Math.Clamp((int)(viewport.Width * 0.42f), 340, 560);
        int panelHeight = Math.Clamp((int)(viewport.Height * 0.48f), 230, 360);
        Rectangle panelRect = new(
            (viewport.Width - panelWidth) / 2,
            (viewport.Height - panelHeight) / 2,
            panelWidth,
            panelHeight);

        int buttonWidth = Math.Clamp(panelRect.Width - 130, 180, 280);
        Rectangle startButtonRect = new(
            panelRect.Center.X - buttonWidth / 2,
            panelRect.Bottom - 94,
            buttonWidth,
            48);

        return new MainMenuLayout(panelRect, startButtonRect);
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

    private readonly record struct MainMenuLayout(Rectangle PanelRect, Rectangle StartButtonRect);

    private enum GameFlowState
    {
        MainMenu,
        Playing
    }
}
