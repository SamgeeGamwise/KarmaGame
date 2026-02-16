using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sandbox.Game;

public sealed class SandboxGame : ExtendedGameHost
{
    private const string DefaultMapAsset = "Town";

    private readonly SandboxScene _scene = new(DefaultMapAsset, TiledMapAuthoringProfile.Default);

    public SandboxGame() : base(640, 360)
    {
    }

    protected override Color ClearColor => new(24, 29, 38);

    protected override bool AutoBeginWorldSpriteBatch => false;

    protected override void ConfigureWindow(GraphicsDeviceManager graphics)
    {
        base.ConfigureWindow(graphics);
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.ApplyChanges();
        Window.AllowUserResizing = true;
    }

    protected override void ConfigureInput(InputBridge input)
    {
        input.BindKey("move_left", Keys.A);
        input.BindKey("move_right", Keys.D);
        input.BindKey("move_up", Keys.W);
        input.BindKey("move_down", Keys.S);
        input.BindKey("run", Keys.LeftShift);
        input.BindKey("exit", Keys.Escape);
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
