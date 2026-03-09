using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Engine.Core;

public readonly struct EngineFrameContext(
    GameTime gameTime,
    ContentManager content,
    SpriteBatch spriteBatch,
    OrthographicCamera camera,
    InputBridge input,
    int virtualWidth,
    int virtualHeight)
{
    public GameTime GameTime { get; } = gameTime;

    public ContentManager Content { get; } = content;

    public SpriteBatch SpriteBatch { get; } = spriteBatch;

    public OrthographicCamera Camera { get; } = camera;

    public InputBridge Input { get; } = input;

    public int VirtualWidth { get; } = virtualWidth;

    public int VirtualHeight { get; } = virtualHeight;

    public float DeltaSeconds => (float)GameTime.ElapsedGameTime.TotalSeconds;
}
