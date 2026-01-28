using AnarchyChess.Api.Shared.Services;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests;

public class RandomProviderTests
{
    [Fact]
    public void NextItemWeighted_selects_expected_item()
    {
        RandomProvider provider = new(new Random(6969));

        string[] items = ["A", "B", "C"];
        Dictionary<string, int> weights = new()
        {
            ["A"] = 3,
            ["B"] = 2,
            ["C"] = 1,
        };

        // A -> [0,1,2]
        // B -> [3,4]
        // C -> [5]
        // next is 3
        var result = provider.NextItemWeighted(items, i => weights[i]);

        result.Should().Be("B");
    }

    [Fact]
    public void NextItemWeighted_can_select_first_item()
    {
        // next is 0
        RandomProvider provider = new(new Random(1));

        var result = provider.NextItemWeighted(["A", "B"], _ => 1);

        result.Should().Be("A");
    }

    [Fact]
    public void NextItemWeighted_can_select_last_item()
    {
        // next is 2
        RandomProvider provider = new(new Random(0));

        var result = provider.NextItemWeighted(["A", "B", "C"], _ => 1);

        result.Should().Be("C");
    }
}
