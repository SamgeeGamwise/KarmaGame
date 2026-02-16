using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core;

public sealed class YSortRenderer
{
    private static readonly Comparison<Entry> EntryComparer = CompareEntries;
    private readonly List<Entry> _entries = new();
    private int _nextSequence;

    public void Clear()
    {
        _entries.Clear();
        _nextSequence = 0;
    }

    public void Add(IYSortDrawable drawable)
    {
        ArgumentNullException.ThrowIfNull(drawable);
        _entries.Add(new Entry(drawable, _nextSequence++));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        ArgumentNullException.ThrowIfNull(spriteBatch);
        if (_entries.Count == 0)
            return;

        _entries.Sort(EntryComparer);
        foreach (Entry entry in _entries)
            entry.Drawable.Draw(spriteBatch);
    }

    private static int CompareEntries(Entry a, Entry b)
    {
        int yOrder = a.Drawable.YSort.CompareTo(b.Drawable.YSort);
        if (yOrder != 0)
            return yOrder;

        return a.Sequence.CompareTo(b.Sequence);
    }

    private readonly record struct Entry(IYSortDrawable Drawable, int Sequence);
}
