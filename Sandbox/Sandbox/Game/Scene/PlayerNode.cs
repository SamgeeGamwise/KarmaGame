using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox.Game;

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
    private const float WalkFps = 8f;
    private const float MoveSpeed = 92f;
    private const float RunSpeed = 150f;

    private Texture2D _sheet = null!;
    private int _sheetOffsetX;
    private int _sheetOffsetY;
    private Vector2 _lastMove = new(0f, 1f);
    private float _walkTimer;

    public Vector2 Position { get; set; } = new(120f, 120f);

    public int CurrentFrameWidth => FrameWidth;

    public int CurrentFrameHeight => FrameHeight;

    public float YSort => Position.Y + FrameHeight;

    public void LoadContent(ContentManager content)
    {
        _sheet = content.Load<Texture2D>("Person2");
        _sheetOffsetX = SheetPadding;
        _sheetOffsetY = SheetPadding;
    }

    public void Update(Engine.Core.EngineFrameContext context, MapNode mapNode)
    {
        Vector2 input = context.Input.Vector("move_left", "move_right", "move_up", "move_down");
        bool run = context.Input.Down("run");
        Vector2 delta = input * context.DeltaSeconds;

        delta = run ? delta * RunSpeed : delta * MoveSpeed;

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

        Position = mapNode.ClampPlayerPosition(Position, FrameWidth, FrameHeight, context.VirtualWidth, context.VirtualHeight);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int row = ResolveRow(_lastMove);
        bool isWalking = _walkTimer > 0f;
        int frame = isWalking
            ? (int)(_walkTimer * WalkFps) % WalkFrameCount
            : 0;

        int column = isWalking ? WalkStartColumn + frame : IdleColumn;

        int sourceX = _sheetOffsetX + column * FrameWidth;
        int sourceY = _sheetOffsetY + row * FrameHeight;
        Rectangle source = new(sourceX, sourceY, FrameWidth, FrameHeight);

        spriteBatch.Draw(_sheet, Position, source, Color.White);
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

    private static Rectangle BuildCollision(Vector2 position)
    {
        const int collisionWidth = 13;
        const int collisionHeight = 12;
        int collisionXOffset = (FrameWidth - collisionWidth) / 2;
        int collisionYOffset = FrameHeight - collisionHeight - 4;

        return new Rectangle(
            (int)MathF.Round(position.X) + collisionXOffset,
            (int)MathF.Round(position.Y) + collisionYOffset,
            collisionWidth,
            collisionHeight);
    }
}
