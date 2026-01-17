namespace AnarchyChess.Api.Shared.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Shared.Models.BoundedSet`1")]
public class BoundedSet<T>(int maxSize)
{
    [Id(0)]
    private readonly int _maxSize = maxSize;

    [Id(1)]
    private readonly HashSet<T> _items = [];

    [Id(2)]
    private readonly Queue<T> _recentItems = [];

    public bool TryAdd(T item)
    {
        if (_items.Contains(item))
            return false;

        _items.Add(item);
        _recentItems.Enqueue(item);

        if (_items.Count > _maxSize)
        {
            var toRemove = _recentItems.Dequeue();
            _items.Remove(toRemove);
        }

        return true;
    }

    public bool Has(T item) => _items.Contains(item);
}
