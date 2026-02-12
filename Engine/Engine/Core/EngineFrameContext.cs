using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Engine.Core;

public readonly struct EngineFrameContext
{
    public EngineFrameContext(
        GameTime gameTime,
        ContentManager content,
        SpriteBatch spriteBatch,
        OrthographicCamera camera,
        InputBridge input,
        int virtualWidth,
        int virtualHeight)
    {
        GameTime = gameTime;
        Content = content;
        SpriteBatch = spriteBatch;
        Camera = camera;
        Input = input;
        VirtualWidth = virtualWidth;
        VirtualHeight = virtualHeight;
    }

    public GameTime GameTime { get; }

    public ContentManager Content { get; }

    public SpriteBatch SpriteBatch { get; }

    public OrthographicCamera Camera { get; }

    public InputBridge Input { get; }

    public int VirtualWidth { get; }

    public int VirtualHeight { get; }

    public float DeltaSeconds => (float)GameTime.ElapsedGameTime.TotalSeconds;
}
