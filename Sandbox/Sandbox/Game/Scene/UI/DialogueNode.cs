using System.Collections.Generic;
using System.Text;
using Engine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox.Game.Scene.UI;

internal sealed class DialogueNode
{
    private static readonly ResponseMenuStyle ResponseMenuStyle = new(
        new Color(66, 102, 150, 235),
        new Color(31, 40, 55, 180),
        new Color(247, 210, 109),
        new Color(91, 106, 135),
        Color.White,
        new Color(210, 220, 235),
        HorizontalMargin: 12,
        ItemHeight: 32,
        ItemGap: 8);

    private readonly List<string> _lines = [];
    private readonly ResponseMenu _responseMenu = new();
    private SpriteFont _font = null!;
    private Texture2D _pixel = null!;
    private string _speaker = string.Empty;
    private int _lineIndex;

    public bool IsActive => _lineIndex < _lines.Count;

    public bool HasResponseOptions => _responseMenu.HasOptions;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("UI/UIFont");
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void StartDialogue(string speaker, IReadOnlyList<string> lines)
    {
        _speaker = speaker;
        _responseMenu.Clear();
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

    public void ShowResponsePrompt(string speaker, string text, IReadOnlyList<ResponseOption> options)
    {
        _speaker = speaker;
        _lines.Clear();
        _lines.Add(string.IsNullOrWhiteSpace(text) ? $"{speaker}: ..." : text.Trim());
        _lineIndex = 0;
        _responseMenu.SetOptions(options);
    }

    public void MoveSelection(int direction)
    {
        _responseMenu.MoveSelection(direction);
    }

    public bool TryGetSelectedResponse(out ResponseOption responseOption)
    {
        return _responseMenu.TryGetSelectedOption(out responseOption);
    }

    public void Advance()
    {
        if (!IsActive || HasResponseOptions)
            return;

        _lineIndex++;
    }

    public void Close()
    {
        _lineIndex = _lines.Count;
        _responseMenu.Clear();
    }

    public void DrawScreen(SpriteBatch spriteBatch, int virtualWidth, int virtualHeight)
    {
        if (!IsActive)
            return;

        const int margin = 16;
        const int gap = 12;
        int panelHeight = HasResponseOptions ? 150 : 122;
        int responseWidth = MathHelper.Clamp((int)(virtualWidth * 0.33f), 175, 300);

        Rectangle responseRect = new(
            margin,
            virtualHeight - panelHeight - margin,
            responseWidth,
            panelHeight);

        Rectangle dialogueRect = new(
            responseRect.Right + gap,
            virtualHeight - panelHeight - margin,
            virtualWidth - margin - (responseRect.Right + gap),
            panelHeight);

        DrawPanel(spriteBatch, responseRect, new Color(16, 20, 28, 235), new Color(76, 96, 128), false);
        DrawPanel(spriteBatch, dialogueRect, new Color(15, 19, 27, 245), new Color(220, 184, 98), true);

        spriteBatch.DrawString(_font, "Response", new Vector2(responseRect.X + 12, responseRect.Y + 8), new Color(188, 204, 230));
        if (HasResponseOptions)
        {
            _responseMenu.Draw(spriteBatch, _font, _pixel, responseRect, 34, ResponseMenuStyle);
        }
        else
        {
            spriteBatch.DrawString(_font, "Listen", new Vector2(responseRect.X + 12, responseRect.Y + 34), new Color(150, 166, 196));
            spriteBatch.DrawString(_font, "No response needed", new Vector2(responseRect.X + 12, responseRect.Y + 56), new Color(118, 132, 160));
        }

        string speakerLine = _speaker.Length == 0 ? "NPC" : _speaker;
        string wrappedText = WrapText(_lines[_lineIndex], dialogueRect.Width - 24);
        spriteBatch.DrawString(_font, speakerLine, new Vector2(dialogueRect.X + 12, dialogueRect.Y + 8), new Color(255, 230, 176));
        spriteBatch.DrawString(_font, wrappedText, new Vector2(dialogueRect.X + 12, dialogueRect.Y + 34), new Color(241, 245, 255));

        string footerText = HasResponseOptions ? "Use [W/S] and [E]" : "Press [E] to continue";
        Vector2 footerSize = _font.MeasureString(footerText);
        spriteBatch.DrawString(
            _font,
            footerText,
            new Vector2(dialogueRect.Right - footerSize.X - 12, dialogueRect.Bottom - footerSize.Y - 8),
            new Color(189, 199, 220));
    }

    private void DrawPanel(SpriteBatch spriteBatch, Rectangle panel, Color fillColor, Color borderColor, bool withTopAccent)
    {
        spriteBatch.Draw(_pixel, panel, fillColor);
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Y, panel.Width, 1), borderColor);
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Bottom - 1, panel.Width, 1), borderColor * 0.7f);
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Y, 1, panel.Height), borderColor * 0.65f);
        spriteBatch.Draw(_pixel, new Rectangle(panel.Right - 1, panel.Y, 1, panel.Height), borderColor * 0.65f);
        if (withTopAccent)
            spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Y, panel.Width, 3), borderColor);
    }

    private string WrapText(string text, int maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        string[] words = text.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
            return string.Empty;

        StringBuilder builder = new();
        string currentLine = words[0];

        for (int i = 1; i < words.Length; i++)
        {
            string candidate = $"{currentLine} {words[i]}";
            if (_font.MeasureString(candidate).X <= maxWidth)
            {
                currentLine = candidate;
                continue;
            }

            if (builder.Length > 0)
                builder.Append('\n');
            builder.Append(currentLine);
            currentLine = words[i];
        }

        if (builder.Length > 0)
            builder.Append('\n');
        builder.Append(currentLine);
        return builder.ToString();
    }
}
