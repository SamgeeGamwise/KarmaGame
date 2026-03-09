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
    private const int DefaultFrameWidth = 23;
    private const int DefaultFrameHeight = 36;
    private const int DefaultSourceOffset = 1;
    private const int DefaultFeetBottomInset = 3;
    private const int IdleColumn = 0;
    private const int IdleDownRow = 0;

    private readonly int _frameWidth;
    private readonly int _frameHeight;
    private readonly int _sourceOffsetX;
    private readonly int _sourceOffsetY;
    private readonly int _feetBottomInset;
    private readonly float _spriteScale;
    private readonly int _scaledFrameWidth;
    private readonly int _scaledFrameHeight;
    private readonly string _spriteSheetAssetName;
    private readonly List<string> _dialogueLines;
    private Texture2D _sheet = null!;

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
        _frameWidth = definition.FrameWidth > 0 ? definition.FrameWidth : DefaultFrameWidth;
        _frameHeight = definition.FrameHeight > 0 ? definition.FrameHeight : DefaultFrameHeight;
        _sourceOffsetX = definition.SourceOffsetX >= 0 ? definition.SourceOffsetX : DefaultSourceOffset;
        _sourceOffsetY = definition.SourceOffsetY >= 0 ? definition.SourceOffsetY : DefaultSourceOffset;
        _feetBottomInset = definition.FeetBottomInset >= 0 ? definition.FeetBottomInset : DefaultFeetBottomInset;
        _spriteScale = definition.TargetHeightInPixels / (float)_frameHeight;
        _scaledFrameWidth = (int)MathF.Round(_frameWidth * _spriteScale);
        _scaledFrameHeight = (int)MathF.Round(_frameHeight * _spriteScale);
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

    public Vector2 FeetPosition => new(Position.X + _scaledFrameWidth * 0.5f, Position.Y + _scaledFrameHeight - _feetBottomInset);

    public void SetFeetPosition(Vector2 feetWorldPosition)
    {
        Position = new Vector2(
            feetWorldPosition.X - _scaledFrameWidth * 0.5f,
            feetWorldPosition.Y - (_scaledFrameHeight - _feetBottomInset));
    }

    public void LoadContent(ContentManager content)
    {
        _sheet = content.Load<Texture2D>(_spriteSheetAssetName);
    }

    public bool IsInInteractionRange(Vector2 playerFeetPosition, float defaultRange)
    {
        float interactionDistance = InteractionRange > 0f ? InteractionRange : defaultRange;
        return Vector2.DistanceSquared(playerFeetPosition, FeetPosition) <= interactionDistance * interactionDistance;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int sourceX = _sourceOffsetX + IdleColumn * _frameWidth;
        int sourceY = _sourceOffsetY + IdleDownRow * _frameHeight;
        Rectangle source = new(sourceX, sourceY, _frameWidth, _frameHeight);

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
