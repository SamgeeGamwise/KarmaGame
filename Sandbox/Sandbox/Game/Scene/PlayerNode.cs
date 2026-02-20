using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sandbox.Game.Config;

namespace Sandbox.Game.Scene;

internal sealed class PlayerNode : IYSortDrawable
{
    private const int SheetColumns = 9;
    private const int SheetRows = 8;
    private const int SheetPadding = 1;
    private const int FrameWidth = 23;
    private const int FrameHeight = 36;
    private const int IdleColumn = 0;
    private const int WalkStartColumn = 1;
    private const int WalkFrameCount = 8;

    private readonly PlayerSettings _settings;
    private readonly float _spriteScale;
    private readonly int _scaledFrameWidth;
    private readonly int _scaledFrameHeight;
    private Texture2D _sheet = null!;
    private int _sheetOffsetX;
    private int _sheetOffsetY;
    private Vector2 _lastMove = new(0f, 1f);
    private float _walkTimer;

    public PlayerNode(PlayerSettings settings)
    {
        _settings = settings;
        _spriteScale = settings.TargetHeightInPixels / (float)FrameHeight;
        _scaledFrameWidth = (int)MathF.Round(FrameWidth * _spriteScale);
        _scaledFrameHeight = (int)MathF.Round(FrameHeight * _spriteScale);
    }

    public Vector2 Position { get; set; } = new(120f, 120f);

    public int CurrentFrameWidth => _scaledFrameWidth;

    public int CurrentFrameHeight => _scaledFrameHeight;

    public float YSort => Position.Y + _scaledFrameHeight;

    public Vector2 FeetPosition => new(
        Position.X + _scaledFrameWidth * 0.5f,
        Position.Y + _scaledFrameHeight - _settings.CollisionBottomInset);

    public Rectangle DoorInteractionBounds => BuildDoorInteractionBounds(FeetPosition);

    public void SetFeetPosition(Vector2 feetWorldPosition)
    {
        Position = new Vector2(
            feetWorldPosition.X - _scaledFrameWidth * 0.5f,
            feetWorldPosition.Y - (_scaledFrameHeight - _settings.CollisionBottomInset));
    }

    public void LoadContent(ContentManager content)
    {
        _sheet = content.Load<Texture2D>(_settings.SpriteSheetAssetName);
        _sheetOffsetX = SheetPadding;
        _sheetOffsetY = SheetPadding;
    }

    public void Update(EngineFrameContext context, MapNode mapNode)
    {
        Vector2 input = context.Input.Vector("move_left", "move_right", "move_up", "move_down");
        bool run = context.Input.Down("run");
        Vector2 delta = input * context.DeltaSeconds;

        delta = run ? delta * _settings.RunSpeed : delta * _settings.MoveSpeed;

        if (input != Vector2.Zero)
        {
            _lastMove = input;
            _walkTimer += context.DeltaSeconds;
            MoveAndCollide(delta, mapNode);
        }
        else
        {
            _walkTimer = 0f;
        }

        Position = mapNode.ClampPlayerPosition(Position, CurrentFrameWidth, CurrentFrameHeight, context.VirtualWidth, context.VirtualHeight);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int row = ResolveRow(_lastMove);
        bool isWalking = _walkTimer > 0f;
        int frame = isWalking
            ? (int)(_walkTimer * _settings.WalkFramesPerSecond) % WalkFrameCount
            : 0;

        int column = isWalking ? WalkStartColumn + frame : IdleColumn;

        int sourceX = _sheetOffsetX + column * FrameWidth;
        int sourceY = _sheetOffsetY + row * FrameHeight;
        Rectangle source = new(sourceX, sourceY, FrameWidth, FrameHeight);

        spriteBatch.Draw(
            _sheet,
            Position,
            source,
            Color.White,
            0f,
            Vector2.Zero,
            _spriteScale,
            SpriteEffects.None,
            0f);
    }

    private void MoveAndCollide(Vector2 delta, MapNode mapNode)
    {
        TryMoveAxis(new Vector2(delta.X, 0f), mapNode);
        TryMoveAxis(new Vector2(0f, delta.Y), mapNode);
    }

    private void TryMoveAxis(Vector2 axisDelta, MapNode mapNode)
    {
        if (axisDelta == Vector2.Zero)
            return;

        Vector2 candidatePosition = Position + axisDelta;
        Rectangle candidateCollision = BuildCollision(candidatePosition);
        if (mapNode.IsWorldRectangleBlocked(candidateCollision))
            return;

        Position = candidatePosition;
    }

    private static int ResolveRow(Vector2 direction)
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

    private Rectangle BuildCollision(Vector2 position)
    {
        int collisionXOffset = (_scaledFrameWidth - _settings.CollisionWidth) / 2;
        int collisionYOffset = _scaledFrameHeight - _settings.CollisionHeight - _settings.CollisionBottomInset;

        return new Rectangle(
            (int)MathF.Round(position.X) + collisionXOffset,
            (int)MathF.Round(position.Y) + collisionYOffset,
            _settings.CollisionWidth,
            _settings.CollisionHeight);
    }

    private Rectangle BuildDoorInteractionBounds(Vector2 feetPosition)
    {
        int left = (int)MathF.Round(feetPosition.X - _settings.DoorInteractionWidth * 0.5f);
        int top = (int)MathF.Round(feetPosition.Y - _settings.DoorInteractionHeight);
        return new Rectangle(left, top, _settings.DoorInteractionWidth, _settings.DoorInteractionHeight);
    }
}
