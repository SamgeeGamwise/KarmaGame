using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sandbox.Game;

public sealed class SandboxGame : ExtendedGameHost
{
    private const string DefaultMapAsset = "Town";
    private const bool ShowCollisionDebugByDefault = true;
    private const int PersonSheetColumns = 9;
    private const int PersonSheetRows = 8;
    private const int PersonIdleColumn = 0;
    private const int PersonWalkStartColumn = 1;
    private const int PersonWalkFrameCount = 8;
    private const float PersonWalkFps = 8f;
    private const float PlayerMoveSpeed = 92f;

    private readonly TiledMapAuthoringProfile _mapProfile = TiledMapAuthoringProfile.Default;

    private Texture2D _tileset = null!;
    private Texture2D _personSheet = null!;
    private Texture2D _debugPixel = null!;
    private TiledMapRuntime? _map;
    private int _personFrameWidth;
    private int _personFrameHeight;
    private int _personSheetOffsetX;
    private int _personSheetOffsetY;
    private bool _showCollisionDebug = ShowCollisionDebugByDefault;

    private Vector2 _playerPosition = new(120f, 120f);
    private Vector2 _lastMove = new(0f, 1f);
    private float _walkTimer;

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
        input.BindKey("toggle_collision_debug", Keys.C);
        input.BindKey("toggle_collision_debug", Keys.F3);
        input.BindKey("exit", Keys.Escape);
    }

    protected override void OnLoadContent()
    {
        _tileset = Content.Load<Texture2D>("Tileset2");
        _personSheet = Content.Load<Texture2D>("Person");
        _debugPixel = new Texture2D(GraphicsDevice, 1, 1);
        _debugPixel.SetData([Color.White]);
        ComputePersonFrameMetrics();

        if (TiledMapRuntime.TryLoad(Content, GraphicsDevice, DefaultMapAsset, out var mapRuntime))
            _map = mapRuntime;

        if (_map is not null && _map.TryGetObjectPosition(_mapProfile.SpawnObjectLayerName, _mapProfile.PlayerSpawnObjectName, out Vector2 spawnPosition))
            _playerPosition = spawnPosition;
    }

    protected override void OnUpdateGame(EngineFrameContext context)
    {
        _map?.Update(context.GameTime);

        if (context.Input.Pressed("toggle_collision_debug"))
            _showCollisionDebug = !_showCollisionDebug;

        if (context.Input.Down("exit"))
            Exit();

        Vector2 input = context.Input.Vector("move_left", "move_right", "move_up", "move_down");
        Vector2 delta = input * PlayerMoveSpeed * context.DeltaSeconds;

        if (input != Vector2.Zero)
        {
            _lastMove = input;
            _walkTimer += context.DeltaSeconds;
            MoveAndCollide(delta);
        }
        else
        {
            _walkTimer = 0f;
        }

        ClampPlayerToMapOrViewport(context.VirtualWidth, context.VirtualHeight);

        Vector2 cameraTarget = _playerPosition + new Vector2(_personFrameWidth * 0.5f, _personFrameHeight * 0.5f);
        if (_map is not null)
            cameraTarget = _map.ClampCameraTarget(cameraTarget, context.VirtualWidth, context.VirtualHeight);

        context.Camera.LookAt(cameraTarget);
    }

    protected override void DrawWorld(EngineFrameContext context)
    {
        Matrix view = context.Camera.GetViewMatrix();

        if (_map is not null)
            _map.DrawLayers(_mapProfile.BackgroundLayerNames, view);

        context.SpriteBatch.Begin(transformMatrix: view);

        if (_map is null)
            DrawFallbackFloor(context.SpriteBatch, context.VirtualWidth, context.VirtualHeight);

        DrawPlayer(context.SpriteBatch);
        context.SpriteBatch.End();

        if (_map is not null)
            _map.DrawLayers(_mapProfile.ForegroundLayerNames, view);

        if (_showCollisionDebug)
        {
            context.SpriteBatch.Begin(transformMatrix: view);
            DrawCollisionDebugOverlay(context.SpriteBatch);
            context.SpriteBatch.End();
        }
    }

    private void DrawFallbackFloor(SpriteBatch spriteBatch, int virtualWidth, int virtualHeight)
    {
        const int tileSize = 32;
        Rectangle srcA = new(0, 0, tileSize, tileSize);
        Rectangle srcB = new(tileSize, 0, tileSize, tileSize);

        int tilesX = (virtualWidth + tileSize - 1) / tileSize;
        int tilesY = (virtualHeight + tileSize - 1) / tileSize;
        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                Rectangle src = ((x + y) & 1) == 0 ? srcA : srcB;
                Rectangle dst = new(x * tileSize, y * tileSize, tileSize, tileSize);
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

        int sourceX = _personSheetOffsetX + column * _personFrameWidth;
        int sourceY = _personSheetOffsetY + row * _personFrameHeight;
        Rectangle source = new(sourceX, sourceY, _personFrameWidth, _personFrameHeight);

        spriteBatch.Draw(_personSheet, _playerPosition, source, Color.White);
    }

    private void MoveAndCollide(Vector2 delta)
    {
        if (_map is null)
        {
            _playerPosition += delta;
            return;
        }

        TryMoveAxis(new Vector2(delta.X, 0f));
        TryMoveAxis(new Vector2(0f, delta.Y));
    }

    private void TryMoveAxis(Vector2 axisDelta)
    {
        if (axisDelta == Vector2.Zero)
            return;

        Vector2 candidatePosition = _playerPosition + axisDelta;
        Rectangle candidateCollision = BuildPlayerCollision(candidatePosition);
        if (_map is not null && _map.IsWorldRectangleBlocked(_mapProfile.CollisionLayerName, candidateCollision))
            return;

        _playerPosition = candidatePosition;
    }

    private Rectangle BuildPlayerCollision(Vector2 position)
    {
        const int collisionWidth = 13;
        const int collisionHeight = 12;
        int collisionXOffset = (_personFrameWidth - collisionWidth) / 2;
        int collisionYOffset = _personFrameHeight - collisionHeight - 4;

        return new Rectangle(
            (int)MathF.Round(position.X) + collisionXOffset,
            (int)MathF.Round(position.Y) + collisionYOffset,
            collisionWidth,
            collisionHeight);
    }

    private void ClampPlayerToMapOrViewport(int viewportWidth, int viewportHeight)
    {
        if (_map is not null)
        {
            _playerPosition = new Vector2(
                Math.Clamp(_playerPosition.X, 0f, _map.WidthInPixels - _personFrameWidth),
                Math.Clamp(_playerPosition.Y, 0f, _map.HeightInPixels - _personFrameHeight));
            return;
        }

        _playerPosition = new Vector2(
            Math.Clamp(_playerPosition.X, 0f, viewportWidth - _personFrameWidth),
            Math.Clamp(_playerPosition.Y, 0f, viewportHeight - _personFrameHeight));
    }

    private void DrawCollisionDebugOverlay(SpriteBatch spriteBatch)
    {
        if (_map is null)
            return;

        int tileWidth = _map.Map.TileWidth;
        int tileHeight = _map.Map.TileHeight;

        for (int y = 0; y < _map.Map.Height; y++)
        {
            for (int x = 0; x < _map.Map.Width; x++)
            {
                Vector2 center = new(
                    x * tileWidth + tileWidth * 0.5f,
                    y * tileHeight + tileHeight * 0.5f);

                if (!_map.IsWorldPointBlocked(_mapProfile.CollisionLayerName, center))
                    continue;

                Rectangle tileRect = new(x * tileWidth, y * tileHeight, tileWidth, tileHeight);
                spriteBatch.Draw(_debugPixel, tileRect, Color.Red * 0.35f);
            }
        }

        Rectangle playerCollision = BuildPlayerCollision(_playerPosition);
        spriteBatch.Draw(_debugPixel, playerCollision, Color.LimeGreen * 0.45f);
    }

    private void ComputePersonFrameMetrics()
    {
        _personFrameWidth = _personSheet.Width / PersonSheetColumns;
        _personFrameHeight = _personSheet.Height / PersonSheetRows;
        _personSheetOffsetX = (_personSheet.Width - (_personFrameWidth * PersonSheetColumns)) / 2;
        _personSheetOffsetY = (_personSheet.Height - (_personFrameHeight * PersonSheetRows)) / 2;
    }

    private static int ResolvePersonRow(Vector2 direction)
    {
        float angle = MathF.Atan2(direction.Y, direction.X);
        float octant = MathF.PI / 4f;
        int index = (int)MathF.Round(angle / octant);

        return index switch
        {
            0 => 2,   // Right
            1 => 1,   // Down-right
            2 => 0,   // Down
            3 => 7,   // Down-left
            4 => 6,   // Left
            -4 => 6,  // Left
            -3 => 5,  // Up-left
            -2 => 4,  // Up
            -1 => 3,  // Up-right
            _ => 0
        };
    }
}
