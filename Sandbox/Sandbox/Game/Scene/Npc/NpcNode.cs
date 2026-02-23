using System;
using System.Collections.Generic;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sandbox.Game.Config;

namespace Sandbox.Game.Scene.Npc;

internal sealed class NpcNode : IYSortDrawable
{
    private const int SheetPadding = 1;
    private const int FrameWidth = 23;
    private const int FrameHeight = 36;
    private const int IdleColumn = 0;
    private const int IdleDownRow = 0;

    private readonly float _spriteScale;
    private readonly int _scaledFrameWidth;
    private readonly int _scaledFrameHeight;
    private readonly string _spriteSheetAssetName;
    private readonly List<string> _dialogueLines;
    private Texture2D _sheet = null!;
    private int _sheetOffsetX;
    private int _sheetOffsetY;

    public NpcNode(NpcDefinitionSettings definition)
    {
        Id = definition.NpcId;
        DisplayName = definition.DisplayName;
        MapAssetName = definition.MapAssetName;
        SpawnObjectName = definition.SpawnObjectName;
        FallbackFeetPosition = new Vector2(definition.FallbackX, definition.FallbackY);
        InteractionRange = definition.InteractionRange;
        _spriteSheetAssetName = definition.SpriteSheetAssetName;
        _dialogueLines = definition.DialogueLines.Count == 0
            ? [$"{definition.DisplayName}: placeholder dialogue."]
            : definition.DialogueLines;
        _spriteScale = definition.TargetHeightInPixels / (float)FrameHeight;
        _scaledFrameWidth = (int)MathF.Round(FrameWidth * _spriteScale);
        _scaledFrameHeight = (int)MathF.Round(FrameHeight * _spriteScale);
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string MapAssetName { get; }

    public string SpawnObjectName { get; }

    public Vector2 FallbackFeetPosition { get; }

    public float InteractionRange { get; }

    public IReadOnlyList<string> DialogueLines => _dialogueLines;

    public Vector2 Position { get; private set; }

    public float YSort => Position.Y + _scaledFrameHeight;

    public Vector2 FeetPosition => new(Position.X + _scaledFrameWidth * 0.5f, Position.Y + _scaledFrameHeight - 3);

    public void SetFeetPosition(Vector2 feetWorldPosition)
    {
        Position = new Vector2(
            feetWorldPosition.X - _scaledFrameWidth * 0.5f,
            feetWorldPosition.Y - (_scaledFrameHeight - 3));
    }

    public void LoadContent(ContentManager content)
    {
        _sheet = content.Load<Texture2D>(_spriteSheetAssetName);
        _sheetOffsetX = SheetPadding;
        _sheetOffsetY = SheetPadding;
    }

    public bool IsInInteractionRange(Vector2 playerFeetPosition, float defaultRange)
    {
        float interactionDistance = InteractionRange > 0f ? InteractionRange : defaultRange;
        return Vector2.DistanceSquared(playerFeetPosition, FeetPosition) <= interactionDistance * interactionDistance;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int sourceX = _sheetOffsetX + IdleColumn * FrameWidth;
        int sourceY = _sheetOffsetY + IdleDownRow * FrameHeight;
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
}
