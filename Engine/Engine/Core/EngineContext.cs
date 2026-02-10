using Engine.Assets;
using Engine.Graphics;
using Engine.Input;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core;

/// <summary>
/// Shared per-frame context passed to nodes.
/// </summary>
public sealed class EngineContext
{
    /// <summary>
    /// Initializes a new context bound to a game host.
    /// </summary>
    public EngineContext(
        Game game,
        AssetSet assets,
        InputState rawInput,
        InputMap input,
        SceneTree sceneTree,
        VirtualResolutionScaler virtualScaler)
    {
        Game = game ?? throw new ArgumentNullException(nameof(game));
        Assets = assets ?? throw new ArgumentNullException(nameof(assets));
        RawInput = rawInput ?? throw new ArgumentNullException(nameof(rawInput));
        Input = input ?? throw new ArgumentNullException(nameof(input));
        SceneTree = sceneTree ?? throw new ArgumentNullException(nameof(sceneTree));
        VirtualScaler = virtualScaler ?? throw new ArgumentNullException(nameof(virtualScaler));
    }

    /// <summary>
    /// Gets the owning <see cref="Game"/>.
    /// </summary>
    public Game Game { get; }

    /// <summary>
    /// Gets engine asset registries and shared sprite batch.
    /// </summary>
    public AssetSet Assets { get; }

    /// <summary>
    /// Gets raw keyboard/mouse state.
    /// </summary>
    public InputState RawInput { get; }

    /// <summary>
    /// Gets action-based input map.
    /// </summary>
    public InputMap Input { get; }

    /// <summary>
    /// Gets scene tree services.
    /// </summary>
    public SceneTree SceneTree { get; }

    /// <summary>
    /// Gets virtual resolution helper.
    /// </summary>
    public VirtualResolutionScaler VirtualScaler { get; }

    /// <summary>
    /// Gets current frame timing data.
    /// </summary>
    public GameTime GameTime { get; private set; } = default!;

    /// <summary>
    /// Gets unscaled frame delta in seconds.
    /// </summary>
    public float DeltaSeconds { get; private set; }

    /// <summary>
    /// Gets total elapsed seconds since start.
    /// </summary>
    public float TotalSeconds { get; private set; }

    /// <summary>
    /// Gets convenience access to graphics device.
    /// </summary>
    public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;

    /// <summary>
    /// Gets the shared sprite batch.
    /// </summary>
    public SpriteBatch SpriteBatch => Assets.SpriteBatch;

    /// <summary>
    /// Gets virtual width in pixels.
    /// </summary>
    public int VirtualWidth => VirtualScaler.VirtualWidth;

    /// <summary>
    /// Gets virtual height in pixels.
    /// </summary>
    public int VirtualHeight => VirtualScaler.VirtualHeight;

    /// <summary>
    /// Gets destination rectangle used when blitting virtual backbuffer.
    /// </summary>
    public Rectangle VirtualDestination => VirtualScaler.DestinationRect;

    /// <summary>
    /// Gets the active camera from the scene tree.
    /// </summary>
    public Camera2D? ActiveCamera => SceneTree.ActiveCamera;

    internal void BeginFrame(GameTime gameTime)
    {
        GameTime = gameTime;
        DeltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        TotalSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
    }
}
