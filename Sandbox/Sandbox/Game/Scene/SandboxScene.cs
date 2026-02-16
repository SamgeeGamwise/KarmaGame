using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox.Game;

internal sealed class SandboxScene
{
    private readonly MapNode _mapNode;
    private readonly PlayerNode _playerNode = new();
    private readonly CameraNode _cameraNode = new();
    private readonly DayNightNode _dayNightNode = new();
    private readonly YSortRenderer _ySortRenderer = new();

    public SandboxScene(string mapAssetName, TiledMapAuthoringProfile mapProfile)
    {
        _mapNode = new MapNode(mapAssetName, mapProfile);
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _mapNode.LoadContent(content, graphicsDevice);
        _playerNode.LoadContent(content);
        _dayNightNode.LoadContent(content, graphicsDevice);

        if (_mapNode.TryGetPlayerSpawn(out Vector2 spawn))
            _playerNode.Position = spawn;
    }

    public void Update(EngineFrameContext context, Action exitGame)
    {
        _mapNode.Update(context.GameTime);
        _dayNightNode.Update(context.DeltaSeconds);

        if (context.Input.Down("exit"))
            exitGame();

        _playerNode.Update(context, _mapNode);
        _cameraNode.Update(context, _playerNode, _mapNode);
    }

    public void Draw(EngineFrameContext context)
    {
        Matrix view = context.Camera.GetViewMatrix();
        _mapNode.DrawBackground(context.SpriteBatch, view, context.VirtualWidth, context.VirtualHeight);

        context.SpriteBatch.Begin(transformMatrix: view);
        _ySortRenderer.Clear();
        _ySortRenderer.Add(_playerNode);
        _ySortRenderer.Draw(context.SpriteBatch);
        context.SpriteBatch.End();

        _mapNode.DrawForeground(view);
    }

    public void DrawScreen(EngineFrameContext context)
    {
        context.SpriteBatch.Begin();
        _dayNightNode.DrawScreen(context.SpriteBatch, context.VirtualWidth, context.VirtualHeight);
        context.SpriteBatch.End();
    }
}
