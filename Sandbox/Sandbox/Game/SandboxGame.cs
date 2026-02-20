using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sandbox.Game.Config;

namespace Sandbox.Game;

public sealed class SandboxGame : ExtendedGameHost
{
    private static readonly SandboxGameSettings Settings = SandboxGameSettingsLoader.Load();

    private readonly SandboxScene _scene = new(Settings, TiledMapAuthoringProfile.Default);

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
                Console.WriteLine($"[Sandbox] unknown key '{binding.Key}' for action '{binding.Action}' in gameplay settings.");
                continue;
            }

            input.BindKey(binding.Action, key);
        }
    }

    protected override void OnLoadContent()
    {
        _scene.LoadContent(Content, GraphicsDevice);
    }

    protected override void OnUpdateGame(EngineFrameContext context)
    {
        _scene.Update(context, Exit);
    }

    protected override void DrawWorld(EngineFrameContext context)
    {
        _scene.Draw(context);
    }

    protected override void DrawScreen(EngineFrameContext context)
    {
        _scene.DrawScreen(context);
    }
}
