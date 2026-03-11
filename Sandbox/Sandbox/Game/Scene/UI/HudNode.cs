using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox.Game.Scene.UI;

internal sealed class HudNode(
    IReadOnlyDictionary<string, string> keysByAction,
    string menuToggleActionName,
    string interactActionName,
    string debugAddMoneyActionName)
{
    private readonly IReadOnlyDictionary<string, string> _keysByAction = keysByAction;
    private readonly string _menuToggleActionName = menuToggleActionName;
    private readonly string _interactActionName = interactActionName;
    private readonly string _debugAddMoneyActionName = debugAddMoneyActionName;
    private SpriteFont _font = null!;
    private Texture2D _pixel = null!;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("UI/UIFont");
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void DrawScreen(
        SpriteBatch spriteBatch,
        int screenWidth,
        int dayNumber,
        string clockText,
        int money,
        int level,
        bool showHintText)
    {
        const int margin = 14;
        const int leftPanelWidth = 264;
        const int leftPanelHeight = 106;
        Rectangle leftPanel = new(margin, margin, leftPanelWidth, leftPanelHeight);
        DrawPanel(spriteBatch, leftPanel, new Color(20, 24, 33, 238), new Color(86, 103, 132), true);

        Vector2 leftCursor = new(leftPanel.X + 12, leftPanel.Y + 11);
        spriteBatch.DrawString(_font, $"Day {dayNumber}", leftCursor, new Color(243, 248, 255));
        leftCursor.Y += 24;
        spriteBatch.DrawString(_font, clockText, leftCursor, new Color(206, 221, 247));
        leftCursor.Y += 24;
        spriteBatch.DrawString(_font, $"$ {money}", leftCursor, new Color(190, 232, 160));
        leftCursor.Y += 24;
        spriteBatch.DrawString(_font, $"Level {level}", leftCursor, new Color(230, 214, 170));

        string menuKey = ResolveKey(_menuToggleActionName);
        string interactKey = ResolveKey(_interactActionName);
        string debugMoneyKey = ResolveKey(_debugAddMoneyActionName);

        List<string> rightLines = [$"{clockText}"];
        if (showHintText)
        {
            rightLines.Add($"[{menuKey}] Menu  [{interactKey}] Interact");
            rightLines.Add($"[{debugMoneyKey}] Add Money");
        }

        float maxLineWidth = 0f;
        foreach (string line in rightLines)
            maxLineWidth = Math.Max(maxLineWidth, _font.MeasureString(line).X);

        const int panelPadding = 10;
        const int lineHeight = 18;
        const int lineGap = 2;
        int rightPanelWidth = (int)Math.Ceiling(maxLineWidth) + panelPadding * 2;
        int rightPanelHeight = panelPadding * 2 + (rightLines.Count * lineHeight) + ((rightLines.Count - 1) * lineGap);
        Rectangle rightPanel = new(
            screenWidth - rightPanelWidth - margin,
            margin,
            rightPanelWidth,
            rightPanelHeight);

        DrawPanel(spriteBatch, rightPanel, new Color(20, 24, 33, 238), new Color(86, 103, 132), true);

        Vector2 rightCursor = new(rightPanel.X + panelPadding, rightPanel.Y + panelPadding);
        for (int i = 0; i < rightLines.Count; i++)
        {
            Color lineColor = i == 0 ? new Color(245, 249, 255) : new Color(184, 198, 226);
            spriteBatch.DrawString(_font, rightLines[i], rightCursor, lineColor);
            rightCursor.Y += lineHeight + lineGap;
        }
    }

    private string ResolveKey(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return "?";

        return _keysByAction.TryGetValue(actionName, out string? key) ? key : actionName;
    }

    private void DrawPanel(SpriteBatch spriteBatch, Rectangle panel, Color fillColor, Color borderColor, bool withTopAccent)
    {
        spriteBatch.Draw(_pixel, panel, fillColor);
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Y, panel.Width, 1), borderColor);
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Bottom - 1, panel.Width, 1), borderColor * 0.7f);
        spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Y, 1, panel.Height), borderColor * 0.65f);
        spriteBatch.Draw(_pixel, new Rectangle(panel.Right - 1, panel.Y, 1, panel.Height), borderColor * 0.65f);
        if (withTopAccent)
            spriteBatch.Draw(_pixel, new Rectangle(panel.X, panel.Y, panel.Width, 2), new Color(211, 177, 103));
    }
}
