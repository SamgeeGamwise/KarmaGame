namespace Engine.UI;

/// <summary>
/// Menu row label and callback.
/// </summary>
public sealed record MenuEntry(string Text, Action Action);
