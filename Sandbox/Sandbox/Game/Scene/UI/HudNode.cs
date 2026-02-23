using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox.Game.Scene.UI;

internal sealed class HudNode
{
    private SpriteFont _font = null!;
    private Texture2D _pixel = null!;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _font = content.Load<SpriteFont>("UIFont");
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void DrawScreen(
        SpriteBatch spriteBatch,
        int virtualWidth,
        int dayNumber,
        string clockText,
        int money,
        int level,
        bool showHintText)
    {
        Rectangle leftPanel = new(14, 14, 210, 74);
        spriteBatch.Draw(_pixel, leftPanel, Color.Black * 0.48f);
        spriteBatch.DrawString(_font, $"Day {dayNumber}", new Vector2(leftPanel.X + 8, leftPanel.Y + 8), Color.White);
        spriteBatch.DrawString(_font, $"{clockText}", new Vector2(leftPanel.X + 8, leftPanel.Y + 26), new Color(225, 235, 255));
        spriteBatch.DrawString(_font, $"$ {money}   Lv {level}", new Vector2(leftPanel.X + 8, leftPanel.Y + 46), new Color(185, 225, 155));

        if (!showHintText)
            return;

        string hint = "[Tab] Menu  [E] Interact  [F6] +$";
        Vector2 hintSize = _font.MeasureString(hint);
        Rectangle hintPanel = new(
            virtualWidth - (int)hintSize.X - 24,
            14,
            (int)hintSize.X + 12,
            24);
        spriteBatch.Draw(_pixel, hintPanel, Color.Black * 0.45f);
        spriteBatch.DrawString(_font, hint, new Vector2(hintPanel.X + 6, hintPanel.Y + 4), Color.LightGray);
    }
}
