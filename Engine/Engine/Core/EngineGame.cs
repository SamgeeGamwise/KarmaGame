using Engine.Assets;
using Engine.Graphics;
using Engine.Input;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core;

/// <summary>
/// High-level MonoGame host that wires engine systems together.
/// </summary>
/// <remarks>
/// <para>
/// Override <see cref="CreateInitialScene"/> to provide your root node.
/// </para>
/// <para>
/// This class aims to reduce boilerplate by handling input updates, scene traversal,
/// virtual resolution rendering, and world/screen render passes.
/// </para>
/// </remarks>
public abstract class EngineGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly bool _useVirtualResolution;
    private RenderTarget2D? _virtualCanvas;

    /// <summary>
    /// Initializes a new engine game host.
    /// </summary>
    protected EngineGame(int virtualWidth = 640, int virtualHeight = 360, bool useVirtualResolution = true)
    {
        _graphics = new GraphicsDeviceManager(this);
        _useVirtualResolution = useVirtualResolution;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Assets = new AssetSet();
        RawInput = new InputState();
        Input = new InputMap();
        SceneTree = new SceneTree();
        VirtualScaler = new VirtualResolutionScaler(virtualWidth, virtualHeight);
        Context = new EngineContext(this, Assets, RawInput, Input, SceneTree, VirtualScaler);
    }

    /// <summary>
    /// Gets runtime asset services.
    /// </summary>
    public AssetSet Assets { get; }

    /// <summary>
    /// Gets raw keyboard/mouse input.
    /// </summary>
    public InputState RawInput { get; }

    /// <summary>
    /// Gets action map input.
    /// </summary>
    public InputMap Input { get; }

    /// <summary>
    /// Gets scene graph.
    /// </summary>
    public SceneTree SceneTree { get; }

    /// <summary>
    /// Gets virtual resolution helper.
    /// </summary>
    public VirtualResolutionScaler VirtualScaler { get; }

    /// <summary>
    /// Gets per-frame context object passed to nodes.
    /// </summary>
    public EngineContext Context { get; }

    /// <summary>
    /// Gets or sets clear color used for world pass.
    /// </summary>
    public Color ClearColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets or sets whether world should use point sampling.
    /// </summary>
    public bool PixelPerfectSampling { get; set; } = true;

    /// <summary>
    /// Called before initialization to customize graphics manager.
    /// </summary>
    protected virtual void ConfigureGraphics(GraphicsDeviceManager graphics) { }

    /// <summary>
    /// Called during loading to register action bindings.
    /// </summary>
    protected virtual void ConfigureInput(InputMap input) { }

    /// <summary>
    /// Creates the first root scene.
    /// </summary>
    protected abstract Node CreateInitialScene();

    /// <summary>
    /// Replaces current root scene.
    /// </summary>
    protected void ChangeScene(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);
        SceneTree.SetRoot(root, Context);
    }

    /// <inheritdoc />
    protected override void Initialize()
    {
        ConfigureGraphics(_graphics);
        _graphics.ApplyChanges();

        RecalculateDestinationRect();
        Window.ClientSizeChanged += (_, _) => RecalculateDestinationRect();

        base.Initialize();
    }

    /// <inheritdoc />
    protected override void LoadContent()
    {
        Assets.Initialize(GraphicsDevice);
        ConfigureInput(Input);

        if (_useVirtualResolution)
            RecreateVirtualCanvas();

        SceneTree.SetRoot(CreateInitialScene(), Context);
    }

    /// <inheritdoc />
    protected override void Update(GameTime gameTime)
    {
        RawInput.Update();
        Input.Update(RawInput);
        Context.BeginFrame(gameTime);

        SceneTree.Update(Context);
        base.Update(gameTime);
    }

    /// <inheritdoc />
    protected override void Draw(GameTime gameTime)
    {
        Context.BeginFrame(gameTime);

        if (_useVirtualResolution)
        {
            GraphicsDevice.SetRenderTarget(_virtualCanvas);
            GraphicsDevice.Clear(ClearColor);

            DrawScenePasses();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            var sampling = PixelPerfectSampling ? SamplerState.PointClamp : SamplerState.LinearClamp;
            Assets.SpriteBatch.Begin(samplerState: sampling, blendState: BlendState.Opaque);
            Assets.SpriteBatch.Draw(_virtualCanvas, VirtualScaler.DestinationRect, Color.White);
            Assets.SpriteBatch.End();
        }
        else
        {
            GraphicsDevice.Clear(ClearColor);
            DrawScenePasses();
        }

        base.Draw(gameTime);
    }

    private void DrawScenePasses()
    {
        int drawWidth = _useVirtualResolution ? VirtualScaler.VirtualWidth : GraphicsDevice.Viewport.Width;
        int drawHeight = _useVirtualResolution ? VirtualScaler.VirtualHeight : GraphicsDevice.Viewport.Height;

        Matrix view = SceneTree.ActiveCamera?.GetViewMatrix(drawWidth, drawHeight) ?? Matrix.Identity;
        var sampling = PixelPerfectSampling ? SamplerState.PointClamp : SamplerState.LinearClamp;

        // World pass: affected by active camera.
        Assets.SpriteBatch.Begin(
            samplerState: sampling,
            blendState: BlendState.AlphaBlend,
            transformMatrix: view);
        SceneTree.Draw(Context, RenderSpace.World);
        Assets.SpriteBatch.End();

        // Screen pass: UI/HUD style nodes.
        Assets.SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            blendState: BlendState.AlphaBlend,
            transformMatrix: Matrix.Identity);
        SceneTree.Draw(Context, RenderSpace.Screen);
        Assets.SpriteBatch.End();
    }

    private void RecreateVirtualCanvas()
    {
        _virtualCanvas?.Dispose();
        _virtualCanvas = new RenderTarget2D(
            GraphicsDevice,
            VirtualScaler.VirtualWidth,
            VirtualScaler.VirtualHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.DiscardContents);
    }

    private void RecalculateDestinationRect()
    {
        int backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        int backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        VirtualScaler.Recalculate(backBufferWidth, backBufferHeight);
    }
}
