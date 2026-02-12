using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sandbox.Game;

public sealed class SandboxGame : ExtendedGameHost
{
    private const int TileSize = 32;
    private const int PersonFrameWidth = 23;
    private const int PersonFrameHeight = 40;
    private const int PersonIdleColumn = 0;
    private const int PersonWalkStartColumn = 1;
    private const int PersonWalkFrameCount = 8;
    private const float PersonWalkFps = 8f;

    private Texture2D _tileset = null!;
    private Texture2D _personSheet = null!;

    private Vector2 _playerPosition = new(120f, 120f);
    private Vector2 _lastMove = new(0f, 1f);
    private float _walkTimer;

    public SandboxGame() : base(640, 360)
    {
    }

    protected override Color ClearColor => new(24, 29, 38);

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
        input.BindKey("exit", Keys.Escape);
    }

    protected override void OnLoadContent()
    {
        _tileset = Content.Load<Texture2D>("Tileset2");
        _personSheet = Content.Load<Texture2D>("Person");
    }

    protected override void OnUpdateGame(EngineFrameContext context)
    {
        if (context.Input.Down("exit"))
            Exit();

        Vector2 input = context.Input.Vector("move_left", "move_right", "move_up", "move_down");
        if (input != Vector2.Zero)
        {
            _lastMove = input;
            _playerPosition += input * 90f * context.DeltaSeconds;
            _walkTimer += context.DeltaSeconds;
        }
        else
        {
            _walkTimer = 0f;
        }

        _playerPosition.X = Math.Clamp(_playerPosition.X, 0f, context.VirtualWidth - PersonFrameWidth);
        _playerPosition.Y = Math.Clamp(_playerPosition.Y, 0f, context.VirtualHeight - 34f);
        context.Camera.LookAt(_playerPosition + new Vector2(PersonFrameWidth * 0.5f, 17f));
    }

    protected override void DrawWorld(EngineFrameContext context)
    {
        DrawCheckerFloor(context.SpriteBatch, context.VirtualWidth, context.VirtualHeight);
        DrawPlayer(context.SpriteBatch);
    }

    private void DrawCheckerFloor(SpriteBatch spriteBatch, int virtualWidth, int virtualHeight)
    {
        Rectangle srcA = new(0, 0, TileSize, TileSize);
        Rectangle srcB = new(TileSize, 0, TileSize, TileSize);

        int tilesX = (virtualWidth + TileSize - 1) / TileSize;
        int tilesY = (virtualHeight + TileSize - 1) / TileSize;
        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                Rectangle src = ((x + y) & 1) == 0 ? srcA : srcB;
                Rectangle dst = new(x * TileSize, y * TileSize, TileSize, TileSize);
                spriteBatch.Draw(_tileset, dst, src, Color.White);
            }
        }
    }

    private void DrawPlayer(SpriteBatch spriteBatch)
    {
        int row = ResolvePersonRow(_lastMove);
        bool isWalking = _walkTimer > 0f;
        int frame = isWalking
            ? (int)(_walkTimer * PersonWalkFps) % PersonWalkFrameCount
            : 0;

        int column = isWalking ? PersonWalkStartColumn + frame : PersonIdleColumn;

        int sourceX = 1 + column * PersonFrameWidth;
        int sourceY = 3 + row * PersonFrameHeight + 6;
        Rectangle source = new(sourceX, sourceY, PersonFrameWidth, 34);

        spriteBatch.Draw(_personSheet, _playerPosition, source, Color.White);
    }

    private static int ResolvePersonRow(Vector2 direction)
    {
        float angle = MathF.Atan2(direction.Y, direction.X);
        float octant = MathF.PI / 4f;
        int index = (int)MathF.Round(angle / octant);

        return index switch
        {
            0 => 2,
            1 => 3,
            2 => 4,
            3 => 5,
            4 => 6,
            -4 => 6,
            -3 => 7,
            -2 => 0,
            -1 => 1,
            _ => 4
        };
    }
}
