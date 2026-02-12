using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace Engine.Core;

public abstract class ExtendedGameHost : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private KeyboardState _previousKeyboard;

    protected ExtendedGameHost(int virtualWidth = 640, int virtualHeight = 360)
    {
        VirtualWidth = virtualWidth;
        VirtualHeight = virtualHeight;

        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    public int VirtualWidth { get; }

    public int VirtualHeight { get; }

    public SpriteBatch SpriteBatch { get; private set; } = null!;

    public BoxingViewportAdapter ViewportAdapter { get; private set; } = null!;

    public OrthographicCamera Camera { get; private set; } = null!;

    public InputBridge Input { get; } = new();

    protected sealed override void Initialize()
    {
        ViewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, VirtualWidth, VirtualHeight);
        Camera = new OrthographicCamera(ViewportAdapter);

        ConfigureWindow(_graphics);
        ConfigureInput(Input);
        OnInitialize();
        base.Initialize();
    }

    protected sealed override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        OnLoadContent();
    }

    protected sealed override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();
        Input.Update(keyboard, _previousKeyboard);
        _previousKeyboard = keyboard;

        var context = new EngineFrameContext(gameTime, Content, SpriteBatch, Camera, Input, VirtualWidth, VirtualHeight);
        OnUpdateGame(context);
        base.Update(gameTime);
    }

    protected sealed override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(ClearColor);

        var context = new EngineFrameContext(gameTime, Content, SpriteBatch, Camera, Input, VirtualWidth, VirtualHeight);

        if (AutoBeginWorldSpriteBatch)
            SpriteBatch.Begin(transformMatrix: Camera.GetViewMatrix());

        DrawWorld(context);

        if (AutoBeginWorldSpriteBatch)
            SpriteBatch.End();

        DrawScreen(context);

        base.Draw(gameTime);
    }

    protected virtual Color ClearColor => Color.Black;

    /// <summary>
    /// Controls whether the host automatically wraps world draw in SpriteBatch Begin/End.
    /// Disable when using renderers that manage their own draw batching (for example TiledMapRenderer).
    /// </summary>
    protected virtual bool AutoBeginWorldSpriteBatch => true;

    protected virtual void ConfigureWindow(GraphicsDeviceManager graphics)
    {
        graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
    }

    protected virtual void ConfigureInput(InputBridge input)
    {
    }

    protected virtual void OnInitialize()
    {
    }

    protected virtual void OnLoadContent()
    {
    }

    protected virtual void OnUpdateGame(EngineFrameContext context)
    {
    }

    protected virtual void DrawWorld(EngineFrameContext context)
    {
    }

    protected virtual void DrawScreen(EngineFrameContext context)
    {
    }
}
