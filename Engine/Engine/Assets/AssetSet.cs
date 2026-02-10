using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Assets;

/// <summary>
/// Runtime asset registry with strongly-typed convenience accessors.
/// </summary>
public sealed class AssetSet
{
    private readonly Dictionary<string, SpriteFont> _fonts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SoundEffect> _sounds = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the shared sprite batch.
    /// </summary>
    public SpriteBatch SpriteBatch { get; private set; } = default!;

    /// <summary>
    /// Gets current viewport snapshot captured during initialization.
    /// </summary>
    public Viewport Viewport { get; private set; }

    /// <summary>
    /// Initializes graphics-backed resources.
    /// </summary>
    public void Initialize(GraphicsDevice graphicsDevice)
    {
        SpriteBatch = new SpriteBatch(graphicsDevice);
        Viewport = graphicsDevice.Viewport;
    }

    /// <summary>
    /// Registers a font under a key.
    /// </summary>
    public void RegisterFont(string key, SpriteFont font) => _fonts[key] = font;

    /// <summary>
    /// Registers a texture under a key.
    /// </summary>
    public void RegisterTexture(string key, Texture2D texture) => _textures[key] = texture;

    /// <summary>
    /// Registers a sound effect under a key.
    /// </summary>
    public void RegisterSound(string key, SoundEffect sound) => _sounds[key] = sound;

    /// <summary>
    /// Gets a previously registered font.
    /// </summary>
    public SpriteFont Font(string key) => _fonts[key];

    /// <summary>
    /// Gets a previously registered texture.
    /// </summary>
    public Texture2D Texture(string key) => _textures[key];

    /// <summary>
    /// Gets a previously registered sound effect.
    /// </summary>
    public SoundEffect Sound(string key) => _sounds[key];

    /// <summary>
    /// Attempts to get a font by key.
    /// </summary>
    public bool TryFont(string key, out SpriteFont font) => _fonts.TryGetValue(key, out font!);

    /// <summary>
    /// Attempts to get a texture by key.
    /// </summary>
    public bool TryTexture(string key, out Texture2D texture) => _textures.TryGetValue(key, out texture!);

    /// <summary>
    /// Attempts to get a sound effect by key.
    /// </summary>
    public bool TrySound(string key, out SoundEffect sound) => _sounds.TryGetValue(key, out sound!);

    /// <summary>
    /// Loads and registers a font from the content pipeline.
    /// </summary>
    public SpriteFont LoadFont(ContentManager content, string key, string assetName)
    {
        var font = content.Load<SpriteFont>(assetName);
        _fonts[key] = font;
        return font;
    }

    /// <summary>
    /// Loads and registers a texture from the content pipeline.
    /// </summary>
    public Texture2D LoadTexture(ContentManager content, string key, string assetName)
    {
        var texture = content.Load<Texture2D>(assetName);
        _textures[key] = texture;
        return texture;
    }

    /// <summary>
    /// Loads and registers a sound effect from the content pipeline.
    /// </summary>
    public SoundEffect LoadSound(ContentManager content, string key, string assetName)
    {
        var sound = content.Load<SoundEffect>(assetName);
        _sounds[key] = sound;
        return sound;
    }
}
