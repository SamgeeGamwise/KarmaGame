namespace Engine.State;

/// <summary>
/// Registry used to construct states by ID.
/// </summary>
public sealed class StateFactory<TStateId> where TStateId : notnull
{
    private readonly Dictionary<TStateId, Func<IState>> _map = [];

    /// <summary>
    /// Registers or replaces state factory for an ID.
    /// </summary>
    public void Register(TStateId id, Func<IState> factory)
    {
        _map[id] = factory;
    }

    /// <summary>
    /// Creates state instance for an ID.
    /// </summary>
    public IState Create(TStateId id)
    {
        if (_map.TryGetValue(id, out var factory))
            return factory();

        throw new InvalidOperationException($"No state factory registered for {id}.");
    }
}
