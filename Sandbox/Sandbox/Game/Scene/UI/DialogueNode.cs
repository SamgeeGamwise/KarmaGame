using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox.Game.Scene.UI;

internal sealed class DialogueNode
{
    private readonly List<string> _lines = [];
    private SpriteFont _font = null!;
    private Texture2D _pixel = null!;
    private string _speaker = string.Empty;
    private int _lineIndex;

    public bool IsActive => _lineIndex < _lines.Count;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("UIFont");
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void StartDialogue(string speaker, IReadOnlyList<string> lines)
    {
        _speaker = speaker;
        _lines.Clear();
        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
                _lines.Add(line.Trim());
        }

        if (_lines.Count == 0)
            _lines.Add($"{speaker}: ...");

        _lineIndex = 0;
    }

    public void Advance()
    {
        if (!IsActive)
            return;

        _lineIndex++;
    }

    public void Close()
    {
        _lineIndex = _lines.Count;
    }

    public void DrawScreen(SpriteBatch spriteBatch, int virtualWidth, int virtualHeight)
    {
        if (!IsActive)
            return;

        const int margin = 16;
        const int panelHeight = 90;

        Rectangle panelRect = new(
            margin,
            virtualHeight - panelHeight - margin,
            virtualWidth - margin * 2,
            panelHeight);

        spriteBatch.Draw(_pixel, panelRect, Color.Black * 0.74f);
        spriteBatch.Draw(_pixel, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), new Color(212, 176, 88));

        string speakerLine = _speaker.Length == 0 ? "NPC" : _speaker;
        spriteBatch.DrawString(_font, speakerLine, new Vector2(panelRect.X + 10, panelRect.Y + 8), new Color(255, 226, 170));
        spriteBatch.DrawString(_font, _lines[_lineIndex], new Vector2(panelRect.X + 10, panelRect.Y + 34), Color.White);
        spriteBatch.DrawString(
            _font,
            "Press [E] to continue",
            new Vector2(panelRect.Right - 182, panelRect.Bottom - 24),
            Color.LightGray);
    }
}
