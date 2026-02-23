using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox.Game.Scene.UI;

internal sealed class NotificationFeedNode
{
    private readonly Queue<NotificationEntry> _entries = new();
    private readonly float _defaultLifetimeSeconds;
    private SpriteFont _font = null!;
    private Texture2D _pixel = null!;

    public NotificationFeedNode(float defaultLifetimeSeconds)
    {
        _defaultLifetimeSeconds = defaultLifetimeSeconds;
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("UIFont");
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void Push(string message, float? lifetimeSeconds = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        _entries.Enqueue(new NotificationEntry(message.Trim(), lifetimeSeconds ?? _defaultLifetimeSeconds));
        while (_entries.Count > 6)
            _entries.Dequeue();
    }

    public void Update(float deltaSeconds)
    {
        if (_entries.Count == 0)
            return;

        var updated = new Queue<NotificationEntry>(_entries.Count);
        while (_entries.Count > 0)
        {
            NotificationEntry entry = _entries.Dequeue();
            float remaining = entry.RemainingSeconds - deltaSeconds;
            if (remaining <= 0f)
                continue;

            updated.Enqueue(entry with { RemainingSeconds = remaining });
        }

        while (updated.Count > 0)
            _entries.Enqueue(updated.Dequeue());
    }

    public void DrawScreen(SpriteBatch spriteBatch)
    {
        if (_entries.Count == 0)
            return;

        const int margin = 14;
        const int rowHeight = 20;
        int y = margin;

        foreach (NotificationEntry entry in _entries)
        {
            Rectangle rowRect = new(margin, y, 320, rowHeight);
            spriteBatch.Draw(_pixel, rowRect, Color.Black * 0.52f);
            spriteBatch.DrawString(_font, entry.Message, new Vector2(rowRect.X + 6, rowRect.Y + 3), Color.White);
            y += rowHeight + 4;
        }
    }

    private readonly record struct NotificationEntry(string Message, float RemainingSeconds);
}
