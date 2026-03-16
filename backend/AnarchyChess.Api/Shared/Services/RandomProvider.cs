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
    T Softmax<T>(IEnumerable<T> items, Func<T, int> getScore, double temperature);
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

    public T NextItemWeighted<T>(IEnumerable<T> items, Func<T, int> getWeight)
    {
        List<int> weights = [.. items.Select(getWeight)];

        int totalWeight = weights.Sum();
        int rnd = _random.Next(totalWeight);

        int cumulative = 0;
        int selectedIndex = 0;
        for (int i = 0; i < items.Count(); i++)
        {
            cumulative += weights[i];
            if (rnd < cumulative)
            {
                selectedIndex = i;
                break;
            }
        }

        return items.ElementAt(selectedIndex);
    }

    public T Softmax<T>(IEnumerable<T> items, Func<T, int> getScore, double temperature)
    {
        double max = items.Max(getScore);
        double[] expScores = [.. items.Select(x => Math.Exp((getScore(x) - max) / temperature))];
        double sumExp = expScores.Sum();
        double[] probabilities = [.. expScores.Select(x => x / sumExp)];

        double threshold = NextDouble();
        double cum = 0; // hehe
        int selectedIdx = items.Count() - 1;

        for (int i = 0; i < items.Count(); i++)
        {
            cum += probabilities[i];
            if (cum >= threshold)
            {
                selectedIdx = i;
                break;
            }
        }
        return items.ElementAt(selectedIdx);
    }
}
