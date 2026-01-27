namespace AnarchyChess.Api.Shared.Services;

public interface IRandomProvider
{
    int Next();
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    double NextDouble();
    void NextBytes(byte[] buffer);
    T NextItem<T>(IEnumerable<T> enumerable);
    T NextItemWeighted<T>(IEnumerable<T> enumerable, Func<T, int> getWeight);
}

public class RandomProvider(Random? random = null) : IRandomProvider
{
    private readonly Random _random = random ?? new();

    public int Next() => _random.Next();

    public int Next(int maxValue) => _random.Next(maxValue);

    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    public double NextDouble() => _random.NextDouble();

    public void NextBytes(byte[] buffer) => _random.NextBytes(buffer);

    public T NextItem<T>(IEnumerable<T> items) => items.ElementAt(_random.Next(items.Count()));

    public T NextItemWeighted<T>(IEnumerable<T> enumerable, Func<T, int> getWeight)
    {
        List<int> weights = [.. enumerable.Select(getWeight)];

        int totalWeight = weights.Sum();
        int rnd = _random.Next(totalWeight);

        int cumulative = 0;
        int selectedIndex = 0;
        for (int i = 0; i < enumerable.Count(); i++)
        {
            cumulative += weights[i];
            if (rnd < cumulative)
            {
                selectedIndex = i;
                break;
            }
        }

        return enumerable.ElementAt(selectedIndex);
    }
}
