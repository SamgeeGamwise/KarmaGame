using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI;

public sealed class ResponseMenu
{
    private readonly List<ResponseOption> _options = [];
    private int _selectedIndex;

    public bool HasOptions => _options.Count > 0;

    public void SetOptions(IReadOnlyList<ResponseOption> options)
    {
        _options.Clear();
        _options.AddRange(options);
        _selectedIndex = 0;
    }

    public void Clear()
    {
        _options.Clear();
        _selectedIndex = 0;
    }

    public void MoveSelection(int direction)
    {
        if (_options.Count == 0 || direction == 0)
            return;

        _selectedIndex = (_selectedIndex + direction) % _options.Count;
        if (_selectedIndex < 0)
            _selectedIndex += _options.Count;
    }

    public bool TryGetSelectedOption(out ResponseOption option)
    {
        if (_options.Count == 0)
        {
            option = default;
            return false;
        }

        option = _options[_selectedIndex];
        return true;
    }

    public void Draw(
        SpriteBatch spriteBatch,
        SpriteFont font,
        Texture2D pixel,
        Rectangle container,
        int topOffset,
        ResponseMenuStyle style)
    {
        if (_options.Count == 0)
            return;

        int currentY = container.Y + topOffset;

        for (int i = 0; i < _options.Count; i++)
        {
            Rectangle optionRect = new(
                container.X + style.HorizontalMargin,
                currentY,
                container.Width - style.HorizontalMargin * 2,
                style.ItemHeight);

            bool isSelected = i == _selectedIndex;
            Color fillColor = isSelected ? style.SelectedFillColor : style.UnselectedFillColor;
            Color borderColor = isSelected ? style.SelectedBorderColor : style.UnselectedBorderColor;
            Color textColor = isSelected ? style.SelectedTextColor : style.UnselectedTextColor;

            spriteBatch.Draw(pixel, optionRect, fillColor);
            DrawBorder(spriteBatch, pixel, optionRect, borderColor);

            string label = $"{i + 1}. {_options[i].Label}";
            Vector2 textSize = font.MeasureString(label);
            Vector2 textPosition = new(
                optionRect.X + 10,
                optionRect.Y + (optionRect.Height - textSize.Y) * 0.5f);

            spriteBatch.DrawString(font, label, textPosition, textColor);
            currentY += style.ItemHeight + style.ItemGap;
        }
    }

    private static void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, Color color)
    {
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), color);
    }
}
