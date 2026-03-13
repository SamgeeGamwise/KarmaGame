using Microsoft.Xna.Framework;

namespace Engine.UI;

public readonly record struct ResponseMenuStyle(
    Color SelectedFillColor,
    Color UnselectedFillColor,
    Color SelectedBorderColor,
    Color UnselectedBorderColor,
    Color SelectedTextColor,
    Color UnselectedTextColor,
    int HorizontalMargin = 12,
    int ItemHeight = 42,
    int ItemGap = 10);
