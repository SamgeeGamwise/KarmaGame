using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace Engine.Core;

public sealed class TiledMapRuntime
{
    private TiledMapRuntime(TiledMap map, TiledMapRenderer renderer)
    {
        Map = map;
        Renderer = renderer;
    }

    public TiledMap Map { get; }

    public TiledMapRenderer Renderer { get; }

    public static TiledMapRuntime Load(ContentManager content, GraphicsDevice graphicsDevice, string assetName)
    {
        var map = content.Load<TiledMap>(assetName);
        var renderer = new TiledMapRenderer(graphicsDevice, map);
        return new TiledMapRuntime(map, renderer);
    }

    public void Update(GameTime gameTime)
    {
        Renderer.Update(gameTime);
    }

    public void Draw(Matrix? cameraMatrix = null)
    {
        Renderer.Draw(cameraMatrix);
    }
}
